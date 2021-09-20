using System;

namespace Genius.Atom.UI.Forms
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class ValidationRuleAttribute : Attribute
    {
        public ValidationRuleAttribute(Type validationRuleType, params object[] parameters)
        {
            ValidationRuleType = validationRuleType;
            Parameters = parameters;
        }

        public Type ValidationRuleType { get; }
        public object[] Parameters { get; }
    }
}
