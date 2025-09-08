using System.ComponentModel.DataAnnotations;

namespace DataLayer.Models
{
    public class MedSestriCatheter
    {
        [Key]
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Date { get; set; }
        public string Address { get; set; }
        public bool IsChecked { get; set; }
    }
}
