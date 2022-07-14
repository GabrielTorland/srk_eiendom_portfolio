using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace srk_website.Models
{
    public class TestimonialModel
    {
        public TestimonialModel() { }

        public TestimonialModel(string firstName, string lastName, string project, string position, string testimonial)
        {
            FirstName = firstName;
            LastName = lastName;
            Project = project;
            Position = position;
            Testimonial = testimonial;
        }
        
        public int Id { get; set; }

        [Required]
        [DisplayName("First name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [DisplayName("Last name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? Project { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? Position { get; set; }

        [Required]
        public string? Testimonial { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + ' ' + LastName;
            }
        }


    }
}
