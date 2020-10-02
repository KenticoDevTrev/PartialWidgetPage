using System;

namespace PartialWidgetPage
{
    public interface IPartialWidgetPageDocumentFinder
    {
        /// <summary>
        /// Returns the DocumentID
        /// </summary>
        /// <param name="NodeAliasPath">The Node Alias Path of the page</param>
        /// <param name="SiteName">Site Name</param>
        /// <param name="Culture">Culture</param>
        /// <returns>DocumentID, 0 if not found</returns>
        int GetDocumentID(string NodeAliasPath, string SiteName, string Culture);

        /// <summary>
        /// Returns the DocumentID
        /// </summary>
        /// <param name="NodeGuid">The Node Guid</param>
        /// <param name="Culture">The Culture to select</param>
        /// <returns>DocumentID, 0 if not found</returns>
        int GetDocumentID(Guid NodeGuid, string Culture);

        /// <summary>
        /// Returns the DocumentID
        /// </summary>
        /// <param name="DocumentGuid">The Document Guid</param>
        /// <returns>DocumentID, 0 if not found</returns>
        int GetDocumentID(Guid DocumentGuid);
    }
}
