using System.Configuration;

namespace BasicBankProject.Modal
{
    public class BaseTest
    {
        public string appUrl => ConfigurationManager.AppSettings.Get("appUrl");
        public string createEndpoint = "/account/create";
        public string deleteEndpoint = "/account/delete";
        public string depositEndpoint = "/account/deposit";
        public string withdrawEndpoint = "/account/withdraw";
        public string accountDetailsEndPoint = "/account/getDetails";
    }
}
