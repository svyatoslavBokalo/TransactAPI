using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TransactionAPI.Models
{
    [Table("GeneralTimes")]
    public class GeneralTimeModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Transaction")]
        [Column("transaction_id")]
        public string TransactionId { get; set; }

        [Column("timezone")]
        public string TimeZone { get; set; }

        [Column("general_time")]
        public DateTime GeneralTime { get; set; }

        public virtual TransactionModel Transaction { get; set; }
    }
}
