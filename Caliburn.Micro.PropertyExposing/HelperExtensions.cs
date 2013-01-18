using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Caliburn.Micro.PropertyExposing
{
    internal static class HelperExtensions
    {
        public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo memberInfo, Func<TAttribute, bool> selector,  bool inherit = true)
            where TAttribute : Attribute
        {
            return memberInfo.GetCustomAttributes<TAttribute>().FirstOrDefault(selector);
        }

        public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo memberInfo, bool inherit = true)
            where TAttribute : Attribute
        {
            return memberInfo.GetCustomAttributes<TAttribute>().FirstOrDefault();
        }

        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(
            this MemberInfo memberInfo, bool inherit = true) where TAttribute : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }

        public static bool Matches(this ExposeAttribute attribute, string propertyName)
        {
            return attribute.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool Bind(this ElementConvention elementConvention, ExposedPropertyInfo propertyInfo, FrameworkElement element)
        {
            return elementConvention.ApplyBinding(propertyInfo.ViewModelType, propertyInfo.Path, propertyInfo.Property, element, elementConvention);
        }
    }
}