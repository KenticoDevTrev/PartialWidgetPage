namespace PartialWidgetPage
{
    /// <summary>
    /// Model for the Partial Widget Page's Widget View Component Rendering
    /// </summary>
    public class PartialWidgetPageWidgetViewComponentModel
    {
        public PartialWidgetPageWidgetRenderMode RenderMode { get; internal set; }
        public string AjaxUrl { get; internal set; }
        public bool Render { get; internal set; }
        public int DocumentID { get; internal set; }
        public PartialWidgetRendering Renderer { get; internal set; }
        public string Error { get; internal set; }
    }
}
