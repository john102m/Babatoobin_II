using Babatoobin_II.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Website.Controllers;

namespace Babatoobin_II.Controllers
{


    public class HomeController(
        IMailService mailservice,
        UmbracoHelper umbracoHelper,
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services, AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider) : SurfaceController(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        private readonly UmbracoHelper _umbracoHelper = umbracoHelper;
        private readonly IMailService _mailService = mailservice;

        [TempData]
        public string StatusMessage { get; set; }

        public IActionResult GetHomeNodeName()
        {
            IPublishedContent? rootNode = _umbracoHelper.ContentAtRoot().FirstOrDefault();

            if (rootNode is null)
            {
                return NotFound();
            }

            return Ok(rootNode.Name);
        }
        public IActionResult GetAllNodeNames()
        {
            IEnumerable<IPublishedContent>? rootNode = _umbracoHelper.ContentAtRoot().First().Children();

            if (rootNode is null)
            {
                return NotFound();
            }
            return Ok(rootNode.Select(x => x.Name));


        }
        [HttpPost]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> HandleSubmit()
        {
            //TODO implement email service and reCaptcha
            var form = await HttpContext.Request.ReadFormAsync();
            var contentService = Services.ContentService;

            ClientMessage clientMessage = new ClientMessage(form["subject"],form["message"], form["name"], form["email"]);
            MailRequest mailRequest = new MailRequest
            {
                
                ToEmail = "john_m102uk@yahoo.co.uk",//"barbaragilchrist52@gmail.com",//
                Body = clientMessage.Message,
                Subject = form["subject"],
                Attachments = null
            };
            StatusMessage = await _mailService.SendEmailAsync(mailRequest);
            StatusMessage = StatusMessage.Contains("Requested mail action okay, completed") ? "Success! Your message has been sent" : "Warning! Message not sent,\n\r please try again later";
            //return RedirectToAction("ViewAction", new { productId = "1148" });
            return RedirectToCurrentUmbracoUrl();

        }

        public record ClientMessage(string? subject, string? message, string? name, string? email)
        {
            public string? Message => $"<!DOCTYPE html>" +
                $"<html lang=\"en\">" +
                $"<head>" +
                $"<meta charset=\"UTF-8\">" +
                $"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" +
                $"<title>Customer Enquiry</title>" +
                $"<style>body {{font-family: Arial, sans-serif;background-color: #f7f7f7;color: #333;padding: 20px;font-size: 17px;}}" +
                $"h2 {{color: #004a8f;}}" +
                $"p {{margin-bottom: 10px;}}" +
                $"strong {{color: #004a8f;}}" +
                $"</style>" +
                $"</head>" +
                $"<body>" +
                $"<h2>Customer Enquiry</h2>" +
                $"<p><strong>Sender's Name:</strong>{name}</p>" +
                $"<p><strong>Email Address:</strong>{email}</p>" +
                $"<p><strong>Subject:</strong> {subject}</p>" +
                $"<p><strong>Message:</strong><br/><br/><span style=\"color:darkslateblue;font-size: 17px\">{message}</span></p>" +
                $"</body>" +
                $"</html>";

        }
        public ActionResult ViewAction()
        {
            IPublishedContent? rootNode = _umbracoHelper.ContentAtRoot().FirstOrDefault();
            IPublishedContent? viewModel = rootNode!.DescendantsOrSelf().Where(x => x.Name == "Product Page").FirstOrDefault();
            return View("StandardPage", viewModel);
        }
    }
}
