using System;
using System.Windows;

namespace Genius.Atom.UI.Forms.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class StyleAttribute : Attribute
    {
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
    }
}
