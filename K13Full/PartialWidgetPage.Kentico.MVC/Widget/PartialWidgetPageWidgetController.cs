using CMS.Core;
using CMS.DocumentEngine;
using CMS.SiteProvider;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using PartialWidgetPage;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

[assembly: RegisterWidget(PartialWidgetPageWidgetModel.IDENTITY, typeof(PartialWidgetPageWidgetController), "Partial Widget Page", Description ="Displays the given Page/Url with Widget Context", IconClass = "icon-doc-torn")]

namespace PartialWidgetPage
{
    public class PartialWidgetPageWidgetController : WidgetController<PartialWidgetPageWidgetModel>
    {

        public const string _VIEWPATH = "~/Views/Shared/Widgets/_PartialWidgetPage.Kentico.MVC.cshtml";

        public PartialWidgetPageWidgetController(IPageRetriever pageRetriever,
            IPartialWidgetPageHelper partialWidgetPageHelper,
            IPartialWidgetRenderingRetriever partialWidgetRenderingRetriever,
            IEventLogWriter eventLogWriter,
            IComponentPropertiesRetriever componentPropertiesRetriever,
            IPageDataContextRetriever pageDataContextRetriever)
        {
            PageRetriever = pageRetriever;
            PartialWidgetPageHelper = partialWidgetPageHelper;
            PartialWidgetRenderingRetriever = partialWidgetRenderingRetriever;
            EventLogWriter = eventLogWriter;
            ComponentPropertiesRetriever = componentPropertiesRetriever;
            PageDataContextRetriever = pageDataContextRetriever;
        }

        public IPageRetriever PageRetriever { get; }
        public IPartialWidgetPageHelper PartialWidgetPageHelper { get; }
        public IPartialWidgetRenderingRetriever PartialWidgetRenderingRetriever { get; }
        public IEventLogWriter EventLogWriter { get; }
        public object RenderAsPartialUrlParameter { get; private set; }
        IComponentPropertiesRetriever ComponentPropertiesRetriever { get; }
        public IPageDataContextRetriever PageDataContextRetriever { get; }

        public ActionResult Index()
        {
            ParitalWidgetPageWidgetActionViewModel model = new ParitalWidgetPageWidgetActionViewModel();
            var widgetProperties = ComponentPropertiesRetriever.Retrieve<PartialWidgetPageWidgetModel>();
            if (widgetProperties == null || (widgetProperties.Path == null && widgetProperties.Page == null && widgetProperties.CustomUrl == null))
            {
                model.Render = false;
            }
            else
            {
                model.Render = true;
                var Properties = widgetProperties;
                if (Properties.RenderMode.Equals(PartialWidgetPageWidgetModel._RenderMode_Ajax))
                {
                    model.RenderMode = PartialWidgetPageWidgetRenderMode.Ajax;
                    // Get path
                    if (!string.IsNullOrWhiteSpace(Properties.CustomUrl))
                    {
                        model.AjaxUrl = Properties.CustomUrl;
                    }
                    else
                    {
                        TreeNode Page = GetPage(Properties, false);
                        if (Page == null)
                        {
                            model.Render = false;
                            model.Error = "Could not locate Page, please check configuration";
                            EventLogWriter.WriteLog(new EventLogData(EventTypeEnum.Warning, "PartialWidgetPageWidget", "PAGENOTFOUND")
                            {
                                EventDescription = "Could not find Page from the configuration of the Parital Widget Page Widget, located on page: " + PageDataContextRetriever.Retrieve<TreeNode>().Page.NodeAlias
                            });
                        }
                        else
                        {
                            // get Relative Url
                            model.AjaxUrl = DocumentURLProvider.GetUrl(Page);
                        }
                    }
                }
                else if (Properties.RenderMode.Equals(PartialWidgetPageWidgetModel._RenderMode_Server))
                {
                    model.RenderMode = PartialWidgetPageWidgetRenderMode.ServerSide;
                    TreeNode Page = GetPage(Properties, false);
                    if (Page == null)
                    {
                        model.Render = false;
                        model.Error = "Could not locate Page, please check configuration";
                        EventLogWriter.WriteLog(new EventLogData(EventTypeEnum.Warning, "PartialWidgetPageWidget", "PAGENOTFOUND")
                        {
                            EventDescription = "Could not find Page from the configuration of the Parital Widget Page Widget, located on page: " + PageDataContextRetriever.Retrieve<TreeNode>().Page.NodeAlias
                        });
                    }
                    else
                    {
                        // get DocumentID and RenderClass
                        model.Renderer = PartialWidgetRenderingRetriever.GetRenderingControllerAction(Page.ClassName);
                        model.DocumentID = Page.DocumentID;
                    }
                } else
                {
                    model.Render = false;
                }
            }

            return View(_VIEWPATH, model);

        }

        private TreeNode GetPage(PartialWidgetPageWidgetModel Properties, bool DocumentIDAndClassOnly = false)
        {
            string Culture = !string.IsNullOrWhiteSpace(Properties.Culture) ? Properties.Culture : System.Globalization.CultureInfo.CurrentCulture.Name;
            if (Properties.PageSelectionMode.Equals(PartialWidgetPageWidgetModel._PageSelectionMode_Path) && !string.IsNullOrWhiteSpace(Properties.Path.FirstOrDefault()?.NodeAliasPath))
            {
                string Path = Properties.Path.FirstOrDefault().NodeAliasPath;
                string SiteName = !string.IsNullOrWhiteSpace(Properties.SiteName) ? Properties.SiteName : SiteContext.CurrentSiteName;

                // Convert path / page to url
                return PageRetriever.RetrieveMultiple(query =>
                {
                    query.Path(Properties.Path.FirstOrDefault().NodeAliasPath, PathTypeEnum.Single)
                    .Culture(Culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .OnSite(SiteName)
                    .Published(false);
                    if (DocumentIDAndClassOnly)
                    {
                        query.Columns(nameof(TreeNode.DocumentID), nameof(TreeNode.ClassName));
                    }
                }
                    , cache =>
                    cache.Key($"PartialWidgetPageWidget_GetPage|{Path}|{Culture}|{SiteName}|{DocumentIDAndClassOnly}")
                    .Dependencies((pages, deps) => deps.Pages(pages))
                    ).FirstOrDefault();
            }
            else if (Properties.PageSelectionMode.Equals(PartialWidgetPageWidgetModel._PageSelectionMode_ByNodeGuid) && Properties.Page.Count() > 0 && Properties.Page.First().NodeGuid != Guid.Empty)
            {
                // Convert path / page to url
                return PageRetriever.RetrieveMultiple(query =>
                {
                    query.WhereEquals(nameof(TreeNode.NodeGUID), Properties.Page.First().NodeGuid)
                    .Culture(Culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .Published(false);
                    if (DocumentIDAndClassOnly)
                    {
                        query.Columns(nameof(TreeNode.DocumentID), nameof(TreeNode.ClassName));
                    }
                }
                    , cache =>
                    cache.Key($"PartialWidgetPageWidget_GetPage|{Properties.Page.First().NodeGuid}|{Culture}|{DocumentIDAndClassOnly}")
                    .Dependencies((pages, deps) => deps.Pages(pages))
                    ).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

    }
}
