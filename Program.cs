using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using srk_website.Data;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using srk_website.Services;

var builder = WebApplication.CreateBuilder(args);

// Key vault options
SecretClientOptions options = new SecretClientOptions()
{
    Retry =
            {
                Delay= TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
            }
};
// Key vault connection
var kVUrl = builder.Configuration.GetValue<string>("AzureKeyVaultUrl");
var secretsClient = new SecretClient(new Uri(kVUrl), new DefaultAzureCredential(), options);

// Get BlobConnectionString from key vault and store it in appsettings.json
builder.Configuration["BlobConnectionString"] = secretsClient.GetSecret("BlobConnectionString").Value.Value;

// Get SendGridKey from key vault and store it in appsettings.json
builder.Configuration["SendGridKey"] = secretsClient.GetSecret("SendGridKey").Value.Value;

// Get MySQL connection string from key vault and store it in appsettings.json
builder.Configuration["ConnectionStrings:DefaultConnection"] = secretsClient.GetSecret("DefaultConnectionString").Value.Value;
    
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Transients
builder.Services.AddTransient<IGenerateRandomImageName, GenerateRandomImageName>();
builder.Services.AddTransient<IAzureStorage, AzureStorage>();
builder.Services.AddTransient<IEmailSender, EmailSender>();


// The default inactivity timeout is 14 days. The following code sets the inactivity timeout to 5 days:
builder.Services.ConfigureApplicationCookie(o => {
    o.ExpireTimeSpan = TimeSpan.FromDays(5);
    o.SlidingExpiration = true;
});

// The following code changes all data protection tokens timeout period to 3 hours:
builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
       o.TokenLifespan = TimeSpan.FromHours(3));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


// Remove the possibility of register a new account(mapping Register subfolder to Login subfolder).
app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect("/Identity/Account/Login", true, true)));
    endpoints.MapPost("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect("/Identity/Account/Login", true, true)));
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Migrate any database changes on startup (includes initial db creation)
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var um = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var _emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
    await ApplicationDbInitializer.Initialize(dataContext, um, _emailSender);
}

app.Run();