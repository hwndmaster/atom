namespace Genius.Atom.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class DangerousAttribute : Attribute
{
    public DangerousAttribute(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
