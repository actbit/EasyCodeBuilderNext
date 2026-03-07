using EasyCodeBuilderNext.Core.Models;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Core.Blocks.Statements;

/// <summary>
/// Mainメソッドブロック（プログラムの開始点）
/// </summary>
public class MainMethodBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Hat;
    public override BlockCategory Category => BlockCategory.Methods;
    public override string DisplayName => "プログラム開始 (Main)";
    public override string CodeTemplate => "static void Main(string[] args)\n{{\n{0}\n}}";

    public override bool HasTopConnector => false;
    public override bool HasBottomConnector => true;

    public MainMethodBlock()
    {
        // 内部ブロック用
    }

    public override string CodeOutput(int level)
    {
        var innerCode = GenerateInnerBlocksCode(level);
        return $"{GetIndent(level)}static void Main(string[] args)\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}";
    }
}

/// <summary>
/// メソッド定義ブロック
/// </summary>
public class MethodDefineBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Definition;
    public override BlockCategory Category => BlockCategory.Methods;
    public override string DisplayName => "メソッド定義: ～";
    public override string CodeTemplate => "{0} {1}({2})\n{{\n{3}\n}}";

    public override bool HasTopConnector => false;
    public override bool HasBottomConnector => false; // 下にはつなげない

    public MethodDefineBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "ReturnType",
            Label = "戻り値の型",
            TypeName = "string",
            InputType = ParameterInputType.Dropdown,
            Value = "void"
        });
        foreach (var option in new[] { "void", "int", "string", "bool", "double", "float", "object" })
        {
            Parameters[0].Options.Add(option);
        }

        Parameters.Add(new BlockParameter
        {
            Name = "MethodName",
            Label = "メソッド名",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "MyMethod"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Parameters",
            Label = "パラメータ",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "",
            IsRequired = false
        });

        Parameters.Add(new BlockParameter
        {
            Name = "IsStatic",
            Label = "静的",
            TypeName = "bool",
            InputType = ParameterInputType.Checkbox,
            Value = false
        });
    }

    public override string CodeOutput(int level)
    {
        var returnType = Parameters[0].GetValueAsString();
        var methodName = Parameters[1].GetValueAsString();
        var parameters = Parameters[2].GetValueAsString();
        var isStatic = (bool?)Parameters[3].Value == true;
        var innerCode = GenerateInnerBlocksCode(level);

        var staticModifier = isStatic ? "static " : "";
        return $"{GetIndent(level)}public {staticModifier}{returnType} {methodName}({parameters})\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}";
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
            InputType = ParameterInputType.Text,
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
            Value = "",
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
            Value = "",
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
/// returnブロック
/// </summary>
public class ReturnBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
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
            Value = "",
            IsRequired = false
        });
    }

    public override string CodeOutput(int level)
    {
        var value = Parameters[0].GetValueAsString();

        if (string.IsNullOrEmpty(value))
        {
            return $"{GetIndent(level)}return;{GenerateNextBlockCode(level)}";
        }

        return $"{GetIndent(level)}return {value};{GenerateNextBlockCode(level)}";
    }
}
