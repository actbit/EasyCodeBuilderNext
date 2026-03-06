using EasyCodeBuilderNext.Core.Models;

namespace EasyCodeBuilderNext.Core.Blocks.Statements;

/// <summary>
/// コンソール出力ブロック
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
            InputType = ParameterInputType.Block
        });
    }

    public override string CodeOutput(int level)
    {
        var value = Parameters[0].GetValueAsString();
        return $"{GetIndent(level)}Console.Write({value});{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// コンソール出力（改行付き）ブロック
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
/// コンソール入力ブロック
/// </summary>
public class ConsoleReadLineBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.IO;
    public override string DisplayName => "入力を受け取る";
    public override string CodeTemplate => "{0} = Console.ReadLine();";

    public ConsoleReadLineBlock()
    {
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
        var variable = Parameters[0].GetValueAsString();
        return $"{GetIndent(level)}{variable} = Console.ReadLine();{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// コンソール入力（数値）ブロック
/// </summary>
public class ConsoleReadIntBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.IO;
    public override string DisplayName => "数値を入力";
    public override string CodeTemplate => "{0} = int.Parse(Console.ReadLine());";

    public ConsoleReadIntBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Variable",
            Label = "変数",
            TypeName = "int",
            InputType = ParameterInputType.Variable
        });
    }

    public override string CodeOutput(int level)
    {
        var variable = Parameters[0].GetValueAsString();
        return $"{GetIndent(level)}{variable} = int.Parse(Console.ReadLine());{GenerateNextBlockCode(level)}";
    }
}
