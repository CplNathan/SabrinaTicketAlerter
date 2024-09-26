using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Locators
{
    public class ConcertPageLocators : BaseLocators
    {
        public override By PageLocator => By.XPath("//span[@data-testid=\"edp-header-date\"]");

        public By Tickets => By.XPath("//div[@data-testid=\"quickpicksList\"]/div[@role=\"button\"]");

        public By TicketSectionAndRow => By.XPath(".//dl//dd");

        public By TicketPrice => By.XPath(".//dl/following::div[1]//div[not(@*)]/span[last()]");

            // //div[@data-testid="quickpicksList"]/div//dl/following::div
    }
}
