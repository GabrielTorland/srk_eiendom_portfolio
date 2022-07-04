using Microsoft.AspNetCore.Mvc;
using srk_website.Services;
using srk_website.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace srk_website.Controllers
{
    public class EmailSenderController : Controller
    {
        private readonly IEmailSender _emailSender;
        
        private readonly string _srkEmailAddress;

        public EmailSenderController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
            _srkEmailAddress = "gabri.torland@gmail.com";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.ComponentModel.Description("Sends an email to SRK Eiendom")]
        public async Task<IActionResult> SendEmail([Bind(include: "Name,Email,Phone,Subject,Message")] EmailSenderModel emaildata)
            {
            if (emaildata.Name == null || emaildata.Email == null || emaildata.Phone == null || emaildata.Subject == null || emaildata.Message == null) 
            {
                return Problem("You forgot to send some parameters!");
            }

            if (ModelState.IsValid)
            {
                await _emailSender.SendEmailAsync(_srkEmailAddress, emaildata.Subject, 
                    "<h4>" + "Kunde" + "</h4>" 
                    + "<p>" 
                    + "Navn: " + emaildata.Name + "<br>" 
                    + "Email: " + $"<a href = 'mailto: {emaildata.Email}'>{emaildata.Email}</a>" + "<br>"
                    + "Tlf: " + $"<a href = 'tel: {emaildata.Phone}'>{emaildata.Phone}</a>" + "<br>"
                    + "</p>"
                    + "<p>"
                    + emaildata.Message
                    + "</p>"
                    );
            }

            IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);

            return Json(allErrors);
        }
    }
}
