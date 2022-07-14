using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace srk_website.Models
{
    public class ImageSlideShowModel
    {
        public ImageSlideShowModel() { }
        
        public ImageSlideShowModel(string projectName, string city, string website, string imageName, string uri) 
        {
            ProjectName = projectName;
            City = city;
            Website = website;
            ImageName = imageName;
            Uri = uri;
        }

        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        [DisplayName("Project Name")]
        public string? ProjectName { get; set; }

        [Required]
        [StringLength(40)]
        public string? City { get; set; }

        [Url]
        [Required]
        [DisplayName("Website URI where you can buy the appartment/house")]
        public string? Website { get; set; }

        [ValidateNever]
        public string? ImageName { get; set; }

        [ValidateNever]
        public string? Uri { get; set; }
    }
}
