using Tests.ClassesToTest;
using static Core.StringFormatter;

namespace Tests;

public class UnitTest1
{
    [Fact]
    public void EmptyMember()
    {
        // Arrange
        const string template = "Hello {}!";
        var target = new { };

        // Act & Assert
        Assert.Throws<FormatException>(() => Shared.Format(template, target));
    }

    [Fact]
    public void NonExistentMember()
    {
        // Arrange
        const string template = "Are you {Some Name}?";
        var target = new { SomeName = "" };

        // Act & Assert
        Assert.Throws<FormatException>(() => Shared.Format(template, target));
    }

    [Fact]
    public void NonPublicMember()
    {
        // Arrange
        const string template = "Machine is in state {_state}";
        var target = new Machine();

        // Act & Assert
        Assert.Throws<FormatException>(() => Shared.Format(template, target));
    }

    [Fact]
    public void NonEscapedCloseBracket()
    {
        // Arrange
        const string template = "uWu:3 } ";
        var target = new { };

        // Act & Assert
        Assert.Throws<FormatException>(() => Shared.Format(template, target));
    }

    [Fact]
    public void NullMembers()
    {
        // Arrange
        const string template = "{LastName}";
        var target = new Person();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => Shared.Format(template, target));
    }

    [Fact]
    public void StringFieldsTarget()
    {
        // Arrange
        const string expected = "Hello, John Doe!";
        const string template = "Hello, {FirstName} {LastName}!";
        var target = new Person
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var actual = Shared.Format(template, target);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TargetWithToStringMembers()
    {
        // Arrange
        const string expected =
            "Formatting wrapper child results in Wrapper\nMachine: 0\nPerson:  \nWrapper\nMachine: 0\nPerson:  \n";
        const string template = "Formatting wrapper child results in {Child}";
        var target = new Wrapper(2);

        // Act
        var actual = Shared.Format(template, target);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void WithEscapedBrackets()
    {
        // Arrange
        const string expected = "{LastName} of new Person { LastName=\"Doe\" }  is formatted to Doe";
        const string template = "{{LastName}} of new Person {{ LastName=\"Doe\" }}  is formatted to {LastName}";
        var target = new Person { LastName = "Doe" };

        // Act
        var actual = Shared.Format(template, target);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void EndsWithMember()
    {
        // Arrange
        const string expected = "Hello, John Doe";
        const string template = "Hello, {FirstName} {LastName}";
        var target = new Person
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var actual = Shared.Format(template, target);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void EndsWithOpenBracket()
    {
        // Arrange
        const string template = "uWu {";
        var target = new { };

        // Act & Assert
        Assert.Throws<FormatException>(() => Shared.Format(template, target));
    }
}