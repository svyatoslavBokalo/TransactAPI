using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;

namespace TransactionAPI.Models
{
    //So, I thought it would be better whether(1) to import all the data(time zone and general time)
    //  into the database and from them to receive data and process it, or (2) to do everything in RAM?!

    //1. if you select 1 item, it will occupy a large amount of memory with large data, but will save the program execution time

    //2. if you do all the operations in the operational and do not store anything,
    //  it can take a large amount of time, but more optimally with big data


    // so i decided to choose option 1
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

        // in this field we import time zone from location
        [Column("timezone")]
        public string TimeZone { get; set; }

        // in this field we calculate time to UTC +0
        [Column("general_time")]
        public DateTime GeneralTime { get; set; }

        public virtual TransactionModel Transaction { get; set; }
    }
}
