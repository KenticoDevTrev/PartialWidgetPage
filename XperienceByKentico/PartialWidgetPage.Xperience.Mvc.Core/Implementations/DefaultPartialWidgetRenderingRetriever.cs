namespace PartialWidgetPage;

internal class DefaultPartialWidgetRenderingRetriever : IPartialWidgetRenderingRetriever
{
    public PartialWidgetRendering? GetRenderingViewComponent(string className, int webPageId = 0)
    {
        return default;
    }
}