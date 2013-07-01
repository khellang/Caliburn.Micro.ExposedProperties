using System;

namespace Caliburn.Micro.ExposedProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ExposeAttribute : Attribute
    {
        public ExposeAttribute(string propertyName, string modelPropertyName = null)
        {
            PropertyName = propertyName;
            ModelPropertyName = modelPropertyName;
        }

        public string PropertyName { get; private set; }

        public string ModelPropertyName { get; private set; }
    }
}