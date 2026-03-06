namespace PipeScript.NET.Engine;

public class Tokenizer
{
    private readonly string _input;
    private int _pos;

    public Tokenizer(string input)
    {
        _input = input;
        _pos = 0;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (_pos < _input.Length)
        {
            SkipWhitespace();
            if (_pos >= _input.Length) break;

            char c = _input[_pos];

            if (char.IsDigit(c))
            {
                tokens.Add(ReadNumber());
            }
            else if (c == '"' || c == '\'')
            {
                tokens.Add(ReadString(c));
            }
            else if (char.IsLetter(c) || c == '_')
            {
                tokens.Add(ReadIdentifierOrKeyword());
            }
            else
            {
                tokens.Add(ReadOperatorOrPunctuation());
            }
        }
        tokens.Add(new Token(TokenType.EOF, ""));
        return tokens;
    }

    private void SkipWhitespace()
    {
        while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos]))
            _pos++;
    }

    private Token ReadNumber()
    {
        int start = _pos;
        bool hasDot = false;
        while (_pos < _input.Length && (char.IsDigit(_input[_pos]) || (_input[_pos] == '.' && !hasDot)))
        {
            if (_input[_pos] == '.') hasDot = true;
            _pos++;
        }
        return new Token(TokenType.Number, _input[start.._pos]);
    }

    private Token ReadString(char quote)
    {
        _pos++;
        var sb = new System.Text.StringBuilder();
        while (_pos < _input.Length && _input[_pos] != quote)
        {
            if (_input[_pos] == '\\' && _pos + 1 < _input.Length)
            {
                _pos++;
                sb.Append(_input[_pos] switch
                {
                    'n' => '\n',
                    't' => '\t',
                    'r' => '\r',
                    '"' => '"',
                    '\'' => '\'',
                    '\\' => '\\',
                    _ => _input[_pos]
                });
            }
            else
            {
                sb.Append(_input[_pos]);
            }
            _pos++;
        }
        if (_pos >= _input.Length)
            throw new InvalidOperationException($"Unterminated string literal starting with {quote}.");
        _pos++;
        return new Token(TokenType.String, sb.ToString());
    }

    private Token ReadIdentifierOrKeyword()
    {
        int start = _pos;
        while (_pos < _input.Length && (char.IsLetterOrDigit(_input[_pos]) || _input[_pos] == '_'))
            _pos++;
        string word = _input[start.._pos];
        return word switch
        {
            "true" => new Token(TokenType.Boolean, "true"),
            "false" => new Token(TokenType.Boolean, "false"),
            "null" => new Token(TokenType.Null, "null"),
            "and" => new Token(TokenType.And, "&&"),
            "or" => new Token(TokenType.Or, "||"),
            "not" => new Token(TokenType.Not, "!"),
            _ => new Token(TokenType.Identifier, word)
        };
    }

    private Token ReadOperatorOrPunctuation()
    {
        char c = _input[_pos];
        char next = _pos + 1 < _input.Length ? _input[_pos + 1] : '\0';

        if (c == '=' && next == '=') { _pos += 2; return new Token(TokenType.Equal, "=="); }
        if (c == '!' && next == '=') { _pos += 2; return new Token(TokenType.NotEqual, "!="); }
        if (c == '>' && next == '=') { _pos += 2; return new Token(TokenType.GreaterOrEqual, ">="); }
        if (c == '<' && next == '=') { _pos += 2; return new Token(TokenType.LessOrEqual, "<="); }
        if (c == '&' && next == '&') { _pos += 2; return new Token(TokenType.And, "&&"); }
        if (c == '|' && next == '|') { _pos += 2; return new Token(TokenType.Or, "||"); }

        _pos++;
        return c switch
        {
            '+' => new Token(TokenType.Plus, "+"),
            '-' => new Token(TokenType.Minus, "-"),
            '*' => new Token(TokenType.Multiply, "*"),
            '/' => new Token(TokenType.Divide, "/"),
            '%' => new Token(TokenType.Modulo, "%"),
            '>' => new Token(TokenType.GreaterThan, ">"),
            '<' => new Token(TokenType.LessThan, "<"),
            '!' => new Token(TokenType.Not, "!"),
            '(' => new Token(TokenType.LParen, "("),
            ')' => new Token(TokenType.RParen, ")"),
            '.' => new Token(TokenType.Dot, "."),
            ',' => new Token(TokenType.Comma, ","),
            _ => throw new InvalidOperationException($"Unexpected character: '{c}' at position {_pos - 1}")
        };
    }
}
