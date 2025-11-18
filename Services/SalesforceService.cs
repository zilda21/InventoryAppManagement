using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using InventoryApp.config;
using Microsoft.Extensions.Options;

namespace InventoryApp.Services
{
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

        public SalesforceService(HttpClient httpClient, IOptions<SalesforceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        // Get OAuth access token using password flow.
        private async Task<string> GetAccessTokenAsync()
        {
            var tokenEndpoint = "https://login.salesforce.com/services/oauth2/token";

            var body = new Dictionary<string, string>
            {
                ["grant_type"]    = "password",
                ["client_id"]     = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["username"]      = _options.Username,
                // IMPORTANT: this must be   password + securityToken
                ["password"]      = _options.Password
            };

            var response = await _httpClient.PostAsync(
                tokenEndpoint,
                new FormUrlEncodedContent(body));

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException(
                    $"Salesforce auth error {(int)response.StatusCode} {response.StatusCode}: {content}");
            }

            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("access_token").GetString()
                   ?? throw new InvalidOperationException("Salesforce auth response missing access_token");
        }

        public async Task CreateAccountAndContactAsync(
            string accountName,
            string firstName,
            string lastName,
            string email)
        {
            var accessToken = await GetAccessTokenAsync();
            var baseUrl = _options.InstanceUrl.TrimEnd('/');

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            // ---------------- 1) Create Account ----------------
            var accountPayload = new
            {
                Name = accountName   // required field for Account
            };

            var accountResponse = await _httpClient.PostAsJsonAsync(
                $"{baseUrl}/services/data/v61.0/sobjects/Account",
                accountPayload);

            var accountContent = await accountResponse.Content.ReadAsStringAsync();

            if (!accountResponse.IsSuccessStatusCode)
            {
                throw new ApplicationException(
                    $"Salesforce account error {(int)accountResponse.StatusCode} {accountResponse.StatusCode}: {accountContent}");
            }

            using var accountDoc = JsonDocument.Parse(accountContent);
            var accountId = accountDoc.RootElement.GetProperty("id").GetString();

            if (string.IsNullOrWhiteSpace(accountId))
            {
                throw new ApplicationException("Salesforce account create succeeded but returned no Id.");
            }

            // ---------------- 2) Create Contact ----------------
            var contactPayload = new
            {
                FirstName = firstName,
                LastName  = lastName,  // LastName is required for Contact
                Email     = email,
                AccountId = accountId
            };

            var contactResponse = await _httpClient.PostAsJsonAsync(
                $"{baseUrl}/services/data/v61.0/sobjects/Contact",
                contactPayload);

            var contactContent = await contactResponse.Content.ReadAsStringAsync();

            if (!contactResponse.IsSuccessStatusCode)
            {
                throw new ApplicationException(
                    $"Salesforce contact error {(int)contactResponse.StatusCode} {contactResponse.StatusCode}: {contactContent}");
            }
        }
    }
}
