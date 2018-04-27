using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Temp_Mail
{
    internal sealed class RGenerator
    {
        const int BufferSize = 1024;
        byte[] RandomBuffer;
        int BufferOffset;
        static System.Security.Cryptography.RNGCryptoServiceProvider rng;
        static RGenerator()
        {
            rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        }
        public RGenerator()
        {
            RandomBuffer = new byte[BufferSize];
            BufferOffset = RandomBuffer.Length;
        }
        private byte Next()
        {
            if (BufferOffset >= RandomBuffer.Length)
            {
                rng.GetBytes(RandomBuffer);
                BufferOffset = 0;
            }
            return RandomBuffer[BufferOffset++];
        }
        public int Next(int minValue, int maxValue)
        {
            int range = maxValue - minValue;
            return minValue + Next() % range;
        }

        const int MinStringLength = 6;
        const int MaxStringLength = 10;
        public string NextString()
        {
            StringBuilder sb = new StringBuilder();
            int count = Next(MinStringLength, MaxStringLength);
            for (int i = 0; i < count; i++)
                sb.Append((char)Next('a', 'z'));
            return sb.ToString();
        }
    }
    internal static class HTTPWrapper
    {
        static DateTime unix = new DateTime(1970, 1, 1);

        static XDocument Get(string url)
        {
            try
            {
                var req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.AutomaticDecompression = DecompressionMethods.GZip;
                req.UserAgent = "CSWrap";
                using (var resp = req.GetResponse())
                using (var s = resp.GetResponseStream())
                    return XDocument.Load(s);
            }
            catch (WebException e)
            {
                if ((e.Status & WebExceptionStatus.ProtocolError) == WebExceptionStatus.ProtocolError)
                    return null;
                throw;
            }
            catch { throw; }
        }
        public static List<string> GetDomains()
        {
            var xml = Get("http://api.temp-mail.org/request/domains/format/xml/");
            
            return xml.Root.Elements().Select(x => x.Value).ToList();
        }
        public static Letter[] GetLetters(string hash, string lastId = null)
        {
           
            var t = Get(string.Format(@"http://api.temp-mail.org/request/mail/id/{0}/format/xml/", hash));
            if (t == null)
                return new Letter[0];

            //Жалко, что нельзя просто запросить новые сообщения без кучи старых,
            //А новые сообщения оказываются в самом конце этой кучи =(
            var arr = t.Root.Elements().Reverse().ToArray();
            var result = new List<Letter>(arr.Length);
            foreach (var x in arr)
            {
                string mailId = x.Element("mail_id").Value;
                if (lastId == mailId)
                    break;
                result.Add(new Letter
                {
                    MailId = mailId,
                    MailAddressId = x.Element("mail_address_id").Value,
                    MailFrom = x.Element("mail_from").Value,
                    MailSubject = x.Element("mail_subject").Value,
                    MailPreview = x.Element("mail_preview").Value,
                    MailTextOnly = x.Element("mail_text_only").Value,
                    MailText = x.Element("mail_text").Value,
                    MailHtml = x.Element("mail_html").Value,
                    MailTimestamp = unix + TimeSpan.FromSeconds(double.Parse(x.Element("mail_timestamp").Value))
                });
            };
            return result.ToArray();
        }
    }
    /// <summary>
    /// Сообщение
    /// </summary>
    public struct Letter
    {
        /// <summary>
        /// Уникальный идентификатор письма, присвоенный системой
        /// </summary>
        public string MailId;
        /// <summary>
        /// Md5 хеш почтового адреса
        /// </summary>
        public string MailAddressId;
        /// <summary>
        /// Отправитель
        /// </summary>
        public string MailFrom;
        /// <summary>
        /// Тема
        /// </summary>
        public string MailSubject;
        /// <summary>
        /// Предпросмотр сообщения
        /// </summary>
        public string MailPreview;
        /// <summary>
        /// Cообщение в текстовом или в html формате (основной)
        /// </summary>
        public string MailTextOnly;
        /// <summary>
        /// Cообщение только в текстовом формате
        /// </summary>
        public string MailText;
        /// <summary>
        /// Cообщение только в html формате
        /// </summary>
        public string MailHtml;
        /// <summary>
        /// Время
        /// </summary>
        public DateTime MailTimestamp;
    }
    /// <summary>
    /// API для работы с temp-mail.ru
    /// </summary>
    public sealed class TempMail
    {
        RGenerator gen;
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

        public string MailName;
        public string lastMailId;
        /// <summary>
        /// Доступные домены
        /// </summary>
        public List<string> Domains { get; set; }
        /// <summary>
        /// Хэш используемого ящика
        /// </summary>
        public byte[] MailHash { get; private set; }

        /// <summary>
        /// Хэш используемого ящика
        /// </summary>
        public string MailHashString { get; private set; }
        /// <summary>
        /// Имя используемого ящика
        /// </summary>
        public string Mail { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public TempMail()
        {
            gen = new RGenerator();
        }
        

        /// <summary>
        /// Получает список доступных доменов (@domain)
        /// </summary>
        public void GetDomains()
        {
            Domains = HTTPWrapper.GetDomains();
            //Domains.Add("@rootfest.net");
            
        }

        /// <summary>
        /// Random name(6-10 length string)@Random domain
        /// </summary>
        public void GetNewMail() { GetNewMail(gen.NextString()); }
        /// <summary>
        /// mailName@Random domain
        /// </summary>
        /// <param name="mailName"></param>
        public void GetNewMail(string mailName)
        {
            var domain = Domains[gen.Next(0, Domains.Count)];
            MailName = mailName;
            Mail = mailName + domain;
            
            MailHash = md5.ComputeHash(Encoding.ASCII.GetBytes(Mail));
            StringBuilder sb = new StringBuilder(MailHash.Length * 2);
            for (int i = 0; i < MailHash.Length; i++)
                sb.Append(MailHash[i].ToString("X2"));
            MailHashString = sb.ToString();
        }
        /// <summary>
        /// Получает сообщения с почты
        /// </summary>
        /// <returns>Пустой массив, если нету сообщений</returns>
        public Letter[] GetLetters()
        {
            var result = HTTPWrapper.GetLetters(MailHashString);
            if (result.Length > 0)
                lastMailId = result[0].MailId;
            return result;
        }
        /// <summary>
        /// Возвращает только новые сообщения
        /// </summary>
        /// <returns>Пустой массив, если нету новых сообщений</returns>
        public Letter[] GetNewLetters()
        {
            var result = HTTPWrapper.GetLetters(MailHashString, lastMailId);
            if (result.Length > 0)
                lastMailId = result[0].MailId;
            return result;
        }
    }
}