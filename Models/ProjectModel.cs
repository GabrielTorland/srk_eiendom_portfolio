using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace srk_website.Models
{
    public class ProjectModel
    {
        public ProjectModel() 
        {
            Images = new HashSet<StorageModel>();
        }

        public ProjectModel(string title, string projectDescription, int coverImageId)
        {
            Title = title;
            ProjectDescription = projectDescription;
            Images = new HashSet<StorageModel>();
            CoverImageId = coverImageId;
        }

         public int Id { get; set; }

        [Required]
        [DisplayName("Project Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }
        
        [Required]
        [DisplayName("Project Description")]
        public string? ProjectDescription { get; set; }

        [DisplayName("Cover Image")]
        public int? CoverImageId { get; set; }
        public virtual ICollection<StorageModel> Images { get; set; }
    }
}
