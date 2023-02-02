using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Core.Models.Hotel
{
    public abstract class BaseHotelDto
    {
            [Required]
            public string Name { get; set; }
            [Required]
            public string Address { get; set; }
            //The Rating is declared as Nullable
            public double? Rating { get; set; }

            [Required]
            [Range(1, int.MaxValue)]
            public int CountryId { get; set; }
     
    }
}
