using CMS.Base;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using System;
using System.Globalization;
using System.Web.Mvc;

namespace PartialWidgetPage
{
    public class PartialWidgetPageController : Controller
    {
        private ISiteService _SiteService;
        private IPartialWidgetPageDocumentFinder _DocFinder;

        public PartialWidgetPageController()
        {
            _SiteService = DependencyResolver.Current.GetService<ISiteService>();
            _DocFinder = DependencyResolver.Current.GetService<IPartialWidgetPageDocumentFinder>();
        }

        public PartialWidgetPageController(ISiteService SiteService, IPartialWidgetPageDocumentFinder DocFinder)
        {
            _SiteService = SiteService;
            _DocFinder = DocFinder;
        }
        /*
        /// <summary>
        /// Renders the Partial Widget Page inline from the widget's properties
        /// </summary>
        /// <param name="DocumentID"></param>
        /// <param name="NodeAliasPath"></param>
        /// <param name="ControllerName"></param>
        /// <param name="ActionName"></param>
        /// <param name="SiteName"></param>
        /// <param name="Culture"></param>
        /// <returns></returns>
        public ActionResult RenderFromWidget(int DocumentID, string NodeAliasPath, string ControllerName, string ActionName = "Index", string SiteName = null, string Culture = null)
        {
            SiteName = string.IsNullOrWhiteSpace(SiteName) ? _SiteService.CurrentSite.SiteName : SiteName;
            Culture = string.IsNullOrWhiteSpace(Culture) ? CultureInfo.CurrentUICulture.Name : Culture;
            int RequestedPageDocumentID = _DocFinder.GetDocumentID(NodeAliasPath, SiteName, Culture);

            // Not found
            if(RequestedPageDocumentID == 0)
            {
                return View("Widgets/PartialWidgetPage/_PartialWidgetPageInlineNotFound");
            }

            PartialWidgetPageInlineModel model = new PartialWidgetPageInlineModel()
            {
                RequestedPageDocumentID = RequestedPageDocumentID,
                CurrentDocumentID = DocumentID,
                ControllerName = ControllerName,
                ActionName = ActionName,
                IsEditMode = HttpContext.Kentico().PageBuilder().EditMode
            };

            return View("Widgets/PartialWidgetPage/_PartialWidgetPageInline", model);
        }
        */

        /// <summary>
        /// Renders the page's content and page builder zones with it's own document context
        /// </summary>
        /// <param name="NodeAliasPath">The page's Node Alias Path</param>
        /// <param name="ControllerName">The Controller's name (ex 'Foo' for FooController)</param>
        /// <param name="ActionName">The Action Name, Index is by default.  MUST take the properties (int DocumentID) or (int? DocumentID)</param>
        /// <param name="SiteName">The Site Name, if not provided will use the current site's name</param>
        /// <param name="Culture">The Culture, if not provided will prefer the current culture</param>
        /// <param name="CurrentDocumentsID">The current Document's ID, if not provided uses the current page builder's context</param>
        /// <returns>The rendered section</returns>
        public ActionResult RenderFromViewByPath(string NodeAliasPath, string ControllerName, string ActionName = "Index", string SiteName = null, string Culture = null, int? CurrentDocumentsID = null)
        {
            int CurrentDocumentID = (CurrentDocumentsID.HasValue ? CurrentDocumentsID.Value : System.Web.HttpContext.Current.Kentico().PageBuilder().PageIdentifier);
            SiteName = string.IsNullOrWhiteSpace(SiteName) ? _SiteService.CurrentSite.SiteName : SiteName;
            Culture = string.IsNullOrWhiteSpace(Culture) ? CultureInfo.CurrentUICulture.Name : Culture;
            int RequestedPageDocumentID = _DocFinder.GetDocumentID(NodeAliasPath, SiteName, Culture);

            PartialWidgetPageInlineModel model = new PartialWidgetPageInlineModel()
            {
                RequestedPageDocumentID = RequestedPageDocumentID,
                CurrentDocumentID = CurrentDocumentID,
                ControllerName = "PartialWidgetPageTestChild",
                ActionName = "Index",
                IsEditMode = HttpContext.Kentico().PageBuilder().EditMode
            };

            return View("Widgets/PartialWidgetPage/_PartialWidgetPageInline", model);
        }

        /// <summary>
        /// Renders the page's content and page builder zones with it's own document context
        /// </summary>
        /// <param name="NodeGuid">The Node Guid to render</param>
        /// <param name="ControllerName">The Controller's name (ex 'Foo' for FooController)</param>
        /// <param name="ActionName">The Action Name, Index is by default.  MUST take the properties (int DocumentID) or (int? DocumentID)</param>
        /// <param name="Culture">The Culture, if not provided will prefer the current culture</param>
        /// <param name="CurrentDocumentsID">The current Document's ID, if not provided uses the current page builder's context</param>
        /// <returns>The rendered section</returns>
        public ActionResult RenderFromViewByNodeGuid(Guid NodeGuid, string ControllerName, string ActionName = "Index", string Culture = null, int? CurrentDocumentsID = null)
        {
            int CurrentDocumentID = (CurrentDocumentsID.HasValue ? CurrentDocumentsID.Value : System.Web.HttpContext.Current.Kentico().PageBuilder().PageIdentifier);

            Culture = string.IsNullOrWhiteSpace(Culture) ? CultureInfo.CurrentUICulture.Name : Culture;
            int RequestedPageDocumentID = _DocFinder.GetDocumentID(NodeGuid, Culture);

            // Not found
            if (RequestedPageDocumentID == 0)
            {
                return View("Widgets/PartialWidgetPage/_PartialWidgetPageInlineNotFound");
            }

            PartialWidgetPageInlineModel model = new PartialWidgetPageInlineModel()
            {
                RequestedPageDocumentID = RequestedPageDocumentID,
                CurrentDocumentID = CurrentDocumentID,
                ControllerName = ControllerName,
                ActionName = ActionName,
                IsEditMode = HttpContext.Kentico().PageBuilder().EditMode
            };

            return View("Widgets/PartialWidgetPage/_PartialWidgetPageInline", model);
        }

        /// <summary>
        /// Renders the page's content and page builder zones with it's own document context
        /// </summary>
        /// <param name="DocumentGuid">The Document's Guid you wish to render</param>
        /// <param name="ActionName">The Action Name, Index is by default.  MUST take the properties (int DocumentID) or (int? DocumentID)</param>
        /// <param name="Culture">The Culture, if not provided will prefer the current culture</param>
        /// <param name="CurrentDocumentsID">The current Document's ID, if not provided uses the current page builder's context</param>
        /// <returns>The rendered section</returns>
        public ActionResult RenderFromViewByDocumentGuid(Guid DocumentGuid, string ControllerName, string ActionName = "Index", int? CurrentDocumentsID = null)
        {
            int CurrentDocumentID = (CurrentDocumentsID.HasValue ? CurrentDocumentsID.Value : System.Web.HttpContext.Current.Kentico().PageBuilder().PageIdentifier);
            int RequestedPageDocumentID = _DocFinder.GetDocumentID(DocumentGuid);

            // Not found
            if (RequestedPageDocumentID == 0)
            {
                return View("Widgets/PartialWidgetPage/_PartialWidgetPageInlineNotFound");
            }

            PartialWidgetPageInlineModel model = new PartialWidgetPageInlineModel()
            {
                RequestedPageDocumentID = RequestedPageDocumentID,
                CurrentDocumentID = CurrentDocumentID,
                ControllerName = ControllerName,
                ActionName = ActionName,
                IsEditMode = HttpContext.Kentico().PageBuilder().EditMode
            };

            return View("Widgets/PartialWidgetPage/_PartialWidgetPageInline", model);
        }
    }
}
