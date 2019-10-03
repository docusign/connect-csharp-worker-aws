using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Worker
{
    public class ProcessNotification
    {
        private static string envelopeId;
        private static string subject;
        private static string senderName;
        private static string senderEmail;
        private static string status;
        private static string created;
        private static bool completed;
        private static string completedMsg;
        private static string orderNumber;
        // Access to the directory - in order to save the folders in the right path
        public readonly static string mainPath = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));

        /**
         *  Process the notification message
         */
        public static void process(string test, string xml)
        {
            // Guarding against injection attacks 
            // Create a pattern
            string pattern = "[^0-9]";
            // Create a Regex  
            Regex rg = new Regex(pattern);
            // Get all matches  
            MatchCollection matchedAuthors = rg.Matches(test);
            // Check the incoming test variable to ensure that it ONLY contains the expected data (empty string "", "/break" or integers string)
            // if matchedAuthors.Count > 0 equals true when it finds wrong input
            bool validInput = test.Equals("/break") || test.Equals("") || !(matchedAuthors.Count > 0);
            if (validInput)
            {
                if (!test.Equals(""))
                {
                    // Message from test mode
                    processTest(test);
                }
            }
            else
            {
                Console.WriteLine(DateTime.Now + " Wrong test value: " + test + "\ntest can only be: /break, empty string or integers string");
            }
            // In test mode there is no xml sting, should be checked before trying to parse it
            if (!xml.Equals(""))
            {
                //Step 1. parse the xml
                XElement xmlroot = XElement.Parse(xml);
                //get the namespace from the xml
                XNamespace xmlns = xmlroot.GetDefaultNamespace();
                // Write the xml name space + the element name
                Func<string, XName> nameSpace = elementName => xmlns + elementName;

                envelopeId = xmlroot.Element(nameSpace("EnvelopeStatus")).Element(nameSpace("EnvelopeID")).Value;
                subject = xmlroot.Element(nameSpace("EnvelopeStatus")).Element(nameSpace("Subject")).Value;
                senderName = xmlroot.Element(nameSpace("EnvelopeStatus")).Element(nameSpace("UserName")).Value;
                senderEmail = xmlroot.Element(nameSpace("EnvelopeStatus")).Element(nameSpace("Email")).Value;
                status = xmlroot.Element(nameSpace("EnvelopeStatus")).Element(nameSpace("Status")).Value;
                created = xmlroot.Element(nameSpace("EnvelopeStatus")).Element(nameSpace("Created")).Value;
                orderNumber = xmlroot.Element(nameSpace("EnvelopeStatus")).Element(nameSpace("CustomFields"))
                    .Element(nameSpace("CustomField")).Element(nameSpace("Value")).Value;

                if (status.Equals("Completed"))
                {
                    completed = true;
                    completedMsg = "Completed " + completed;
                }
                else
                {
                    completed = false;
                    completedMsg = "";
                }
                
                // For debugging, you can print the entire notification
                Console.WriteLine("EnvelopeId: " + envelopeId);
                Console.WriteLine("Status: " + status);
                Console.WriteLine("Order Number: " + orderNumber);
                Console.WriteLine("Subject: " + subject);
                Console.WriteLine("Sender: " + senderName + ", " + senderEmail);
                Console.WriteLine("Sent: " + created + ", " + completedMsg);

                // Step 2. Filter the notifications
                bool ignore = false;
                // Guarding against injection attacks
                // Check the incoming orderNumber variable to ensure that it ONLY contains the expected data ("Test_Mode" or integers string)
                // if matchedAuthors.Count > 0 equals true when it finds wrong input
                // Envelope might not have Custom field when orderNumber == null
                matchedAuthors = rg.Matches(orderNumber);
                validInput = orderNumber.Equals("Test_Mode") || !(matchedAuthors.Count > 0) || orderNumber == null;
                if (validInput)
                {
                    // Check if the envelope was sent from the test mode 
                    // If sent from test mode - ok to continue even if the status != Completed
                    if (!orderNumber.Equals("Test_Mode"))
                    {
                        if (!status.Equals("Completed"))
                        {
                            ignore = true;
                            if (DSConfig.Debug.Equals("true"))
                            {
                                Console.WriteLine(DateTime.Now + " IGNORED: envelope status is " + status);
                            }
                        }
                    }
                    if (orderNumber == null || orderNumber.Equals(""))
                    {
                        ignore = true;
                        if (DSConfig.Debug.Equals("true"))
                        {
                            Console.WriteLine(DateTime.Now + " IGNORED: envelope does not have a " +
                                    DSConfig.CustomFiled + " envelope custom field.");
                        }
                    }
                }
                else
                {
                    ignore = true;
                    Console.WriteLine(DateTime.Now + " Wrong orderNumber value: " + orderNumber);
                    Console.WriteLine("orderNumber can only be: Test_Mode or integers string");
                }
                // Step 3. (Future) Check that this is not a duplicate notification
                // The queuing system delivers on an "at least once" basis. So there is a 
                // chance that we have already processes this notification.
                //
                // For this example, we'll just repeat the document fetch if it is duplicate notification

                // Step 4 save the document - it can raise an exception which will be caught at startQueue 
                if (!ignore)
                {
                    saveDoc(envelopeId, orderNumber);
                }
            }
        }

        /**
         * Creating a new file that contains the envelopeId and orderNumber
         */
        private static void saveDoc(string envelopeId, string orderNumber)
        {
            try
            {
                ApiClient dsApiClient = new ApiClient();
                JWTAuth dsJWTAuth = new JWTAuth(dsApiClient);
                // Checks for the token before calling the function getToken
                dsJWTAuth.CheckToken();
                var config = new Configuration(new ApiClient(dsJWTAuth.getBasePath()));
                config.AddDefaultHeader("Authorization", "Bearer " + dsJWTAuth.getToken());
                EnvelopesApi envelopesApi = new EnvelopesApi(config);

                // Create the output directory if needed
                string outputPath = Path.Combine(mainPath, "output");

                if (!Directory.Exists(outputPath))
                {
                    DirectoryInfo directory = Directory.CreateDirectory(outputPath);
                    if (!directory.Exists)
                    {
                        throw new Exception(DateTime.Now + " Failed to create directory.");
                    }
                }

                Stream results = envelopesApi.GetDocument(dsJWTAuth.getAccountID(), envelopeId, "combined");
                string filePath = Path.Combine(mainPath, Path.Combine("output", DSConfig.FilePrefix + orderNumber + ".pdf"));
                // Create the output file
                using (System.IO.Stream stream = File.Create(filePath))
                    results.CopyTo(stream);

                if (!File.Exists(filePath))
                {
                    throw new Exception(DateTime.Now + " Failed to create file");
                }
            
                // Catch exception if BREAK_TEST equals to true or if orderNumber contains "/break"
                if (DSConfig.EnableBreakTest.Equals("true") && ("" + orderNumber).Contains("/break"))
                {
                    throw new Exception(DateTime.Now + " Break test");
                }
            }
            catch (ApiException e)
            {
                Console.WriteLine(DateTime.Now + " API exception: " + e.Message);
                throw new Exception(DateTime.Now + " saveDoc error");

            }

            // Catch exception while trying to save the document
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now + " Error while fetching and saving docs for envelope " + envelopeId + ", order " + orderNumber);
                Console.WriteLine(e.Message);
                throw new Exception(DateTime.Now + " saveDoc error");
            }
        }
        
        private static void processTest(string test)
        {
            // Exit the program if BREAK_TEST equals to true or if orderNumber contains "/break"
            if (DSConfig.EnableBreakTest.Equals("true") && ("" + test).Contains("/break"))
            {
                Console.WriteLine(DateTime.Now + " BREAKING worker test!");
                Environment.Exit(2);
            }

            Console.WriteLine("Processing test value " + test);

            // Create the test_messages directory if needed 
            string testDirName = DSConfig.testOutputDirectory;
            string outputPath = Path.Combine(mainPath, testDirName);

            if (!Directory.Exists(outputPath))
            {
                DirectoryInfo directory = Directory.CreateDirectory(outputPath);
                if (!directory.Exists)
                {
                    throw new Exception(DateTime.Now + " Failed to create directory.");
                }
            }

            // First shuffle test1 to test2 (if it exists) and so on
            for( int i=9 ; i>0 ; i--)
            {
                if (File.Exists(Path.Combine(outputPath, "test" + i + ".txt")))
                {
                    if (File.Exists(Path.Combine(outputPath, "test" + (i+1) + ".txt")))
                    {
                        File.Delete(Path.Combine(outputPath, "test" + (i + 1) + ".txt"));
                    }
                    File.Copy(Path.Combine(outputPath, "test" + i + ".txt"), Path.Combine(outputPath, "test" + (i + 1) + ".txt"));
                }   
            }
            
            // The new test message will be placed in test1 - creating new file
            File.WriteAllText(Path.Combine(outputPath, "test1.txt"), test);
            Console.WriteLine(DateTime.Now + " new file created");
        }
    }
}
