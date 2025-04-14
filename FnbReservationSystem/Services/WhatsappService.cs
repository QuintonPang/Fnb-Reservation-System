using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
namespace FnbReservationSystem.Services;

public class WhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;

   public WhatsAppService(HttpClient httpClient, IOptions<WhatsAppSettings> options)
    {
        _httpClient = httpClient;
        _accessToken = options.Value.AccessToken;
    }
    public class TemplateParameter
    {
        public string type { get; set; } = "text";
        public string text { get; set; }
        public string parameter_name { get; set; }
    }

    public async Task SendTemplateMessageAsync(
        string toPhoneNumber,
        string templateName,
        string languageCode,
        List<TemplateParameter> parameters)
    {
        var payload = new
        {
            messaging_product = "whatsapp",
            to = toPhoneNumber,
            type = "template",
            template = new
            {
                name = templateName,
                language = new { code = languageCode },
                components = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = parameters
                    }
                }
            }
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://graph.facebook.com/v22.0/576259568911890/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status Code: {response.StatusCode}");
        Console.WriteLine($"Response: {responseContent}");
    }
}
