namespace DataLayer.Models.DTOs
{
    public class MedSestriPatientsDTO
    {
        public string FullName { get; set; }
        public string EGN { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public List<MedSestriBloodTest> BloodTests { get; set; } = new();
    }
}
