namespace PartialWidgetPage
{
    /// <summary>
    /// Represents the data needed in order to render an Inline Widget
    /// </summary>
    public class ParitalWidgetRendering
    {
        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public bool SetContextPriorToCall { get; set; } = true;

        public object RouteValues { get; set; }
    }
}
