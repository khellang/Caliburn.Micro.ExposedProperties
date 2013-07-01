using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Caliburn.Micro.ExposedProperties
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
                // Get first exposed property
                var bindableProperty = GetBindableProperty(viewModelType, element.Name);
                if (bindableProperty == null)
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
                var applied = convention.Bind(bindableProperty, element);
                if (!applied)
                {
                    SkipElement(element, "Element {0} has existing binding.", element.Name);
                    continue;
                }

                Log.Info("Binding Convention Applied: Element {0}.", element.Name);
            }

            return UnhandledElements;
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
                return new BindablePropertyInfo(existingProperty, currentPath.AddProperty(existingProperty));
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
                var bindableProperty = GetBindableProperty(property.PropertyType, exposedPropertyName, currentPath.AddProperty(property));
                if (bindableProperty == null) continue;

                return bindableProperty;
            }

            return null;
        }

        private static void SkipElement(FrameworkElement element, string format, params object[] args)
        {
            Log.Info("Binding Convention Not Applied: {0}", string.Format(format, args));
            UnhandledElements.Add(element);
        }
        
        internal class BindablePropertyInfo
        {
            public BindablePropertyInfo(PropertyInfo property, string path)
            {
                Property = property;
                Path = path;
            }
    
            public string Path { get; private set; }
    
            public PropertyInfo Property { get; private set; }
        }
    }

    internal static class HelperExtensions
    {
        public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo memberInfo, Func<TAttribute, bool> selector, bool inherit = true)
            where TAttribute : Attribute
        {
            return memberInfo.GetCustomAttributes<TAttribute>().FirstOrDefault(selector);
        }

        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(
            this MemberInfo memberInfo, bool inherit = true) where TAttribute : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }

        public static ExposeAttribute GetExposeAttribute(this MemberInfo property, string propertyName)
        {
            return property.GetCustomAttribute<ExposeAttribute>(a => a.Matches(propertyName));
        }

        public static bool Matches(this ExposeAttribute attribute, string propertyName)
        {
            return attribute.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool Bind(this ElementConvention elementConvention, BindablePropertyInfo propertyInfo, FrameworkElement element)
        {
            return elementConvention.ApplyBinding(propertyInfo.Property.PropertyType, propertyInfo.Path, propertyInfo.Property, element, elementConvention);
        }

        public static string AddProperty(this string value, PropertyInfo property)
        {
            return !string.IsNullOrEmpty(value) ? string.Join(".", value, property.Name) : property.Name;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ExposeAttribute : Attribute
    {
        public ExposeAttribute(string propertyName) : this(propertyName, null) { }

        public ExposeAttribute(string propertyName, string modelPropertyName)
        {
            PropertyName = propertyName;
            ModelPropertyName = modelPropertyName;
        }

        public string PropertyName { get; private set; }

        public string ModelPropertyName { get; private set; }
    }
}
