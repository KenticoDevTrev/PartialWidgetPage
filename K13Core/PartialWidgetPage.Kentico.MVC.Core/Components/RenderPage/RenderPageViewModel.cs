namespace PartialWidgetPage
{
    public class RenderPageViewModel
    {
        public PreservedPageBuilderContext PreservedContext { get; set; }
        public string ViewPath { get; set; }
        public object ComponentViewModel { get; set; }
    }
}