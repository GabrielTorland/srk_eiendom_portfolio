using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace srk_website.Models
{
    public class ProjectModel
    {
        public ProjectModel() 
        {
            Images = new HashSet<ProjectImageModel>();
        }

        public ProjectModel(string title, string projectDescription, string location, string coverImageUri, string thumbnailUri)
        {
            Title = title;
            ProjectDescription = projectDescription;
            Location = location;
            CoverImageUri = coverImageUri;
            ThumbnailUri = thumbnailUri;
            Images = new HashSet<ProjectImageModel>();
        }

         public int Id { get; set; }

        [Required]
        [DisplayName("Project Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }
        
        [Required]
        [DisplayName("Project Description")]
        public string? ProjectDescription { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? Location { get; set; }

        [Required]
        [DisplayName("Cover Image")]
        public string? CoverImageUri { get; set; }

        [Required]
        [DisplayName("Thumbnail Image")]
        public string? ThumbnailUri { get; set; }

        public virtual ICollection<ProjectImageModel> Images { get; set; }
    }
}
