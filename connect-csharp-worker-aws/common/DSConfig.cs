using System;
using System.Configuration;
using Amazon;

namespace Worker
{
    public class DSConfig
    {
        private const string CLIENT_ID = "DS_CLIENT_ID";
        private const string IMPERSONATED_USER_GUID = "DS_IMPERSONATED_USER_GUID";
        private const string TARGET_ACCOUNT_ID = "DS_TARGET_ACCOUNT_ID";
        private const string SIGNER_1_EMAIL = "DS_SIGNER_1_EMAIL";
        private const string SIGNER_1_NAME = "DS_SIGNER_1_NAME";
        private const string CC_1_EMAIL = "DS_CC_1_EMAIL";
        private const string CC_1_NAME = "DS_CC_1_NAME";
        private const string PRIVATE_KEY = "DS_PRIVATE_KEY";
        private const string DS_AUTH_SERVER = "DS_AUTH_SERVER";
        private const string QUEUE_URL = "QUEUE_URL";
        private const string AWS_ACCOUNT = "AWS_ACCOUNT";
        private const string AWS_SECRET = "AWS_SECRET";
        private const string BASIC_AUTH_NAME = "BASIC_AUTH_NAME";
        private const string BASIC_AUTH_PW = "BASIC_AUTH_PW";
        private const string DEBUG = "DEBUG";
        private const string ENVELOPE_CUSTOM_FIELD = "ENVELOPE_CUSTOM_FIELD";
        private const string OUTPUT_FILE_PREFIX = "OUTPUT_FILE_PREFIX";
        private const string ENABLE_BREAK_TEST = "ENABLE_BREAK_TEST";
        private const string TEST_OUTPUT_DIR_NAME = "TEST_OUTPUT_DIR_NAME";
        private const string ENQUEUE_URL = "ENQUEUE_URL";

        static DSConfig()
        {
            ClientID = GetSetting(CLIENT_ID);
            ImpersonatedUserGuid = GetSetting(IMPERSONATED_USER_GUID);
            TargetAccountID = GetSetting(TARGET_ACCOUNT_ID);
            OAuthRedirectURI = "https://www.docusign.com";
            Signer1Email = GetSetting(SIGNER_1_EMAIL);
            Signer1Name = GetSetting(SIGNER_1_NAME);
            Cc1Email = GetSetting(CC_1_EMAIL);
            Cc1Name = GetSetting(CC_1_NAME);
            PrivateKey = GetSetting(PRIVATE_KEY);
            AuthServer = GetSetting(DS_AUTH_SERVER);
            AuthenticationURL = GetSetting(DS_AUTH_SERVER);
            API = "restapi/v2";
            PermissionScopes = "signature impersonation";
            JWTScope = "signature";
            QueueURL = GetSetting(QUEUE_URL);
            QueueRegion = RegionEndpoint.USEast2; //Choose your own region
            AWSAccount = GetSetting(AWS_ACCOUNT);
            AWSSecret = GetSetting(AWS_SECRET);
            Debug = GetSetting(DEBUG);
            CustomFiled = GetSetting(ENVELOPE_CUSTOM_FIELD);
            FilePrefix = GetSetting(OUTPUT_FILE_PREFIX);
            EnableBreakTest = GetSetting(ENABLE_BREAK_TEST);
            testOutputDirectory = GetSetting(TEST_OUTPUT_DIR_NAME);
            EnqueueURL = GetSetting(ENQUEUE_URL);
            basicAuthName = GetSetting(BASIC_AUTH_NAME);
            basicAuthPW = GetSetting(BASIC_AUTH_PW);
        }

        private static string GetSetting(string configKey)
        {
            string val = Environment.GetEnvironmentVariable(configKey)
                ?? ConfigurationManager.AppSettings.Get(configKey);

            //Finds the config file path
            //System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (PRIVATE_KEY.Equals(configKey) && "FALSE".Equals(val))
                return null;

            return val ?? "";
        }

        public static string ClientID { get; private set; }
        public static string ImpersonatedUserGuid { get; private set; }
        public static string TargetAccountID { get; private set; }
        public static string OAuthRedirectURI { get; private set; }
        public static string Signer1Email { get; private set; }
        public static string Signer1Name { get; private set; }
        public static string Cc1Email { get; private set; }
        public static string Cc1Name { get; private set; }
        public static string PrivateKey { get; private set; }
        private static string authServer;
        public static string AuthServer
        {
            get { return authServer; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value) && value.StartsWith("https://"))
                {
                    authServer = value.Substring(8);
                }
                else if (!String.IsNullOrWhiteSpace(value) && value.StartsWith("http://"))
                {
                    authServer = value.Substring(7);
                }
                else
                {
                    authServer = value;
                }
            }
        }
        public static string AuthenticationURL { get; private set; }
        public static string API { get; private set; }
        public static string PermissionScopes { get; private set; }
        public static string JWTScope { get; private set; }
        public static string QueueURL { get; private set; }
        public static RegionEndpoint QueueRegion { get; private set; }
        public static string AWSAccount { get; private set; }
        public static string AWSSecret { get; private set; }
        public static string Debug { get; private set; }
        public static string CustomFiled { get; private set; }
        public static string FilePrefix { get; private set; }
        public static string EnableBreakTest { get; private set; }
        public static string testOutputDirectory { get; private set; }
        public static string EnqueueURL { get; private set; }
        public static string basicAuthName { get; private set; }
        public static string basicAuthPW { get; private set; }
    }
}
