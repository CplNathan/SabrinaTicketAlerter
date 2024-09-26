using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Locators
{
    public sealed class ArtistPageLocators : BaseLocators
    {
        public override By PageLocator => By.XPath("//a[@href=\"#concerts\"]");

        public By Concerts => By.XPath("//ul[@data-testid=\"eventList\"]/li");

        public By ConcertLocation => By.XPath(".//div[not(@*)]/span[@*][1]/span[1]");

        public By ConcertMonthAndDay => By.XPath(".//div[not(@*)]/preceding-sibling::div/span[not(@*)]");

        public By ConcertListLink => By.XPath(".//a[@data-testid=\"event-list-link\"]");
    }
}
