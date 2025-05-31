using System.ComponentModel.DataAnnotations;

namespace Babatoobin_II.Services
{
    public class MailRequest
    {
        [Required]
        [EmailAddress]
        public string? ToEmail { get; set; }
        public List<string>? CcEmails { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }
}
