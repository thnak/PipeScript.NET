namespace PipeScript.NET.Engine;

/// <summary>
/// Recursive descent parser for expression strings.
/// Grammar:
///   expr        := or_expr
///   or_expr     := and_expr ('||' and_expr)*
///   and_expr    := not_expr ('&&' not_expr)*
///   not_expr    := '!' not_expr | compare_expr
///   compare_expr := add_expr (('==' | '!=' | '>' | '<' | '>=' | '<=') add_expr)*
///   add_expr    := mul_expr (('+' | '-') mul_expr)*
///   mul_expr    := unary_expr (('*' | '/' | '%') unary_expr)*
///   unary_expr  := '-' unary_expr | primary
///   primary     := '(' expr ')' | number | string | bool | null | identifier
/// </summary>
public class ExpressionParser
{
    private readonly List<Token> _tokens;
    private int _pos;

    public ExpressionParser(List<Token> tokens)
    {
        _tokens = tokens;
        _pos = 0;
    }

    private Token Current => _tokens[_pos];
    private Token Consume() => _tokens[_pos++];

    private Token Expect(TokenType type)
    {
        if (Current.Type != type)
            throw new InvalidOperationException($"Expected {type} at position {_pos} but got {Current.Type}({Current.Value})");
        return Consume();
    }

    public ExpressionNode Parse()
    {
        var node = ParseOr();
        Expect(TokenType.EOF);
        return node;
    }

    private ExpressionNode ParseOr()
    {
        var left = ParseAnd();
        while (Current.Type == TokenType.Or)
        {
            Consume();
            var right = ParseAnd();
            left = new BinaryNode("||", left, right);
        }
        return left;
    }

    private ExpressionNode ParseAnd()
    {
        var left = ParseNot();
        while (Current.Type == TokenType.And)
        {
            Consume();
            var right = ParseNot();
            left = new BinaryNode("&&", left, right);
        }
        return left;
    }

    private ExpressionNode ParseNot()
    {
        if (Current.Type == TokenType.Not)
        {
            Consume();
            return new UnaryNode("!", ParseNot());
        }
        return ParseCompare();
    }

    private ExpressionNode ParseCompare()
    {
        var left = ParseAdd();
        while (Current.Type is TokenType.Equal or TokenType.NotEqual
               or TokenType.GreaterThan or TokenType.LessThan
               or TokenType.GreaterOrEqual or TokenType.LessOrEqual)
        {
            string op = Current.Value;
            Consume();
            var right = ParseAdd();
            left = new BinaryNode(op, left, right);
        }
        return left;
    }

    private ExpressionNode ParseAdd()
    {
        var left = ParseMul();
        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            string op = Current.Value;
            Consume();
            var right = ParseMul();
            left = new BinaryNode(op, left, right);
        }
        return left;
    }

    private ExpressionNode ParseMul()
    {
        var left = ParseUnary();
        while (Current.Type is TokenType.Multiply or TokenType.Divide or TokenType.Modulo)
        {
            string op = Current.Value;
            Consume();
            var right = ParseUnary();
            left = new BinaryNode(op, left, right);
        }
        return left;
    }

    private ExpressionNode ParseUnary()
    {
        if (Current.Type == TokenType.Minus)
        {
            Consume();
            return new UnaryNode("-", ParseUnary());
        }
        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        var tok = Current;

        if (tok.Type == TokenType.LParen)
        {
            Consume();
            var expr = ParseOr();
            Expect(TokenType.RParen);
            return expr;
        }

        if (tok.Type == TokenType.Number)
        {
            Consume();
            return new LiteralNode(double.Parse(tok.Value, System.Globalization.CultureInfo.InvariantCulture));
        }

        if (tok.Type == TokenType.String)
        {
            Consume();
            return new LiteralNode(tok.Value);
        }

        if (tok.Type == TokenType.Boolean)
        {
            Consume();
            return new LiteralNode(tok.Value == "true");
        }

        if (tok.Type == TokenType.Null)
        {
            Consume();
            return new LiteralNode(null);
        }

        if (tok.Type == TokenType.Identifier)
        {
            Consume();
            return new PropertyNode(tok.Value);
        }

        throw new InvalidOperationException($"Unexpected token: {tok}. Expected a literal, identifier, or '('.");
    }
}
