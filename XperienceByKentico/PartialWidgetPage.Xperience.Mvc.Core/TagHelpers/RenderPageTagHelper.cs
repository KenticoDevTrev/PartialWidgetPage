using CMS.Core;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PartialWidgetPage;

public class RenderPageTagHelper(
    IPartialWidgetPageHelper partialWidgetPageHelper,
    IEventLogService eventLogService,
    IRenderPageViewModelGenerator pageViewModelGenerator,
    IWebsiteChannelContext channelContext,
    IHtmlHelper htmlHelper)
    : PartialWidgetPageTagHelperBase(partialWidgetPageHelper, channelContext)
{
    private readonly IPartialWidgetPageHelper mPartialWidgetPageHelper = partialWidgetPageHelper;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (output == null)
            throw new ArgumentNullException(nameof(output));

        output.TagName = null;

        ((IViewContextAware) htmlHelper).Contextualize(ViewContext);

        try
        {
            await mPartialWidgetPageHelper.ChangeContextAsync(WebPageId, Language, Channel, ViewContext.HttpContext.RequestAborted);

            var model = await pageViewModelGenerator.GeneratePageViewModel(WebPageId, PreservedContext,
                ViewContext.HttpContext.RequestAborted);

            if (model.ViewExists)
                output.Content.SetHtmlContent(await htmlHelper.PartialAsync(model.ViewPath,
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
            eventLogService.LogException("PartialWidgetPage", "RENDER", ex);
            output.SuppressOutput();
        }

        mPartialWidgetPageHelper.RestoreContext(PreservedContext);
    }
}