using Babatoobin_II.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
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
        IWebHostEnvironment env,
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
        private readonly IWebHostEnvironment _env = env;

        [TempData]
        public string? StatusMessage { get; set; }

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
            IPublishedContent? rootNode = _umbracoHelper.ContentAtRoot().FirstOrDefault();
            IEnumerable<string>? recipients = (IEnumerable<string>?)rootNode!.Value("emailRecipients"); 
            if (recipients == null) { 
                return RedirectToCurrentUmbracoUrl(); 
            }
            //TODO implement  reCaptcha
            var form = await HttpContext.Request.ReadFormAsync();
            //var contentService = Services.ContentService;


            var subject = form["subject"];
            var message = form["message"];
            var name = form["name"]; 
            var email = form["email"];

            string filePath = Path.Combine(_env.WebRootPath, "vendor", "email.html");
            var msg = System.IO.File.ReadAllText(filePath);

            msg = Regex.Replace(msg, @"\{\{Administrator}}", "Barbara");
            msg = Regex.Replace(msg, @"\{\{CustomerName}}", name);
            msg = Regex.Replace(msg, @"\{\{Subject}}", subject);
            msg = Regex.Replace(msg, @"\{\{CustomerMessage}}", message);
            msg = Regex.Replace(msg, @"\{\{CompanyName}}", "Babatoobins");
            msg = Regex.Replace(msg, @"\{\{CustomerEmail}}", email);

            MailRequest mailRequest = new MailRequest
            {
                
                ToEmail = recipients.FirstOrDefault(),//"john_m102uk@yahoo.co.uk",//"barbaragilchrist52@gmail.com",//
                Body =  msg,
                Subject = form["subject"],
                Attachments = null
            };

            StatusMessage = await _mailService.SendEmailAsync(mailRequest);
            StatusMessage = StatusMessage.Contains("Requested mail action okay, completed") ? "Success! Your message has been sent" : "Warning! Message not sent,\n\r please try again later";
            //return RedirectToAction("ViewAction", new { productId = "1148" });
            return RedirectToCurrentUmbracoUrl();

        }

        public ActionResult ViewAction()
        {
            IPublishedContent? rootNode = _umbracoHelper.ContentAtRoot().FirstOrDefault();
            IPublishedContent? viewModel = rootNode!.DescendantsOrSelf().Where(x => x.Name == "Product Page").FirstOrDefault();
            return View("StandardPage", viewModel);
        }
    }
}
