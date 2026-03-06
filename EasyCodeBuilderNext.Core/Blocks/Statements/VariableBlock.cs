using EasyCodeBuilderNext.Core.Models;

namespace EasyCodeBuilderNext.Core.Blocks.Statements;

/// <summary>
/// 変数宣言ブロック
/// </summary>
public class VariableDeclareBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Variables;
    public override string DisplayName => "変数を宣言";
    public override string CodeTemplate => "{0} {1} = {2};";

    public VariableDeclareBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Type",
            Label = "型",
            TypeName = "string",
            InputType = ParameterInputType.TypeSelector,
            Value = "var"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Name",
            Label = "名前",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "variable"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Value",
            Label = "初期値",
            TypeName = "object",
            InputType = ParameterInputType.Block,
            IsRequired = false
        });
    }

    public override string CodeOutput(int level)
    {
        var type = Parameters[0].GetValueAsString();
        var name = Parameters[1].GetValueAsString();
        var value = Parameters[2].GetValueAsString();

        if (string.IsNullOrEmpty(value))
        {
            return $"{GetIndent(level)}{type} {name};{GenerateNextBlockCode(level)}";
        }

        return $"{GetIndent(level)}{type} {name} = {value};{GenerateNextBlockCode(level)}";
    }

    protected override void AddDefinedVariables(List<VariableInfo> variables)
    {
        variables.Add(new VariableInfo
        {
            Name = Parameters[1].GetValueAsString(),
            TypeName = Parameters[0].GetValueAsString(),
            ScopeLevel = OwnerObject != null ? 2 : 1
        });
    }
}

/// <summary>
/// 変数代入ブロック
/// </summary>
public class VariableAssignBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Variables;
    public override string DisplayName => "変数に代入";
    public override string CodeTemplate => "{0} = {1};";

    public VariableAssignBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Variable",
            Label = "変数",
            TypeName = "string",
            InputType = ParameterInputType.Variable,
            Value = ""
        });

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
        var variable = Parameters[0].GetValueAsString();
        var value = Parameters[1].GetValueAsString();

        return $"{GetIndent(level)}{variable} = {value};{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// ローカル変数宣言ブロック（var使用）
/// </summary>
public class LocalVariableBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Variables;
    public override string DisplayName => "ローカル変数";
    public override string CodeTemplate => "var {0} = {1};";

    public LocalVariableBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Name",
            Label = "名前",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "value"
        });

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
        var name = Parameters[0].GetValueAsString();
        var value = Parameters[1].GetValueAsString();

        return $"{GetIndent(level)}var {name} = {value};{GenerateNextBlockCode(level)}";
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
