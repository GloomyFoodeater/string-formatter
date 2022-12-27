using static Core.StringFormatter;

namespace Tests.ClassesToTest;

public class Person
{
    public string LastName;
    public string FirstName;

    public override string ToString() => Formatter.Format("Person: {FirstName} {LastName}", this);
}