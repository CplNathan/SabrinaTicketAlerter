using OpenQA.Selenium;
using SabrinaTicketAlerter.Locators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Pages
{
    public class RateLimitPage(IWebDriver driver) : BasePage<RateLimitPageLocators, object>(driver)
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

        protected TimeSpan CurrentCooldown { get; set; } = TimeSpan.Zero;

        protected override async Task ActionAsyncImplementation()
        {
            await Task.Delay(CurrentCooldown);

            CurrentCooldown = CurrentCooldown.Add(TimeSpan.FromSeconds(10));
        }

        protected override ValueTask<object> GetDataAsyncImplementation()
        {
            throw new NotImplementedException();
        }
    }
}
