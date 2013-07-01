using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace Caliburn.Micro.ExposedProperties
{
    public static class ExposedPropertyBinder
    {
        private static readonly ILog Log = LogManager.GetLog(typeof(ExposedPropertyBinder));

        public static IEnumerable<FrameworkElement> BindProperties(IEnumerable<FrameworkElement> elements, Type viewModelType)
        {
            foreach (var element in elements)
            {
                // Get first exposed property
                var bindableProperty = GetBindableProperty(viewModelType, element.Name);
                if (bindableProperty == null)
                {
                    Log.Info(Strings.ConventionNotApplied, string.Format(Strings.NoMatchingProperty, element.Name));
                    yield return element;
                    continue;
                }

                var elementType = element.GetType();
                var convention = ConventionManager.GetElementConvention(elementType);

                // Apply convention binding to element
                var bindingApplied = convention.Bind(bindableProperty, element);
                if (!bindingApplied)
                {
                    Log.Info(Strings.ConventionNotApplied, string.Format(Strings.ExistingBinding, element.Name));
                    yield return element;
                    continue;
                }

                // Great success! :)
                Log.Info(Strings.ConventionApplied, element.Name);
            }
        }

        private static BindablePropertyInfo GetBindableProperty(Type viewModelType, string elementName)
        {
            var cleanName = elementName.Trim('_');
            if (string.IsNullOrEmpty(cleanName)) return null;

            // Split path in parts
            IList<string> nameParts = cleanName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            string currentPath = null;
            BindablePropertyInfo bindableProperty = null;
            foreach (var namePart in nameParts)
            {
                bindableProperty = GetBindableProperty(viewModelType, namePart, currentPath);
                if (bindableProperty == null) return null;

                viewModelType = bindableProperty.Property.PropertyType;
                currentPath = bindableProperty.Path;
            }

            return bindableProperty;
        }

        private static BindablePropertyInfo GetBindableProperty(Type type, string propertyName, string currentPath)
        {
            // First, check if the type has an existing property.
            var existingProperty = type.GetPropertyCaseInsensitive(propertyName);
            if (existingProperty != null)
            {
                var newPath = GetNewPath(currentPath, existingProperty);
                return new BindablePropertyInfo(existingProperty, newPath);
            }

            // Then check for exposed properties
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                // Get first ExposeAttribute which matches property name
                var exposeAttribute = property.GetExposeAttribute(propertyName);
                if (exposeAttribute == null) continue;

                var exposedPropertyName = exposeAttribute.ModelPropertyName ?? exposeAttribute.PropertyName;

                // Get bindable property
                var newPath = GetNewPath(currentPath, property);
                var bindableProperty = GetBindableProperty(property.PropertyType, exposedPropertyName, newPath);
                if (bindableProperty == null) continue;

                return bindableProperty;
            }

            return null;
        }

        private static string GetNewPath(string currentPath, PropertyInfo property)
        {
            return string.IsNullOrEmpty(currentPath) ? property.Name : string.Join(".", currentPath, property.Name);
        }
    }
}
