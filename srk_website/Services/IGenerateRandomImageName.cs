namespace srk_website.Services
{
    public interface IGenerateRandomImageName
    {
        Task<string> Generate(string dataType, int length);
    }
}
