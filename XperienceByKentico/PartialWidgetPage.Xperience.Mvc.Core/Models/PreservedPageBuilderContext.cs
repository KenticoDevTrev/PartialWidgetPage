namespace PartialWidgetPage;

public class PreservedPageBuilderContext
{
    public IPageBuilderFeature PageBuilderFeature { get; set; }
    public IPageBuilderDataContext PageBuilderContext { get; set; }
    public IRoutedWebPage Page { get; set; }
}