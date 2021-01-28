using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ScholarlyInsightsTelegramBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace ScholarlyInsightsTelegramBot
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> links = new List<string>() { 
                //@"https://twitter.com/Contentions",
                @"https://twitter.com/drshadeeelmasry" };
            foreach (string link in links)
            {
                ScrapeFeed(link);
            }
        }

        public static void ScrapeFeed(string link)
        {
            DownloadLatestVersionOfChromeDriver(); 

            //new DriverManager().SetUpDriver(new ChromeConfig());
            var chromeOptions = new ChromeOptions();

            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
            // chromeOptions.AddArgument("headless");

            IWebDriver driver = new ChromeDriver(chromeOptions);
            //BMGServices.Selenium.Main.MakeChromeDriver(chromeOptions);


            driver.Url = link;
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(link);
            Console.WriteLine(Environment.NewLine);


            List<Tweet> tweets = new List<Tweet>();
 

            int i_article = 0;
            

            var articles = driver.FindElements(By.TagName("article")).ToList();
        
            foreach (var article in articles)
            {
                Tweet tweet = new Tweet(); 
                int i_span = 0;
                //Console.WriteLine($"Article : {i_article}");
                string aText = article.Text;
                var time = article.FindElements(By.TagName("time")).ToList();
                var spans = article.FindElements(By.TagName("span")).ToList();

                string datetimeString = article.FindElements(By.TagName("time")).First().GetAttribute("datetime");
                DateTime tweetDate = DateTime.Now;
                bool successParse = DateTime.TryParse(datetimeString, out tweetDate);
                if (i_article != 0 && tweetDate < DateTime.Today)
                {
                    break;
                }
                if(i_article == 2)
                {

                }
                foreach (var span in spans)
                {
                    Console.WriteLine($"Span : {i_span}");
                    string sText = span.Text;
                    if(sText == "Pinned Tweet")
                    {
                        break;
                    }
                    if (i_span == 4)
                    {
                        if (successParse)
                        {
                            tweet.postDate = tweetDate;
                            tweet.tweetText = sText;
                            //Console.WriteLine(tweetDate.ToString());
                            //Console.WriteLine(sText);
                            //Console.WriteLine(Environment.NewLine);
                            tweets.Add(tweet);
                        }
                       
                    }
                    if(sText.ToLowerInvariant() == "show this thread")
                    {
                        //span.Click();
                    }
                    i_span++;
                  
                }
                i_article++;
            }

            foreach(Tweet tweet in tweets)
            {
                Console.WriteLine();
                Console.WriteLine(tweet.postDate);
                Console.WriteLine(tweet.tweetText);
                Console.WriteLine();
            }

            int x = 76;




            driver.Close();
            driver.Quit();
        }

        public static void DownloadLatestVersionOfChromeDriver()
        {
            // string path = DownloadLatestVersionOfChromeDriverGetVersionPath();
            var version = getChromeVersion();
            var urlToDownload = DownloadLatestVersionOfChromeDriverGetURLToDownload(version);
            DownloadLatestVersionOfChromeDriverKillAllChromeDriverProcesses();
            DownloadLatestVersionOfChromeDriverDownloadNewVersionOfChrome(urlToDownload);
        }

        public static string DownloadLatestVersionOfChromeDriverGetChromeVersion(string productVersionPath)
        {
            if (String.IsNullOrEmpty(productVersionPath))
            {
                throw new ArgumentException("Unable to get version because path is empty");
            }

            if (!File.Exists(productVersionPath))
            {
                throw new FileNotFoundException("Unable to get version because path specifies a file that does not exists");
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(productVersionPath);
            if (versionInfo != null && !String.IsNullOrEmpty(versionInfo.FileVersion))
            {
                return versionInfo.FileVersion;
            }
            else
            {
                throw new ArgumentException("Unable to get version from path because the version is either null or empty: " + productVersionPath);
            }
        }

        public static string DownloadLatestVersionOfChromeDriverGetURLToDownload(string version)
        {
            if (String.IsNullOrEmpty(version))
            {
                throw new ArgumentException("Unable to get url because version is empty");
            }

            //URL's originates from here: https://chromedriver.chromium.org/downloads/version-selection
            string html = string.Empty;
            string urlToPathLocation = @"https://chromedriver.storage.googleapis.com/LATEST_RELEASE_" + String.Join(".", version.Split('.').Take(3));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlToPathLocation);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            if (String.IsNullOrEmpty(html))
            {
                throw new WebException("Unable to get version path from website");
            }

            return "https://chromedriver.storage.googleapis.com/" + html + "/chromedriver_win32.zip";
        }

        public static void DownloadLatestVersionOfChromeDriverKillAllChromeDriverProcesses()
        {
            //It's important to kill all processes before attempting to replace the chrome driver, because if you do not you may still have file locks left over
            var processes = Process.GetProcessesByName("chromedriver");
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    //We do our best here but if another user account is running the chrome driver we may not be able to kill it unless we run from a elevated user account + various other reasons we don't care about
                }
            }
        }

        public static void DownloadLatestVersionOfChromeDriverDownloadNewVersionOfChrome(string urlToDownload)
        {
            if (String.IsNullOrEmpty(urlToDownload))
            {
                throw new ArgumentException("Unable to get url because urlToDownload is empty");
            }
            string driverlocation = Path.GetFullPath(Directory.GetCurrentDirectory()) + @"\";

            //Downloaded files always come as a zip, we need to do a bit of switching around to get everything in the right place
            using (var client = new WebClient())
            {
                if (File.Exists(driverlocation + @"chromedriver.zip"))
                {
                    File.Delete(driverlocation + @"chromedriver.zip");
                }
                client.DownloadFile(urlToDownload, driverlocation + @"chromedriver.zip");

                if (File.Exists(driverlocation + @"chromedriver.exe"))
                {
                    File.Delete(driverlocation + @"chromedriver.exe");
                }

                if (File.Exists(driverlocation + @"chromedriver.zip"))
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(driverlocation + @"chromedriver.zip", driverlocation);
                }
            }
        }


        public static string getChromeVersion()
        {
            try
            {
                RegistryKey urlKey = Registry.CurrentUser.OpenSubKey(@"Software\Google\Chrome\BLBeacon");


                if (urlKey != null)
                {
                    object urlObject = urlKey.GetValue("version");

                    if (urlObject != null)
                    {
                        return urlObject.ToString();

                    }
                }

            }
            catch (Exception e)
            {
                var x = 1;
                Console.WriteLine(e.ToString());
            }


            return "UNKNOWN";
        }
    }
}
