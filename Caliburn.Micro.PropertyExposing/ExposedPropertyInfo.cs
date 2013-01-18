using System;
using System.Reflection;

namespace Caliburn.Micro.PropertyExposing
{
    internal class ExposedPropertyInfo
    {
        public Type ViewModelType { get; set; }

        public string Path { get; set; }

        public PropertyInfo Property { get; set; }
    }
}