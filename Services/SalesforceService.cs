using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using InventoryApp.Config;
using Microsoft.Extensions.Options;

namespace InventoryApp.Services;

public interface ISalesforceService
{
    Task CreateAccountAndContactAsync(string accountName, string firstName, string lastName, string email);
}

public class SalesforceService : ISalesforceService
{
    private readonly HttpClient _httpClient;
    private readonly SalesforceOptions _options;
    private string? _accessToken;

    public SalesforceService(HttpClient httpClient, IOptions<SalesforceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    private async Task EnsureAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken))
            return;

        var body = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string,string>("grant_type", "password"),
            new KeyValuePair<string,string>("client_id", _options.ClientId),
            new KeyValuePair<string,string>("client_secret", _options.ClientSecret),
            new KeyValuePair<string,string>("username", _options.Username),
            new KeyValuePair<string,string>("password", _options.Password)
        });

        var tokenUrl = $"{_options.InstanceUrl}/services/oauth2/token";
        var resp = await _httpClient.PostAsync(tokenUrl, body);
        resp.EnsureSuccessStatusCode();

        using var stream = await resp.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);
        _accessToken = doc.RootElement.GetProperty("access_token").GetString();
    }

    public async Task CreateAccountAndContactAsync(string accountName, string firstName, string lastName, string email)
    {
        await EnsureAccessTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);

        // 1) Create Account
        var accountPayload = new { Name = accountName };
        var accountContent = new StringContent(
            JsonSerializer.Serialize(accountPayload),
            Encoding.UTF8,
            "application/json");

        var accountUrl = $"{_options.InstanceUrl}/services/data/v61.0/sobjects/Account";
        var accResp = await _httpClient.PostAsync(accountUrl, accountContent);
        accResp.EnsureSuccessStatusCode();

        using var accStream = await accResp.Content.ReadAsStreamAsync();
        var accDoc = await JsonDocument.ParseAsync(accStream);
        var accountId = accDoc.RootElement.GetProperty("id").GetString();

        // 2) Create Contact linked to that Account
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

        var contactUrl = $"{_options.InstanceUrl}/services/data/v61.0/sobjects/Contact";
        var contactResp = await _httpClient.PostAsync(contactUrl, contactContent);
        contactResp.EnsureSuccessStatusCode();
    }
}
