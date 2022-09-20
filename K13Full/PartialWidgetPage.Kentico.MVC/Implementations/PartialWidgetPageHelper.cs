using CMS.Base.Internal;
using CMS.DocumentEngine;
using CMS.SiteProvider;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;
using Kentico.Web.Mvc;
using System;
using System.Linq;
using System.Web;

namespace PartialWidgetPage
{
    public class PartialWidgetPageHelper : IPartialWidgetPageHelper
    {
        private IPageDataContextInitializer pageDataContextInitializer;
        private readonly IHttpContextRetriever httpContextRetriever;
        private readonly IPageRetriever pageRetriever;
        private readonly IPageDataContextRetriever pageDataContextRetriever;
        private readonly IPageBuilderDataContextRetriever pageBuilderDataContextRetriever;

        public PartialWidgetPageHelper(IPageDataContextInitializer pageDataContextInitializer,
            IHttpContextRetriever httpContextRetriever,
            IPageRetriever pageRetriever,
            IPageDataContextRetriever pageDataContextRetriever,
            IPageBuilderDataContextRetriever pageBuilderDataContextRetriever)
        {
            this.pageDataContextInitializer = pageDataContextInitializer;
            this.httpContextRetriever = httpContextRetriever;
            this.pageRetriever = pageRetriever;
            this.pageDataContextRetriever = pageDataContextRetriever;
            this.pageBuilderDataContextRetriever = pageBuilderDataContextRetriever;
        }

        public string GetPartialUrlParameter()
        {
            return "PWP57f5689392c54854bccac791a7d69f2f";
        }

        public PreservedPageBuilderContext GetCurrentContext()
        {
            IPageBuilderDataContext PageBuilderContext = pageBuilderDataContextRetriever.Retrieve();
            TreeNode Page = null;
            if(pageDataContextRetriever.TryRetrieve(out IPageDataContext<TreeNode> data))
            {
                Page = data.Page;
            }
            return new PreservedPageBuilderContext()
            {
                PageBuilderContext = PageBuilderContext,
                Page = Page,
            };
        }

        public void ChangeContext()
        {
            httpContextRetriever.GetContext().Items["Kentico.PageBuilder.DataContext"] = new PageBuilderDataContext()
            {
                Options = HttpContext.Current.Kentico().PageBuilder().Options,
                EditMode = false
            };
            httpContextRetriever.GetContext().Items["Kentico.Content.PageDataContext"] = null;
        }

        public void ChangeContext(int DocumentID)
        {
            httpContextRetriever.GetContext().Items["Kentico.PageBuilder.DataContext"] = new PageBuilderDataContext()
            {
                Options = HttpContext.Current.Kentico().PageBuilder().Options,
                EditMode = false
            };
            pageDataContextInitializer.Initialize(DocumentID);
        }

        public void ChangeContext(TreeNode Document)
        {
            httpContextRetriever.GetContext().Items["Kentico.PageBuilder.DataContext"] = new PageBuilderDataContext()
            {
                Options = HttpContext.Current.Kentico().PageBuilder().Options,
                EditMode = false
            };
            pageDataContextInitializer.Initialize(Document);
        }

        public void RestoreContext(PreservedPageBuilderContext PreviousContext)
        {
            // Restore 
            httpContextRetriever.GetContext().Items["Kentico.PageBuilder.DataContext"] = PreviousContext.PageBuilderContext;
            httpContextRetriever.GetContext().Items["Kentico.Content.PageDataContext"] = PreviousContext.Page;

            // The PageBuilderFeature in the Featureset gets changed, thus killing EditMode, must add our own to have the previous context's edit mode.
            ((IFeatureSet)httpContextRetriever.GetContext().Items["Kentico.Features"]).SetFeature<IPageBuilderFeature>(new ClonePageBuilderFeature(pageDataContextInitializer)
            {
                EditMode = PreviousContext.PageBuilderContext.EditMode,
                Options = PreviousContext.PageBuilderContext.Options,
                PageIdentifier = PreviousContext.Page != null ? PreviousContext.Page.DocumentID : 0
            });
        }

        public string LayoutIfEditMode(string Layout)
        {
            if (HttpContext.Current.Request.QueryString.AllKeys.Contains(GetPartialUrlParameter()) || !HttpContext.Current.Kentico().PageBuilder().EditMode)
            {
                return null;
            }
            return Layout;
        }

        public int GetDocumentIDByNode(string Path, string Culture = null, string SiteName = null)
        {
            Culture = !string.IsNullOrWhiteSpace(Culture) ? Culture : System.Globalization.CultureInfo.CurrentCulture.Name;
            SiteName = !string.IsNullOrWhiteSpace(SiteName) ? SiteName : SiteContext.CurrentSiteName;
            var Page =
                pageRetriever.RetrieveMultiple(query =>
                    query.Path(Path, PathTypeEnum.Single)
                    .Culture(Culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .OnSite(SiteName)
                    .Published(false)
                    , cache =>
                    cache.Key($"GetDocumentIDByNode|{Path}|{Culture}|{SiteName}")
                    .Dependencies((pages, deps) => deps.Pages(pages))
                    ).FirstOrDefault();
            return Page != null ? Page.DocumentID : 0;
        }

        public int GetDocumentIDByNode(Guid NodeGuid, string Culture = null)
        {
            Culture = !string.IsNullOrWhiteSpace(Culture) ? Culture : System.Globalization.CultureInfo.CurrentCulture.Name;
            var Page =
                pageRetriever.RetrieveMultiple(query =>
                    query.WhereEquals(nameof(TreeNode.NodeGUID), NodeGuid)
                    .Culture(Culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .Published(false)
                    .Columns(nameof(TreeNode.DocumentID))
                    , cache =>
                    cache.Key($"GetDocumentIDByNode|{NodeGuid}|{Culture}")
                    .Dependencies((pages, deps) => deps.Pages(pages))
                    ).FirstOrDefault();
            return Page != null ? Page.DocumentID : 0;
        }

        public int GetDocumentIDByNode(int NodeID, string Culture = null)
        {
            var Page =
                pageRetriever.RetrieveMultiple(query =>
                    query.WhereEquals(nameof(TreeNode.NodeID), NodeID)
                    .Culture(Culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .Published(false)
                    .Columns(nameof(TreeNode.DocumentID))
                    , cache =>
                    cache.Key($"GetDocumentIDByNode|{NodeID}|{Culture}")
                    .Dependencies((pages, deps) => deps.Pages(pages))
                    ).FirstOrDefault();
            return Page != null ? Page.DocumentID : 0;
        }

        public int GetDocumentIDByDocument(Guid DocumentGuid)
        {
            var Page =
                pageRetriever.RetrieveMultiple(query =>
                    query.WhereEquals(nameof(TreeNode.DocumentGUID), DocumentGuid)
                    .Published(false)
                    .Columns(nameof(TreeNode.DocumentID))
                    , cache =>
                    cache.Key($"GetDocumentIDByDocument|{DocumentGuid}")
                    .Dependencies((pages, deps) => deps.Pages(pages))
                    ).FirstOrDefault();
            return Page != null ? Page.DocumentID : 0;
        }
    }
}
