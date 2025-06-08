namespace HouseholdBudget.Core.Services.Remote
{
    public class AnalyzedReceipt
    {
        public string? MerchantName { get; set; }
        public decimal? Total { get; set; }
        public DateTime? TransactionDate { get; set; }
    }

    public interface IAzureDocumentAnalysisService
    {
        Task<AnalyzedReceipt> AnalyzeReceiptFromBlobAsync(BlobObject blob);
    }
}
