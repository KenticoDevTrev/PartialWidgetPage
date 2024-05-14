namespace PartialWidgetPage;

[HtmlTargetElement("inlinewidgetpage", TagStructure = TagStructure.NormalOrSelfClosing)]
public class PartialWidgetPageTagHelper : PartialWidgetPageTagHelperBase
{
    public PartialWidgetPageTagHelper(IPartialWidgetPageHelper partialWidgetPageHelper) : base(partialWidgetPageHelper)
    {
    }

    /// <summary>
    ///     If False, will not set the Page Context to the Node/DocumentID (if the View Component sets this). Defaults to True
    ///     where it sets it given the Page / DocumentID
    /// </summary>
    public bool InitializeDocumentPrior { get; set; } = true;

    public override void Init(TagHelperContext context)
    {
        base.Init(context);

        // Preserve Context
        // Change context
        if (InitializeDocumentPrior)
        {
            if (WebPageId > 0) 
                PartialWidgetPageHelper.ChangeContext(WebPageId, Language);
        }
        else
        {
            PartialWidgetPageHelper.ChangeContext();
        }

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