using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cuttr.Business.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cuttr.Infrastructure.Services
{
    public class ExpoPushNotificationService : IExpoPushNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExpoPushNotificationService> _logger;
        private readonly string _expoPushEndpoint;

        public ExpoPushNotificationService(HttpClient httpClient, IConfiguration configuration, ILogger<ExpoPushNotificationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            // Optionally you can configure this via appsettings.json; otherwise use the default Expo endpoint.
            _expoPushEndpoint = configuration["ExpoPushNotifications:Endpoint"] ?? "https://exp.host/--/api/v2/push/send";
        }

        public async Task SendPushNotificationAsync(string expoPushToken, string title, string body, object data = null)
        {
            if (string.IsNullOrEmpty(expoPushToken))
            {
                _logger.LogWarning("Expo push token is null or empty. Notification not sent.");
                return;
            }

            var payload = new
            {
                to = expoPushToken,
                sound = "default",
                title = title,
                body = body,
                data = data
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_expoPushEndpoint, httpContent);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to send push notification. Status Code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending push notification.");
            }
        }
    }
}
