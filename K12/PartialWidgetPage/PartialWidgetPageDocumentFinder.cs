using CMS.AspNet.Platform.Cache.Extension;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using NuGet;
using System;
using System.Linq;
using System.Net;
using System.Web;

namespace PartialWidgetPage
{
    public class PartialWidgetPageDocumentFinder : IPartialWidgetPageDocumentFinder
    {
        public int GetDocumentID(string NodeAliasPath, string SiteName, string Culture)
        {
            var DocumentNode = CacheHelper.Cache(cs =>
            {
                var Document = new DocumentQuery()
                .WhereEquals("NodeAliasPath", "/" + NodeAliasPath.Trim(new char[] { '%', '/' }))
                .OnSite(SiteName)
                .Culture(Culture)
                .CombineWithDefaultCulture()
                .CombineWithAnyCulture()
                .TopN(1)
                .Columns(nameof(TreeNode.DocumentID), nameof(TreeNode.NodeID))
                .FirstOrDefault();

                if (Document == null)
                {
                    EventLogProvider.LogEvent("W", "PartialWidgetPage", "CouldNotLocateDocument", eventDescription: $"Could not locate a document with NodeAliasPath {NodeAliasPath}, on Site {SiteName}.");
                    return null;
                }
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        $"nodeid|{Document.NodeID}",
                        $"documentid|{Document.DocumentID}"
                    });
                }

                return Document;
            }, new CacheSettings(1440, "PartialWidgetPage_GetDocumentID", NodeAliasPath, SiteName, Culture));

            if (DocumentNode == null)
            {
                return 0;
            }

            // Add dependencies to response manually
            AddCacheItemDependency($"nodeid|{DocumentNode.NodeID}");
            AddCacheItemDependency($"documentid|{DocumentNode.DocumentID}");

            return DocumentNode.DocumentID;
        }

        public int GetDocumentID(Guid NodeGuid, string Culture)
        {
            var DocumentNode = CacheHelper.Cache(cs =>
            {
                var Document = new DocumentQuery()
                .WhereEquals(nameof(TreeNode.NodeGUID), NodeGuid)
                .Culture(Culture)
                .CombineWithDefaultCulture()
                .CombineWithAnyCulture()
                .TopN(1)
                .Columns(nameof(TreeNode.DocumentID), nameof(TreeNode.NodeID))
                .FirstOrDefault();

                if (Document == null)
                {
                    EventLogProvider.LogEvent("W", "PartialWidgetPage", "CouldNotLocateDocument", eventDescription: $"Could not locate a document with NodeGuid {NodeGuid}.");
                    return null;
                }
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        $"nodeid|{Document.NodeID}",
                        $"documentid|{Document.DocumentID}"
                    });
                }

                return Document;

            }, new CacheSettings(1440, "PartialWidgetPage_GetDocumentID", NodeGuid, Culture));

            if (DocumentNode == null)
            {
                return 0;
            }

            // Add dependencies to response manually
            AddCacheItemDependency($"nodeid|{DocumentNode.NodeID}");
            AddCacheItemDependency($"documentid|{DocumentNode.DocumentID}");

            return DocumentNode.DocumentID;
        }

        public int GetDocumentID(Guid DocumentGuid)
        {
            var DocumentID = CacheHelper.Cache(cs =>
            {
                var Document = new DocumentQuery()
                .WhereEquals(nameof(TreeNode.DocumentGUID), DocumentGuid)
                .TopN(1)
                .Columns(nameof(TreeNode.DocumentID))
                .FirstOrDefault();

                if (Document == null)
                {
                    EventLogProvider.LogEvent("W", "PartialWidgetPage", "CouldNotLocateDocument", eventDescription: $"Could not locate a document with DocumentGuid {DocumentGuid}.");
                    return 0;
                }
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        $"documentid|{Document.DocumentID}"
                    });
                }

                return Document.DocumentID;

            }, new CacheSettings(1440, "PartialWidgetPage_GetDocumentID", DocumentGuid));

            // Add dependencies to response manually
            AddCacheItemDependency($"documentid|{DocumentID}");

            return DocumentID;
        }

        /// <summary>
        /// Adds the custom Cache Dependency for a view.
        /// </summary>
        /// <param name="dependencyCacheKey">The Kentico Cache Dependency Key</param>
        public static void AddCacheItemDependency(string dependencyCacheKey)
        {
            // Ensure to lower variant
            dependencyCacheKey = dependencyCacheKey.ToLowerInvariant();
            CacheHelper.EnsureDummyKey(dependencyCacheKey);
            HttpContext.Current.Response.AddCacheDependency(new CMSCacheDependency(null, new string[] { dependencyCacheKey }, DateTime.Now).CreateCacheDependency());
        }
    }
}
