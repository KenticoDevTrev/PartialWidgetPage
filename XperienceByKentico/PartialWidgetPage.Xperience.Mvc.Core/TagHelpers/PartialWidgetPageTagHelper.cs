using CMS.Websites.Routing;

namespace PartialWidgetPage;

[HtmlTargetElement("inlinewidgetpage", TagStructure = TagStructure.NormalOrSelfClosing)]
public class PartialWidgetPageTagHelper(IPartialWidgetPageHelper partialWidgetPageHelper, IWebsiteChannelContext channelContext)
    : PartialWidgetPageTagHelperBase(partialWidgetPageHelper, channelContext)
{
    /// <summary>
    ///     If False, will not set the Page Context to the Node/DocumentID (if the View Component sets this). Defaults to True
    ///     where it sets it given the Page / DocumentID
    /// </summary>
    public bool InitializeDocumentPrior { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Render out inner content
        output.TagName = null;

        if (InitializeDocumentPrior)
        {
            if (WebPageId > 0)
                await PartialWidgetPageHelper.ChangeContextAsync(
                    WebPageId,
                    Language,
                    Channel,
                    ViewContext.HttpContext.RequestAborted
                );
        }
        else
        {
            PartialWidgetPageHelper.ChangeContext();
        }
        
        var content = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(content.GetContent());

        
        // restore previous context
        PartialWidgetPageHelper.RestoreContext(PreservedContext);
    }
}