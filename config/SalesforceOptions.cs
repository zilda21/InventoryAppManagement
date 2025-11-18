using System;

namespace InventoryApp.config
{
    public class SalesforceOptions
    {
        public const string SectionName = "Salesforce";

        public string InstanceUrl { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        // Username of your Salesforce user (the same that you used for the token)
        public string Username { get; set; } = string.Empty;

        // Password **plus** security token concatenated
        public string Password { get; set; } = string.Empty;
    }
}
