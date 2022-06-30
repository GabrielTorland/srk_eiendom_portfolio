using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace srk_website.Models
{
    public class ServiceModel
    {   
        public ServiceModel() { }
        public ServiceModel(string title, string description, string imageName, string uri)
        {
            Title = title;
            Description = description;
            ImageName = imageName;
            Uri = uri;
        }

        public int Id { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [StringLength(400, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
        public string? Description { get; set; }

        [ValidateNever]
        public string? ImageName { get; set; }

        [ValidateNever]
        public string? Uri { get; set; }
    }
}
