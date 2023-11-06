namespace PartialWidgetPage;

public class PartialWidgetPageViewComponentModel
{
    public PartialWidgetPageWidgetRenderMode RenderMode { get; set; }
    public string AjaxUrl { get; set; }
    public bool Render { get; set; } = true;
    public int WebPageId { get; set; }
    public ParitalWidgetRendering Renderer { get; set; }
    public string Error { get; set; }
    public string Language { get; set; }
}