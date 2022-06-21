using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace srk_website.Models
{
    public class Contact
    {
        public Contact(){}
        public Contact(string address, string zip, string city, string country, string phone, string email)
        {
            Address = address;
            Zip = zip;
            City = city;
            Country = country;
            Phone = phone;
            Email = email;
        }

        public int Id { get; set; }
        
        [Required]
        public string Address { get; set; }

        [Required]
        [StringLength(4)]
        public string Zip { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        [StringLength(8)]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
