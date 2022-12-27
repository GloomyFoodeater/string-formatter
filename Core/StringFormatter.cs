using System.Text;

namespace Core;

public class StringFormatter : IStringFormatter
{
    private ExpressionCache _cache = new ExpressionCache();
    public static StringFormatter Shared = new StringFormatter();

    private int GetLetterType(char letter)
    {
        return letter switch
        {
            '{' => 0,
            '}' => 1,
            _ => 2
        };
    }

    private const int InitialState = 1;

    private static readonly int[,] Transitions =
    {
        { 0, 0, 0 }, // Error state
        { 2, 5, 1 }, // Reading plain text
        { 1, 0, 3 }, // First open bracket is read
        { 0, 4, 3 }, // Reading field or property 
        { 2, 5, 1 }, // Finish reading field or property
        { 0, 1, 0 }, // Escape close bracket is read
    };

    private bool IsFinalState(int state) => state is 1 or 4;


    public string Format(string template, object target)
    {
        var output = new StringBuilder(template.Length);
        var memberName = new StringBuilder(20);

        var state = InitialState;
        foreach (var letter in template)
        {
            state = Transitions[state, GetLetterType(letter)];
            switch (state)
            {
                // Add letter(s) to output string.
                case 1:
                    output.Append(letter);
                    break;

                // Add letter to member name string.
                case 3:
                    memberName.Append(letter);
                    break;

                // Add member value to output string.
                case 4:
                    try
                    {
                        var memberValue = _cache.GetString(memberName.ToString(), target);
                        output.Append(memberValue);
                        memberName.Clear();
                    }
                    catch (ArgumentException e)
                    {
                        throw new FormatException(e.Message);
                    }
                    catch (Exception)
                    {
                        throw new FormatException("Could not obtain value of member '" + memberName + "'");
                    }

                    break;

                // Skip.
                case 2:
                case 5:
                    break;
                default:
                    throw new FormatException("Error state reached while formatting");
            }
        }

        if (!IsFinalState(state))
            throw new FormatException("Formatter was not in final state after processing template");

        return output.ToString();
    }
}