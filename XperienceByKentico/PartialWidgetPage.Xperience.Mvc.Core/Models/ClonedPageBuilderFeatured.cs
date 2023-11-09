using CMS.Helpers.Internal;

namespace PartialWidgetPage;

internal class ClonedPageBuilderFeature : IPageBuilderFeature
{
    public ClonedPageBuilderFeature(IHttpContext context, PageBuilderDataContext pageBuilderDataContext)
    {
        if (VirtualContext.IsPreviewLinkInitialized)
        {
            Guid result;

            if (Guid.TryParse((string) context.Request.QueryString["instance"], out result))
                EditingInstanceIdentifier = result;
        }

        EditMode = pageBuilderDataContext.EditMode;
        Options = pageBuilderDataContext.Options;
    }

    public Guid EditingInstanceIdentifier { get; }
    public bool EditMode { get; }
    public PageBuilderOptions Options { get; set; }
}