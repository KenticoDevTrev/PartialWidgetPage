using Microsoft.AspNetCore.Mvc.Rendering;

namespace PartialWidgetPage;

public static class HttpContextExtensions
{
    public static PartialWidgetPageExtensionPoint PartialWidgetPage(this IHtmlHelper helper)
    {
        return new PartialWidgetPageExtensionPoint(helper.ViewContext.HttpContext);
    }

    public static PartialWidgetPageExtensionPoint PartialWidgetPage(this HttpContext context)
    {
        return new PartialWidgetPageExtensionPoint(context);
    }
}