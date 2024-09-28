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

        public Task<bool> Navigate(CancellationToken token);

        public Task<bool> PerformActionAsync(CancellationToken token);

        public ValueTask<object?> GetDataAsync();
    }

    public abstract class BasePage<TLocators, TData>(IWebDriver driver) : IPage
        where TLocators : BaseLocators, new()
    {
        public TLocators Locators { get; } = new();

        public virtual bool IsCurrentPage => new Uri(driver.Url).ToString().Equals(PageUrl.ToString(), StringComparison.InvariantCultureIgnoreCase);

        private string BaseUrl => "https://www.ticketmaster.co.uk/";

        protected Uri PageUrl => new Uri(new Uri(BaseUrl), PagePath);

        protected abstract string PagePath { get; }

        protected WebDriverWait Waiter { get; } = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        public async Task<bool> Navigate(CancellationToken token)
        {
            return await Task.Run(() =>
            {
                try
                {
                    driver.Navigate().GoToUrl(PageUrl);
                    return Waiter.Until(x =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            throw new TaskCanceledException();
                        }

                        return x.FindElement(Locators.PageLocator);
                    }) != null;
                }
                catch (WebDriverTimeoutException)
                {
                    return false;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
                catch (TaskCanceledException)
                {
                    return false;
                }
            });
        }

        public async Task<bool> PerformActionAsync(CancellationToken token)
        {
            if (!IsCurrentPage && !string.IsNullOrWhiteSpace(PagePath))
            {
                var navigateSuccess = await Navigate(token);

                if (!navigateSuccess)
                {
                    return false;
                }
            }

            try
            {
                await ActionAsyncImplementation(token);
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }

            return true;
        }

        protected abstract Task ActionAsyncImplementation(CancellationToken token);

        public async ValueTask<object?> GetDataAsync()
            => await GetDataAsyncImplementation();

        protected abstract ValueTask<TData> GetDataAsyncImplementation();
    }
}
