using System.ComponentModel.DataAnnotations;

namespace DataLayer.Models.ImotBg
{
    public class ImotBgSettings
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
