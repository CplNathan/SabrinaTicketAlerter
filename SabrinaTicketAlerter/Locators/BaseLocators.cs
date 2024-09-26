using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Locators
{
    public abstract class BaseLocators
    {
        public abstract By PageLocator { get; }
    }
}
