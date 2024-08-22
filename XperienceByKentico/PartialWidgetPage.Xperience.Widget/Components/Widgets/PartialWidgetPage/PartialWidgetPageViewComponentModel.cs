namespace PartialWidgetPage;

public class PartialWidgetPageViewComponentModel
{
    public PartialWidgetPageWidgetRenderMode RenderMode { get; set; }
    public string? AjaxUrl { get; set; }
    public int WebPageId { get; set; }
    public PartialWidgetRendering? Renderer { get; set; }
    public string? Error { get; set; }
    public string? Language { get; set; }
}