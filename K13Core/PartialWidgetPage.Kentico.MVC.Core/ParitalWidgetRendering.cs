namespace PartialWidgetPage
{
    /// <summary>
    /// Represents the data needed in order to render an Inline Widget
    /// </summary>
    public class PartialWidgetRendering
    {
        public string ViewComponentName { get; set; }

        public bool SetContextPriorToCall { get; set; } = true;

        public dynamic ViewComponentData { get; set; }
    }
}
