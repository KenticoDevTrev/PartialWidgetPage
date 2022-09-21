using CMS.Core;
using CMS.DocumentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace PartialWidgetPage.Internal
{
    public class ComponentViewModelGenerator : IComponentViewModelGenerator
    {
        private readonly IEventLogService _eventLogService;

        public ComponentViewModelGenerator(IEventLogService eventLogService)
        {
            _eventLogService = eventLogService;
        }

        public Tuple<object, bool> GetComponentViewModel(TreeNode page, object properties, Type propertyType)
        {
            try
            {
                if (page == null || propertyType == null || !propertyType.GetInterfaces().Contains(typeof(IComponentProperties)))
                {
                    return new Tuple<object, bool>(null, false);
                }

                // If null, get default instance of the property type
                if ((properties == null || properties.ToString() == "{}") && propertyType != null)
                {
                    properties = Activator.CreateInstance(propertyType);
                }

                var modelType = typeof(ComponentViewModel<>).MakeGenericType(propertyType);
                if (modelType != null && properties != null)
                {
                    // Try to convert properties into the propertyType if they do not match
                    if (properties.GetType() != propertyType)
                    {
                        // Try to get the JsonConvert.Deserialize<T> method, it will return all parameter variations so need to loop to find the "string" one.
                        try
                        {
                            var deserializeMethods = typeof(JsonConvert).GetMethods().Where(x => x.Name.Equals(nameof(JsonConvert.DeserializeObject)) && x.IsGenericMethod).ToArray();
                            bool success = false;
                            for (int i = 0; i < deserializeMethods.Length && !success; i++)
                            {
                                try
                                {
                                    properties = deserializeMethods[i].MakeGenericMethod(new Type[] { propertyType }).Invoke(null, new object[] { properties.ToString() });
                                    success = true;
                                }
                                catch (TargetParameterCountException) { } // wrong parameters
                                catch (ArgumentException) { } // constructor that doesn't take an int
                            }
                        }
                        catch (Exception)
                        {
                            // Couldn't convert
                            properties = null;
                        }

                        if (properties == null)
                        {
                            // Get default instance
                            properties = Activator.CreateInstance(propertyType);
                        }
                    }

                    var result = modelType.GetMethod(nameof(ComponentViewModel.Create))?.Invoke(null, new object[] { page, properties }) ?? null;

                    if (result != null)
                    {
                        return new Tuple<object, bool>(result, true);
                    }
                    else
                    {
                        return new Tuple<object, bool>(null, false);
                    }
                }
                else
                {
                    return new Tuple<object, bool>(null, false);
                }
            }
            catch (Exception ex)
            {
                _eventLogService.LogException("RenderPageViewComponent", "GetComponentViewModelError", ex, additionalMessage: $"For Page {page.NodeAliasPath} [{page.DocumentID}]");
                return new Tuple<object, bool>(null, false);
            }
        }
    }
}
