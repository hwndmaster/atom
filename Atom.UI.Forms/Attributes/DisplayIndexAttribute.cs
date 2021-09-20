using System;

namespace Genius.Atom.UI.Forms
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DisplayIndexAttribute : Attribute
    {
        public DisplayIndexAttribute(int index)
        {
            Index = index;
        }

        public int Index { get; }
    }
}
