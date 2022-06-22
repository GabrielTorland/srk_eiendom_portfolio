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
        
        public string ProjectName { get; set; }

        public string City { get; set; }

        [Url]
        [DisplayName("Website where you can buy the appartment/house")]
        public string Website { get; set; }
    }
}
