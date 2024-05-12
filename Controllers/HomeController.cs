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
        UmbracoHelper umbracoHelper,
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services, AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider) : SurfaceController(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        private readonly UmbracoHelper _umbracoHelper = umbracoHelper;

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

            return RedirectToAction("ViewAction", new { productId = "1148" });
            //return RedirectToCurrentUmbracoUrl();

        }

        public ActionResult ViewAction()
        {
            IPublishedContent? rootNode = _umbracoHelper.ContentAtRoot().FirstOrDefault();
            IPublishedContent? viewModel = rootNode!.DescendantsOrSelf().Where(x => x.Name == "Product Page").FirstOrDefault();
            return View("StandardPage", viewModel);
        }
    }
}
