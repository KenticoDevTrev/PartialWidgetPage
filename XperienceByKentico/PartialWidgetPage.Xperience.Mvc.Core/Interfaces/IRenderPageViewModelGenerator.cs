namespace PartialWidgetPage;

public interface IRenderPageViewModelGenerator
{
    public Task<RenderPageViewModel> GeneratePageViewModel(int pageId,
        PreservedPageBuilderContext preservedPageBuilderContext, CancellationToken token = default);
}