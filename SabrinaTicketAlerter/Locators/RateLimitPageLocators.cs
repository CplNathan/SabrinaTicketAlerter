using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Locators
{
    public class RateLimitPageLocators : BaseLocators
    {
        public override By PageLocator => By.XPath("//div[text()[contains(., \"Your Browsing Activity Has Been Paused\")]]");
    }
}
