using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace srk_website.Models
{
    public class StorageModel
    {
        public StorageModel() { }
        
        public StorageModel(string name, string imageName, string imageUri)
        {
            Name = name;
            ImageName = imageName;
            ImageUri = imageUri;
        }
        
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Name { get; set; }
        
        [Required]
        [StringLength(30)]
        public string? ImageName { get; set; }

        [Url]        
        [Required]
        public string? ImageUri { get; set; }

    }
}
