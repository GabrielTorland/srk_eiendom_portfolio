namespace srk_website.Models
{
    public class AboutModel
    {
        public AboutModel() { }
        public AboutModel(string text)
        {
            Text = text;
        }
        
        public int Id { get; set; }
        
        public string? Text { get; set; }
    }
}
