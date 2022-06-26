using System.ComponentModel.DataAnnotations;

namespace srk_website.Models
{
    public class StorageModel
    {
        public StorageModel() { }
        
        public StorageModel(string imageName, string imageUri)
        {
            ImageName = imageName;
            ImageUri = imageUri;
        }
        
        public int Id { get; set; }
        
        public string ImageName { get; set; }

        [Url]
        public string ImageUri { get; set; }

    }
}
