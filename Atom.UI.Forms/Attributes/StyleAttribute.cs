using System;
using System.Windows;

namespace Genius.Atom.UI.Forms
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class StyleAttribute : Attribute
    {
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
    }
}
