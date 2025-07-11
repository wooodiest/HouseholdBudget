using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.Core.Services.Remote
{

    public class AnalyzedResult
    {
        public string? MerachentName { get; set; }
        public decimal? Total {  get; set; }
        public DateTime? TransactionDate { get; set; }
    }

    public interface IAzureDocumentInteligence
    {
        Task<AnalyzedResult> AnalyzeReceiptFromBlobAsync(BlobObject blob);
    }
}
