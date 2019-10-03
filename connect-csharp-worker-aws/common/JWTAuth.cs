using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using System;
using System.Linq;
using System.Text;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace Worker
{
    public class JWTAuth
    {        
        private const int TOKEN_REPLACEMENT_IN_SECONDS = 10 * 60;

        private static string AccessToken { get; set; }
        private static int expiresIn;
        private static string path;
        private static Account Account { get; set; }
        protected static ApiClient ApiClient { get; private set; }

        protected static string AccountID
        {
            get { return Account.AccountId; }
        }

        public JWTAuth(ApiClient client)
        {
            ApiClient = client;
        }

        public void CheckToken()
        {
            if (AccessToken == null
                || (DateTime.Now.Millisecond + TOKEN_REPLACEMENT_IN_SECONDS) > expiresIn)
            {
                Console.WriteLine("Obtaining a new access token...");
                UpdateToken();
            }
        }

        private void UpdateToken()
        {
            OAuth.OAuthToken authToken = ApiClient.RequestJWTUserToken(DSConfig.ClientID,
                            DSConfig.ImpersonatedUserGuid,
                            DSConfig.AuthServer,
                            Encoding.UTF8.GetBytes(DSConfig.PrivateKey),
                            1);

            AccessToken = authToken.access_token;

            if (Account == null)
                Account = GetAccountInfo(authToken);

            ApiClient = new ApiClient(Account.BaseUri + "/restapi");
            path = Account.BaseUri + "/restapi";
            expiresIn = DateTime.Now.Second + authToken.expires_in.Value;

        }

        public string getToken()
        {
            return AccessToken;
        }
        public string getBasePath()
        {
            return path;
        }
        public string getAccountID()
        {
            return Account.AccountId;
        }

        private Account GetAccountInfo(OAuth.OAuthToken authToken)
        {
            ApiClient.SetOAuthBasePath(DSConfig.AuthServer);
            OAuth.UserInfo userInfo = ApiClient.GetUserInfo(authToken.access_token);
            Account acct = null;

            var accounts = userInfo.Accounts;

            if (!string.IsNullOrEmpty(DSConfig.TargetAccountID) && !DSConfig.TargetAccountID.Equals("FALSE"))
            {
                acct = accounts.FirstOrDefault(a => a.AccountId == DSConfig.TargetAccountID);

                if (acct == null)
                {
                    throw new Exception("The user does not have access to account " + DSConfig.TargetAccountID);
                }
            }
            else
            {
                acct = accounts.FirstOrDefault(a => a.IsDefault == "true");
            }

            return acct;
        }
    }
}
