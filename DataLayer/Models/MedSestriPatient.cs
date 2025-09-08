using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DataLayer.Models
{
    public class MedSestriPatient
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string EGN { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public List<MedSestriPatientBloodTest> PatientBloodTests { get; set; } = new();

        [NotMapped]
        [JsonPropertyName("bloodTests")]
        public List<MedSestriBloodTest> BloodTests { get; set; } = new();
    }
}
