# Partial Widget Page
This tool allows you to render out a widget (Page Builder) page as a partial.

## Installation
Install the `PartialWidgetPage.Kentico.MVC` NuGet package on the MVC site.

## Usage

### Setup Partial-Viewable Content
On a page with Kentico's Page Builder enabled and an editable area existing, set the layout from 
```
Layout = "~/Views/Shared/_Layout.cshtml";
```
 to 
```
Layout = Html.LayoutIfEditMode("~/Views/Shared/_layout.cshtml");
```

This will enable the normal view to be resolved as a partial view, depending on the context. When visiting through Kentico, EditMode is enabled and the full layout will render, allowing you to add Widgets.

#### WARNING: INFINITE LOOPS
Do not use a layout that renders the page you are editing in itself. This will cause an infinite loop when editing the page. If you editing a page that will be used in the header and footer, for example, please make a different layout view that does not have the PartialWidgetPage HTML helpers.

#### Allow page to render as normal page AND partial view
If you wish for a page to render as a full page on live, but also still want to pull this in as a partial in certain cases, use the following in the view:

```
Layout = Html.LayoutIfEditMode("~/Views/Shared/_layout.cshtml", "RenderAsPartial");
``` 
and call it using 
```
@Html.PartialWidgetPage("/Route/To/Page", "RenderAsPartial")
```
(or add RenderAsPartial as the `Render as partial URL parameter` on the partial widget page widget). The system will now pass this Url Parameter in its render partial requests.

### Showing partial widget pages
On your other pages, you can either use the 
`@Html.PartialWidgetPage` / `@Html.PartialWidgetPageAjax` HTML helpers to pull in the content, or use the partial Widget page widget in a widget zone.

You can pass either a custom path, or a NodeAliasPath. 

The normal method will perform a client side `DownloadString(Url)` on the page and return the content, this is done syncly so the content is there upon page load.

The AJAX method is also available which will pull in the content through an AJAX call client side.

# Contributions, bug fixes and license
Feel free to fork and submit pull requests to contribute.

You can submit bugs through the issue list and I will get to them as soon as i can, unless you want to fix it yourself and submit a pull request!

This is free to use and modify!

# Compatability
Can be used on any Kentico 12 SP site (hotfix 29 or above).
