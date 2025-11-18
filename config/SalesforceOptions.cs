namespace InventoryApp.config
{
    public class SalesforceOptions
    {
        public const string SectionName = "Salesforce";

        public string ClientId       { get; set; } = string.Empty;
        public string ClientSecret   { get; set; } = string.Empty;
        public string InstanceUrl    { get; set; } = string.Empty;

        // Salesforce username (the login you see in Salesforce)
        public string Username       { get; set; } = string.Empty;

        // *** ONLY your normal Salesforce login password ***
        public string Password       { get; set; } = string.Empty;

        // *** ONLY the security token that Salesforce emailed you ***
        public string SecurityToken  { get; set; } = string.Empty;
    }
}
