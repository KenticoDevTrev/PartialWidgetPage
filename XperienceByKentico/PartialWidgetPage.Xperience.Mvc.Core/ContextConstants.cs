namespace PartialWidgetPage;

public class ContextConstants
{
    public const string PAGE_BUILDER_DATA_CONTEXT_KEY = "Kentico.PageBuilder.DataContext";

    public const string WEB_PAGE_DATA_CONTEXT_KEY = "Kentico.Content.WebPageDataContext";
}

public class PartialWidgetPageConstants
{
    public const string PARTIAL_WIDGET_AJAX_ID = "PWP57f5689392c54854bccac791a7d69f2f";
}

public class RenderingConstants
{
    public const string RENDER_MODE_SOURCE = @$"
            {RENDER_MODE_SERVER_PAGE_BUILDER_LOGIC};Server Render (Default)
            {RENDER_MODE_SERVER};Server Render (Custom)
            {RENDER_MODE_AJAX};Ajax
        ";

    public const string PAGE_SELECTION_SOURCE = $@"
            {PAGE_SELECTION_MODE_PATH};By NodeAliasPath
            {PAGE_SELECTION_MODE_BY_NODE_GUID};By Selected Page
        ";

    public const string RENDER_MODE_SERVER = "ServerSide";
    public const string RENDER_MODE_SERVER_PAGE_BUILDER_LOGIC = "ServerSidePageBuilderLogic";
    public const string RENDER_MODE_AJAX = "Ajax";

    public const string PAGE_SELECTION_MODE_PATH = "ByPath";
    public const string PAGE_SELECTION_MODE_BY_NODE_GUID = "ByNodeGuid";
}