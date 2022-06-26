namespace srk_website.Services
{
    public class GenerateRandomImageName : IGenerateRandomImageName
    {
        #region Dependency Injection / Constructor
        private readonly string _randomCharPool;
        private readonly ILogger<AzureStorage> _logger;
        private static Random random = new Random();
        public GenerateRandomImageName(ILogger<AzureStorage> logger, IConfiguration configuration)
        {
            _logger = logger;
            // Random chars that we choose from.
            _randomCharPool = configuration.GetValue<string>("RandomCharPool");
        }
        #endregion
        public Task<string> Generate(string dataType, int length)
        {
            string rImageName = new string(Enumerable.Repeat(_randomCharPool, length).Select(s => s[random.Next(s.Length)]).ToArray()) + '.' + dataType;
            return Task.FromResult(rImageName);
        }
    }
}
