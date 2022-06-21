using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace srk_website.Models
{
    public class Contact
    {
        public Contact() {}

        public int Id { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Zip { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Email { get; set; }

    }
}
