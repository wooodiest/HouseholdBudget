using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Configuration;

namespace HouseholdBudget.Core.Services.Remote
{
    public class AzureDocumentAnalysisService : IAzureDocumentAnalysisService
    {
        private readonly string _endpoint;
        private readonly string _apiKey;

        public AzureDocumentAnalysisService(IConfiguration configuration)
        {
            _endpoint = configuration["AzureDocumentIntelligence:Endpoint"];
            _apiKey   = configuration["AzureDocumentIntelligence:ApiKey"];
        }

        public async Task<AnalyzedReceipt> AnalyzeReceiptFromBlobAsync(BlobObject blob)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blob.ImageUrl))
                    throw new ArgumentException("Blob image URL is null or empty");

                var client = GetDocumentIntelligenceClient();
                var uri    = new Uri(blob.ImageUrl);

                if (uri is null)
                    throw new ArgumentException("Blob image URL is not a valid URI");
   
                var operation = await client.AnalyzeDocumentAsync(
                    WaitUntil.Completed,
                    "prebuilt-receipt",
                    uri
                );

                var result   = operation.Value;
                var document = result.Documents.FirstOrDefault();
                if (document is null)
                    throw new InvalidOperationException("The document could not be read.");

                return new AnalyzedReceipt
                {
                    MerchantName = document.Fields.TryGetValue("MerchantName", out var m)
                            ? m.ValueString : null,
                    Total = document.Fields.TryGetValue("Total", out var t)
                            ? (decimal?)t.ValueCurrency?.Amount : null,
                    TransactionDate = document.Fields.TryGetValue("TransactionDate", out var d)
                            ? d.ValueDate?.DateTime : null
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while processing in document intelligence: {ex.Message}");
            }
        }

        private DocumentIntelligenceClient GetDocumentIntelligenceClient()
        {
            return new DocumentIntelligenceClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
        }
    }
}
