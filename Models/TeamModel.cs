using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace srk_website.Models
{
    public class TeamModel
    {
        public TeamModel() { }
        
        public TeamModel(string firstName, string lastName, string position, string email, string phone, string linkedIn, IFormFile file)
        {
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            Email = email;
            Phone = phone;
            LinkedIn = linkedIn;
        }

        public int Id { get; set; }

        [DisplayName("First name")]
        [RegularExpression("[A-Z][a-zA-Z]+")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [DisplayName("Last name")]
        [RegularExpression("[A-Z][a-zA-Z]+")]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string Position { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [Url]
        public string LinkedIn { get; set; }

        [ValidateNever]
        public string ImageName { get; set; }

        [ValidateNever]
        public string Uri { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + ' ' + LastName;
            }
        }
    }
}
