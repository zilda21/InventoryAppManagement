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

        // -------------------- OAuth token (username-password flow) --------------------
        private async Task<string> GetAccessTokenAsync()
        {
            // For Developer / Production orgs
            var tokenEndpoint = "https://login.salesforce.com/services/oauth2/token";
            // For sandbox it would be:
            // var tokenEndpoint = "https://test.salesforce.com/services/oauth2/token";

            // Build password + security token here
            var passwordPlusToken = _options.Password + _options.SecurityToken;

            var body = new Dictionary<string, string>
            {
                ["grant_type"]    = "password",
                ["client_id"]     = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["username"]      = _options.Username,
                ["password"]      = passwordPlusToken
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
            var token = doc.RootElement.GetProperty("access_token").GetString();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Salesforce auth response missing access_token.");
            }

            return token;
        }

        // -------------------- Create Account + Contact --------------------
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

            // 1) Create Account
            var accountPayload = new
            {
                Name = accountName   // required for Account
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

            // 2) Create Contact
            var contactPayload = new
            {
                FirstName = firstName,
                LastName  = lastName,  // required for Contact
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
