using CMS.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PartialWidgetPage;

public class RenderPageTagHelper : PartialWidgetPageTagHelperBase
{
    private readonly IEventLogService mEventLogService;
    private readonly IHtmlHelper mHtmlHelper;
    private readonly IRenderPageViewModelGenerator mPageViewModelGenerator;

    private readonly IPartialWidgetPageHelper mPartialWidgetPageHelper;

    public RenderPageTagHelper(IPartialWidgetPageHelper partialWidgetPageHelper,
        IEventLogService eventLogService,
        IRenderPageViewModelGenerator pageViewModelGenerator,
        IHtmlHelper htmlHelper) : base(partialWidgetPageHelper)
    {
        mPartialWidgetPageHelper = partialWidgetPageHelper;
        mPageViewModelGenerator = pageViewModelGenerator;
        mHtmlHelper = htmlHelper;
        mEventLogService = eventLogService;
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
            mPartialWidgetPageHelper.ChangeContext(WebPageId, Language);

            var model = await mPageViewModelGenerator.GeneratePageViewModel(WebPageId, PreservedContext,
                ViewContext.HttpContext.RequestAborted);

            if (model.ViewExists)
                output.Content.SetHtmlContent(await mHtmlHelper.PartialAsync(model.ViewPath,
                    model.ComponentViewModel));
            else
                output.Content.SetHtmlContent(@$"
                            <p style=""color: red"">
                                There was no view defined at the path {model.ViewPath}
                            </p>"
                );
        }
        catch (Exception ex)
        {
            mEventLogService.LogException("PartialWidgetPage", "RENDER", ex);
            output.SuppressOutput();
        }

        mPartialWidgetPageHelper.RestoreContext(PreservedContext);
    }
}