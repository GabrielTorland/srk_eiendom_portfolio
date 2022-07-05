using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace srk_website.Models
{
    public class ProjectModel
    {
        public ProjectModel() { }

        public ProjectModel(string title, string projectDescription, List<string> uris, List<string> imageNames)
        {
            Title = title;
            ProjectDescription = projectDescription;
            Uris = uris;
            ImageNames = imageNames;
        }
        
        public int Id { get; set; }

        [Required]
        [DisplayName("Project Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [DisplayName("Project Description")]
        public string? ProjectDescription { get; set; }
        
        [Url]
        [ValidateNever]
        [NotMapped]
        public List<string>? Uris { get; set; }

        [ValidateNever]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [NotMapped]
        public List<string>? ImageNames { get; set; }
    }
}
