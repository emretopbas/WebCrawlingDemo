using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Net;
using System.Diagnostics.Metrics;
using WebCrawlingDemo;

public class Program
{
    static void Main(string[] args)
    {
        //var cService = ChromeDriverService.CreateDefaultService(); 
        //cService.HideCommandPromptWindow = true; 
        //IWebDriver webdriver; 
        //var options = new ChromeOptions(); 
        //options.AddArgument(""); 
        //options.AddArgument("--proxy-server="); 
        //webdriver = new ChromeDriver(options); 
        //webdriver.Navigate().GoToUrl("https://www.sahibinden.com"); 

        CheckForCorrectProxies();

        //SendSeleniumRequests();

    }

    private static void SendSeleniumRequests()
    {

        var proxyList = GetProxyIpList();

        foreach (var p in proxyList)
        {
            try
            {
                
                Console.WriteLine("[DENEME] İSTEK ATAN IP: " + p.address + ":" + p.port);
                IWebDriver driver;
                var options = new ChromeOptions();
                options.AddArgument("--enable-javascript");
                options.AddArgument("--disable-web-security");
                options.AddArgument("--allow-running-insecure-content");
                //options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--proxy-server=" + "https://" + p.address + ":" + p.port);
                options.AddArgument("ignore-certificate-errors");
                options.AddArgument("");
                options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
                driver = new ChromeDriver(options);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));

                driver.Navigate().GoToUrl("https://www.sahibinden.com");
                driver.Manage().Timeouts().ImplicitWait= TimeSpan.FromSeconds(10);

                var revealed = driver.FindElement(By.TagName("body"));

                wait.Until(d => revealed.Displayed);

                Console.WriteLine(revealed.Text);

                if(revealed != null)
                {
                    Console.Write("BAŞARILI \n BAŞARILI \n");
                    Console.WriteLine(revealed.Text);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                continue;
            }

        }

    }

    public static List<ProxyList> GetProxyIpList()
    {
        List<ProxyList> proxies = new List<ProxyList>();

        using (StreamReader sr = new StreamReader("C:\\Users\\emret\\source\\repos\\.NetTestProjects\\CrawlingApp\\Test\\proxy_list.txt"))
        {
            while (sr.Peek() >= 0)
            {
                string str;
                string[] strArray;
                str = sr.ReadLine();
                strArray = str.Split(':');
                ProxyList proxyList = new ProxyList();
                proxyList.address = strArray[0];
                proxyList.port = Int32.Parse(strArray[1]);
                proxies.Add(proxyList);

            }
            sr.Close();
        }
        return proxies;

    }

    public static void CheckForCorrectProxies()
    {
        string url = "http://ipinfo.io/json";
        Uri uri = new Uri(url);
        var proxyList = GetProxyIpList();

        List<ProxyList> successfulProxies = new List<ProxyList>();

        foreach (var p in proxyList)
        {
            try
            {
                Console.WriteLine("istek atan proxy : " + p.address+":"+p.port);

                WebProxy proxy = new WebProxy(p.address, p.port);

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.Method = "GET";
                httpWebRequest.UserAgent = "Mozilla/5.0";
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml";
                httpWebRequest.Proxy = proxy;

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Console.WriteLine(httpWebResponse.StatusCode+"\n");
                if(httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine(proxyList);
                    successfulProxies.Add(p);
                }

            }
            catch (WebException ex)
            {
                using (var stream = ex?.Response?.GetResponseStream())
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            Console.WriteLine(reader.ReadToEnd());
                            Console.WriteLine(ex.InnerException);
                        }
                    }
                }
                continue;
            }

        }
        if (successfulProxies.Count > 0)
        {
            var counter = successfulProxies.Count;
            while (counter > 0)
            {
                
                using (StreamWriter writer = new StreamWriter("C:\\Users\\emret\\source\\repos\\.NetTestProjects\\CrawlingApp\\Test\\proxy_list.txt"))
                {
                    writer.WriteLine("BAŞARILILAR: \n"+successfulProxies.Select(x=> x.address).ToString()+":"+successfulProxies.Select(x=>x.port).ToString());
                }
            }
        }

    }
}