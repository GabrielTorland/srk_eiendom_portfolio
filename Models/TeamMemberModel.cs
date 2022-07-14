using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace srk_website.Models
{
    public class TeamMemberModel
    {
        public TeamMemberModel() { }

        public TeamMemberModel(string firstName, string lastName, string position, string email, string phone, string linkedIn, string imageName, string uri)
        {
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            Email = email;
            Phone = phone;
            LinkedIn = linkedIn;
            ImageName = imageName;
            Uri = uri;
        }

        public int Id { get; set; }

        [DisplayName("First name")]
        [StringLength(50)]
        [Required]
        public string? FirstName { get; set; }

        [DisplayName("Last name")]
        [StringLength(50)]
        [Required]
        public string? LastName { get; set; }

        [StringLength(50)]
        [Required]
        public string? Position { get; set; }

        [EmailAddress]
        [Required]
        public string? Email { get; set; }

        [Phone]
        [Required]
        public string? Phone { get; set; }

        [Url]
        [Required]
        public string? LinkedIn { get; set; }

        [ValidateNever]
        public string? ImageName { get; set; }

        [ValidateNever]
        public string? Uri { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + ' ' + LastName;
            }
        }
    }
}
