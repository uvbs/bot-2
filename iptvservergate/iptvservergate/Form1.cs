using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FluentFTP;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Temp_Mail;

namespace iptvservergate
{
    public partial class Form1 : Form
    {
        TempMail email = null;
        String newmail = null;
        string nameList = "Lista.m3u";
        System.Timers.Timer t;
        System.Timers.Timer refresh; //Timer for autorefresh page
        Random random = new Random();
        string[] word1 = new string[] { "ciao", "hey", "salve", "yo", "Buonasera", "Buongiorno", "weila", "hi", "we" };
        string[] word2 = new string[] { "è possibile avere una prova?", "per favore, potreste mandarmi una lista?"
            ,"can i test?","come va? posso provare lista?","grazie mille, posso testare?" };
        string mailLink = null;
        string link = null;
       bool dwnList=false;
        bool received = false;
        DateTime hourDwn;
        //Thread CheckMail = null;

        /*****************Add key for webbrowser control to registry*****************/
        private static void addKey()
        {
            try
            {
                int BrowserVer, RegVal;

                // get the installed IE version
                using (System.Windows.Forms.WebBrowser Wb = new System.Windows.Forms.WebBrowser())
                    BrowserVer = Wb.Version.Major;

                // set the appropriate IE version
                if (BrowserVer >= 11)
                    RegVal = 11001;
                else if (BrowserVer == 10)
                    RegVal = 10001;
                else if (BrowserVer == 9)
                    RegVal = 9999;
                else if (BrowserVer == 8)
                    RegVal = 8888;
                else
                    RegVal = 7000;

                // set the actual key

                RegistryKey Key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
                RegistryKey Key2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION", true);
                string FindAppkey = Convert.ToString(Key.GetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe"));
                string FindAppkey2 = Convert.ToString(Key2.GetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe"));
                if (string.IsNullOrEmpty(FindAppkey))
                {
                    Key.SetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe", RegVal, RegistryValueKind.DWord);
                    Key.Close();

                }
                else if (string.IsNullOrEmpty(FindAppkey2))
                {
                    Key2.SetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe", RegVal, RegistryValueKind.DWord);
                    Key2.Close();
                }
                else
                    Console.WriteLine("\n"+GetTimestamp(DateTime.Now)+" Chiave nel registro già presente");
            }
            catch (System.Security.SecurityException)
            {
                Console.WriteLine("\n"+GetTimestamp(DateTime.Now)+
                    " Permessi amministratore richiesti per cambiare chiavi nel registro per il WebBrowser");
            }

        }

        //Initialize Form
        public Form1()
        {
            addKey();
            InitializeComponent();
            
            /*****************Start Navigation*****************/
            navigate();
        }

        /********************************Function refreshTimer********************************/
        private void refreshTimer()
        {
            refresh = new System.Timers.Timer(TimeSpan.FromSeconds(20).TotalMilliseconds); // set interval timer
            refresh.AutoReset = true;
            refresh.Elapsed += new ElapsedEventHandler(refreshPage);
            refresh.Start();
        }

        /********************************Function Navigation********************************/
        private async void navigate()
        {
            //Ogni 4 ore visita il sito
            while (true)
            {
                //email = getRandomEmail();
                getEmail();
                try
                {
                    Visible = true;
                    //WebProxy myProxy = new WebProxy("208.52.92.160:80");
                    //myRequest.Proxy = myProxy;
                    webBrowser1.Navigate("https://www.123contactform.com/js-form-username-2088859.html");
                    webBrowser2.Navigate("https://temp-mail.org/option/change/");
                    await Task.Delay(14400000); //wait 4 ore
                    refresh.Dispose();
                    if (dwnList)
                    {
                        TimeSpan startTime = Convert.ToDateTime(DateTime.Now).TimeOfDay;
                        TimeSpan endTime = Convert.ToDateTime(hourDwn).TimeOfDay;
                        TimeSpan diff = endTime > startTime ? endTime - startTime : endTime - startTime + TimeSpan.FromDays(1);
                        int hour = (int)diff.TotalHours-1;
                        Console.WriteLine(GetTimestamp(DateTime.Now) + " La lista sarà aggiornata tra: {0} ore ",hour);
                        await Task.Delay((hour*3600*1000)+1);
                    }
                    //CheckMail.Abort();
                    //t.Dispose();
                    // Thread dwnl = new Thread(threadDownload);
                    //dwnl.Start();
                    /*if (dwnl.IsAlive)
                    {
                        dwnl.Abort();
                        t.Dispose();
                    }*/
                }
                catch (Exception c)
                {
                    Console.WriteLine(GetTimestamp(DateTime.Now) + " " + c.Message);
                    continue;
                }
            }
        }

        /********************************Function remove file from local disk********************************/
        public void removeFile()
        {
            Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.m3u").ToList().ForEach(x => File.Delete(x));
            Console.WriteLine(GetTimestamp(DateTime.Now) + " Lista cancellata dal disco");
            dwnList = true;
        }

        public void getEmail()
        {
            List<string> domains=new List<string>();
            Random r = new Random();
            Random r2 = new Random();
            String name = RandomString(r2.Next(7, 10));
            domains.Add("@haydoo.com");
            domains.Add("@refurhost.com");
            domains.Add("@lilylee.com");
            domains.Add("@micsocks.net");
            domains.Add("@ucylu.com");
            domains.Add("@getapet.net");
            domains.Add("@dnsdeer.com");
            domains.Add("@lucyu.com");
            int x = r.Next(0, domains.Count());
            newmail = name + domains.ToArray().GetValue(x);
            Console.WriteLine(GetTimestamp(DateTime.Now) + " E-mail generata: " + newmail);
        }

        /********************************Function documentCompleted2 event checkmail********************************/
        private async void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                if(received && e.Url.ToString().Contains("about:blank"))
                {
                  if(webBrowser2.DocumentTitle.Contains("trial 6 Hours Full Package"))
                    {
                       
                    }
                }
                if (e.Url.Equals("https://temp-mail.org/option/change/"))
                {
                    webBrowser2.Document.GetElementsByTagName("input")[2].SetAttribute("value", newmail.Split('@')[0]);
                    webBrowser2.Document.GetElementById("domain").SetAttribute("value", "@" + newmail.Split('@')[1]);
                    await Task.Delay(5000);
                    webBrowser2.Document.GetElementById("postbut").InvokeMember("Click");
                    await Task.Delay(5000);
                    webBrowser2.Navigate("https://temp-mail.org/option/refresh");
                }
                else if (e.Url.Equals("https://temp-mail.org/") || e.Url.Equals("https://temp-mail.org/en/"))
                {
                    if (webBrowser2.DocumentText.Contains("iptv"))
                    {
                        refresh.Stop();
                       // Visible = false;
                        Console.WriteLine(GetTimestamp(DateTime.Now) + " Email ricevuta");
                        mailLink = webBrowser2.Document.GetElementById("mails").GetElementsByTagName("a")[0].GetAttribute("href");
                        Console.WriteLine(GetTimestamp(DateTime.Now) + " "+mailLink);
                       // mailLink=mailLink.Replace("view", "source");
                        received = true;
                       // refreshTimer();
                        //webBrowser2.Document.GetElementById("mails").GetElementsByTagName("a")[0].InvokeMember("Click");
                        webBrowser1.Navigate(mailLink);
                    }
                }
                else if (e.Url.Equals(mailLink+"/"))
                {

                }
                else if (e.Url.Equals("https://temp-mail.org/option/refresh/"))
                {
                    Console.WriteLine(GetTimestamp(DateTime.Now) + " Waiting email...");
                    refreshTimer();
                    /*CheckMail = new Thread(threadCheckMail);
                    CheckMail.Start();*/
                }
                else
                {
                    webBrowser2.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(GetTimestamp(DateTime.Now) + " " + ex.Message);
            }
        }

        /********************************Function documentCompleted event********************************/
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            if (e.Url.Equals(mailLink + "/"))
            {
                refresh.Stop();
                int i = 0;
                webBrowser2.Stop();
                webBrowser1.Stop();
                Console.WriteLine(GetTimestamp(DateTime.Now) + " Email link: " + mailLink);
                received = false;
                //CheckMail.Abort();
                String regex = "\\(?\\b(http://|www[.])[-A-Za-z0-9+&amp;@#/%?=~_()|!:,.;]*[-A-Za-z0-9+&amp;@#/%=~_()|]";
                Regex regx = new Regex(regex, RegexOptions.IgnoreCase);
                MatchCollection matches = regx.Matches(webBrowser1.DocumentText);
                link = matches[i].Value.Replace("&amp;", "&");
                while (!link.Contains("output="))
                {
                    link = matches[i].Value.Replace("&amp;", "&");
                   //link = matches[i].Value.Replace("3D", "");
                    i++;
                }
                Console.WriteLine(GetTimestamp(DateTime.Now) + " Link ricevuto: " + link);
                Console.WriteLine(GetTimestamp(DateTime.Now) + " Stato timer: " + refresh.Enabled);
                while (download() == 39) ;
                SplitList(nameList);
                UploadFtp(); //Upload to server FTP
            }
            if (e.Url.ToString().CompareTo("https://www.123contactform.com/js-thank-you-2088859.html") == 0)
            {
                Console.WriteLine(GetTimestamp(DateTime.Now) + " Richiesta inviata");
                System.Threading.Thread.Sleep(4000);
                webBrowser1.Stop();
                return;
            }
            bool loop = false;
            while (!loop)
            {
                try
                {
                    string message = String.Format("{0} {1}", word1[random.Next(word1.Length)], word2[random.Next(word2.Length)]);
                    // webBrowser1.Document.GetElementById("id123-control20555312").InnerText = email.MailName;
                    //webBrowser1.Document.GetElementById("id123-control20556161").InnerText = email.Mail;
                    webBrowser1.Document.GetElementById("id123-control20555312").InnerText = newmail.Split('@')[0];
                    webBrowser1.Document.GetElementById("id123-control20556161").InnerText = newmail;
                    System.Threading.Thread.Sleep(3000);
                    webBrowser1.Document.GetElementById("id123-control20555328").GetElementsByTagName("option")[80].SetAttribute("selected", "selected");
                    webBrowser1.Document.GetElementById("id123-control29643997_0").InvokeMember("Click");
                    System.Threading.Thread.Sleep(5000);
                    //webBrowser1.Document.GetElementById("id123-control20555329").InnerText = message;
                    webBrowser1.Document.GetElementById("id123-button-send").InvokeMember("Click");
                    loop = true;
                }
                catch (System.NullReferenceException)
                {
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine(GetTimestamp(DateTime.Now) + " Richiesta non inviata");
                    break;
                }
            }
        }

        /********************************Function removeOldM3ufiles from FTP server********************************/
        private void removeOldM3uFiles()
        {
            string host = "files.000webhost.com";
            string path = "/public_html/";
            string user = "user";
            string pass = "pass";
            var credentials = new NetworkCredential(user, pass);
            try
            {
                using (FtpClient conn = new FtpClient())
                {
                    conn.Host = host;
                    conn.Credentials = credentials;
                    foreach (FtpListItem item in conn.GetListing(path, FtpListOption.AllFiles | FtpListOption.ForceList))
                    {
                        switch (item.Type)
                        {
                            case FtpFileSystemObjectType.File:
                                conn.DeleteFile(item.FullName);
                                Console.WriteLine(GetTimestamp(DateTime.Now)+" File cancellato: "+item.Name);
                                break;  
                        }
                    }

                }
            }
            catch (Exception e2)
            {
                Console.WriteLine("Liste non cancellate " + e2.Message);
                removeOldM3uFiles();
            }
        }

        /********************************Function upload list to FTP and rename list********************************/
        private void UploadFtp()
        {
            removeOldM3uFiles();
            nameList = "New" + nameList;
            DateTime data = DateTime.Now;
            data = data.AddHours(6);
            hourDwn = data;
            string rename = "ListaTV_Scade_" + data.Day + "_" + data.Month + "_" + data.Year + "_" + data.Hour + "_" + data.Minute + ".m3u";

            //Rename to ListTV_Scade_Hour_Minutes.m3u and delete Lista.m3u
            if (File.Exists(nameList))
            {
                File.Copy(nameList, rename, true);
                File.Delete(nameList);
                Console.WriteLine(GetTimestamp(DateTime.Now) + " Lista rinominata: " + rename);
            }
            string host = "files.000webhost.com";
            string path = "/public_html/";
            string user = "user";
            string pass = "pass";
            string source = rename;
            try
            {
                FtpClient client = new FtpClient();
                client.Host = host;
                client.Credentials = new NetworkCredential(user, pass);
                client.Connect();
                Console.WriteLine(GetTimestamp(DateTime.Now) + " Upload lista: " + client.GetWorkingDirectory() + @"/" + source);
                client.UploadFile(source, path + source);
                client.Disconnect();
                Console.WriteLine(GetTimestamp(DateTime.Now) + " Lista caricata sul server");
                removeFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lista non caricata " + ex.Message);
                Console.WriteLine(GetTimestamp(DateTime.Now) + " Riprovo caricamento...");
                UploadFtp();
            }
        }

        /********************************Download list******************************************/
        private int download()
        {
            int length = 0;
                try
                {
                    new WebClient().DownloadFile(new Uri(link), nameList);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(GetTimestamp(DateTime.Now) + " " + ex.Message);
                    System.Threading.Thread.Sleep(1800000);
                    new WebClient().DownloadFile(new Uri(link), nameList);
            }
            //Check if the list is good
            var lines = File.ReadLines(nameList);
            foreach (var line in lines)
            {
                if (line.Contains("#EXTM3U"))
                    length = line.Length;
                else
                    length = line.Length;
            }
            Console.WriteLine(GetTimestamp(DateTime.Now) + " Lista scaricata");
            refresh.Stop();
            //t.Stop();
            //webBrowser2.Navigate(mailLink.Replace("view", "delete"));
            mailLink = null;
            link = null;
            return length;
        }

        /********************************refresh page every X minutes********************************/
        private void refreshPage(object sender, ElapsedEventArgs e)
        {
            if (received == true)
            {
                webBrowser2.Navigate(mailLink);
            }
            webBrowser2.Navigate("https://temp-mail.org/");
            dwnList = false;
        }

        /********************************Function timestamp********************************/
        public static string GetTimestamp(DateTime value)
        {
            return value.ToString("[HH:mm:ss]");
        }

        /********************************Function get temporary mail********************************/
        private static TempMail getRandomEmail()
        {
            TempMail temp = new TempMail();
            while (true)
            {
                try
                {
                    temp.GetDomains();
                    temp.GetNewMail();
                    Console.WriteLine(GetTimestamp(DateTime.Now) + " E-mail generata: " + temp.Mail);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(GetTimestamp(DateTime.Now) + " Problema dominio mails " + e.Message);
                }
            }
            return temp;
        }

        /********************************Function clear list with only IT channels********************************/
        private static void SplitList(string name)
        {
            string line;
            StreamReader file = new StreamReader(name);
            StreamWriter outputFile = new StreamWriter("New" + name);
            while ((line = file.ReadLine()) != null && (line.CompareTo("#EXTINF:-1,------ ITALY ------") != 0)) ;
            do{
                 outputFile.WriteLine(line);
                 line = file.ReadLine();
            } while ((line.CompareTo("#EXTINF:-1,----- NORDIC -----") != 0) && (line != null));
            outputFile.Close();
            file.Close();
        }


        //function non utilizzate
        /********************************Function threadCheckMail********************************/
        private void threadCheckMail()
        {
            t = new System.Timers.Timer(TimeSpan.FromMinutes(30).TotalMilliseconds); // set interval timer
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(refreshPage);
            t.Start();
            while (mailLink == null)
            {
                webBrowser2.Navigate("https://temp-mail.org/");
                Thread.Sleep(10 * 60 * 1000); //Refresh every X minutes  
            }
            t.Dispose();
            webBrowser2.Stop();
        }

        private string RandomString(int Size)
        {
            string input = "abcdefghijklmnopqrstuvwxyz";
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < Size; i++)
            {
                ch = input[random.Next(0, input.Length)];
                builder.Append(ch);
            }
            return builder.ToString();
        }

        /********************************Download list from server(check link every X mins)********************************/
        private int download2()
        {
            bool done = false;
            int length = 0;
            var url = "http://dmtn-iptv.com:8080/get.php?username=" + email.MailName + "&password=" + email.MailName + "&type=m3u&output=ts";
            //test url
            //var url = "http://dmtn-iptv.com:8080/get.php?username=ASgpjxisae&password=orpsyMbzIG&type=m3u&output=ts";
            Console.WriteLine(GetTimestamp(DateTime.Now) + " Check Download link...");
            while (!done)
            {
                try
                {
                    new WebClient().DownloadFile(new Uri(url), nameList);
                    done = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(420000); // Check download link every 7 mins
                }
            }
            //Check if the list is right
            var lines = File.ReadLines(nameList);
            foreach (var line in lines)
            {
                if (line.Contains("#EXTM3U"))
                    length = line.Length;
                else
                    length = line.Length;
            }
            Console.WriteLine(GetTimestamp(DateTime.Now) + " Lista scaricata");
            t.Stop();
            return length;
        }

        /*********************************Function threadDownload********************************/
        private void threadDownload()
        {
            t = new System.Timers.Timer(TimeSpan.FromMinutes(35).TotalMilliseconds); // set interval timer
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(refreshPage);
            t.Start();
            while (download() == 39) ;
            SplitList(nameList);
            UploadFtp(); //Upload to server FTP
            t.Dispose();
        }

           Letter[] mails= email.GetLetters();
            foreach (Letter mail in mails)
            {
                Console.WriteLine(mail.MailTextOnly);
            }
           /* string urlxml = @"http://api.temp-mail.ru/request/mail/id/";
            string url = String.Format(urlxml + @"{0}/format/xml/", email.MailHashString);
            var client = (HttpWebRequest)WebRequest.Create(new Uri(url));
            client.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            client.CookieContainer = new CookieContainer();
            client.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
            var htmlCodae = client.GetResponse() as HttpWebResponse;
            */
            /*string urlxml = @"http://api.temp-mail.ru/request/mail/id/";
            string url = String.Format(urlxml + @"{0}/format/xml/", email.MailHashString);
            Console.WriteLine(url);
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 3000;
            request.UserAgent = "Mozilla";
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)
                    request.GetResponse();
            }
            catch (WebException e)
            {
                response = (HttpWebResponse)e.Response;
            }
            Console.WriteLine("- " + response.StatusCode);

            XmlTextReader reader = new XmlTextReader(response.GetResponseStream());*/

            /*  XmlTextReader reader = new XmlTextReader(urlxml);
			  Console.WriteLine(reader.BaseURI);
			  webBrowser1.Navigate(urlxml);



			  while (reader.Read())
			   {
				   // Do some work here on the data.
				   Console.WriteLine(reader.Name);
			   }*/

            /*  HttpWebRequest webRequest = (HttpWebRequest)WebRequest.CreateHttp(urlxml);


			  WebResponse response = webRequest.GetResponse();
			  Stream responseStream = response.GetResponseStream();
			  StreamReader responseReader = new StreamReader(responseStream);
			  string responseString = responseReader.ReadToEnd();
			  XmlDocument xmlDoc = new XmlDocument();
			  xmlDoc.LoadXml("<xml>" + responseString + "</xml");
			  XmlNodeList address = xmlDoc.GetElementsByTagName("error");
			  Console.WriteLine(address.Item(0));*/

            //Console.WriteLine(mails[0].MailTextOnly);

        }
    }
}