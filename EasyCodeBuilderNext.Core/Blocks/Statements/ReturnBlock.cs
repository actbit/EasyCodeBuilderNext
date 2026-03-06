using EasyCodeBuilderNext.Core.Models;

namespace EasyCodeBuilderNext.Core.Blocks.Statements;

/// <summary>
/// Return文ブロック
/// </summary>
public class ReturnBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Terminal;
    public override BlockCategory Category => BlockCategory.Methods;
    public override string DisplayName => "戻り値";
    public override string CodeTemplate => "return {0};";

    public ReturnBlock()
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
            return $"{GetIndent(level)}return;";
        }

        return $"{GetIndent(level)}return {value};";
    }
}

/// <summary>
/// メソッド呼び出しブロック
/// </summary>
public class MethodCallBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Methods;
    public override string DisplayName => "メソッド呼び出し";
    public override string CodeTemplate => "{0}.{1}({2});";

    public MethodCallBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "ObjectName",
            Label = "オブジェクト",
            TypeName = "string",
            InputType = ParameterInputType.Variable,
            Value = "this"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "MethodName",
            Label = "メソッド名",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "Method"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Arguments",
            Label = "引数",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            IsRequired = false
        });
    }

    public override string CodeOutput(int level)
    {
        var objectName = Parameters[0].GetValueAsString();
        var methodName = Parameters[1].GetValueAsString();
        var arguments = Parameters[2].GetValueAsString();

        if (objectName == "this" || string.IsNullOrEmpty(objectName))
        {
            return $"{GetIndent(level)}{methodName}({arguments});{GenerateNextBlockCode(level)}";
        }

        return $"{GetIndent(level)}{objectName}.{methodName}({arguments});{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// 静的メソッド呼び出しブロック
/// </summary>
public class StaticMethodCallBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Methods;
    public override string DisplayName => "静的メソッド呼び出し";
    public override string CodeTemplate => "{0}.{1}({2});";

    public StaticMethodCallBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "ClassName",
            Label = "クラス名",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "Console"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "MethodName",
            Label = "メソッド名",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "WriteLine"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Arguments",
            Label = "引数",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            IsRequired = false
        });
    }

    public override string CodeOutput(int level)
    {
        var className = Parameters[0].GetValueAsString();
        var methodName = Parameters[1].GetValueAsString();
        var arguments = Parameters[2].GetValueAsString();

        return $"{GetIndent(level)}{className}.{methodName}({arguments});{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// プロパティアクセスブロック
/// </summary>
public class PropertyAccessBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Classes;
    public override string DisplayName => "プロパティアクセス";
    public override string CodeTemplate => "{0}.{1};";

    public PropertyAccessBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "ObjectName",
            Label = "オブジェクト",
            TypeName = "string",
            InputType = ParameterInputType.Variable
        });

        Parameters.Add(new BlockParameter
        {
            Name = "PropertyName",
            Label = "プロパティ名",
            TypeName = "string",
            InputType = ParameterInputType.Text
        });
    }

    public override string CodeOutput(int level)
    {
        var objectName = Parameters[0].GetValueAsString();
        var propertyName = Parameters[1].GetValueAsString();

        return $"{GetIndent(level)}{objectName}.{propertyName};{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// プロパティ代入ブロック
/// </summary>
public class PropertyAssignBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Classes;
    public override string DisplayName => "プロパティに代入";
    public override string CodeTemplate => "{0}.{1} = {2};";

    public PropertyAssignBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "ObjectName",
            Label = "オブジェクト",
            TypeName = "string",
            InputType = ParameterInputType.Variable
        });

        Parameters.Add(new BlockParameter
        {
            Name = "PropertyName",
            Label = "プロパティ名",
            TypeName = "string",
            InputType = ParameterInputType.Text
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
        var objectName = Parameters[0].GetValueAsString();
        var propertyName = Parameters[1].GetValueAsString();
        var value = Parameters[2].GetValueAsString();

        return $"{GetIndent(level)}{objectName}.{propertyName} = {value};{GenerateNextBlockCode(level)}";
    }
}
