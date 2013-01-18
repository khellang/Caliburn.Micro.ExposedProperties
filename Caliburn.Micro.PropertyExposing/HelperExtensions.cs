using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caliburn.Micro.PropertyExposing
{
    public static class HelperExtensions
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
    }
}