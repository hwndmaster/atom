using System;

namespace Genius.Atom.UI.Forms.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ShowOnlyBrowsableAttribute : Attribute
    {
        public ShowOnlyBrowsableAttribute(bool onlyBrowsable)
        {
            OnlyBrowsable = onlyBrowsable;
        }

        public bool OnlyBrowsable { get; }
    }
}
