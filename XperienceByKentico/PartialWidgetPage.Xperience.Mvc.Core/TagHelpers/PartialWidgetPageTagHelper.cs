namespace PartialWidgetPage;

[HtmlTargetElement("inlinewidgetpage", TagStructure = TagStructure.NormalOrSelfClosing)]
public class PartialWidgetPageTagHelper : TagHelper
{
    public PartialWidgetPageTagHelper(IPartialWidgetPageHelper partialWidgetPageHelper)
    {
        PartialWidgetPageHelper = partialWidgetPageHelper;
    }

    /// <summary>
    ///     If False, will not set the Page Context to the Node/DocumentID (if the View Component sets this). Defaults to True
    ///     where it sets it given the Page / DocumentID
    /// </summary>
    public bool InitializeDocumentPrior { get; set; } = true;

    public int WebPageId { get; set; }

    private IPartialWidgetPageHelper PartialWidgetPageHelper { get; }
    private PreservedPageBuilderContext PreservedContext { get; set; }

    public override void Init(TagHelperContext context)
    {
        PreservedContext = PartialWidgetPageHelper.GetCurrentContext();

        // Preserve Context
        // Change context
        if (InitializeDocumentPrior)
        {
            if (WebPageId > 0) 
                PartialWidgetPageHelper.ChangeContext(WebPageId);
        }
        else
        {
            PartialWidgetPageHelper.ChangeContext();
        }

        base.Init(context);
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Render out inner content
        output.TagName = null;
        var content = output.Content.GetContent();
        output.Content.SetHtmlContent(content);
        // restore previous context
        PartialWidgetPageHelper.RestoreContext(PreservedContext);
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Render out inner content
        output.TagName = null;
        var content = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(content.GetContent());
        // restore previous context
        PartialWidgetPageHelper.RestoreContext(PreservedContext);
    }
}