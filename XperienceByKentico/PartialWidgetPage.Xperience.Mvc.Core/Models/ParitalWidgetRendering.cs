namespace PartialWidgetPage;

public class ParitalWidgetRendering
{
    public string ViewComponentName { get; set; }

    public bool SetContextPriorToCall { get; set; } = true;

    public dynamic ViewComponentData { get; set; }
}