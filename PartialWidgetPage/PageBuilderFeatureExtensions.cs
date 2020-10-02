using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace PartialWidgetPage
{
    public static class PageBuilderFeatureExtensions
    {
        /// <summary>
        /// Clears the current PageBuilder Data Context so context can be reset.
        /// </summary>
        /// <param name="feature"></param>
        public static void ResetPagebuilderContext(this IPageBuilderFeature feature, int PartialPageDocumentID)
        {
            var prop = feature.GetType().GetField("mDataContext", BindingFlags.NonPublic | BindingFlags.Instance);

            if (prop != null)
            {
                prop.SetValue(feature, null);
            }

            HttpContext.Current.Kentico().PageBuilder().Initialize(PartialPageDocumentID);
        }
        /*
        public static object GetCurrentConfigurationObject(this IPageBuilderFeature feature)
        {
            return HttpContext.Current.Kentico().GetFeature<IPageBuilderFeature>();
        }

        public static void SetPartialWidgetPageFeature(this IPageBuilderFeature feature, int PartialPageDocumentID)
        {
            var OriginalIPageBuilderFeature = GetOriginalPageBuilderFeature();
            // Use original one to clear mDataContext, re-initailize, and set the DataContext  
            var OriginalProp = OriginalIPageBuilderFeature.GetType().GetField("mDataContext", BindingFlags.NonPublic | BindingFlags.Instance);
            OriginalProp.SetValue(OriginalIPageBuilderFeature, null);
            OriginalIPageBuilderFeature.Initialize(PartialPageDocumentID);

            HttpContext.Current.Kentico().PageBuilder()

            HttpContext.Current.Kentico().SetFeature<IPageBuilderFeature>(NewFeature);
        }


       

        public static object GetCurrentPageBuilderFeatureContext(this IPageBuilderFeature feature)
        {
            var prop = feature.GetType().GetField("mDataContext", BindingFlags.NonPublic | BindingFlags.Instance);

            if (prop != null)
            {
                return prop.GetValue(feature);
            } else
            {
                return null;
            }
        }

        public static void SetCurrentPageBuilderFeatureContext(this IPageBuilderFeature feature, object DataContext)
        {
            var prop = feature.GetType().GetField("mDataContext", BindingFlags.NonPublic | BindingFlags.Instance);

            if (prop != null)
            {
                prop.SetValue(feature, DataContext);
            }
        }

        /*
        public static void RestoreConfigurationObject(this IPageBuilderFeature feature, object OriginalPageBuilderFeature)
        {
            HttpContext.Current.Kentico().SetFeature<IPageBuilderFeature>((IPageBuilderFeature)OriginalPageBuilderFeature);
        }
        private static IPageBuilderFeature GetOriginalPageBuilderFeature()
        {
            if (HttpContext.Current.Items["OriginalPageBuilderFeature"] == null)
            {
                HttpContext.Current.Items["OriginalPageBuilderFeature"] = HttpContext.Current.Kentico().PageBuilder();
            }
            return (IPageBuilderFeature) HttpContext.Current.Items["OriginalPageBuilderFeature"];
        }
        */
    }
}
