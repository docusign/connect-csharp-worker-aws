using System;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System.Text;
using System.Collections.Generic;

namespace Worker
{
    /// <summary>
    /// Send an envelope with a signer and cc recipient; with three docs:
    /// an HTML, a Word, and a PDF doc.
    /// Anchor text positioning is used for the fields.
    /// </summary>
    internal class CreateEnvelope : JWTAuth
    {
        private const String DOC_2_DOCX = "World_Wide_Corp_Battle_Plan_Trafalgar.docx";
        private const String DOC_3_PDF = "World_Wide_Corp_lorem.pdf";

        public static string ENVELOPE_1_DOCUMENT_1
        {
            get => "<!DOCTYPE html>" +
            "<html>" +
            "    <head>" +
            "      <meta charset=\"UTF-8\">" +
            "    </head>" +
            "    <body style=\"font-family:sans-serif;margin-left:2em;\">" +
            "    <h1 style=\"font-family: 'Trebuchet MS', Helvetica, sans-serif;" +
            "         color: darkblue;margin-bottom: 0;\">World Wide Corp</h1>" +
            "    <h2 style=\"font-family: 'Trebuchet MS', Helvetica, sans-serif;" +
            "         margin-top: 0px;margin-bottom: 3.5em;font-size: 1em;" +
            "         color: darkblue;\">Order Processing Division</h2>" +
            "  <h4>Ordered by " + DSConfig.Signer1Name + "</h4>" +
            "    <p style=\"margin-top:0em; margin-bottom:0em;\">Email: " + DSConfig.Signer1Email + "</p>" +
            "    <p style=\"margin-top:0em; margin-bottom:0em;\">Copy to: " + DSConfig.Cc1Name + ", " + DSConfig.Cc1Email + "</p>" +
            "    <p style=\"margin-top:3em;\">" +
            "  Candy bonbon pastry jujubes lollipop wafer biscuit biscuit. Topping brownie sesame snaps" +
            " sweet roll pie. Croissant danish biscuit soufflé caramels jujubes jelly. Dragée danish caramels lemon" +
            " drops dragée. Gummi bears cupcake biscuit tiramisu sugar plum pastry." +
            " Dragée gummies applicake pudding liquorice. Donut jujubes oat cake jelly-o. Dessert bear claw chocolate" +
            " cake gummies lollipop sugar plum ice cream gummies cheesecake." +
            "    </p>" +
            "    <!-- Note the anchor tag for the signature field is in white. -->" +
            "    <h3 style=\"margin-top:3em;\">Agreed: <span style=\"color:white;\">**signature_1**</span></h3>" +
            "    </body>" +
            "</html>";
        }

        /// <summary>
        /// This class create and send envelope
        /// </summary>
        /// <param name="apiClient"></param>
        public CreateEnvelope(ApiClient apiClient) : base(apiClient)
        {
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        internal EnvelopeSummary Send()
        {
            CheckToken();

            EnvelopeDefinition envelope = this.CreateEvelope();
            EnvelopesApi envelopeApi = new EnvelopesApi(ApiClient.Configuration);
            EnvelopeSummary results = envelopeApi.CreateEnvelope(AccountID, envelope);
            return results;
        }
        /// <summary>
        /// This method creates the envelope request body 
        /// </summary>
        /// <returns></returns>
        private EnvelopeDefinition CreateEvelope()
        {
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
            {
                EmailSubject = "Please sign this document sent from the Test Mode"
            };

            Document doc1 = CreateDocumentFromTemplate("1", "Order acknowledgement", "html",
                    Encoding.UTF8.GetBytes(ENVELOPE_1_DOCUMENT_1));
            Document doc2 = CreateDocumentFromTemplate("2", "Battle Plan", "docx",
                    DSHelper.ReadContent(DOC_2_DOCX));
            Document doc3 = CreateDocumentFromTemplate("3", "Lorem Ipsum", "pdf",
                    DSHelper.ReadContent(DOC_3_PDF));

            // The order in the docs array determines the order in the envelope
            envelopeDefinition.Documents = new List<Document>() { doc1, doc2, doc3 };
            // create a signer recipient to sign the document, identified by name and email
            // We're setting the parameters via the object creation
            Signer signer1 = CreateSigner();
            // routingOrder (lower means earlier) determines the order of deliveries
            // to the recipients. Parallel routing order is supported by using the
            // same integer as the order for two or more recipients.

            // create a cc recipient to receive a copy of the documents, identified by name and email
            // We're setting the parameters via setters
            CarbonCopy cc1 = CreateCarbonCopy();
            // Create signHere fields (also known as tabs) on the documents,
            // We're using anchor (autoPlace) positioning
            //
            // The DocuSign platform seaches throughout your envelope's
            // documents for matching anchor strings. So the
            // sign_here_2 tab will be used in both document 2 and 3 since they
            // use the same anchor string for their "signer 1" tabs.
            SignHere signHere1 = CreateSignHere("**signature_1**", "pixels", "20", "10");
            SignHere signHere2 = CreateSignHere("/sn1/", "pixels", "20", "10");
            // Tabs are set per recipient / signer
            SetSignerTabs(signer1, signHere1, signHere2);
            // Add the recipients to the envelope object
            Recipients recipients = CreateRecipients(signer1, cc1);
            envelopeDefinition.Recipients = recipients;
            // Request that the envelope be sent by setting |status| to "sent".
            // To request that the envelope be created as a draft, set to "created"
            envelopeDefinition.Status = "sent";

            // create a CustomFields object and fill with text or list custom fields
            CustomFields customFields = new CustomFields();
            TextCustomField textCustomField = new TextCustomField();
            textCustomField.Name = "Sales order";
            textCustomField.Value = "Test_Mode";
            textCustomField.Show = "true";

            // add the textCustomFields to the CustomFields list, then assign the customFields to the envelope
            List<TextCustomField> textFieldsList = new List<TextCustomField>() { textCustomField };
            customFields.TextCustomFields = textFieldsList;
            envelopeDefinition.CustomFields = customFields;

            return envelopeDefinition;
        }
        /// <summary>
        /// This method creates Recipients instance and populates its signers and carbon copies
        /// </summary>
        /// <param name="signer">Signer instance</param>
        /// <param name="cc">CarbonCopy array</param>
        /// <returns></returns>
        private Recipients CreateRecipients(Signer signer, params CarbonCopy[] cc)
        {
            Recipients recipients = new Recipients
            {
                Signers = new List<Signer>() { signer },
                CarbonCopies = new List<CarbonCopy>(cc)
            };

            return recipients;
        }
        /// <summary>
        /// This method create Tabs
        /// </summary>
        /// <param name="signer">Signer instance to be set tabs</param>
        /// <param name="signers">SignHere array</param>
        private void SetSignerTabs(Signer signer, params SignHere[] signers)
        {
            signer.Tabs = new Tabs
            {
                SignHereTabs = new List<SignHere>(signers)
            };
        }
        /// <summary>
        /// This method create SignHere anchor
        /// </summary>
        /// <param name="anchorPattern">anchor pattern</param>
        /// <param name="anchorUnits">anchor units</param>
        /// <param name="anchorXOffset">anchor x offset</param>
        /// <param name="anchorYOffset">anchor y offset</param>
        /// <returns></returns>
        private SignHere CreateSignHere(String anchorPattern, String anchorUnits, String anchorXOffset, String anchorYOffset)
        {
            return new SignHere
            {
                AnchorString = anchorPattern,
                AnchorUnits = anchorUnits,
                AnchorXOffset = anchorXOffset,
                AnchorYOffset = anchorYOffset
            };
        }
        /// <summary>
        /// This method creates CarbonCopy instance and populate its members
        /// </summary>
        /// <returns>CarbonCopy instance</returns>
        private CarbonCopy CreateCarbonCopy()
        {
            return new CarbonCopy
            {
                Email = DSConfig.Cc1Email,
                Name = DSConfig.Cc1Name,
                RoutingOrder = "2",
                RecipientId = "2"
            };
        }
        /// <summary>
        /// This method creates Signer instance and populates its members
        /// </summary>
        /// <returns>Signer instance</returns>
        private Signer CreateSigner()
        {
            return new Signer
            {
                Email = DSConfig.Signer1Email,
                Name = DSConfig.Signer1Name,
                RecipientId = "1",
                RoutingOrder = "1"
            };
        }
        /// <summary>
        /// This method create document from byte array template
        /// </summary>
        /// <param name="documentId">document id</param>
        /// <param name="fileName">file name</param>
        /// <param name="fileExtension">file extention</param>
        /// <param name="templateContent">file content byte array</param>
        /// <returns></returns>
        private Document CreateDocumentFromTemplate(String documentId, String fileName, String fileExtension, byte[] templateContent)
        {
            Document document = new Document();

            String base64Content = Convert.ToBase64String(templateContent);

            document.DocumentBase64 = base64Content;
            // can be different from actual file name
            document.Name = fileName;
            // Source data format. Signed docs are always pdf.
            document.FileExtension = fileExtension;
            // a label used to reference the doc
            document.DocumentId = documentId;

            return document;
        }
    }
}
