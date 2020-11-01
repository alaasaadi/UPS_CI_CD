using System;

namespace UPS.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class SearchableFieldAttribute : Attribute
    {
        public string FieldName { get; private set; }
        public bool ExcludeDefaultValue { get; private set; }
        public object[] ExcludedValues { get; private set; }

        public SearchableFieldAttribute(string FieldName, bool ExcludeDefaultValue = false, object[] ExcludedValues = null)
        {
            this.FieldName = FieldName;
            this.ExcludedValues = ExcludedValues;
            this.ExcludeDefaultValue = ExcludeDefaultValue;
        }
    }
}
