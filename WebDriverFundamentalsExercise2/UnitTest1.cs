using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using System;
using WebDriverManager;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Firefox;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

[assembly:Parallelizable(ParallelScope.Fixtures)]
namespace WebDriverFundamentalsExercise2
{
    [TestFixture("Chrome", "101.0", "Windows 10", "Chrome100")]
    [TestFixture("Safari", "15.0", "MacOS Monterey", "Safari16")]
    [TestFixture("Firefox", "99.0", "Windows 10", "Firefox99")]
    public class Tests
    {
        IWebDriver _driver;
        private string _ltUserName = "ivaylo.dimg";
        private string _ltAppKey = "sp72iFEuOCAov1M36gxDcwsqYTFkIfsNI85W670Ai7QRysEkKq";
        private static string _browser;
        private static string _browserVersion;
        private static string _os;
        private static string _name;

        public Tests(string browser, string browserVersion, string os, string name)
        {
            _browser = browser;
            _browserVersion = browserVersion;
            _os = os;
            _name = name;
        }

        [SetUp]
        public void Setup()
        {
            dynamic capabilities = GetBrowserOptions(_browser);

            var ltOptions = new Dictionary<string, object>();
            ltOptions.Add("user", _ltUserName);
            ltOptions.Add("accessKey", _ltAppKey);
            ltOptions.Add("build", "WebFundamentalsExercise2");
            ltOptions.Add("name", _name);
            ltOptions.Add("platformName", _os);
            capabilities.AddAdditionalOption("LT:Options", ltOptions);

            capabilities.PageLoadStrategy = PageLoadStrategy.Eager;

            _driver = new RemoteWebDriver(new Uri($"https://{_ltUserName}:{_ltAppKey}@hub.lambdatest.com/wd/hub"), capabilities.ToCapabilities());

            //Implicit wait
            //_driver.Manage().Timeouts().ImplicitWait = System.TimeSpan.FromSeconds(60);

            _driver.Manage().Window.Maximize();
            _driver.Url = "https://login.bluehost.com/hosting/webmail";

            //Explicit wait and find element
            var gdprButton = WaitAndFindElement(By.XPath("//*[@id='onetrust-accept-btn-handler']"));

            gdprButton.Click();
        }

        //Test1
        [Test]
        public void TryLoggedIn_With_IncorrectEmail_And_IncorrectPassword()
        {
            var loginEmail = WaitAndFindElement(By.XPath("//article//*[@id='email']"));
            var loginPassword = WaitAndFindElement(By.XPath("//article//*[@id='password']"));
            var loginButton = WaitAndFindElement(By.XPath("//article//*[@class='btn_secondary']"));
            var expectedEmailError = "Invalid login attempt. That account doesn't seem to be available.";

            loginEmail.SendKeys("wrongUser_asdf");
            loginPassword.SendKeys("wrongPassword_asdf");
            loginButton.Click();

            var actualEmailError = WaitAndFindElement(By.XPath("//article//*[contains(text(),'Invalid login attempt')]")).Text;

            Assert.AreEqual(expectedEmailError,actualEmailError);
        }

        //Test2
        [Test]
        public void TryLoggedIn_With_WithoutEnteringEmail_And_Password()
        {
            var loginButton = WaitAndFindElement(By.XPath("//article//*[@class='btn_secondary']"));
            var expectedEmailError = "Email is required.";
            var expectedPasswordError = "Password is required.";

            loginButton.Click();

            var actualEmailError = WaitAndFindElement(By.XPath("//article//span[contains(text(),'Email')]")).Text;
            var actualPasswordError = WaitAndFindElement(By.XPath("//article//span[contains(text(),'Password')]")).Text;

            Assert.AreEqual(expectedEmailError, actualEmailError);
            Assert.AreEqual(expectedPasswordError, actualPasswordError);
        }

        [TearDown]
        public void CloseBrowser()
        {
            _driver.Quit();
        }

        //Explicit wait and find element
        private IWebElement WaitAndFindElement(By by)

        {
            var webDriverWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            return webDriverWait.Until(ExpectedConditions.ElementExists(by));
        }

        private dynamic GetBrowserOptions(string browserName)
        {
            if (browserName.Equals("Chrome"))
            {
                return new ChromeOptions();
            }
            if (browserName.Equals("Safari"))
            {
                return new SafariOptions();
            }
            if (browserName.Equals("Firefox"))
            {
                return new FirefoxOptions();
            }

            return new ChromeOptions();
        }
    }
}