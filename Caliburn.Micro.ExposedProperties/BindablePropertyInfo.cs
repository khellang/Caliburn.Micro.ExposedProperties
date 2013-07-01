using System.Reflection;

namespace Caliburn.Micro.ExposedProperties
{
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