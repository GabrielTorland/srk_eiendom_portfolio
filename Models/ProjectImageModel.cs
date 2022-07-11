using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace srk_website.Models
{
    public class ProjectImageModel
    {
        public ProjectImageModel() 
        {
            Projects = new HashSet<ProjectModel>();
        }
        
        public ProjectImageModel(string name, string projectName, string imageName, string imageUri)
        {
            Name = name;
            ProjectName = projectName;
            ImageName = imageName;
            ImageUri = imageUri;
            Projects = new HashSet<ProjectModel>();
        }
        
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Name { get; set; }

        [Required]
        [DisplayName("Project Name")]
        [StringLength(50)]
        public string? ProjectName { get; set; }

        [ValidateNever]
        public string? ImageName { get; set; }

        [ValidateNever]
        public string? ImageUri { get; set; }

        public virtual ICollection<ProjectModel> Projects { get; set; }

    }
}
