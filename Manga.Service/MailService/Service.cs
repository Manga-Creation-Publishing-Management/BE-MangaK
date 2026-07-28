using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Manga.Service.MailService;

public class Service : IService
{
    public MailOptions _mailOptions = new();

    public Service(IConfiguration configuration)
    {
        configuration.GetSection("MailOptions").Bind(_mailOptions);
    }

    public async Task SendMail(MailContent mailContent)
    {
        if (string.IsNullOrEmpty(_mailOptions.ApiKey))
        {
            throw new Exception("Brevo API Key is missing. Please configure it in appsettings.json or Environment Variables.");
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("api-key", _mailOptions.ApiKey);

        var payload = new
        {
            sender = new { name = _mailOptions.DisplayName, email = _mailOptions.Mail },
            to = new[] { new { email = mailContent.To } },
            subject = mailContent.Subject,
            htmlContent = mailContent.Body
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);

        if (!response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to send email via Brevo. Status Code: {response.StatusCode}. Response: {responseString}");
        }
    }
}