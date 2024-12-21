using CMS.Websites.Routing;

namespace PartialWidgetPage;

[HtmlTargetElement("inlinewidgetpage", TagStructure = TagStructure.NormalOrSelfClosing)]
public class PartialWidgetPageTagHelper(IPartialWidgetPageHelper partialWidgetPageHelper, IWebsiteChannelContext channelContext, IPreferredLanguageRetriever preferredLanguageRetriever)
    : PartialWidgetPageTagHelperBase(partialWidgetPageHelper, channelContext, preferredLanguageRetriever)
{
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever = preferredLanguageRetriever;

    /// <summary>
    ///     If False, will not set the Page Context to the Node/DocumentID (if the View Component sets this). Defaults to True
    ///     where it sets it given the Page / DocumentID
    /// </summary>
    public bool InitializeDocumentPrior { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var preservedContext = PartialWidgetPageHelper.GetCurrentContext();
        
        // Render out inner content
        output.TagName = null;
        
        if (InitializeDocumentPrior)
        {
            if (WebPageId > 0)
                await PartialWidgetPageHelper.ChangeContextAsync(
                    WebPageId,
                    string.IsNullOrWhiteSpace(Language) ? _preferredLanguageRetriever.Get() : Language,
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
        PartialWidgetPageHelper.RestoreContext(preservedContext);
    }
}