using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionAPI.Models
{
    [Table("Transactions")]
    public class TransactionModel
    {
        [Key]
        [Column("transaction_id")]
        public string? TransactionId { get; set; }
        [Column("name")]
        public string? Name { get; set; }
        [Column("email")]
        public string? Email { get; set; }
        [Column("amount")]
        public decimal? Amount { get; set; }
        [Column("transaction_date")]
        public DateTime TransactionDate { get; set; }
        [Column("client_location")]
        public string? ClientLocation { get; set; }

        public virtual GeneralTimeModel? GeneralTime { get; set; }
    }
}
