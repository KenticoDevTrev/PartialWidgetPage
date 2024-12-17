namespace PartialWidgetPage;

public interface IPartialWidgetPageHelper
{
    /// <summary>
    ///     Gets the current Page Builder and Page Context, used to restore context after a partial widget page is rendered.
    /// </summary>
    /// <returns>The Page Builder Context and Page Context.</returns>
    PreservedPageBuilderContext GetCurrentContext();

    /// <summary>
    ///     Changes the context, turning Edit Mode Off.  This should only be used if the View Component Rendering the item
    ///     initializes the DocumentID itself.
    /// </summary>
    void ChangeContext();

    
    /// <summary>
    ///     Changes the context to the given identifier, this also sets Edit Mode to false during this rendering.
    /// </summary>
    /// <param name="identifier">The identifier of the page</param>
    /// <param name="language">Then language identifier of the page</param>
    /// <param name="channel">Then website channel of the page</param>
    void ChangeContext(int identifier, string language, string channel);
    
    /// <summary>
    ///     Changes the context to the given identifier, this also sets Edit Mode to false during this rendering.
    /// </summary>
    /// <param name="identifier">The identifier of the page</param>
    /// <param name="languageName">Then language identifier of the page</param>
    /// <param name="channel">Then website channel of the page</param>
    /// <param name="token">The cancellation token for the request</param>
    Task ChangeContextAsync(int identifier, string languageName, string channel, CancellationToken token = default);

    /// <summary>
    ///     Restores the Page Builder Context to the given previous context
    /// </summary>
    /// <param name="context">
    ///     The Previous Context, gotten through <see cref="IPartialWidgetPageHelper.GetCurrentContext()" />
    /// </param>
    void RestoreContext(PreservedPageBuilderContext context);

    /// <summary>
    ///     Returns the given Layout if the the Edit Mode is true.
    /// </summary>
    /// <param name="layout">The given Layout to return if Edit Mode</param>
    /// <returns>The Layout if it's in Edit mode, null if not (so renders as a partial)</returns>
    string? LayoutIfEditMode(string layout);

    /// <summary>
    ///     Returns the given Layout if this is called from a Partial Widget Page Ajax call.
    /// </summary>
    /// <param name="layout">The given Layout to return if it's not called from a partial widget page Ajax call</param>
    /// <returns>The Layout if it's not called from an Ajax call (so renders as a partial)</returns>
    string? LayoutIfNotAjax(string layout);
}