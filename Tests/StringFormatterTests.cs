using System.Collections.Concurrent;
using Tests.ClassesToTest;
using static Core.StringFormatter;

namespace Tests;

public class StringFormatterTests
{
    [Fact]
    public void EmptyMember()
    {
        // Arrange
        const string template = "Hello {}!";
        var target = new { };

        // Act & Assert
        Assert.Throws<FormatException>(() => Formatter.Format(template, target));
    }

    [Fact]
    public void NonExistentMember()
    {
        // Arrange
        const string template = "Are you {Some Name}?";
        var target = new { SomeName = "" };

        // Act & Assert
        Assert.Throws<FormatException>(() => Formatter.Format(template, target));
    }

    [Fact]
    public void NonPublicMember()
    {
        // Arrange
        const string template = "Machine is in state {_state}";
        var target = new Machine();

        // Act & Assert
        Assert.Throws<FormatException>(() => Formatter.Format(template, target));
    }

    [Fact]
    public void NonEscapedCloseBracket()
    {
        // Arrange
        const string template = "uWu:3 } ";
        var target = new { };

        // Act & Assert
        Assert.Throws<FormatException>(() => Formatter.Format(template, target));
    }

    [Fact]
    public void NullMembers()
    {
        // Arrange
        const string template = "{LastName}";
        var target = new Person();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => Formatter.Format(template, target));
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
        var actual = Formatter.Format(template, target);

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
        var actual = Formatter.Format(template, target);

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
        var actual = Formatter.Format(template, target);

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
        var actual = Formatter.Format(template, target);

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
        Assert.Throws<FormatException>(() => Formatter.Format(template, target));
    }

    [Fact]
    public void MultiThread()
    {
        // Arrange
        var threads = new List<Thread>();
        var expected = new List<string>();
        var unsortedActual = new ConcurrentBag<string>();
        
        // Append elements to thread and expected lists
        for (var i = 0; i < 50; i++)
        {
            var data = Guid.NewGuid().ToString();
            expected.Add("Test " + i + ": " + data);
         
            // Create new thread with formatting and adding to actual collection
            var indexCopy = i; // Index must be immutable for usage in lambda
            threads.Add(new Thread(() =>
            {
                var obj = new CountedStrings { Counter = indexCopy, Data = data };
                var output = Formatter.Format("Test {Counter}: {Data}", obj);
                unsortedActual.Add(output);
            }));
        }

        // Act
        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();
        
        // Sort to compare
        var actual = unsortedActual.ToList();
        actual.Sort();
        expected.Sort();
        
        // Assert
        Assert.Equal(expected, actual);
    }
}