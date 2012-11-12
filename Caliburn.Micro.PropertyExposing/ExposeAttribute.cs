using System;

namespace Caliburn.Micro.PropertyExposing
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ExposeAttribute : Attribute
    {
        public ExposeAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public ExposeAttribute(string propertyName, string modelPropertyName)
        {
            PropertyName = propertyName;
            ModelPropertyName = modelPropertyName;
        }

        public string PropertyName { get; set; }

        public string ModelPropertyName { get; set; }
    }
}