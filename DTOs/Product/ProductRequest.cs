using System.ComponentModel.DataAnnotations;

namespace dotnet_hero.DTOs.Product
{
    public class ProductRequest
    {
        public int? ProductId { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Name, Maximum length 100.")]
        public string Name { get; set; } = null!;

        [Range(0, 10000)]
        public int Stock { get; set; }

        [Range(0, 1_000_000)]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        public List<IFormFile>? FormFiles { get; set; }
    }
}