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
using static Umbraco.Cms.Core.Collections.TopoGraph;

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
            IEnumerable<IPublishedContent>? rootNodeChildren = _umbracoHelper.ContentAtRoot().First().Children();

            if (rootNodeChildren is null)
            {
                return NotFound();
            }
            return Ok(rootNodeChildren.Select(x => x.Name));
        }

        [HttpPost]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> HandleEmail()
        {
            var form = await HttpContext.Request.ReadFormAsync();
            StatusMessage = await SendMail(form, "response_email.html");
            StatusMessage = StatusMessage.Contains("Requested mail action okay, completed") ? "Your message has been sent" : $"{StatusMessage}";
            IPublishedContent? homePage = _umbracoHelper.ContentAtRoot().FirstOrDefault(); 
            return RedirectToUmbracoPage(homePage!.Key);
        }

        [HttpPost]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> HandleSubmit()
        {
            IPublishedContent? rootNode = _umbracoHelper.ContentAtRoot().FirstOrDefault();
            IEnumerable<string>? recipients = (IEnumerable<string>?)rootNode!.Value("emailRecipients");
            if (recipients == null)
            { 
                return RedirectToCurrentUmbracoUrl(); 
            }
            var ccRecipients = recipients.Count() > 1 ? recipients.SkipWhile(x => x == recipients.FirstOrDefault()).ToList() : null;

            //TODO implement  reCaptcha
            var form = await HttpContext.Request.ReadFormAsync();
            StatusMessage = await SendMail(form, "email.html", recipients.FirstOrDefault(), ccRecipients);
            StatusMessage = StatusMessage.Contains("Requested mail action okay, completed") ? "Your message has been sent" : $"{StatusMessage}";
            return RedirectToCurrentUmbracoUrl();

        }

        private async Task<string> SendMail(IFormCollection form,  string template, string? recipient = null, List<string>? ccEmails = null)
        {
            var subject = form["subject"];
            var message = form["message"];
            var customerName = form["name"];
            var email = form["email"];

            string filePath = Path.Combine(_env.WebRootPath, "vendor", template);
            var msg = System.IO.File.ReadAllText(filePath);

            msg = Regex.Replace(msg, @"\{\{Administrator}}", "Barbara");
            msg = Regex.Replace(msg, @"\{\{CustomerName}}", customerName!);
            msg = Regex.Replace(msg, @"\{\{Subject}}", subject!);
            msg = Regex.Replace(msg, @"\{\{CustomerMessage}}", message!);
            msg = Regex.Replace(msg, @"\{\{CompanyName}}", "Babatoobins");
            msg = Regex.Replace(msg, @"\{\{CustomerEmail}}", email!);
            msg = Regex.Replace(msg, @"\{\{AdministratorMessage}}", message!);

            MailRequest mailRequest = new MailRequest
            {
                ToEmail = recipient ?? email,
                CcEmails = ccEmails,
                Body = msg,
                Subject = form["subject"],
                Attachments = null
            };

            return await _mailService.SendEmailAsync(mailRequest);
        }

        public ActionResult ViewAction()
        {
            IPublishedContent? rootNode = _umbracoHelper.ContentAtRoot().FirstOrDefault();
            IPublishedContent? viewModel = rootNode!.DescendantsOrSelf().Where(x => x.Name == "Product Page").FirstOrDefault();
            return View("StandardPage", viewModel);
        }
    }
}
