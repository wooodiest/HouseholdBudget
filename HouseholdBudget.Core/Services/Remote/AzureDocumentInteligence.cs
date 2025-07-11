using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Configuration;
using Azure;

namespace HouseholdBudget.Core.Services.Remote
{
    public class AzureDocumentInteligence : IAzureDocumentInteligence
    {
        private readonly string _endpoint;
        private readonly string _apiKey;

        public AzureDocumentInteligence(IConfiguration configuration)
        {
            _endpoint = configuration["AzureDocumentIntelligence:Endpoint"];
            _apiKey = configuration["AzureDocumentIntelligence:ApiKey"];
        }

        public async Task<AnalyzedResult> AnalyzeReceiptFromBlobAsync(BlobObject blob)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blob.ImageUrl))
                    throw new ArgumentException("Blob image URL is null or empty");

                var client = GetDocumentIntelligenceClient();
                var uri = new Uri(blob.ImageUrl);

                if (uri is null)
                    throw new ArgumentException("Blob image URL is not a valid URI");

                var operation = await client.AnalyzeDocumentAsync(
                    WaitUntil.Completed,
                    "prebuilt-receipt",
                    uri
                );

                var result = operation.Value;
                var document = result.Documents.FirstOrDefault();

                if (document is null)
                    throw new InvalidOperationException("The document could not be read.");

                return new AnalyzedResult
                {
                    MerachentName = document.Fields.TryGetValue("MerchantName", out var m) ? m.ValueString : null,
                    Total = document.Fields.TryGetValue("Total", out var t) ? (decimal?)t.ValueCurrency?.Amount : null,
                    TransactionDate = document.Fields.TryGetValue("TransactionDate", out var d) ? d.ValueDate?.DateTime : null,
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while proccessing in document inteligence: {ex.Message}");
            }
        }

        private DocumentIntelligenceClient GetDocumentIntelligenceClient()
        {
            return new DocumentIntelligenceClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
        }
    }
}
