using System;
using System.Configuration;

namespace FortnoxConnect.Models
{
    /// <summary>
    /// Configuration class for Fortnox OAuth2 settings
    /// </summary>
    public class FortnoxConfig
    {
        public static string ClientId => ConfigurationManager.AppSettings["Fortnox:ClientId"];
        public static string ClientSecret => ConfigurationManager.AppSettings["Fortnox:ClientSecret"];
        public static string RedirectUri => ConfigurationManager.AppSettings["Fortnox:RedirectUri"];
        public static string AuthEndpoint => ConfigurationManager.AppSettings["Fortnox:AuthEndpoint"];
        public static string TokenEndpoint => ConfigurationManager.AppSettings["Fortnox:TokenEndpoint"];
        public static string ApiBaseUrl => ConfigurationManager.AppSettings["Fortnox:ApiBaseUrl"];
        public static string Scopes => ConfigurationManager.AppSettings["Fortnox:Scopes"];
    }
}
