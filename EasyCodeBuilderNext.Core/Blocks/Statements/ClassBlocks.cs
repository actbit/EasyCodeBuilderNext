using EasyCodeBuilderNext.Core.Blocks.Expressions;
using EasyCodeBuilderNext.Core.Models;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Core.Blocks.Statements;

/// <summary>
/// クラス定義ブロック
/// </summary>
public class ClassDefineBlock : BlockBase
{
    public override BlockType BlockType => BlockType.Statement;
    public override BlockCategory Category => BlockCategory.Classes;
    public override string DisplayName => "クラスを定義";
    public override string CodeTemplate => "class {0}\n{{\n{1}\n}}";

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
            Value = "",
            IsRequired = false
        });

        Parameters.Add(new BlockParameter
        {
            Name = "Interfaces",
            Label = "インターフェース",
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
            Value = "false"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "IsAbstract",
            Label = "抽象",
            TypeName = "bool",
            InputType = ParameterInputType.Checkbox,
            Value = "false"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "IsSealed",
            Label = "シール",
            TypeName = "bool",
            InputType = ParameterInputType.Checkbox,
            Value = "false"
        });
    }

    public override string CodeOutput(int level)
    {
        var className = Parameters[0].GetValueAsString();
        var baseClass = Parameters[1].GetValueAsString();
        var interfaces = Parameters[2].GetValueAsString();
        var isStatic = Parameters[3].GetValueAsString() == "true";
        var isAbstract = Parameters[4].GetValueAsString() == "true";
        var isSealed = Parameters[5].GetValueAsString() == "true";
        var innerCode = GenerateInnerBlocksCode(level);

        var modifiers = new List<string>();
        if (isStatic) modifiers.Add("static");
        if (isAbstract) modifiers.Add("abstract");
        if (isSealed) modifiers.Add("sealed");

        var modifierStr = modifiers.Count > 0 ? string.Join(" ", modifiers) + " " : "";

        var inheritance = new List<string>();
        if (!string.IsNullOrEmpty(baseClass)) inheritance.Add(baseClass);
        if (!string.IsNullOrEmpty(interfaces)) inheritance.AddRange(interfaces.Split(',', ';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));

        var inheritanceStr = inheritance.Count > 0 ? " : " + string.Join(", ", inheritance) : "";

        var code = $"{GetIndent(level)}public {modifierStr}class {className}{inheritanceStr}\n{GetIndent(level)}{{\n{innerCode}\n{GetIndent(level)}}}{GenerateNextBlockCode(level)}";
        return code;
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
    public override string CodeTemplate => "{0} {1} {2};";

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

        var accessParam = new BlockParameter
        {
            Name = "AccessModifier",
            Label = "アクセス修飾子",
            TypeName = "string",
            InputType = ParameterInputType.Dropdown,
            Value = "private"
        };
        accessParam.Options.Add("public");
        accessParam.Options.Add("private");
        accessParam.Options.Add("protected");
        accessParam.Options.Add("internal");
        Parameters.Add(accessParam);

        Parameters.Add(new BlockParameter
        {
            Name = "IsStatic",
            Label = "静的",
            TypeName = "bool",
            InputType = ParameterInputType.Checkbox,
            Value = "false"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "IsReadonly",
            Label = "読み取り専用",
            TypeName = "bool",
            InputType = ParameterInputType.Checkbox,
            Value = "false"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "InitialValue",
            Label = "初期値",
            TypeName = "object",
            InputType = ParameterInputType.Block,
            Value = "",
            IsRequired = false
        });
    }

    public override string CodeOutput(int level)
    {
        var type = Parameters[0].GetValueAsString();
        var name = Parameters[1].GetValueAsString();
        var accessModifier = Parameters[2].GetValueAsString();
        var isStatic = Parameters[3].GetValueAsString() == "true";
        var isReadonly = Parameters[4].GetValueAsString() == "true";
        var initialValue = Parameters[5].GetValueAsString();

        var modifiers = new List<string> { accessModifier };
        if (isStatic) modifiers.Add("static");
        if (isReadonly) modifiers.Add("readonly");

        var modifierStr = string.Join(" ", modifiers);

        if (string.IsNullOrEmpty(initialValue))
        {
            return $"{GetIndent(level)}{modifierStr} {type} {name};{GenerateNextBlockCode(level)}";
        }

        return $"{GetIndent(level)}{modifierStr} {type} {name} = {initialValue};{GenerateNextBlockCode(level)}";
    }

    protected override void AddDefinedVariables(List<VariableInfo> variables)
    {
        variables.Add(new VariableInfo
        {
            Name = Parameters[1].GetValueAsString(),
            TypeName = Parameters[0].GetValueAsString(),
            ScopeLevel = 1
        });
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
    public override string CodeTemplate => "{0} {1} {2} {{ get; set; }}";

    public PropertyDefineBlock()
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
            Value = "Property"
        });

        var accessParam = new BlockParameter
        {
            Name = "AccessModifier",
            Label = "アクセス修飾子",
            TypeName = "string",
            InputType = ParameterInputType.Dropdown,
            Value = "public"
        };
        accessParam.Options.Add("public");
        accessParam.Options.Add("private");
        accessParam.Options.Add("protected");
        accessParam.Options.Add("internal");
        Parameters.Add(accessParam);

        Parameters.Add(new BlockParameter
        {
            Name = "HasGetter",
            Label = "getter",
            TypeName = "bool",
            InputType = ParameterInputType.Checkbox,
            Value = "true"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "HasSetter",
            Label = "setter",
            TypeName = "bool",
            InputType = ParameterInputType.Checkbox,
            Value = "true"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "InitialValue",
            Label = "初期値",
            TypeName = "object",
            InputType = ParameterInputType.Block,
            Value = "",
            IsRequired = false
        });
    }

    public override string CodeOutput(int level)
    {
        var type = Parameters[0].GetValueAsString();
        var name = Parameters[1].GetValueAsString();
        var accessModifier = Parameters[2].GetValueAsString();
        var hasGetter = Parameters[3].GetValueAsString() == "true";
        var hasSetter = Parameters[4].GetValueAsString() == "true";
        var initialValue = Parameters[5].GetValueAsString();

        var accessors = new List<string>();
        if (hasGetter) accessors.Add("get");
        if (hasSetter) accessors.Add("set");

        var accessorStr = accessors.Count > 0 ? "{ " + string.Join("; ", accessors) + "; }" : "{}";

        if (string.IsNullOrEmpty(initialValue))
        {
            return $"{GetIndent(level)}{accessModifier} {type} {name} {accessorStr}{GenerateNextBlockCode(level)}";
        }

        return $"{GetIndent(level)}{accessModifier} {type} {name} {accessorStr} = {initialValue};{GenerateNextBlockCode(level)}";
    }

    protected override void AddDefinedVariables(List<VariableInfo> variables)
    {
        variables.Add(new VariableInfo
        {
            Name = Parameters[1].GetValueAsString(),
            TypeName = Parameters[0].GetValueAsString(),
            ScopeLevel = 1
        });
    }
}

/// <summary>
/// プロパティアクセスブロック（式ブロック）
/// </summary>
public class PropertyAccessBlock : ExpressionBlockBase
{
    public override BlockType BlockType => BlockType.Expression;
    public override BlockCategory Category => BlockCategory.Classes;
    public override string DisplayName => "プロパティアクセス";
    public override string CodeTemplate => "{0}.{1}";
    public override string ReturnType => "object";

    public PropertyAccessBlock()
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
            Name = "PropertyName",
            Label = "プロパティ名",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "Property"
        });
    }

    public override string CodeOutput(int level)
    {
        var objectName = Parameters[0].GetValueAsString();
        var propertyName = Parameters[1].GetValueAsString();

        if (objectName == "this" || string.IsNullOrEmpty(objectName))
        {
            return propertyName;
        }

        return $"{objectName}.{propertyName}";
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
            InputType = ParameterInputType.Text,
            Value = "this"
        });

        Parameters.Add(new BlockParameter
        {
            Name = "PropertyName",
            Label = "プロパティ名",
            TypeName = "string",
            InputType = ParameterInputType.Text,
            Value = "Property"
        });

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
        var objectName = Parameters[0].GetValueAsString();
        var propertyName = Parameters[1].GetValueAsString();
        var value = Parameters[2].GetValueAsString();

        if (objectName == "this" || string.IsNullOrEmpty(objectName))
        {
            return $"{GetIndent(level)}{propertyName} = {value};{GenerateNextBlockCode(level)}";
        }

        return $"{GetIndent(level)}{objectName}.{propertyName} = {value};{GenerateNextBlockCode(level)}";
    }
}
