namespace PartialWidgetPage
{
    public class PartialWidgetPageInlineModel
    {
        public int CurrentDocumentID { get; set; }
        public int RequestedPageDocumentID { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; } = "Index";
        public bool IsEditMode { get; set; }
    }
}
