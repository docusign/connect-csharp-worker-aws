
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using DocuSign.eSign.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;

namespace Worker
{
    class AWSWorker
    {
        private static readonly ApiClient apiClient = new ApiClient();
        private static RegionEndpoint queueRegion = DSConfig.QueueRegion; // Choose your own region in the DSConfig.cs file
        private static AmazonSQSClient queue = new AmazonSQSClient(DSConfig.AWSAccount, DSConfig.AWSSecret, queueRegion); 
        private static Queue checkLogQ = new Queue();
        private static string queueUrl = DSConfig.QueueURL;
        private static bool restart = true;

        static void Main(string[] args)
        {
            listenForever();
        }

        /**
         * The function will listen forever, dispatching incoming notifications
         * to the processNotification library. 
         * See https://docs.aws.amazon.com/sdk-for-javascript/v2/developer-guide/sqs-examples-send-receive-messages.html#sqs-examples-send-receive-messages-receiving
         */
        private static void listenForever()
        {
            // Check that we can get a DocuSign token
            testToken();

            while (true)
            {
                if (restart)
                {
                    Console.WriteLine(DateTime.Now + " Starting queue worker");
                    restart = false;
                    // Start the queue worker
                    startQueue();
                }
                System.Threading.Thread.Sleep(5000);
            }
            
        }

        /**
         * Check that we can get a DocuSign token and handle common error
         * cases: ds_configuration not configured, need consent.
         */
        private static void testToken()
        {
            try
            {
                if (String.Equals(DSConfig.ClientID, "{CLIENT_ID}"))
                {
                    Console.WriteLine("Problem: you need to configure this example, either via environment variables (recommended)");
                    Console.WriteLine("or via the ds_configuration.js file.");
                    Console.WriteLine("See the README file for more information\n");
                    return;
                }

                JWTAuth dsJWTAuth = new JWTAuth(apiClient);
                dsJWTAuth.CheckToken();

            }
            // An API problem
            catch (ApiException e)
            {
                Console.WriteLine("\nDocuSign Exception!");

                // Special handling for consent_required
                String message = e.Message;
                if (!String.IsNullOrWhiteSpace(message) && message.Contains("consent_required"))
                {
                    String consent_url = String.Format("\n    {0}/oauth/auth?response_type=code&scope={1}&client_id={2}&redirect_uri={3}",
                        DSConfig.AuthenticationURL, DSConfig.PermissionScopes, DSConfig.ClientID, DSConfig.OAuthRedirectURI);

                    Console.WriteLine("C O N S E N T   R E Q U I R E D");
                    Console.WriteLine("Ask the user who will be impersonated to run the following url: ");
                    Console.WriteLine(consent_url);
                    Console.WriteLine("\nIt will ask the user to login and to approve access by your application.");
                    Console.WriteLine("Alternatively, an Administrator can use Organization Administration to");
                    Console.WriteLine("pre-approve one or more users.");
                    Environment.Exit(0);
                }
                else
                {
                    // Some other DocuSign API problem 
                    Console.WriteLine("    Reason: {0}", e.ErrorCode, " ", e.Message);
                    Console.WriteLine("    Error Reponse: {0}", e.ErrorContent);
                    Environment.Exit(0);
                }
            }
            // Not an API problem
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now + " " + e.Message);
            }

        }

        /**
         * Receive and wait for messages from queue
         */
        private async static void startQueue()
        {
            try
            {
                // Receive messages from queue, maximum waits for 20 seconds for message
                ReceiveMessageRequest receive_request = new ReceiveMessageRequest();
                receive_request.MaxNumberOfMessages = 10;
                receive_request.QueueUrl = queueUrl;
                receive_request.WaitTimeSeconds = 20;
                               
                while (true)
                {
                    addCheckLogQ(DateTime.Now + " Awaiting a message...");
                    // Contain all the queue messages
                    ReceiveMessageResponse receiveMessageResponse = await queue.ReceiveMessageAsync(receive_request);
                    addCheckLogQ(DateTime.Now + " found " + receiveMessageResponse.Messages.Count + " message(s)");
                    // If at least one message has been received
                    if (receiveMessageResponse.Messages.Count != 0)
                    {
                        printCheckLogQ();
                        foreach (Message m in receiveMessageResponse.Messages)
                        {
                            messageHandler(m, queue);
                        }
                    }
                }
            }
            // Catches all types of errors that may occur during the program
            catch (Exception e)
            {
                printCheckLogQ();
                Console.WriteLine(DateTime.Now + " Queue receive error:");
                Console.WriteLine(e.Message);
                System.Threading.Thread.Sleep(5000);
                // Restart the program
                restart = true;
            }
        }

        /**
         * Maintain the array checkLogQ as a FIFO buffer with length 4.
         * When a new entry is added, remove oldest entry and shuffle.
         */
        private static void addCheckLogQ(string message)
        {
            int length = 4;
            // If checkLogQ size is smaller than 4 add the message
            if (checkLogQ.Count < length)
            {
                checkLogQ.Enqueue(message);
            }
            // If checkLogQ size is bigger than 4
            else
            {
                // Remove the oldest message
                checkLogQ.Dequeue();
                // Create temporary queue in order to change checkLogQ
                Queue temp = new Queue();
                foreach (string msg in checkLogQ)
                {
                    temp.Enqueue(msg);
                }
                // Add the new message
                temp.Enqueue(message);
                checkLogQ.Clear();
               
                foreach( string msg in temp)
                {
                    checkLogQ.Enqueue(msg);
                }
                temp.Clear(); // Reset
            }
        }

        /**
         * Prints all checkLogQ messages to the console
         */
        private static void printCheckLogQ()
        {
            // Prints all the elements in the checkLogQ
            foreach (string message in checkLogQ)
            {
               Console.WriteLine(message);
            }
            checkLogQ.Clear(); // reset
        }

        /**
         * Process a message
         * See https://github.com/Azure/azure-sdk-for-js/tree/master/sdk/servicebus/service-bus#register-message-handler
         */
        private static void messageHandler(Message message, AmazonSQSClient queue)
        {
            string test = null;
            string xml = null;
            // If there is an error the program will catch it and the JSONCreated will change to false
            bool JSONCreated = true;
            string ReceiptHandle = message.ReceiptHandle;
            if (DSConfig.Debug.Equals("true"))
            {
                String str = " Processing message id " + message.MessageId;
                Console.WriteLine(DateTime.Now + str);
            }
            
            // Parse the information from message body. the information contains contains fields like test and xml
            try
            {
                string body = message.Body;
                JObject json = JObject.Parse(body);
                xml = (string) json.GetValue("xml");
                test = (string)json.GetValue("test");
            }
            // Catch exceptions while trying to create a JSON object
            catch (JsonException e)
            {
                Console.WriteLine(DateTime.Now + " " + e.Message);
                JSONCreated = false;
            }
            // Catch java.lang exceptions (trying to convert null to String) - make sure your message contains both those fields
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now + " " + e.Message);
                JSONCreated = false;
            }
            
            // If JSON object created successfully - continue
            if (JSONCreated)
            {
                ProcessNotification.process(test, xml);
            }
            // If JSON object wasn't created - ignore message
            else
            {
                String errorMessage = " Null or bad body in message id " + message.MessageId + ". Ignoring.";
                Console.WriteLine(DateTime.Now + errorMessage);
            }
            // Delete the message after all its information has been parsed
            queue.DeleteMessageAsync(queueUrl, ReceiptHandle);
        }
    }
}
