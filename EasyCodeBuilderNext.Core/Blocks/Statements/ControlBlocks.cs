using EasyCodeBuilderNext.Core.Models;

namespace EasyCodeBuilderNext.Core.Blocks.Statements;

/// <summary>
/// Ifブロック
/// </summary>
public class IfBlock : BlockBase
{
    public override BlockType BlockType => BlockType.ControlStructure;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "もし～なら";
    public override string CodeTemplate => "if ({0}) {{ }}";

    public IfBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Condition",
            Label = "条件",
            TypeName = "bool",
            InputType = ParameterInputType.Block
        });

        Height = 60;
    }

    public override string CodeOutput(int level)
    {
        var condition = Parameters[0].GetValueAsString();
        var innerCode = GenerateInnerBlocksCode(level);

        var code = $"{GetIndent(level)}if ({condition})\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}";

        // else節がある場合
        if (AdditionalInnerBlocks.Count > 0)
        {
            var elseBlocks = AdditionalInnerBlocks[0];
            if (elseBlocks.Count > 0)
            {
                var elseCode = new System.Text.StringBuilder();
                foreach (var block in elseBlocks)
                {
                    elseCode.AppendLine(block.CodeOutput(level + 1));
                }
                code += $"\n{GetIndent(level)}else\n{GetIndent(level)}{{\n{elseCode.ToString().TrimEnd()}\n{GetIndent(level)}}}";
            }
            else
            {
                code += $"\n{GetIndent(level)}else\n{GetIndent(level)}{{\n{GetIndent(level + 1)}// 空のブロック\n{GetIndent(level)}}}";
            }
        }

        return code + GenerateNextBlockCode(level);
    }
}

/// <summary>
/// If-Elseブロック
/// </summary>
public class IfElseBlock : IfBlock
{
    public IfElseBlock() : base()
    {
        AdditionalInnerBlocks.Add(new ObservableCollection<BlockBase>());
        Height = 80;
    }
}

/// <summary>
/// Whileブロック
/// </summary>
public class WhileBlock : BlockBase
{
    public override BlockType BlockType => BlockType.ControlStructure;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "～の間繰り返す";
    public override string CodeTemplate => "while ({0}) {{ }}";

    public WhileBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Condition",
            Label = "条件",
            TypeName = "bool",
            InputType = ParameterInputType.Block
        });

        Height = 60;
    }

    public override string CodeOutput(int level)
    {
        var condition = Parameters[0].GetValueAsString();
        var innerCode = GenerateInnerBlocksCode(level);

        return $"{GetIndent(level)}while ({condition})\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// Forブロック
/// </summary>
public class ForBlock : BlockBase
{
    public override BlockType BlockType => BlockType.ControlStructure;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "回数繰り返す";
    public override string CodeTemplate => "for (int {0} = 0; {0} < {1}; {0}++) {{ }}";

    public ForBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Counter",
            Label = "カウンタ変数",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "i"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Count",
            Label = "回数",
            TypeName = "int",
            InputType = ParameterInputType.Block
        });

        Height = 60;
    }

    public override string CodeOutput(int level)
    {
        var counter = Parameters[0].GetValueAsString();
        var count = Parameters[1].GetValueAsString();
        var innerCode = GenerateInnerBlocksCode(level);

        return $"{GetIndent(level)}for (int {counter} = 0; {counter} < {count}; {counter}++)\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}{GenerateNextBlockCode(level)}";
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
/// ForEachブロック
/// </summary>
public class ForEachBlock : BlockBase
{
    public override BlockType BlockType => BlockType.ControlStructure;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "各要素について繰り返す";
    public override string CodeTemplate => "foreach (var {0} in {1}) {{ }}";

    public ForEachBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Item",
            Label = "要素変数",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "item"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Collection",
            Label = "コレクション",
            TypeName = "object",
            InputType = ParameterInputType.Block
        });

        Height = 60;
    }

    public override string CodeOutput(int level)
    {
        var item = Parameters[0].GetValueAsString();
        var collection = Parameters[1].GetValueAsString();
        var innerCode = GenerateInnerBlocksCode(level);

        return $"{GetIndent(level)}foreach (var {item} in {collection})\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}{GenerateNextBlockCode(level)}";
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
/// Breakブロック
/// </summary>
public class BreakBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Terminal;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "繰り返しを抜ける";
    public override string CodeTemplate => "break;";

    public override string CodeOutput(int level)
    {
        return $"{GetIndent(level)}break;{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// Continueブロック
/// </summary>
public class ContinueBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Terminal;
    public override BlockCategory Category => BlockCategory.Control;
    public override string DisplayName => "次の繰り返しへ";
    public override string CodeTemplate => "continue;";

    public override string CodeOutput(int level)
    {
        return $"{GetIndent(level)}continue;{GenerateNextBlockCode(level)}";
    }
}
