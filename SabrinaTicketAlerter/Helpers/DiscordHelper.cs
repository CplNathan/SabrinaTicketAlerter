using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SabrinaTicketAlerter.Helpers
{
    public class DiscordHelper
    {
        private readonly IFlurlClient flurlClient;

        private readonly string webhookUrl;

        public DiscordHelper(IFlurlClient flurlClient, string webhookUrl)
        {
            ArgumentNullException.ThrowIfNull(flurlClient);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(webhookUrl);

            this.flurlClient = flurlClient;
            this.webhookUrl = webhookUrl;
        }
        public async Task SendMessage(string message)
        {
            var values = new JsonObject
            {
                { "content", message }
            };

            await flurlClient.Request(webhookUrl).PostJsonAsync(values);
        }
    }
}
