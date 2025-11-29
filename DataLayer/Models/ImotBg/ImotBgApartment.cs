using System.ComponentModel.DataAnnotations;

namespace DataLayer.Models.ImotBg
{
    public class ImotBgApartment
    {
        [Key]
        public int Id { get; set; } 
        public string Title { get; set; } 
        public string Neighbour { get; set; } 
        public double SquareMetres { get; set; } 
        public double? Price { get; set; } 
        public double? OldPrice { get; set; } 
        public double? PricePerSqMetre { get; set; } 
        public int? Floor { get; set; }
        public string MoreInformation { get; set; }
        public string City { get; set; } 
        public string? Address { get; set; }
        public string URl { get; set; } 
        public bool IsActive { get; set; } 
        public string ApartmentId { get; set; }
        public DateTime? Date { get; set; }
        public string? Error { get; set; }
        public string Construction { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
