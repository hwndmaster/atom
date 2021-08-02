using System;

namespace Genius.Atom.UI.Forms.Attributes
{
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ValueConverterAttribute : Attribute
    {
        public ValueConverterAttribute(Type valueConverterType)
        {
            ValueConverterType = valueConverterType;
        }

        public Type ValueConverterType { get; }
    }
}
