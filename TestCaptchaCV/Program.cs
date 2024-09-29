using SabrinaTicketAlerter.Helpers;
using SabrinaTicketAlerter.Pages.RateLimit;
using SeleniumUndetectedChromeDriver;

var driver = UndetectedChromeDriver.Create(userDataDir: UserDataHelper.UserDataPath, driverExecutablePath: await new ChromeDriverInstaller().Auto());

driver.Navigate().GoToUrl("http://localhost:8000");

await Task.Delay(1000);

var captcha = new RateLimitCaptchaPage(driver);
await captcha.TestAction();