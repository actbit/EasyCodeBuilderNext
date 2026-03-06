using EasyCodeBuilderNext.Core.Models;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Core.Blocks.Statements;

/// <summary>
/// ifブロック
/// </summary>
public class IfBlock : BlockBase
{
    public override BlockType BlockType => BlockType.ControlStructure;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "もし～なら";
    public override string CodeTemplate => "if ({0})\n{{\n{1}\n}}";

    public IfBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Condition",
            Label = "条件",
            TypeName = "bool",
            InputType = ParameterInputType.Block,
            Value = "true"
        });
    }

    public override string CodeOutput(int level)
    {
        var condition = Parameters[0].GetValueAsString();
        var innerCode = GenerateInnerBlocksCode(level);

        var code = $"{GetIndent(level)}if ({condition})\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}{GenerateNextBlockCode(level)}";
        return code;
    }
}

/// <summary>
/// if-elseブロック
/// </summary>
public class IfElseBlock : BlockBase
{
    public override BlockType BlockType => BlockType.ControlStructure;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "もし～なら/そうでなければ";
    public override string CodeTemplate => "if ({0})\n{{\n{1}\n}}\nelse\n{{\n{2}\n}}";

    public IfElseBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Condition",
            Label = "条件",
            TypeName = "bool",
            InputType = ParameterInputType.Block,
            Value = "true"
        });

        // else用の内部ブロックコレクションを追加
        AdditionalInnerBlocks.Add(new ObservableCollection<BlockBase>());
    }

    public override string CodeOutput(int level)
    {
        var condition = Parameters[0].GetValueAsString();
        var ifCode = GenerateInnerBlocksCode(level);
        var elseCode = GenerateAdditionalInnerBlocksCode(level, 0);

        var code = $"{GetIndent(level)}if ({condition})\n{GetIndent(level)}{{\n{ifCode}\n{GetIndent(level)}}}\n{GetIndent(level)}else\n{GetIndent(level)}{{\n{elseCode}\n{GetIndent(level)}}}{GenerateNextBlockCode(level)}";
        return code;
    }

    /// <summary>
    /// 追加の内部ブロック（else節など）のコードを生成
    /// </summary>
    protected string GenerateAdditionalInnerBlocksCode(int level, int index)
    {
        if (AdditionalInnerBlocks.Count <= index || AdditionalInnerBlocks[index].Count == 0)
            return GetIndent(level + 1) + "// 空のブロック";

        var sb = new System.Text.StringBuilder();
        foreach (var block in AdditionalInnerBlocks[index])
        {
            sb.AppendLine(block.CodeOutput(level + 1));
        }
        return sb.ToString().TrimEnd();
    }
}

/// <summary>
/// whileブロック
/// </summary>
public class WhileBlock : BlockBase
{
    public override BlockType BlockType => BlockType.ControlStructure;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "～の間繰り返す";
    public override string CodeTemplate => "while ({0})\n{{\n{1}\n}}";

    public WhileBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Condition",
            Label = "条件",
            TypeName = "bool",
            InputType = ParameterInputType.Block,
            Value = "true"
        });
    }

    public override string CodeOutput(int level)
    {
        var condition = Parameters[0].GetValueAsString();
        var innerCode = GenerateInnerBlocksCode(level);

        var code = $"{GetIndent(level)}while ({condition})\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}{GenerateNextBlockCode(level)}";
        return code;
    }
}

/// <summary>
/// forブロック
/// </summary>
public class ForBlock : BlockBase
{
    public override BlockType BlockType => BlockType.ControlStructure;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "回数繰り返す";
    public override string CodeTemplate => "for (int {0} = 0; {0} < {1}; {0}++)\n{{\n{2}\n}}";

    public ForBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "VariableName",
            Label = "変数名",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "i"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Count",
            Label = "回数",
            TypeName = "int",
            InputType = ParameterInputType.Block,
            Value = "10"
        });
    }

    public override string CodeOutput(int level)
    {
        var varName = Parameters[0].GetValueAsString();
        var count = Parameters[1].GetValueAsString();
        var innerCode = GenerateInnerBlocksCode(level);

        var code = $"{GetIndent(level)}for (int {varName} = 0; {varName} < {count}; {varName}++)\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}{GenerateNextBlockCode(level)}";
        return code;
    }

    protected override void AddDefinedVariables(List<VariableInfo> variables)
    {
        variables.Add(new VariableInfo
        {
            Name = Parameters[0].GetValueAsString(),
            TypeName = "int",
            ScopeLevel = 2
        });
    }
}

/// <summary>
/// foreachブロック
/// </summary>
public class ForEachBlock : BlockBase
{
    public override BlockType BlockType => BlockType.ControlStructure;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "各要素について繰り返す";
    public override string CodeTemplate => "foreach (var {0} in {1})\n{{\n{2}\n}}";

    public ForEachBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "VariableName",
            Label = "変数名",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "item"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Collection",
            Label = "コレクション",
            TypeName = "object",
            InputType = ParameterInputType.Block,
            Value = "array"
        });
    }

    public override string CodeOutput(int level)
    {
        var varName = Parameters[0].GetValueAsString();
        var collection = Parameters[1].GetValueAsString();
        var innerCode = GenerateInnerBlocksCode(level);

        var code = $"{GetIndent(level)}foreach (var {varName} in {collection})\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}{GenerateNextBlockCode(level)}";
        return code;
    }

    protected override void AddDefinedVariables(List<VariableInfo> variables)
    {
        variables.Add(new VariableInfo
        {
            Name = Parameters[0].GetValueAsString(),
            TypeName = "var",
            ScopeLevel = 2
        });
    }
}

/// <summary>
/// breakブロック
/// </summary>
public class BreakBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "繰り返しを抜ける";
    public override string CodeTemplate => "break;";

    public BreakBlock() { }

    public override string CodeOutput(int level)
    {
        return $"{GetIndent(level)}break;{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// continueブロック
/// </summary>
public class ContinueBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "次の繰り返しへ";
    public override string CodeTemplate => "continue;";

    public ContinueBlock() { }

    public override string CodeOutput(int level)
    {
        return $"{GetIndent(level)}continue;{GenerateNextBlockCode(level)}";
    }
}
