using Flurl.Http;
using Microsoft.VisualBasic;
using OpenQA.Selenium;
using SabrinaTicketAlerter.Helpers;
using SabrinaTicketAlerter.Models;
using SabrinaTicketAlerter.Pages;
using SabrinaTicketAlerter.Pages.RateLimit;
using SeleniumUndetectedChromeDriver;

namespace SabrinaTicketAlerter
{
    public static class App
    {
        public static async Task MainLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var driver = UndetectedChromeDriver.Create(userDataDir: UserDataHelper.UserDataPath, driverExecutablePath: await new ChromeDriverInstaller().Auto());

                var webhookUrl = "https://discordapp.com/api/webhooks/1288911791975563375/bfX0ukcOE1e-UfAz32SjukcTWjhq3lAUBlfyxxvKjEJe1GE-wNJbjgl-rhom-PHGrET6";
                var discordNotification = new DiscordHelper(new FlurlClient(), webhookUrl);

                List<IPage> registeredPages = [];

                var sentNotificationsKey = new Dictionary<string, DateTime>();

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var concertPages = registeredPages.OfType<ConcertPage>();
                        if (concertPages.Any())
                        {
                            string[] ignoredLocations = ["Dublin", "Glasgow"];

                            foreach (var concertPage in concertPages.Where(x => !ignoredLocations.Any(y => x.ConcertData.Location?.Contains(y, StringComparison.InvariantCultureIgnoreCase) ?? false)))
                            {
                                var concertActionSuccess = await concertPage.PerformActionAsync(token);
                                if (!concertActionSuccess || !concertPage.IsCurrentPage)
                                {
                                    break;
                                }

                                await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(15, 20)), token);

                                var tickets = (await concertPage.GetDataAsync()) as IEnumerable<TicketData> ?? [];
                                foreach (var ticket in tickets)
                                {
                                    var message = $"@everyone - {concertPage.ConcertData.Location} @ {concertPage.ConcertData.Month} {concertPage.ConcertData.Day} - ({ticket.Row}-{ticket.Section}) - {ticket.Price} - {ticket.Link}";
                                    var existingValue = sentNotificationsKey.TryGetValue(message, out var dateTime);
                                    if (!existingValue || (DateTime.UtcNow - dateTime) > TimeSpan.FromMinutes(15))
                                    {
                                        sentNotificationsKey[message] = DateTime.UtcNow;

                                        await discordNotification.SendMessage(message);
                                    }
                                }
                            }
                        }

                        var rateLimitPages = registeredPages.OfType<IRateLimitPage>();
                        var otherPages = registeredPages.Except(rateLimitPages);
                        var isTimeoutPage = rateLimitPages.Any(x => x.IsCurrentPage);
                        var currentPage = isTimeoutPage ? rateLimitPages.FirstOrDefault(x => x.IsCurrentPage) : otherPages.OfType<ConcertPage>().Any() ? otherPages.FirstOrDefault(x => x.IsCurrentPage) : null;
                        switch (currentPage?.GetType())
                        {
                            case Type concertType when typeof(ArtistPage).IsAssignableFrom(concertType):
                            case Type ticketType when typeof(ConcertPage).IsAssignableFrom(ticketType):
                                foreach (var rateLimitPage in rateLimitPages)
                                {
                                    rateLimitPage.SignalSuccess();
                                }
                                break;
                            case Type rateLimitType when typeof(IRateLimitPage).IsAssignableFrom(rateLimitType):
                                var captchaSolved = await currentPage.PerformActionAsync(token);
                                if (!captchaSolved)
                                {
                                    throw new WebDriverException("Captcha was not solveable");
                                }
                                break;
                            default:
                                registeredPages = [new ArtistPage(driver), new RateLimitPage(driver), new RateLimitCaptchaPage(driver)];
                                await registeredPages[0].PerformActionAsync(token);

                                if (!concertPages.Any())
                                {
                                    registeredPages.AddRange(await registeredPages[0].GetDataAsync() as IEnumerable<IPage> ?? []);
                                }
                                break;
                        }
                    }
                    catch (WebDriverException)
                    {
                        break;
                    }
                }

                driver.Quit();
            }
        }
    }
}
