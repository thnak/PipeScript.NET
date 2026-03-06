namespace PipeScript.NET.Engine;

public abstract class ExpressionNode
{
    public abstract object? Evaluate(IDictionary<string, object?> context);
}

public class LiteralNode : ExpressionNode
{
    private readonly object? _value;
    public LiteralNode(object? value) => _value = value;
    public override object? Evaluate(IDictionary<string, object?> context) => _value;
}

public class PropertyNode : ExpressionNode
{
    private readonly string _name;
    public PropertyNode(string name) => _name = name;

    public override object? Evaluate(IDictionary<string, object?> context)
    {
        return context.TryGetValue(_name, out var val) ? val : null;
    }
}

public class UnaryNode : ExpressionNode
{
    private readonly string _op;
    private readonly ExpressionNode _operand;

    public UnaryNode(string op, ExpressionNode operand)
    {
        _op = op;
        _operand = operand;
    }

    public override object? Evaluate(IDictionary<string, object?> context)
    {
        var val = _operand.Evaluate(context);
        return _op switch
        {
            "-" => Negate(val),
            "!" => !(bool)Convert.ChangeType(val ?? false, typeof(bool)),
            _ => throw new InvalidOperationException($"Unknown unary operator: {_op}")
        };
    }

    private static object? Negate(object? val)
    {
        if (val is double d) return -d;
        if (val is decimal dec) return -dec;
        if (val is int i) return (double)(-i);
        return -(Convert.ToDouble(val));
    }
}

public class BinaryNode : ExpressionNode
{
    private readonly string _op;
    private readonly ExpressionNode _left;
    private readonly ExpressionNode _right;

    public BinaryNode(string op, ExpressionNode left, ExpressionNode right)
    {
        _op = op;
        _left = left;
        _right = right;
    }

    public override object? Evaluate(IDictionary<string, object?> context)
    {
        if (_op == "&&")
        {
            var leftVal = _left.Evaluate(context);
            if (!ToBoolean(leftVal)) return false;
            var rightVal = _right.Evaluate(context);
            return ToBoolean(rightVal);
        }
        if (_op == "||")
        {
            var leftVal = _left.Evaluate(context);
            if (ToBoolean(leftVal)) return true;
            var rightVal = _right.Evaluate(context);
            return ToBoolean(rightVal);
        }

        var left = _left.Evaluate(context);
        var right = _right.Evaluate(context);

        return _op switch
        {
            "+" => Add(left, right),
            "-" => ArithmeticOp(left, right, (a, b) => a - b),
            "*" => ArithmeticOp(left, right, (a, b) => a * b),
            "/" => ArithmeticOp(left, right, (a, b) => a / b),
            "%" => ArithmeticOp(left, right, (a, b) => a % b),
            "==" => AreEqual(left, right),
            "!=" => !AreEqual(left, right),
            ">" => Compare(left, right) > 0,
            "<" => Compare(left, right) < 0,
            ">=" => Compare(left, right) >= 0,
            "<=" => Compare(left, right) <= 0,
            _ => throw new InvalidOperationException($"Unknown binary operator: {_op}")
        };
    }

    private static object? Add(object? left, object? right)
    {
        if (left is string || right is string)
            return $"{left}{right}";
        return ArithmeticOp(left, right, (a, b) => a + b);
    }

    private static object? ArithmeticOp(object? left, object? right, Func<double, double, double> op)
    {
        double l = ToDouble(left);
        double r = ToDouble(right);
        return op(l, r);
    }

    private static bool AreEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        if (left is string ls && right is string rs) return ls == rs;
        if (IsNumeric(left) && IsNumeric(right)) return ToDouble(left) == ToDouble(right);
        return left.Equals(right);
    }

    private static int Compare(object? left, object? right)
    {
        if (left is string ls && right is string rs) return string.Compare(ls, rs, StringComparison.Ordinal);
        double l = ToDouble(left);
        double r = ToDouble(right);
        return l.CompareTo(r);
    }

    private static bool IsNumeric(object? val) =>
        val is int or long or double or float or decimal or short or byte;

    private static double ToDouble(object? val)
    {
        if (val == null) return 0;
        return Convert.ToDouble(val);
    }

    private static bool ToBoolean(object? val)
    {
        if (val == null) return false;
        if (val is bool b) return b;
        return Convert.ToBoolean(val);
    }
}
