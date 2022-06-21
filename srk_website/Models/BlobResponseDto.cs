namespace srk_website.Models
{
    public class BlobResponseDto
    {
        public string? Status { get; set; }
        public bool Error { get; set; }
        public BlobDto Blob { get; set; }
        public BlobResponseDto()
        {
            Blob = new BlobDto();
        }
    }
}


