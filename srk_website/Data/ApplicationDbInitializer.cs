using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using srk_website.Services;
using srk_website.Models;

namespace srk_website.Data
{
    public class ApplicationDbInitializer
    { 
        
        public static async void Initialize(ApplicationDbContext db, UserManager<IdentityUser> um, IEmailSender _emailSender)
        {
            // Delete the database before we initialize it. This is common to do during development.
            
            // Reenable this after first build
            db.Database.EnsureDeleted();

            // Recreate the database and tables according to our models
            db.Database.EnsureCreated();

            // If there are any uses in the database, we dont create a new admin.
            if (um.Users.Any())
            {
                db.SaveChanges();
                return;
            }
            
            
            // Generating random string.
            Random random = new Random();
            int length = 8;
            var rString = "";
            for (var i = 0; i < length; i++)
            {
                rString += ((char)(random.Next(1, 26) + 64)).ToString();
                rString += ((char)(random.Next(1, 26) + 96)).ToString();
                rString += ((char)(random.Next(1, 10) + 47)).ToString();
            }
            rString += "?";           
            
            // This is the admin user.
            var email = "gabri.torland@gmail.com";
            var user = new IdentityUser();
            user.UserName = email;
            user.Email = email;
            user.EmailConfirmed = true;
            // Change to rString later.
            var password = "Password1.";
            await um.CreateAsync(user,password);

            string[] addresses = { "Ulveveien 3", "Gaupeveien 35A", "Osloveien 3" };
            string[] zips = { "4323", "4873", "4532" };
            string[] cities = { "Sandnes", "Sandnes", "Oslo" };
            string[] countires = { "Norge", "Norge", "Norge" };
            string[] tlfs = { "95402323", "47232342", "95432309" };
            string[] emails = { "rfk@gmail.com", "rfk2@gmail.com", "rfk3@gmail.com" };
            for (int i = 0; i < 3; ++i)
            {
                db.Contact.Add(new ContactModel(addresses[i], zips[i], cities[i], countires[i], tlfs[i], emails[i]));
            }

            await db.SaveChangesAsync();
            
            //await _emailSender.SendEmailAsync(user.Email, "Initial password",$"<p>Here is your initial password: {password} . This password should be changed!<p>");
            

            


        }
    }
}