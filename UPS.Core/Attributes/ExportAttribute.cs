using System;

namespace UPS.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ExportAttribute : Attribute
    {
        public ExportAttribute()
        {

        }
    }
}
