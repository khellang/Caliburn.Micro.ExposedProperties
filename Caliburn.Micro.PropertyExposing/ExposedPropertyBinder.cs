using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Caliburn.Micro.PropertyExposing
{
    public static class ExposedPropertyBinder
    {
        private static readonly ILog Log = LogManager.GetLog(typeof(ExposedPropertyBinder));

        public static IEnumerable<FrameworkElement> BindProperties(IEnumerable<FrameworkElement> elements, Type viewModelType)
        {
            var unmatchedElements = new List<FrameworkElement>();

            foreach (var element in elements)
            {
                var cleanName = element.Name.Trim('_');
                var parts = cleanName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

                // Get first exposed property
                var exposedPropertyInfo = GetExposedPropertyInfo(viewModelType, parts[0]);
                if (exposedPropertyInfo == null)
                {
                    Log.Info("Binding Convention Not Applied: Element {0} did not match a property.", element.Name);
                    unmatchedElements.Add(element);
                    continue;
                }

                // Use a list to make a breadcrumb for the property path
                var breadCrumb = new List<string> { exposedPropertyInfo.Path };

                // Loop over all parts and get exposed properties
                for (var i = 1; i < parts.Length; i++)
                {
                    var exposedViewModelType = exposedPropertyInfo.ViewModelType;

                    exposedPropertyInfo = GetExposedPropertyInfo(exposedViewModelType, parts[i]);
                    if (exposedPropertyInfo == null) break;

                    breadCrumb.Add(exposedPropertyInfo.Path);
                }

                if (exposedPropertyInfo == null)
                {
                    Log.Info("Binding Convention Not Applied: Element {0} did not match a property.", element.Name);
                    unmatchedElements.Add(element);
                    continue;
                }

                var convention = ConventionManager.GetElementConvention(element.GetType());
                if (convention == null)
                {
                    Log.Warn("Binding Convention Not Applied: No conventions configured for {0}.", element.GetType());
                    unmatchedElements.Add(element);
                    continue;
                }

                var path = string.Join(".", breadCrumb);
                var applied = convention.ApplyBinding(exposedPropertyInfo.ViewModelType, path, exposedPropertyInfo.Property, element, convention);

                if (applied)
                {
                    Log.Info("Binding Convention Applied: Element {0}.", element.Name);
                }
                else
                {
                    Log.Info("Binding Convention Not Applied: Element {0} has existing binding.", element.Name);
                    unmatchedElements.Add(element);
                }
            }

            return unmatchedElements;
        }

        private static ExposedPropertyInfo GetExposedPropertyInfo(Type type, string propertyName)
        {
            var exposedProperty = type.GetPropertyCaseInsensitive(propertyName);
            if (exposedProperty != null)
                return new ExposedPropertyInfo(exposedProperty.PropertyType, exposedProperty.Name, exposedProperty);

            foreach (var property in type.GetProperties())
            {
                // Get first ExposeAttribute which matches property name
                var exposeAttribute = GetExposeAttribute(property, propertyName);
                if (exposeAttribute == null) continue;

                // Get the name of the exposed property
                var exposedPropertyName = exposeAttribute.ModelPropertyName ?? exposeAttribute.PropertyName;

                // Get exposed property info
                var exposedPropertyInfo = GetExposedPropertyInfo(property, exposedPropertyName);
                if (exposedPropertyInfo == null) continue;

                return exposedPropertyInfo;
            }

            return null;
        }

        private static ExposedPropertyInfo GetExposedPropertyInfo(PropertyInfo property, string exposedPropertyName)
        {
            var propertyType = property.PropertyType;

            // Check if property exists
            var exposedProperty = propertyType.GetPropertyCaseInsensitive(exposedPropertyName);
            if (exposedProperty == null)
            {
                // Do recursive check for exposed properties
                var child = GetExposedPropertyInfo(propertyType, exposedPropertyName);
                if (child == null) return null;

                return new ExposedPropertyInfo(child.ViewModelType, string.Join(".", property.Name, child.Path), child.Property);
            }

            return new ExposedPropertyInfo(exposedProperty.PropertyType, string.Join(".", property.Name, exposedPropertyName), exposedProperty);
        }

        private static ExposeAttribute GetExposeAttribute(PropertyInfo property, string propertyName)
        {
            return property
                .GetCustomAttributes(typeof(ExposeAttribute), true)
                .Cast<ExposeAttribute>()
                .FirstOrDefault(a => a.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        }

        private class ExposedPropertyInfo
        {
            public ExposedPropertyInfo(Type viewModelType, string path, PropertyInfo property)
            {
                ViewModelType = viewModelType;
                Path = path;
                Property = property;
            }

            public Type ViewModelType { get; private set; }

            public string Path { get; private set; }

            public PropertyInfo Property { get; private set; }
        }
    }
}