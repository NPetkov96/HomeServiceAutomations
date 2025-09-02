using System.ComponentModel.DataAnnotations;

namespace DataLayer.Models.Common
{
    public class Settings
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
    }
}
