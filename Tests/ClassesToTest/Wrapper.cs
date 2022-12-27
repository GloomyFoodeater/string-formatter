using System.Diagnostics.CodeAnalysis;
using static Core.StringFormatter;

namespace Tests.ClassesToTest;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Wrapper
{
    public readonly Person Person = new() { FirstName = "", LastName = "" };
    public readonly Machine Machine = new();
    public Wrapper? Child { get; }

    public Wrapper(int deepness)
    {
        for (var i = 0; i < deepness; i++) Child = new Wrapper(deepness - 1);
    }

    public override string ToString()
    {
        // Child is outside of template since it can be null
        return Formatter.Format("Wrapper\n{Machine}\n{Person}\n", this) + Child;
    }
}