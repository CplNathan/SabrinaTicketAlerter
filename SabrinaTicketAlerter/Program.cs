using Flurl.Http;
using SabrinaTicketAlerter.Helpers;
using SabrinaTicketAlerter.Models;
using SabrinaTicketAlerter.Pages;
using SeleniumUndetectedChromeDriver;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Canceling...");
    cts.Cancel();
    e.Cancel = true;
};

var driver = UndetectedChromeDriver.Create(userDataDir: UserDataHelper.UserDataPath, driverExecutablePath: await new ChromeDriverInstaller().Auto(), commandTimeout: TimeSpan.FromSeconds(30));
driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

var webhookUrl = "https://discordapp.com/api/webhooks/1288911791975563375/bfX0ukcOE1e-UfAz32SjukcTWjhq3lAUBlfyxxvKjEJe1GE-wNJbjgl-rhom-PHGrET6";
var discordNotification = new DiscordHelper(new FlurlClient(), webhookUrl);

var startPage = new ArtistPage(driver);
var rateLimitPage = new RateLimitPage(driver);
List<IPage> registeredPages = [startPage, rateLimitPage];

var sentNotificationsKey = new Dictionary<string, DateTime>();

while (!cts.Token.IsCancellationRequested)
{
    var concertPages = registeredPages.OfType<ConcertPage>();
    if (concertPages.Any())
    {
        foreach (var concertPage in concertPages.Where(x => x.ConcertData.Location?.Equals("London", StringComparison.InvariantCultureIgnoreCase) ?? false))
        {
            var concertActionSuccess = await concertPage.PerformActionAsync();
            if (!concertActionSuccess || !concertPage.IsCurrentPage)
            {
                break;
            }

            var tickets = (await concertPage.GetDataAsync()) as IEnumerable<TicketData> ?? [];
            foreach (var ticket in tickets)
            {
                var message = "@everyone - {concertPage.ConcertData.Location} @ {concertPage.ConcertData.Month} {concertPage.ConcertData.Day} - ({ticket.Row}-{ticket.Section}) - {ticket.Price}";
                var existingValue = sentNotificationsKey.TryGetValue(message, out var dateTime);
                if (!existingValue || (DateTime.UtcNow - dateTime) > TimeSpan.FromMinutes(15))
                {
                    sentNotificationsKey[message] = DateTime.UtcNow;

                    await discordNotification.SendMessage(message);
                    // await discordNotification.SendMessage("");
                    // Send Notification - also debounce so no spam
                }
            }
        }

        var currentPage = registeredPages.FirstOrDefault(x => x.IsCurrentPage);
        switch (currentPage?.GetType())
        {
            case Type concertType when concertType == typeof(ArtistPage):
            case Type ticketType when ticketType == typeof(ConcertPage):
                await Task.Delay(TimeSpan.FromSeconds(10));
                break;
            case Type rateLimitType when rateLimitType == typeof(RateLimitPage):
                await rateLimitPage.PerformActionAsync();
                break;
            default:
                await startPage.PerformActionAsync();
                break;
        }
    }
    else
    {
        await startPage.PerformActionAsync();
        registeredPages.AddRange(await startPage.GetDataAsync() as IEnumerable<IPage> ?? []);
    }
}

driver.Quit();