using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Caliburn.Micro.ExposedProperties
{
    internal static class HelperExtensions
    {
        public static bool Bind(this ElementConvention elementConvention, BindablePropertyInfo propertyInfo, FrameworkElement element)
        {
            return elementConvention.ApplyBinding(propertyInfo.Property.PropertyType, propertyInfo.Path, propertyInfo.Property, element, elementConvention);
        }

        public static ExposeAttribute GetExposeAttribute(this MemberInfo memberInfo, string propertyName)
        {
            return memberInfo.GetCustomAttributes(typeof(ExposeAttribute), true)
                .Cast<ExposeAttribute>()
                .FirstOrDefault(a => a.Matches(propertyName));
        }

        private static bool Matches(this ExposeAttribute attribute, string propertyName)
        {
            return attribute.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase);
        }
    }
}