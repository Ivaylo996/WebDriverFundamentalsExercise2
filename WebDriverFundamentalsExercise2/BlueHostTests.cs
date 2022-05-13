using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

[assembly: Parallelizable(ParallelScope.Fixtures)]
namespace WebDriverFundamentalsExercise2
{
    [TestFixture("Chrome", "101.0", "Windows 10", "Chrome100")]
    [TestFixture("Safari", "15.0", "MacOS Monterey", "Safari16")]
    [TestFixture("Firefox", "99.0", "Windows 10", "Firefox99")]
    public class BlueHostTests
    {
        IWebDriver _driver;
        private static string _browser;
        private static string _browserVersion;
        private static string _os;
        private static string _name;

        public BlueHostTests(string browser, string browserVersion, string os, string name)
        {
            _browser = browser;
            _browserVersion = browserVersion;
            _os = os;
            _name = name;
        }

        [SetUp]
        public void Setup()
        {
            var lambdaTestUseName = Environment.GetEnvironmentVariable("LT_USERNAME", EnvironmentVariableTarget.User);
            var lambdaTestAccessKey = Environment.GetEnvironmentVariable("LT_ACCESS_KEY", EnvironmentVariableTarget.User);
            dynamic capabilities = GetBrowserOptions(_browser);
            var ltOptions = new Dictionary<string, object>();
            ltOptions.Add("user", Environment.GetEnvironmentVariable("LT_USERNAME"));
            ltOptions.Add("accessKey", Environment.GetEnvironmentVariable("LT_ACCESS_KEY"));
            ltOptions.Add("build", "WebFundamentalsExercise2");
            ltOptions.Add("name", _name);
            ltOptions.Add("platformName", _os);
            capabilities.AddAdditionalOption("LT:Options", ltOptions);

            capabilities.PageLoadStrategy = PageLoadStrategy.Eager;

            string checkUrl = $"https://{Environment.GetEnvironmentVariable("LT_USERNAME")}:{Environment.GetEnvironmentVariable("LT_ACCESS_KEY")}@hub.lambdatest.com/wd/hub";
            _driver = new RemoteWebDriver(new Uri($"https://{Environment.GetEnvironmentVariable("LT_USERNAME")}:{Environment.GetEnvironmentVariable("LT_ACCESS_KEY")}@hub.lambdatest.com/wd/hub"), capabilities.ToCapabilities());

            _driver.Manage().Window.Maximize();
            _driver.Url = "https://login.bluehost.com/hosting/webmail";

            var gdprButton = WaitAndFindElement(By.XPath("//*[@id='onetrust-accept-btn-handler']"));

            if (gdprButton.Displayed)
            {
                gdprButton.Click();
            }
        }

        [Test]
        public void TryToAuthenticate_With_IncorrectEmail_And_IncorrectPassword()
        {
            var loginEmail = WaitAndFindElement(By.XPath("//article//*[@id='email']"));
            var loginPassword = WaitAndFindElement(By.XPath("//article//*[@id='password']"));
            var loginButton = WaitAndFindElement(By.XPath("//article//*[@class='btn_secondary']"));
            var expectedEmailError = "Invalid login attempt. That account doesn't seem to be available.";

            loginEmail.SendKeys("wrongUser_asdf");
            loginPassword.SendKeys("wrongPassword_asdf");
            loginButton.Click();

            var actualEmailError = WaitAndFindElement(By.XPath("//article//*[contains(text(),'Invalid login attempt')]")).Text;

            Assert.AreEqual(expectedEmailError, actualEmailError);
        }

        [Test]
        public void TryToAuthenticate_With_WithEmptyEmail_And_EmptyPassword()
        {
            var loginButton = WaitAndFindElement(By.XPath("//article//*[@class='btn_secondary']"));

            loginButton.Click();

            var actualEmailError = WaitAndFindElement(By.XPath("//article//span[contains(text(),'Email')]")).Text.Trim();
            var actualPasswordError = WaitAndFindElement(By.XPath("//article//span[contains(text(),'Password')]")).Text.Trim();

            Assert.AreEqual("Email is required.", actualEmailError);
            Assert.AreEqual("Password is required.", actualPasswordError);
        }

        [TearDown]
        public void TestCleanUp()
        {
            _driver.Quit();
        }

        private IWebElement WaitAndFindElement(By by)
        {
            var webDriverWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            return webDriverWait.Until(ExpectedConditions.ElementExists(by));
        }

        private dynamic GetBrowserOptions(string browserName)
        {
            switch (browserName)
            {
                case "Chrome":
                    return new ChromeOptions();
                case "Safari":
                    return new SafariOptions();
                case "Firefox":
                    return new FirefoxOptions();
                default:
                    return new ChromeOptions();
            }
        }
    }
}