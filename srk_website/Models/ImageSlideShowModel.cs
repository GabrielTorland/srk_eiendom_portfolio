using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace srk_website.Models
{
    public class ImageSlideShowModel
    {
        public ImageSlideShowModel() { }

        public ImageSlideShowModel(string imageName, string projectName, string city, string website) 
        {
            ImageName = imageName;
            ProjectName = projectName;
            City = city;
            Website = website;
        }
        [Key]
        public string ImageName { get; set; }

        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string City { get; set; }

        [Url]
        [Required]
        [DisplayName("Website URI where you can buy the appartment/house")]
        public string Website { get; set; }
    }
}
