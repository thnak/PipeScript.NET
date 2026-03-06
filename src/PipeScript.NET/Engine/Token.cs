namespace PipeScript.NET.Engine;

public enum TokenType
{
    Number, String, Boolean, Null,
    Identifier,
    Plus, Minus, Multiply, Divide, Modulo,
    Equal, NotEqual, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual,
    And, Or, Not,
    LParen, RParen,
    Dot, Comma,
    EOF
}

public class Token
{
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString() => $"{Type}({Value})";
}
