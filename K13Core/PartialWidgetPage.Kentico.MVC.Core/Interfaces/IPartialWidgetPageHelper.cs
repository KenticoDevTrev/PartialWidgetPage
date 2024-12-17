using CMS.DocumentEngine;
using System;

namespace PartialWidgetPage
{
    public interface IPartialWidgetPageHelper
    {
        /// <summary>
        /// Gets the current Page Builder and Page Context, used to restore context after a partial widget page is rendered.
        /// </summary>
        /// <returns>The Page Builder Context and Page Context.</returns>
        PreservedPageBuilderContext GetCurrentContext();

        /// <summary>
        /// Changes the context, turning Edit Mode Off.  This should only be used if the View Component Rendering the item initializes the DocumentID itself.
        /// </summary>
        void ChangeContext();

        /// <summary>
        /// Changes the context to the given DocumentID, this also sets Edit Mode to false during this rendering.
        /// </summary>
        /// <param name="DocumentID">The Document ID</param>
        void ChangeContext(int DocumentID);

        /// <summary>
        /// Changes the context to the given Document, this also sets Edit Mode to false during this rendering.
        /// </summary>
        /// <param name="Document">The Document</param>
        void ChangeContext(TreeNode Document);

        /// <summary>
        /// Restores the Page Builder Context to the given previous context
        /// </summary>
        /// <param name="PreviousContext">The Previous Context, gotten through IPartialWidgetPageHelper.GetCurrentContext();</param>
        void RestoreContext(PreservedPageBuilderContext PreviousContext);

        /// <summary>
        /// Returns the given Layout if the the Edit Mode is true.
        /// </summary>
        /// <param name="Layout">The given Layout to return if Edit Mode</param>
        /// <returns>The Layout if it's in Edit mode, null if not (so renders as a partial)</returns>
        string LayoutIfEditMode(string Layout);

        /// <summary>
        /// Returns the given Layout if this is called from a Partial Widget Page Ajax call.
        /// </summary>
        /// <param name="Layout">The given Layout to return if it's not called from a partial widget page Ajax call</param>
        /// <returns>The Layout if it's not called from an Ajax call (so renders as a partial)</returns>
        string LayoutIfNotAjax(string Layout);


        /// <summary>
        /// Gets the URL Parameter that is appended to AJAX requests to trigger a Null layout in LayoutIfEditMode
        /// </summary>
        /// <returns>The Partial URL Query string Key</returns>
        string GetPartialUrlParameter();

        /// <summary>
        /// Gets the Document ID by the given Node Alias Path
        /// </summary>
        /// <param name="Path">The Node Alias Path</param>
        /// <param name="Culture">The Culture, if not provided uses current request's culture</param>
        /// <param name="SiteName">The Site name, if not provided uses the current site name</param>
        /// <returns>The DocumentID</returns>
        int GetDocumentIDByNode(string Path, string Culture = null, string SiteName = null);

        /// <summary>
        /// Gets the Document ID by the given Node Guid
        /// </summary>
        /// <param name="NodeGuid">The Node Guid</param>
        /// <param name="Culture">The Culture, if not provided uses current request's culture</param>
        /// <returns>The DocumentID</returns>
        int GetDocumentIDByNode(Guid NodeGuid, string Culture = null);

        /// <summary>
        /// Gets the Document ID by the given Node ID
        /// </summary>
        /// <param name="NodeID">The Node ID</param>
        /// <param name="Culture">The Culture, if not provided uses current request's culture</param>
        /// <returns>The DocumentID</returns>
        int GetDocumentIDByNode(int NodeID, string Culture = null);

        /// <summary>
        /// Gets the Document ID by the given Document Guid
        /// </summary>
        /// <param name="DocumentGuid">The Document Guid</param>
        /// <returns>The DocumentID</returns>
        int GetDocumentIDByDocument(Guid DocumentGuid);

        
    }
}
