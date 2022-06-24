using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace srk_website.Models
{
    public class ServiceModel
    {
        public ServiceModel() { }
        
        public ServiceModel(string imageName, string title, string description, string uri)
        {
            ImageName = imageName;
            Title = title;
            Description = description;
            Uri = uri;
        }

        [Key]
        public string ImageName { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Url]
        public string Uri { get; set; }
    }
}
