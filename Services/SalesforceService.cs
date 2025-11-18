using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using InventoryApp.config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InventoryApp.Services
{
    public class SalesforceAuthResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string instance_url { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
    }

    public interface ISalesforceService
    {
        Task CreateAccountAndContactAsync(
            string accountName,
            string firstName,
            string lastName,
            string email);
    }

    public class SalesforceService : ISalesforceService
    {
        private readonly HttpClient _httpClient;
        private readonly SalesforceOptions _options;
        private readonly ILogger<SalesforceService> _logger;

        private string? _accessToken;

        public SalesforceService(
            HttpClient httpClient,
            IOptions<SalesforceOptions> options,
            ILogger<SalesforceService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            if (!string.IsNullOrEmpty(_options.InstanceUrl))
            {
                _httpClient.BaseAddress = new Uri(_options.InstanceUrl);
            }
        }

        private async Task EnsureAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                return;
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type", "password"),
                new KeyValuePair<string,string>("client_id", _options.ClientId),
                new KeyValuePair<string,string>("client_secret", _options.ClientSecret),
                new KeyValuePair<string,string>("username", _options.Username),
                new KeyValuePair<string,string>("password", _options.Password) // password + token
            });

            var response = await _httpClient.PostAsync("/services/oauth2/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var auth = JsonSerializer.Deserialize<SalesforceAuthResponse>(json);

            if (auth == null || string.IsNullOrWhiteSpace(auth.access_token))
            {
                throw new Exception("Could not obtain Salesforce access token.");
            }

            _accessToken = auth.access_token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task CreateAccountAndContactAsync(
            string accountName,
            string firstName,
            string lastName,
            string email)
        {
            await EnsureAccessTokenAsync();

            // ---- Create Account ----
            var accountPayload = new { Name = accountName };
            var accountContent = new StringContent(
                JsonSerializer.Serialize(accountPayload),
                Encoding.UTF8,
                "application/json");

            var accountResponse = await _httpClient.PostAsync(
                "/services/data/v58.0/sobjects/Account",
                accountContent);

            accountResponse.EnsureSuccessStatusCode();

            var accountJson = await accountResponse.Content.ReadAsStringAsync();
            using var accountDoc = JsonDocument.Parse(accountJson);
            var accountId = accountDoc.RootElement.GetProperty("id").GetString();

            // ---- Create Contact ----
            var contactPayload = new
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                AccountId = accountId
            };

            var contactContent = new StringContent(
                JsonSerializer.Serialize(contactPayload),
                Encoding.UTF8,
                "application/json");

            var contactResponse = await _httpClient.PostAsync(
                "/services/data/v58.0/sobjects/Contact",
                contactContent);

            contactResponse.EnsureSuccessStatusCode();

            var body = await contactResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Salesforce contact created successfully: {Body}", body);
        }
    }
}
