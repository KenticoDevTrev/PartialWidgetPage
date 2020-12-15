using CMS.Base.Internal;
using CMS.DocumentEngine;
using CMS.SiteProvider;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;
using Kentico.Web.Mvc;
using PartialWidgetPage;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

public static class PartialWidgetPageExtensions
{

    public static string LayoutIfEditMode(this HtmlHelper helper, string Layout)
    {
        if (!HttpContext.Current.Kentico().PageBuilder().EditMode)
        {
            return null;
        }
        return Layout;
    }

    public static PreservedPageBuilderContext GetCurrentContext(this HtmlHelper helper)
    {
        IPartialWidgetPageHelper Helper = DependencyResolver.Current.GetService<IPartialWidgetPageHelper>();
        return Helper.GetCurrentContext();
    }

    public static string GetPartialUrlParameter(this HtmlHelper helper)
    {
        IPartialWidgetPageHelper Helper = DependencyResolver.Current.GetService<IPartialWidgetPageHelper>();
        return Helper.GetPartialUrlParameter();
    }

    public static void ChangeContext(this HtmlHelper helper)
    {
        IHttpContextRetriever httpContextRetriever = DependencyResolver.Current.GetService<IHttpContextRetriever>();
        httpContextRetriever.GetContext().Items["Kentico.PageBuilder.DataContext"] = new PageBuilderDataContext()
        {
            Options = HttpContext.Current.Kentico().PageBuilder().Options,
            EditMode = false
        };
        httpContextRetriever.GetContext().Items["Kentico.Content.PageDataContext"] = null;
    }

    public static void ChangeContext(this HtmlHelper helper, int DocumentID)
    {
        IHttpContextRetriever httpContextRetriever = DependencyResolver.Current.GetService<IHttpContextRetriever>();
        IPageDataContextInitializer pageDataContextInitializer = DependencyResolver.Current.GetService<IPageDataContextInitializer>();
        httpContextRetriever.GetContext().Items["Kentico.PageBuilder.DataContext"] = new PageBuilderDataContext()
        {
            Options = HttpContext.Current.Kentico().PageBuilder().Options,
            EditMode = false
        };
        pageDataContextInitializer.Initialize(DocumentID);
    }

    public static void ChangeContext(this HtmlHelper helper, TreeNode Document)
    {
        IHttpContextRetriever httpContextRetriever = DependencyResolver.Current.GetService<IHttpContextRetriever>();
        IPageDataContextInitializer pageDataContextInitializer = DependencyResolver.Current.GetService<IPageDataContextInitializer>();
        httpContextRetriever.GetContext().Items["Kentico.PageBuilder.DataContext"] = new PageBuilderDataContext()
        {
            Options = HttpContext.Current.Kentico().PageBuilder().Options,
            EditMode = false
        };
        pageDataContextInitializer.Initialize(Document);
    }

    public static void RestoreContext(this HtmlHelper helper, PreservedPageBuilderContext PreviousContext)
    {
        // Restore 
        IHttpContextRetriever httpContextRetriever = DependencyResolver.Current.GetService<IHttpContextRetriever>();
        httpContextRetriever.GetContext().Items["Kentico.PageBuilder.DataContext"] = PreviousContext.PageBuilderContext;
        httpContextRetriever.GetContext().Items["Kentico.Content.PageDataContext"] = PreviousContext.Page;
    }

    public static int GetDocumentIDByNode(this HtmlHelper helper, string Path, string Culture = null, string SiteName = null)
    {
        IPageRetriever pageRetriever = DependencyResolver.Current.GetService<IPageRetriever>();
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

    public static int GetDocumentIDByNode(this HtmlHelper helper, Guid NodeGuid, string Culture = null)
    {
        IPageRetriever pageRetriever = DependencyResolver.Current.GetService<IPageRetriever>();
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

    public static int GetDocumentIDByNode(this HtmlHelper helper, int NodeID, string Culture = null)
    {
        IPageRetriever pageRetriever = DependencyResolver.Current.GetService<IPageRetriever>();
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

    public static int GetDocumentIDByDocument(this HtmlHelper helper, Guid DocumentGuid)
    {
        IPageRetriever pageRetriever = DependencyResolver.Current.GetService<IPageRetriever>();
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
