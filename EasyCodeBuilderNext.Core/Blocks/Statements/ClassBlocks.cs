using EasyCodeBuilderNext.Core.Models;

namespace EasyCodeBuilderNext.Core.Blocks.Statements;

/// <summary>
/// クラス定義ブロック
/// </summary>
public class ClassDefineBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Definition;
    public override BlockCategory Category => BlockCategory.Classes;
    public override string DisplayName => "クラスを定義";
    public override string CodeTemplate => "class {0} {{ }}";

    public ClassDefineBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "ClassName",
            Label = "クラス名",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "MyClass"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "BaseClass",
            Label = "基底クラス",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            IsRequired = false
        });

        Height = 80;
    }

    public override bool HasTopConnector => false;
    public override bool HasBottomConnector => false;
    public override bool HasInnerConnector => true;

    public override string CodeOutput(int level)
    {
        var className = Parameters[0].GetValueAsString();
        var baseClass = Parameters[1].GetValueAsString();
        var innerCode = GenerateInnerBlocksCode(level);

        var declaration = string.IsNullOrEmpty(baseClass)
            ? $"{GetIndent(level)}class {className}"
            : $"{GetIndent(level)}class {className} : {baseClass}";

        return $"{declaration}\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}";
    }
}

/// <summary>
/// メソッド定義ブロック
/// </summary>
public class MethodDefineBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Definition;
    public override BlockCategory Category => BlockCategory.Methods;
    public override string DisplayName => "メソッドを定義";
    public override string CodeTemplate => "{0} {1}({2}) {{ }}";

    public MethodDefineBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "ReturnType",
            Label = "戻り値の型",
            TypeName = "string",
            InputType = ParameterInputType.TypeSelector,
            Value = "void"
        });

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

        Height = 80;
    }

    public override bool HasTopConnector => false;
    public override bool HasBottomConnector => false;
    public override bool HasInnerConnector => true;

    public override string CodeOutput(int level)
    {
        var returnType = Parameters[0].GetValueAsString();
        var methodName = Parameters[1].GetValueAsString();
        var parameters = Parameters[2].GetValueAsString();
        var isStatic = Parameters[3].Value as bool? ?? false;

        var staticModifier = isStatic ? "static " : "";
        var innerCode = GenerateInnerBlocksCode(level);

        return $"{GetIndent(level)}public {staticModifier}{returnType} {methodName}({parameters})\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}";
    }
}

/// <summary>
/// フィールド定義ブロック
/// </summary>
public class FieldDefineBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Classes;
    public override string DisplayName => "フィールドを定義";
    public override string CodeTemplate => "{0} {1};";

    public FieldDefineBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Type",
            Label = "型",
            TypeName = "string",
            InputType = ParameterInputType.TypeSelector,
            Value = "int"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Name",
            Label = "名前",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "field"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "InitialValue",
            Label = "初期値",
            TypeName = "object",
            InputType = ParameterInputType.Block,
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
        var type = Parameters[0].GetValueAsString();
        var name = Parameters[1].GetValueAsString();
        var initialValue = Parameters[2].GetValueAsString();
        var isStatic = Parameters[3].Value as bool? ?? false;

        var staticModifier = isStatic ? "static " : "";

        if (string.IsNullOrEmpty(initialValue))
        {
            return $"{GetIndent(level)}public {staticModifier}{type} {name};{GenerateNextBlockCode(level)}";
        }

        return $"{GetIndent(level)}public {staticModifier}{type} {name} = {initialValue};{GenerateNextBlockCode(level)}";
    }
}

/// <summary>
/// プロパティ定義ブロック
/// </summary>
public class PropertyDefineBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Classes;
    public override string DisplayName => "プロパティを定義";
    public override string CodeTemplate => "{0} {1} {{ get; set; }}";

    public PropertyDefineBlock()
    {
        Parameters.Add(new BlockParameter
        {
            Name = "Type",
            Label = "型",
            TypeName = "string",
            InputType = ParameterInputType.TypeSelector,
            Value = "string"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Name",
            Label = "名前",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "Property"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "InitialValue",
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
        var initialValue = Parameters[2].GetValueAsString();

        if (string.IsNullOrEmpty(initialValue))
        {
            return $"{GetIndent(level)}public {type} {name} {{ get; set; }}{GenerateNextBlockCode(level)}";
        }

        return $"{GetIndent(level)}public {type} {name} {{ get; set; }} = {initialValue};{GenerateNextBlockCode(level)}";
    }
}
