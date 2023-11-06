using Microsoft.AspNetCore.Mvc;

namespace PartialWidgetPage;

[ViewComponent]
public class PartialWidgetPageViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(PartialWidgetPageViewComponentModel model)
    {
        return View("~/ViewComponents/PartialWidgetPage/Default.cshtml", model);
    }
}