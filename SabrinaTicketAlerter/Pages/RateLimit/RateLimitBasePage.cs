using OpenQA.Selenium;
using SabrinaTicketAlerter.Locators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Pages.RateLimit
{
    public interface IRateLimitPage : IPage
    {
        public void SignalSuccess();
    }

    public abstract class RateLimitBasePage<TLocators, TData>(IWebDriver driver) : BasePage<TLocators, TData>(driver), IRateLimitPage
        where TLocators : BaseLocators, new()
    {
        public abstract void SignalSuccess();
    }
}
