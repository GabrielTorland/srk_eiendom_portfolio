using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace srk_website.Models
{
    public class StorageModel
    {
        public StorageModel() 
        {
            Projects = new HashSet<ProjectModel>();
        }
        
        public StorageModel(string name, string imageName, string imageUri)
        {
            Name = name;
            ImageName = imageName;
            ImageUri = imageUri;
            Projects = new HashSet<ProjectModel>();
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

        public virtual ICollection<ProjectModel> Projects { get; set; }

    }
}
