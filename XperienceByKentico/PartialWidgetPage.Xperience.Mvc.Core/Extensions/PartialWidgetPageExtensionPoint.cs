namespace PartialWidgetPage;

public class PartialWidgetPageExtensionPoint
{
    internal readonly HttpContext Instance;

    public PartialWidgetPageExtensionPoint(HttpContext context)
    {
        Instance = context;
    }

    public string LayoutIfNotAjax(string layout)
    {
        if (Instance.Request.Query.ContainsKey(PARTIAL_WIDGET_AJAX_ID)) return null;

        return layout;
    }

    public string LayoutIfEditMode(string layout)
    {
        if (!Instance.Kentico().PageBuilder().EditMode) return null;

        return layout;
    }
}