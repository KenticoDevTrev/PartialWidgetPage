using Microsoft.AspNetCore.Mvc.Rendering;

namespace PartialWidgetPage;

public static class HttpContextExtensions
{
    public static PartialWidgetPageExtensionPoint PartialWidgetPage(this IHtmlHelper helper) => new(helper.ViewContext.HttpContext);

    public static PartialWidgetPageExtensionPoint PartialWidgetPage(this HttpContext context) => new(context);
}