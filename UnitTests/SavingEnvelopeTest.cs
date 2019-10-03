using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

namespace Worker
{
    class SavingEnvelopeTest
    {
        [Test]
        public void savingEnvelopeTest()
        {
           Console.WriteLine("Starting");
           sendEnvelope();
           created();
           Console.WriteLine("Done");
        }

        /**
         * Sending envelope to the AmazonSQS
         */
        public void sendEnvelope()
        {
            try
            {
                ApiClient apiClient = new ApiClient();
                Console.WriteLine("\nSending an envelope. The envelope includes HTML, Word, and PDF documents.\n" +
                    "It takes about 15 seconds for DocuSign to process the envelope request... ");
                // Create the envelope anns send it 
                EnvelopeSummary result = new CreateEnvelope(apiClient).Send();
                Console.WriteLine("\nDone. Envelope status: {0}. Envelope ID: {1}", result.Status, result.EnvelopeId);

            }
            catch (IOException e)
            {
                throw new Exception("\nIOException!" + e.Message);
            }
            catch (ApiException e)
            {
               throw new Exception("\nDocuSign Exception!" + e.Message);
            }
        }

        /**
         * Search for the file that contains the envelope details
         */
        public void created()
        {
            try
            {
                string derectoryPath = Path.GetDirectoryName(Path.GetDirectoryName(
                   Path.GetDirectoryName(
                   Path.GetDirectoryName(
                   Path.GetDirectoryName(
                   Assembly.GetExecutingAssembly().Location)))));

                string outputDirPath = Path.Combine(derectoryPath, Path.Combine("connect-csharp-worker-aws",Path.Combine("output", "order_Test_Mode.pdf")));

                // It takes about 15 seconds for DocuSign to process the envelope request
                // Sleep inorder to insure the envelope was accepted and analyzed
                System.Threading.Thread.Sleep(30000);

                // Search for the file in the specific directory
                if (!File.Exists(outputDirPath))
                {
                    Assert.Fail("Failed to find the file");
                }
            }
            catch(IOException e)
            {
                throw new Exception("\nIOException!" + e.Message);
            }
        }
    }
}
