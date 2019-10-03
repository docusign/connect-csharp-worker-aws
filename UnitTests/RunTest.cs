using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace Worker
{
    [TestClass]
    public class RunTest
    {
        private static DateTime timeStart;
        private static DateTime[] timeChecks = new DateTime[8];
        private static int timeCheckNumber = 0; // 0..6 
        private static int successes = 0;
        private static int enqueueErrors = 0;
        private static int dequeueErrors = 0;
        public static string mode; // many or few
        ArrayList testsSent = new ArrayList(); // test values sent that should also be receieved
        private static bool foundAll = false;
        public static string logDirectoryPath = Path.GetDirectoryName(Path.GetDirectoryName(
                   Path.GetDirectoryName(
                   Path.GetDirectoryName(
                   Assembly.GetExecutingAssembly().Location))));


        [TestCase("few")]
        [TestCase("many")]
        public void runTest(string modeName)//string modeName
        {
            mode = modeName;
            timeStart = DateTime.Now;
            for (int i = 0; i <= 7; i++)
            {
                timeChecks[i] = timeStart.AddHours(i + 1);
              
            }
            LogWriter log = new LogWriter("Starting");
            doTests();
            log = new LogWriter("Done");
        }

        private void doTests()
        {
            while (timeCheckNumber <= 7)
            {
                //Return Value: This method return a signed number indicating the relative values of this instance and the value parameter.
                //Less than zero: If this instance is earlier than value.
                //Zero : If this instance is the same as value.
                //Greater than zero : If this instance is later than value.
                while (timeChecks[timeCheckNumber].CompareTo(DateTime.Now) > 0)
                {
                    doTest();
                    if (mode == "few")
                    {
                        double secondsToSleep = (timeChecks[timeCheckNumber].TimeOfDay- DateTime.Now.TimeOfDay).Seconds + 2 ;
                        int sleep = (int)Math.Round(secondsToSleep);
                        Thread.Sleep(sleep * 60 * 1000);
                    }
                }
                showStatus();
                timeCheckNumber++;
            }
            showStatus();
        }

        private void showStatus()
        {
            double rate = (100.0 * successes) / (enqueueErrors + dequeueErrors + successes);
            LogWriter log = new LogWriter("#### Test statistics: " + successes + " (" + (double)Math.Round(rate, 2) + "%) successes, " +
                    enqueueErrors + " enqueue errors, " + dequeueErrors + " dequeue errors.");
            
        }

        private void doTest()
        {
            send(); // sets testsSent
            DateTime endTime = DateTime.Now.AddMinutes(3);
            foundAll = false;
            int tests = testsSent.Count;
            int successesStart = successes;
            while (!foundAll && endTime.CompareTo(DateTime.Now) > 0)
            {
                Thread.Sleep(1000);
                checkResults(); // sets foundAll and updates testsSent
            }
            if (!foundAll)
            {
                dequeueErrors += testsSent.Count;
            }
            LogWriter log = new LogWriter("Test: " + tests + " sent. " + (successes - successesStart) + " successes, " +
                testsSent.Count + " failures.");
        }
        /**
         * Look for the reception of the testsSent values
         */
        private void checkResults()
        {
            ArrayList testsReceived = new ArrayList();
            string fileData = "";
            for (int i = 0; i <= 20; i++)
            {
                fileData = "";
                try
                {
                    string derectoryPath = Path.GetDirectoryName(Path.GetDirectoryName(
                    Path.GetDirectoryName(
                    Path.GetDirectoryName(
                    Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location)))));
                
                    string testOutputDirPath = Path.Combine(derectoryPath, Path.Combine("connect-csharp-worker-aws", Path.Combine(DSConfig.testOutputDirectory,"\\test" + i + ".txt")));
                    // If the file exists read the data from it and save the data in fileData
                    if (File.Exists(testOutputDirPath))
                    {
                        fileData = File.ReadAllText(testOutputDirPath);
                    }
                }
                catch (IOException e)
                {
                    DateTime date = DateTime.Now;
                    LogWriter log = new LogWriter("IOException " + e.Message);
                    
                }
                if (!fileData.Equals(null) && !fileData.Equals(""))
                {
                    testsReceived.Add(fileData);
                }
            }
            // Create a private copy of testsSent (testsSentOrig) and reset testsSent
            // Then, for each element in testsSentOrig not found, add back to testsSent.
            ArrayList testsSentOrig = new ArrayList(testsSent);
            testsSent.Clear();
            bool found = false;
            foreach (string testValue in testsSentOrig)
            {
                found = testsReceived.Contains(testValue);
                if (found)
                {
                    successes++;
                }
                else
                {
                    testsSent.Add(testValue);
                }
            }

            // Update foundAll
            foundAll = testsSent.Count == 0;
        }

        /**
         * Send 5 messages
         */
        private void send()
        {
            testsSent.Clear();
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    string testValue = "" + now;
                    send1(testValue);
                    testsSent.Add(testValue);
                }
                catch (System.Exception e)
                {
                    enqueueErrors++;
                    DateTime date = DateTime.Now;
                    LogWriter log = new LogWriter("send: Enqueue error: " + e.Message);
                    Thread.Sleep(30 * 1000);
                }

            }
        }

        /**
         * Send one enqueue request. Errors will be caught by caller
         */
        private void send1(string test)
        {
            try
            {
                string url = DSConfig.EnqueueURL + "?test=" + test;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 20 * 1000; 
                request.Credentials = new NetworkCredential(DSConfig.AWSAccount, DSConfig.AWSSecret);
                string auth = authObject();
                if (!auth.Equals(null))
                {
                    string basicAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
                    request.Headers.Add("Authorization", "Basic " + basicAuth);
                }
                
                HttpWebResponse Response = (HttpWebResponse)request.GetResponse();

                if (Response.StatusCode != HttpStatusCode.OK)
                {
                    DateTime date = DateTime.Now;
                    LogWriter log = new LogWriter("send1: GET not worked, StatusCode= " + Response.StatusCode);
                    
                }
                Response.Close();
            }
            catch (Exception e)
            {
                DateTime date = DateTime.Now;
                LogWriter log = new LogWriter("send1: https error: " + e.Message);
             
            }
        }

        /**
         *  Returns a string for the HttpWebResponse header
         */
        private string authObject()
        {
            if (!DSConfig.basicAuthName.Equals(null) && DSConfig.basicAuthName != "{BASIC_AUTH_NAME}"
                    && !DSConfig.basicAuthPW.Equals(null) && DSConfig.basicAuthPW != "{BASIC_AUTH_PS}")
            {
                return DSConfig.basicAuthName + ":" + DSConfig.basicAuthPW;
            }
            else
            {
                return null;
            }
        }
    }
}