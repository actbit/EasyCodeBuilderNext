using EasyCodeBuilderNext.Core.Models;

namespace EasyCodeBuilderNext.Core.Blocks.Statements;

/// <summary>
/// Console.Writeブロック
/// </summary>
public class ConsoleWriteBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.IO;
    public override string DisplayName => "出力する";
    public override string CodeTemplate => "Console.Write({0});";

    public ConsoleWriteBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Value",
            Label = "値",
            TypeName = "object",
            InputType = ParameterInputType.Block,
            Value = ""
        });
    }

    public override string CodeOutput(int level)
    {
        var value = Parameters[0].GetValueAsString();
        return $"{GetIndent(level)}Console.Write({value});{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// Console.WriteLineブロック
/// </summary>
public class ConsoleWriteLineBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.IO;
    public override string DisplayName => "出力して改行";
    public override string CodeTemplate => "Console.WriteLine({0});";

    public ConsoleWriteLineBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Value",
            Label = "値",
            TypeName = "object",
            InputType = ParameterInputType.Block,
            Value = "",
            IsRequired = false
        });
    }

    public override string CodeOutput(int level)
    {
        var value = Parameters[0].GetValueAsString();
        if (string.IsNullOrEmpty(value))
        {
            return $"{GetIndent(level)}Console.WriteLine();{GenerateNextBlockCode(level)}";
        }
        return $"{GetIndent(level)}Console.WriteLine({value});{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// Console.ReadLineブロック（式ブロック）
/// </summary>
public class ConsoleReadLineBlock : ExpressionBlockBase
{
    public override BlockType BlockType => BlockType.Expression;
    public override BlockCategory Category => BlockCategory.IO;
    public override string DisplayName => "入力を受け取る";
    public override string CodeTemplate => "Console.ReadLine()";
    public override string ReturnType => "string?";

    public ConsoleReadLineBlock() { }

    public override string CodeOutput(int level)
    {
        return "Console.ReadLine()";
    }
}

/// <summary>
/// 整数入力ブロック
/// </summary>
public class ConsoleReadIntBlock : ExpressionBlockBase
{
    public override BlockType BlockType => BlockType.Expression;
    public override BlockCategory Category => BlockCategory.IO;
    public override string DisplayName => "数値を入力";
    public override string CodeTemplate => "int.Parse(Console.ReadLine())";
    public override string ReturnType => "int";

    public ConsoleReadIntBlock() { }

    public override string CodeOutput(int level)
    {
        return "int.Parse(Console.ReadLine())";
    }
}
