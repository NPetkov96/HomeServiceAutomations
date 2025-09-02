using System.ComponentModel.DataAnnotations;

namespace DataLayer.Models
{
    public class MedSestriBloodTest
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public double BngPrice { get; set; }
        public double EuroPrice { get; set; }
    }
}
