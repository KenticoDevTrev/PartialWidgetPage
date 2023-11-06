using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PartialWidgetPage;

public class RenderPageTagHelper : TagHelper
{
    private readonly IHtmlHelper mHtmlHelper;
    private readonly IHttpContextRetriever mHttpContextRetriever;
    private readonly IRenderPageViewModelGenerator mPageViewModelGenerator;

    private readonly IPartialWidgetPageHelper mPartialWidgetPageHelper;


    public RenderPageTagHelper(IPartialWidgetPageHelper partialWidgetPageHelper,
        IHttpContextRetriever httpContextRetriever,
        IRenderPageViewModelGenerator pageViewModelGenerator, IHtmlHelper htmlHelper)
    {
        mPartialWidgetPageHelper = partialWidgetPageHelper;
        mPageViewModelGenerator = pageViewModelGenerator;
        mHtmlHelper = htmlHelper;
        mHttpContextRetriever = httpContextRetriever;
    }


    public override void Init(TagHelperContext context)
    {
        base.Init(context);

        PreservedPageBuilderContext = mPartialWidgetPageHelper.GetCurrentContext();
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (output == null)
            throw new ArgumentNullException(nameof(output));

        output.TagName = null;

        ((IViewContextAware) mHtmlHelper).Contextualize(ViewContext);

        try
        {
            mPartialWidgetPageHelper.ChangeContext(WebPageId);


            var model = await mPageViewModelGenerator.GeneratePageViewModel(WebPageId, PreservedPageBuilderContext);

            if (model.ViewExists)
                output.Content.SetHtmlContent(await mHtmlHelper.PartialAsync(model.ViewPath,
                    model.ComponentViewModel));
            else
                output.Content.SetHtmlContent(@"
                            <p style=""color: red"">
                                There was no view defined at the path @Model.ViewPath
                            </p>"
                );
        }
        catch (Exception ex)
        {
            output.SuppressOutput();
        }

        mPartialWidgetPageHelper.RestoreContext(PreservedPageBuilderContext);
    }

    #region Properties

    public int WebPageId { get; set; }

    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

    protected PreservedPageBuilderContext PreservedPageBuilderContext { get; set; }

    #endregion
}