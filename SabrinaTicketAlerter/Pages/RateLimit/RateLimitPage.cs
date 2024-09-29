using OpenQA.Selenium;
using SabrinaTicketAlerter.Locators.RateLimit;

namespace SabrinaTicketAlerter.Pages.RateLimit
{
    public class RateLimitPage(IWebDriver driver) : RateLimitBasePage<RateLimitPageLocators, object>(driver)
    {
        public override bool IsCurrentPage
        {
            get
            {
                try
                {
                    return driver.FindElement(Locators.PageLocator) != null;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            }
        }

        protected override string PagePath => string.Empty;

        protected TimeSpan CurrentCooldown { get; set; } = cooldownInterval;

        private static readonly TimeSpan cooldownInterval = TimeSpan.FromSeconds(30);

        public override void SignalSuccess()
        {
            CurrentCooldown = TimeSpan.FromSeconds(Math.Max(cooldownInterval.TotalSeconds, CurrentCooldown.Subtract(cooldownInterval).TotalSeconds));
        }

        protected override async Task ActionAsyncImplementation(CancellationToken token)
        {
            await Task.Delay(CurrentCooldown);

            var nextDelay = Random.Shared.Next((int)cooldownInterval.TotalSeconds, (int)cooldownInterval.TotalSeconds * 2);
            CurrentCooldown = CurrentCooldown.Add(TimeSpan.FromSeconds(nextDelay));

            driver.Navigate().Refresh();
        }

        protected override ValueTask<object> GetDataAsyncImplementation()
        {
            throw new NotImplementedException();
        }
    }
}
