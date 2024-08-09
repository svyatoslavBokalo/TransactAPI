namespace TransactionAPI.Models
{
    // this model for export data to csv file (because we want to get not all columns or all. It depends))
    public class TransactionExportRequest
    {
        public bool IncludeTransactionId { get; set; }
        public bool IncludeName { get; set; }
        public bool IncludeEmail { get; set; }
        public bool IncludeAmount { get; set; }
        public bool IncludeTransactionDate { get; set; }
        public bool IncludeClientLocation { get; set; }
    }
}
