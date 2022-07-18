using Microsoft.AspNetCore.Identity;
using srk_website.Services;
using srk_website.Models;

namespace srk_website.Data
{
    public class ApplicationDbInitializer
    {
        public static void Initialize(ApplicationDbContext db, UserManager<IdentityUser> um, IEmailSender _emailSender)
        {
            // Delete the database before we initialize it. This is common to do during development.
            
            // Reenable this after first build
            db.Database.EnsureDeleted();

            // Recreate the database and tables according to our models
            db.Database.EnsureCreated();

            // Create admins
            if (!um.Users.Any())
            {
                // Generating random string.
                Random random = new Random();
                string charPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
                var rString = new string(Enumerable.Repeat(charPool, 15).Select(s => s[random.Next(s.Length)]).ToArray());

                // This is the admin user.
                var user = new IdentityUser();                
                var email = "gabri.torland@gmail.com";
                user.UserName = email;
                user.Email = email;
                user.EmailConfirmed = true;
                // Change to rString later.
                var password = "Password1.";
                
                um.CreateAsync(user, password);
                
                // Send email with password
                _emailSender.SendEmailAsync(user.Email, "Initial password", $"<p>Here is your initial password: {password} . This password should be changed!<p>");
            }

            // Create about page
            if (!db.About.Any())
            {
                var about = new AboutModel("");
                db.About.Add(about);
            }

            // Create contact page
            if (!db.Contact.Any())
            {
                var newContact = new ContactModel();
                newContact.Email = "Roy@srkeiendom.no";
                newContact.Address = "Ålgårds veien 21";
                newContact.Phone = "94875234";
                newContact.City = "Ålgård";
                newContact.Country = "Norge";
                newContact.Zip = "4330";
                db.Contact.Add(newContact);
            }

            db.SaveChangesAsync();

        }
    }
}