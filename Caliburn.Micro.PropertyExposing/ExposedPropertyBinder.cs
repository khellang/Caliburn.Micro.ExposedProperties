using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace Caliburn.Micro.PropertyExposing
{
    public static class ExposedPropertyBinder
    {
        private static readonly ILog Log = LogManager.GetLog(typeof(ExposedPropertyBinder));

        public static readonly List<FrameworkElement> UnhandledElements = new List<FrameworkElement>();

        public static IEnumerable<FrameworkElement> BindProperties(IEnumerable<FrameworkElement> elements, Type viewModelType)
        {
            UnhandledElements.Clear();

            foreach (var element in elements)
            {
                var cleanName = element.Name.Trim('_');
                if (string.IsNullOrEmpty(cleanName))
                {
                    SkipElement(element, "Element {0} did not match a property.", element.Name);
                    continue;
                }

                // Split name in parts
                var nameParts = cleanName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

                // Get first exposed property
                var exposedProperty = GetExposedProperty(viewModelType, nameParts);
                if (exposedProperty == null)
                {
                    SkipElement(element, "Element {0} did not match a property.", element.Name);
                    continue;
                }

                // Get convetion for element type
                var convention = ConventionManager.GetElementConvention(element.GetType());
                if (convention == null)
                {
                    SkipElement(element, "No conventions configured for {0}.", element.GetType());
                    continue;
                }

                // Apply convention binding to element
                var applied = convention.Bind(exposedProperty, element);
                if (!applied)
                {
                    SkipElement(element, "Element {0} has existing binding.", element.Name);
                    continue;
                }

                Log.Info("Binding Convention Applied: Element {0}.", element.Name);
            }

            return UnhandledElements;
        }

        private static void SkipElement(FrameworkElement element, string format, params object[] args)
        {
            Log.Info("Binding Convention Not Applied: {0}", string.Format(format, args));
            UnhandledElements.Add(element);
        }

        private static ExposedPropertyInfo GetExposedProperty(Type rootViewModelType, IList<string> nameParts)
        {
            var exposedPropertyInfo = GetExposedProperty(rootViewModelType, nameParts[0]);

            // Use a list to make a breadcrumb for the property path
            var breadCrumb = new List<string> { exposedPropertyInfo.Path };

            // Loop over all parts and get exposed properties
            for (var i = 1; i < nameParts.Count; i++)
            {
                exposedPropertyInfo = GetExposedProperty(exposedPropertyInfo.ViewModelType, nameParts[i]);
                if (exposedPropertyInfo == null) return null;

                breadCrumb.Add(exposedPropertyInfo.Path);
            }

            exposedPropertyInfo.Path = string.Join(".", breadCrumb);

            return exposedPropertyInfo;
        }

        private static ExposedPropertyInfo GetExposedProperty(Type type, string propertyName)
        {
            // First, check if the type has a matching property.
            var regularProperty = type.GetPropertyCaseInsensitive(propertyName);
            if (regularProperty != null)
            {
                return new ExposedPropertyInfo
                {
                    ViewModelType = regularProperty.PropertyType,
                    Path = regularProperty.Name,
                    Property = regularProperty
                };
            }

            // Check all properties to see if they expose any properties.
            var allProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in allProperties)
            {
                // Get first ExposeAttribute which matches property name
                var exposeAttribute = GetExposeAttribute(property, propertyName);
                if (exposeAttribute == null) continue;

                // Get the name of the exposed property
                var exposedPropertyName = exposeAttribute.ModelPropertyName ?? exposeAttribute.PropertyName;

                // Get exposed property info
                var exposedPropertyInfo = GetExposedChildProperty(property, exposedPropertyName);
                if (exposedPropertyInfo == null) continue;

                return exposedPropertyInfo;
            }

            return null;
        }

        private static ExposedPropertyInfo GetExposedChildProperty(PropertyInfo parentProperty, string propertyName)
        {
            var viewModelType = parentProperty.PropertyType;

            // Check if property exists
            var regularProperty = viewModelType.GetPropertyCaseInsensitive(propertyName);
            if (regularProperty != null)
            {
                return new ExposedPropertyInfo
                {
                    ViewModelType = regularProperty.PropertyType,
                    Path = string.Join(".", parentProperty.Name, regularProperty.Name),
                    Property = regularProperty
                };
            }

            // Do recursive check for exposed properties
            var exposedProperty = GetExposedProperty(viewModelType, propertyName);
            if (exposedProperty != null)
            {
                return new ExposedPropertyInfo
                {
                    ViewModelType = exposedProperty.ViewModelType,
                    Path = string.Join(".", parentProperty.Name, exposedProperty.Path),
                    Property = exposedProperty.Property
                };
            }

            return null;
        }

        private static ExposeAttribute GetExposeAttribute(MemberInfo property, string propertyName)
        {
            return property.GetCustomAttribute<ExposeAttribute>(a => a.Matches(propertyName));
        }
    }
}