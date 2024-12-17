namespace PartialWidgetPage;

public record RenderPageViewModel(string ViewPath, object ComponentViewModel, bool ViewExists = false);