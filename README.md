# Partial Widget Page
This tool allows you to render out a Widget (Page Builder) Page as a Partial.

## Installation
Install the Nuget Package on the MVC Site

## Usage

### Setup Partial-Viewable Content
On a Page with Kentico's Page Builder Enabled and an Editable Area existing, set the Layout from 
```
Layout = "~/Views/Shared/_Layout.cshtml";
```
 to 
```
Layout = Html.LayoutIfEditMode("~/Views/Shared/_layout.cshtml");
```

This will enable the normal view to be resolved as a partial view, depending on the context.  When visiting through Kentico, EditMode is enabled and the full layout will render, allowing you to add Widgets.

#### WARNING: INFINITE LOOPS
Do not use a Layout that renders the page you are editing in itself.  This will cause an infinite loop when editing the page.  If you editing a page that will be used in the Header and Footer, for example, please make a different Layout view that does not have the PartialWidgetPage Html Helpers.

#### Allow page to render as normal page AND Partial View
If you wish for a page to render as a full page on Live, but also still want to pull this in as a Partial in certain cases, use the following in the View:

```
Layout = Html.LayoutIfEditMode("~/Views/Shared/_layout.cshtml", "RenderAsPartial");
``` 
and call it using 
```
@Html.PartialWidgetPage("/Route/To/Page", "RenderAsPartial")
```

(or add RenderAsPartial as the `Render as Parital Url Parameter` on the Partial Widget Page widget).  The system will now pass this Url Parameter in it's Render Partial requests.

### Showing Partial Widget Pages
There are two ways and three methods ways you can render Partial widget pages:

#### Methods

| Method         | Description                                                                       | Render Location | Speed                                                                                                                      | Widget | Razor Support | Render in Edit Mode | Output Cache Dependencies                             | Notes                                                                                                             |
|----------------|-----------------------------------------------------------------------------------|-----------------|----------------------------------------------------------------------------------------------------------------------------|--------|---------------|---------------------|-------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------|
| Inline         | Switches the Page Builder context to render the partial pages in the same request | Server Side     | Fast - No different than a RenderAction                                                                                    | No     | Yes           | No                  | Shared with main request                              | Requires adding PartialWidgetPageDocumentFinder as implementation of IPartialWidgetPageDocumentFinder to your IoC |
| Server Request | Makes a server-side web request to load the page html and display                 | Server Side     | Slower - Individual web request made, although still relatively fast.                                                      | Yes    | Yes           | Yes                 | Only DocumentID / NodeID added to cache, nothing else |                                                                                                                   |
| Ajax           | Makes a client-side web request to load the page html and display                 | Client Side     | Fast-ish - Does not impact request rendering, but content is loaded client side so may be delay in seeing content to users | Yes    | Yes           | Yes                 | Independent request means it has it's own Cache       |                                                                                                                   |
#### Way 1: Widget
You can use the Partial Widget Page Widget to pull in content through a normal page builder widget.  The widget only works with **Server Request** and **Ajax** methods.

#### Way 2: In Razor View
** Server Request Method**
`@Html.PartialWidgetPage` allows you to pull in with a Server Request
You can pass either a custom Path, or a NodeAliasPath. 

** Ajax Method**
`@Html.PartialWidgetPageAjax` allows you to pull in with a client side Ajax Request (Ajax Method)
You can pass either a custom Path, or a NodeAliasPath. 

**Inline Method**
In order to do the Inline method, you first must wire up the `PartialWidgetPageDocumentFinder` as the implementation of `IPartialWidgetPageDocumentFinder`, for example using AutoFac: `builder.RegisterType<PartialWidgetPageDocumentFinder>().As<IPartialWidgetPageDocumentFinder>()`

Then you can use any one of these methods to render by NodeGuid, NodeID, or NodeAliasPath

``` csharp
@* public ActionResult RenderFromViewByDocumentGuid(Guid DocumentGuid, string ControllerName, string ActionName = "Index", int? CurrentDocumentsID = null) *@
        @{ Html.RenderAction("RenderFromViewByDocumentGuid", "PartialWidgetPage", new { DocumentGuid = new Guid("f860cfbd-5423-4769-8f9d-2d2f833d9575"), ControllerName = "PartialWidgetPageTestChild", ActionName = "Index" }); }
```
``` csharp
@* public ActionResult RenderFromViewByPath(string NodeAliasPath, string ControllerName, string ActionName = "Index", string SiteName = null, string Culture = null, int? CurrentDocumentsID = null) *@
        @{ Html.RenderAction("RenderFromViewByPath", "PartialWidgetPage", new { NodeAliasPath = "Child2", ControllerName = "PartialWidgetPageTestChild", ActionName = "Index", SiteName = "Baseline", Culture = "en-US" }); }
```
``` csharp
@* public ActionResult RenderFromViewByNodeGuid(Guid NodeGuid, string ControllerName, string ActionName = "Index", string Culture = null, int? CurrentDocumentsID = null) *@
        @{ Html.RenderAction("RenderFromViewByNodeGuid", "PartialWidgetPage", new { NodeGuid = new Guid("b8dfc82d-a6bf-4d13-a723-93020e25d386"), ControllerName = "PartialWidgetPageTestChild", ActionName = "Index", Culture = "en-US" }); }

```

These methods render the view inline.  It requires a call to an Action Method that accepts `int DocumentID` or `int? DocumentID`, as the Partial widget system will pass the documentID to that method to render.  It takes care of setting the Page Builder context and resetting it after.

Here is an example method:

``` csharp
// GET: PartialWidgetPageTestChild
        public ActionResult Index(int? DocumentID)
        {
            if (DocumentID.HasValue)
            {
                // Rendered through Partial Widget Page, Document context already set
                return View();
            }
            else
            {
                // Rendering through normal way
                ITreeNode FoundNode = _DynamicRouteHelper.GetPage();

                if (FoundNode != null)
                {
                    HttpContext.Kentico().PageBuilder().Initialize(FoundNode.DocumentID);
                    return View();
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }
```

# Contributions, bug fixes and License
Feel free to Fork and submit pull requests to contribute.

You can submit bugs through the issue list and i will get to them as soon as i can, unless you want to fix it yourself and submit a pull request!

This is free to use and modify!

# Compatability
Can be used on any Kentico 12 SP site (hotfix 29 or above).

