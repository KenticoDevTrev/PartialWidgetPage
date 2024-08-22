namespace PartialWidgetPage;

public class PartialWidgetPageExtensionPoint(HttpContext context)
{
    public string? LayoutIfNotAjax(string layout) => 
        context.Request.Query.ContainsKey(PARTIAL_WIDGET_AJAX_ID) ? null : layout;

    public string? LayoutIfEditMode(string layout) => 
        !context.Kentico().PageBuilder().EditMode ? null : layout;
}