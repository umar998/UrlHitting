using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace UrlHitting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");

            using (var driver = new ChromeDriver(options))
            {
                for (int page = 0; page < 5; page++)
                {
                    string url = $"https://www.google.com/search?q=pakistan&start={page * 10}";
                    driver.Navigate().GoToUrl(url);

                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

                    // Define a custom condition to wait for the presence of search result links
                    Func<IWebDriver, IReadOnlyCollection<IWebElement>> searchResultsCondition = drv =>
                    {
                        var searchResultLinks = drv.FindElements(By.CssSelector("div.g a"));
                        if (searchResultLinks.Count > 0)
                            return searchResultLinks;
                        return null;
                    };

                    var searchResultLinks = wait.Until(searchResultsCondition);

                    if (searchResultLinks == null)
                    {
                        Console.WriteLine("No search result links found.");
                        return;
                    }

                    string outputPath = @"C:\Users\umara\Desktop\AKSA Tasks\BackEndC#\User\Url\UrlHitting\search_results.txt";

                    using (StreamWriter writer = new StreamWriter(outputPath, true))
                    {
                        writer.WriteLine("Page " + (page + 1)); // Write the page number

                        int linkCount = 0;
                        HashSet<string> capturedDomains = new HashSet<string>(); // To track captured domains

                        foreach (var link in searchResultLinks)
                        {
                            if (linkCount >= 5) // Capture only top 5 links
                                break;

                            // Skip non-link elements (e.g., ads)
                            if (string.IsNullOrEmpty(link.GetAttribute("href")))
                                continue;

                            string urlToCapture = link.GetAttribute("href");

                            if (IsRelatedLink(urlToCapture))
                                continue;

                            Uri uri = new Uri(urlToCapture);
                            string domain = uri.Host.ToLower();

                            // Check if the domain was already captured
                            if (capturedDomains.Contains(domain))
                                continue;

                            capturedDomains.Add(domain);

                            writer.WriteLine(new string('-', 30) + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + new string('-', 30));
                            writer.WriteLine("URL: " + urlToCapture);

                            try
                            {
                                string title = link.FindElement(By.CssSelector("h3")).Text;
                                writer.WriteLine("Title: " + title);
                            }
                            catch (NoSuchElementException)
                            {
                                // Handle if title not found
                                Console.WriteLine("Title not found for link: " + urlToCapture);
                            }

                            linkCount++;
                        }
                    }
                }
            }
        }

        // Function to check if a URL is a related link
        static bool IsRelatedLink(string url)
        {
            return url.Contains("google.com/search?sca_esv") || url.Contains("data-ved=");
        }
    }
}
