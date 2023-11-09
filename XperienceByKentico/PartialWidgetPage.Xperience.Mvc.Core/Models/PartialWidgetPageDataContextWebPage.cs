namespace PartialWidgetPage;

internal class PartialWidgetPageDataContextWebPage : IRoutedWebPage
{
    public PartialWidgetPageDataContextWebPage(int webPageId, string language)
    {
        WebPageItemID = webPageId;
        LanguageName = language;
    }

    public int WebPageItemID { get; }
    public string LanguageName { get; }
}