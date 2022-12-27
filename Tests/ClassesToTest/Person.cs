using static Core.StringFormatter;

namespace Tests.ClassesToTest;

public class Person
{
    public string LastName;
    public string FirstName;

    public override string ToString() => Shared.Format("Person: {FirstName} {LastName}", this);
}