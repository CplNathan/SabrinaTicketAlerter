using OpenQA.Selenium;
using SabrinaTicketAlerter.Locators;
using SabrinaTicketAlerter.Models;

namespace SabrinaTicketAlerter.Pages
{
    public sealed class ArtistPage(IWebDriver driver) : BasePage<ArtistPageLocators, List<ConcertPage>>(driver)
    {
        public IReadOnlyCollection<IWebElement> ConcertList
        {
            get
            {
                try
                {
                    return driver.FindElements(Locators.Concerts);
                }
                catch (NoSuchElementException)
                {
                    return [];
                }
            }
        }

        protected override string PagePath => "/sabrina-carpenter-tickets/artist/2001092";

        protected override Task ActionAsyncImplementation(CancellationToken token)
        {
            // throw new NotImplementedException();

            // Any other important page navigational actions?
            return Task.CompletedTask;
        }

        protected override ValueTask<List<ConcertPage>> GetDataAsyncImplementation()
        {
            var newPages = ConcertList.Select(x =>
            {
                try
                {
                    var monthAndDay = x.FindElements(Locators.ConcertMonthAndDay);

                    var concertData = new ConcertData
                    {
                        Location = x.FindElement(Locators.ConcertLocation).GetAttribute("innerHTML"),
                        Month = monthAndDay.First().GetAttribute("innerHTML"),
                        Day = monthAndDay.Last().GetAttribute("innerHTML"),
                        Path = new Uri(x.FindElement(Locators.ConcertListLink).GetAttribute("href")).PathAndQuery
                    };

                    return new ConcertPage(driver, concertData);
                }
                catch (NoSuchElementException)
                {
                    return default;
                }
            }).OfType<ConcertPage>();

            return new ValueTask<List<ConcertPage>>(Task.FromResult(newPages.ToList()));
        }
    }
}
