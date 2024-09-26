﻿using OpenQA.Selenium;
using SabrinaTicketAlerter.Locators;
using SabrinaTicketAlerter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Pages
{
    public class ConcertPage(IWebDriver driver, ConcertData concertData) : BasePage<ConcertPageLocators, List<TicketData>>(driver)
    {
        public IReadOnlyCollection<IWebElement> TicketList => driver.FindElements(Locators.Tickets);

        public ConcertData ConcertData { get; } = concertData;

        protected override string PagePath => ConcertData.Path;

        protected override Task ActionAsyncImplementation()
        {
            // throw new NotImplementedException();

            // Any other important page navigational actions?
            return Task.CompletedTask;
        }

        protected override ValueTask<List<TicketData>> GetDataAsyncImplementation()
        {
            var ticketData = TicketList.Select(x =>
            {
                try
                {
                    var sectionAndRow = x.FindElements(Locators.TicketSectionAndRow);

                    var ticketData = new TicketData
                    {
                        Section = sectionAndRow.First().GetAttribute("innerHTML"),
                        Row = sectionAndRow.Last().GetAttribute("innerHTML"),
                        Price = x.FindElement(Locators.TicketPrice).GetAttribute("innerHTML")
                    };

                    return ticketData;
                }
                catch (NoSuchElementException)
                {
                    return default;
                }
            }).OfType<TicketData>();

            return new ValueTask<List<TicketData>>(Task.FromResult(ticketData.ToList()));
        }
    }
}