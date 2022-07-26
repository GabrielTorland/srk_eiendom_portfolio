﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace srk_website.Models
{
    public class ContactModel
    {
        public ContactModel(){}
        public ContactModel(string address, string zip, string city, string country, string phone, string email)
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
        [StringLength(100)]        
        public string? Address { get; set; }

        [Required]
        [StringLength(4)]
        public string? Zip { get; set; }

        [Required]
        [StringLength(40)]
        public string? City { get; set; }

        [Required]
        [StringLength(40)]
        public string? Country { get; set; }

        [Required]
        [Phone]
        public string? Phone { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
