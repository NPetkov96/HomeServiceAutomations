using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Models
{
    public class MedSestriPatientBloodTest
    {
        [Key, Column(Order = 0)]
        public int PatientId { get; set; }
        public MedSestriPatient Patient { get; set; }

        [Key, Column(Order = 1)]
        public int BloodTestId { get; set; }
        public MedSestriBloodTest BloodTest { get; set; }
    }
}
