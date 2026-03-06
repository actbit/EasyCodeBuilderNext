using EasyCodeBuilderNext.Core.Models;

namespace EasyCodeBuilderNext.Core.Blocks.Expressions;

/// <summary>
/// 式ブロックの基底クラス
/// </summary>
public abstract class ExpressionBlockBase : BlockBase
{
    public override BlockType BlockType => BlockType.Expression;

    /// <summary>
    /// 戻り値の型
    /// </summary>
    public abstract string ReturnType { get; }
}

/// <summary>
/// 数値リテラルブロック
/// </summary>
public class NumberLiteralBlock : ExpressionBlockBase
{
    public override BlockCategory Category => BlockCategory.Operators;
    public override string DisplayName => "数値";
    public override string CodeTemplate => "{0}";
    public override string ReturnType => "int";

    public NumberLiteralBlock()
    {
        Width = 80;
        Parameters.Add(new BlockParameter
        {
            Name = "Value",
            Label = "値",
            TypeName = "int",
            InputType = ParameterInputType.Number,
            Value = 0
        });
    }

    public override string CodeOutput(int level)
    {
        return Parameters[0].GetValueAsString();
    }
}

/// <summary>
/// 文字列リテラルブロック
/// </summary>
public class StringLiteralBlock : ExpressionBlockBase
{
    public override BlockCategory Category => BlockCategory.Operators;
    public override string DisplayName => "文字列";
    public override string CodeTemplate => "\"{0}\"";
    public override string ReturnType => "string";

    public StringLiteralBlock()
    {
        Width = 120;
        Parameters.Add(new BlockParameter
        {
            Name = "Value",
            Label = "値",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = ""
        });
    }

    public override string CodeOutput(int level)
    {
        var value = Parameters[0].GetValueAsString();
        return $"\"{value}\"";
    }
}

/// <summary>
/// 真偽値リテラルブロック
/// </summary>
public class BooleanLiteralBlock : ExpressionBlockBase
{
    public override BlockCategory Category => BlockCategory.Operators;
    public override string DisplayName => "真偽値";
    public override string CodeTemplate => "{0}";
    public override string ReturnType => "bool";

    public BooleanLiteralBlock()
    {
        Width = 80;
        Parameters.Add(new BlockParameter
        {
            Name = "Value",
            Label = "値",
            TypeName = "bool",
            InputType = ParameterInputType.Dropdown,
            Value = true
        });

        Parameters[0].Options.Add("true");
        Parameters[0].Options.Add("false");
    }

    public override string CodeOutput(int level)
    {
        return Parameters[0].GetValueAsString().ToLower();
    }
}

/// <summary>
/// 変数参照ブロック
/// </summary>
public class VariableReferenceBlock : ExpressionBlockBase
{
    public override BlockCategory Category => BlockCategory.Variables;
    public override string DisplayName => "変数";
    public override string CodeTemplate => "{0}";
    public override string ReturnType => "object";

    public VariableReferenceBlock()
    {
        Width = 100;
        Parameters.Add(new BlockParameter
        {
            Name = "Variable",
            Label = "変数",
            TypeName = "string",
            InputType = ParameterInputType.Variable
        });
    }

    public override string CodeOutput(int level)
    {
        return Parameters[0].GetValueAsString();
    }
}

/// <summary>
/// 比較演算ブロック
/// </summary>
public class ComparisonBlock : ExpressionBlockBase
{
    public override BlockCategory Category => BlockCategory.Operators;
    public override string DisplayName => "比較";
    public override string CodeTemplate => "{0} {1} {2}";
    public override string ReturnType => "bool";

    public ComparisonBlock()
    {
        Width = 150;
        Parameters.Add(new BlockParameter
        {
            Name = "Left",
            Label = "左辺",
            TypeName = "object",
            InputType = ParameterInputType.Block
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Operator",
            Label = "演算子",
            TypeName = "string",
            InputType = ParameterInputType.Dropdown,
            Value = "=="
        });

        Parameters[1].Options.AddRange(new[] { "==", "!=", "<", ">", "<=", ">=" });

        Parameters.Add(new BlockParameter
        {
            Name = "Right",
            Label = "右辺",
            TypeName = "object",
            InputType = ParameterInputType.Block
        });
    }

    public override string CodeOutput(int level)
    {
        var left = Parameters[0].GetValueAsString();
        var op = Parameters[1].GetValueAsString();
        var right = Parameters[2].GetValueAsString();

        return $"{left} {op} {right}";
    }
}

/// <summary>
/// 算術演算ブロック
/// </summary>
public class ArithmeticBlock : ExpressionBlockBase
{
    public override BlockCategory Category => BlockCategory.Operators;
    public override string DisplayName => "計算";
    public override string CodeTemplate => "{0} {1} {2}";
    public override string ReturnType => "object";

    public ArithmeticBlock()
    {
        Width = 150;
        Parameters.Add(new BlockParameter
        {
            Name = "Left",
            Label = "左辺",
            TypeName = "object",
            InputType = ParameterInputType.Block
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Operator",
            Label = "演算子",
            TypeName = "string",
            InputType = ParameterInputType.Dropdown,
            Value = "+"
        });

        Parameters[1].Options.AddRange(new[] { "+", "-", "*", "/", "%" });

        Parameters.Add(new BlockParameter
        {
            Name = "Right",
            Label = "右辺",
            TypeName = "object",
            InputType = ParameterInputType.Block
        });
    }

    public override string CodeOutput(int level)
    {
        var left = Parameters[0].GetValueAsString();
        var op = Parameters[1].GetValueAsString();
        var right = Parameters[2].GetValueAsString();

        return $"{left} {op} {right}";
    }
}

/// <summary>
/// 論理演算ブロック
/// </summary>
public class LogicalBlock : ExpressionBlockBase
{
    public override BlockCategory Category => BlockCategory.Operators;
    public override string DisplayName => "論理演算";
    public override string CodeTemplate => "{0} {1} {2}";
    public override string ReturnType => "bool";

    public LogicalBlock()
    {
        Width = 150;
        Parameters.Add(new BlockParameter
        {
            Name = "Left",
            Label = "左辺",
            TypeName = "bool",
            InputType = ParameterInputType.Block
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Operator",
            Label = "演算子",
            TypeName = "string",
            InputType = ParameterInputType.Dropdown,
            Value = "&&"
        });

        Parameters[1].Options.AddRange(new[] { "&&", "||" });

        Parameters.Add(new BlockParameter
        {
            Name = "Right",
            Label = "右辺",
            TypeName = "bool",
            InputType = ParameterInputType.Block
        });
    }

    public override string CodeOutput(int level)
    {
        var left = Parameters[0].GetValueAsString();
        var op = Parameters[1].GetValueAsString();
        var right = Parameters[2].GetValueAsString();

        return $"{left} {op} {right}";
    }
}

/// <summary>
/// Not演算ブロック
/// </summary>
public class NotBlock : ExpressionBlockBase
{
    public override BlockCategory Category => BlockCategory.Operators;
    public override string DisplayName => "否定";
    public override string CodeTemplate => "!{0}";
    public override string ReturnType => "bool";

    public NotBlock()
    {
        Width = 80;
        Parameters.Add(new BlockParameter
        {
            Name = "Value",
            Label = "値",
            TypeName = "bool",
            InputType = ParameterInputType.Block
        });
    }

    public override string CodeOutput(int level)
    {
        var value = Parameters[0].GetValueAsString();
        return $"!{value}";
    }
}
