using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using srk_website.Services;
using srk_website.Models;

namespace srk_website.Data
{
    public class ApplicationDbInitializer
    {
        public async static Task Initialize(ApplicationDbContext db, UserManager<IdentityUser> um, IEmailSender _emailSender)
        {
            //db.Database.EnsureDeleted();
            //db.Database.EnsureCreated();

            // Create admins
           if (!um.Users.Any())
            {
                // Generating random string.
                Random random = new Random();
                string charPool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                var rString = new string(Enumerable.Repeat(charPool, 15).Select(s => s[random.Next(s.Length)]).ToArray());
                
                // Validate strong password
                Regex validatePassword = new Regex("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[-._@+]).{6,}$");
                while (!validatePassword.IsMatch(rString))
                {
                    rString = new string(Enumerable.Repeat(charPool, 15).Select(s => s[random.Next(s.Length)]).ToArray());
                }

                // This is the admin user.
                var user = new IdentityUser();                
                var email = "gabri.torland@gmail.com";
                user.UserName = email;
                user.Email = email;
                user.EmailConfirmed = true;
                var password = rString;

                await um.CreateAsync(user, password);

                // Send email with password
                await _emailSender.SendEmailAsync(user.Email, "Initial password", $"<p>Here is your initial password: {password} . This password should be changed!<p>");
            }

            // Create about page
            if (!db.About.Any())
            {
                var about = new AboutModel("");
                await db.About.AddAsync(about);
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
                await db.Contact.AddAsync(newContact);
            }
            
            await db.SaveChangesAsync();
        }
    }
}