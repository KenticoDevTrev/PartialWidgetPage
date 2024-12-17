namespace PartialWidgetPage;

public record PreservedPageBuilderContext(
    IPageBuilderFeature PageBuilderFeature,
    IPageBuilderDataContext PageBuilderContext,
    RoutedWebPage? Page);

