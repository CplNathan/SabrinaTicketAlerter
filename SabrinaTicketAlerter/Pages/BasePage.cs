using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SabrinaTicketAlerter.Locators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Pages
{
    public interface IPage
    {
        public bool IsCurrentPage { get; }

        public Task<bool> Navigate();

        public Task<bool> PerformActionAsync();

        public ValueTask<object?> GetDataAsync();
    }

    public abstract class BasePage<TLocators, TData>(IWebDriver driver) : IPage
        where TLocators : BaseLocators, new()
    {
        public TLocators Locators { get; } = new();

        public virtual bool IsCurrentPage => new Uri(driver.Url).ToString().Equals(PageUrl.ToString(), StringComparison.InvariantCultureIgnoreCase);

        private string BaseUrl => "https://www.ticketmaster.co.uk/";

        private Uri PageUrl => new Uri(new Uri(BaseUrl), PagePath);

        protected abstract string PagePath { get; }

        protected WebDriverWait Waiter { get; } = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        public async Task<bool> Navigate()
        {
            return await Task.Run(() =>
            {
                driver.Navigate().GoToUrl(PageUrl);
                return Waiter.Until(x => driver.FindElement(Locators.PageLocator)) != null;
            });
        }

        public async Task<bool> PerformActionAsync()
        {
            if (!IsCurrentPage)
            {
                var navigateSuccess = await Navigate();

                if (!navigateSuccess)
                {
                    return false;
                }
            }

            try
            {
                await ActionAsyncImplementation();
            }
            catch (NoSuchElementException)
            {
                return false;
            }

            return true;
        }

        protected abstract Task ActionAsyncImplementation();

        public async ValueTask<object?> GetDataAsync()
            => await GetDataAsyncImplementation();

        protected abstract ValueTask<TData> GetDataAsyncImplementation();
    }
}
