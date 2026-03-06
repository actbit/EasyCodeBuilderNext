using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.Blocks.Statements;
using EasyCodeBuilderNext.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.ObjectModel;
using System.Text;

namespace EasyCodeBuilderNext.Core.CodeGeneration;

/// <summary>
/// Roslynを使用したコード生成器
/// </summary>
public class RoslynCodeGenerator
{
    /// <summary>
    /// プロジェクト全体からコードを生成
    /// </summary>
    public string GenerateCode(Project project)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// 自動生成されたコード");
        sb.AppendLine("// EasyCodeBuilderNext");
        sb.AppendLine();

        // usingディレクティブ
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();

        // 名前空間
        sb.AppendLine($"namespace {project.DefaultNamespace}");
        sb.AppendLine("{");

        foreach (var obj in project.Objects)
        {
            var classCode = GenerateClassCode(obj, 1);
            sb.AppendLine(classCode);
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// 単一のオブジェクトからコードを生成
    /// </summary>
    public string GenerateCode(CodeObject obj)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// 自動生成されたコード");
        sb.AppendLine("// EasyCodeBuilderNext");
        sb.AppendLine();

        // usingディレクティブ
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();

        // 名前空間
        sb.AppendLine($"namespace {obj.Namespace}");
        sb.AppendLine("{");

        var classCode = GenerateClassCode(obj, 1);
        sb.AppendLine(classCode);

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// クラスコードを生成
    /// </summary>
    private string GenerateClassCode(CodeObject obj, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        var sb = new StringBuilder();

        // クラス修飾子
        var modifiers = new List<string> { GetAccessibilityString(obj.Accessibility) };
        if (obj.IsStatic) modifiers.Add("static");
        if (obj.IsAbstract) modifiers.Add("abstract");
        if (obj.IsSealed) modifiers.Add("sealed");

        var modifierStr = string.Join(" ", modifiers);

        // 継承
        var inheritance = new List<string>();
        if (!string.IsNullOrEmpty(obj.BaseClassName)) inheritance.Add(obj.BaseClassName);
        foreach (var iface in obj.ImplementedInterfaces)
        {
            inheritance.Add(iface);
        }
        var inheritanceStr = inheritance.Count > 0 ? " : " + string.Join(", ", inheritance) : "";

        sb.AppendLine($"{indent}{modifierStr} class {obj.Name}{inheritanceStr}");
        sb.AppendLine($"{indent}{{");

        // メンバー（フィールド、プロパティ、メソッド）
        foreach (var member in obj.Members)
        {
            var memberCode = GenerateMemberCode(member, indentLevel + 1);
            sb.AppendLine(memberCode);
        }

        // ブロックからのコード生成
        foreach (var block in obj.Blocks)
        {
            var blockCode = block.CodeOutput(indentLevel + 1);
            sb.AppendLine(blockCode);
        }

        sb.AppendLine($"{indent}}}");

        return sb.ToString();
    }

    /// <summary>
    /// メンバーコードを生成
    /// </summary>
    private string GenerateMemberCode(MemberInfo member, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        var accessStr = GetAccessibilityString(member.Accessibility);
        var staticStr = member.IsStatic ? "static " : "";

        return member.Kind switch
        {
            MemberKind.Field => $"{indent}{accessStr} {staticStr}{member.ReturnType} {member.Name};",
            MemberKind.Property => GeneratePropertyCode(member, indent, accessStr, staticStr),
            MemberKind.Method => GenerateMethodCode(member, indent, accessStr, staticStr),
            MemberKind.Constructor => GenerateConstructorCode(member, indent, accessStr),
            _ => $"// Unknown member type: {member.Kind}"
        };
    }

    /// <summary>
    /// プロパティコードを生成
    /// </summary>
    private string GeneratePropertyCode(MemberInfo member, string indent, string accessStr, string staticStr)
    {
        return $"{indent}{accessStr} {staticStr}{member.ReturnType} {member.Name} {{ get; set; }}";
    }

    /// <summary>
    /// メソッドコードを生成
    /// </summary>
    private string GenerateMethodCode(MemberInfo member, string indent, string accessStr, string staticStr)
    {
        var parameters = string.Join(", ", member.Parameters.Select(p => $"{p.TypeName} {p.Name}"));
        var sb = new StringBuilder();

        sb.AppendLine($"{indent}{accessStr} {staticStr}{member.ReturnType} {member.Name}({parameters})");
        sb.AppendLine($"{indent}{{");

        if (member.AssociatedBlock != null)
        {
            var blockCode = member.AssociatedBlock.CodeOutput(2);
            sb.AppendLine(blockCode);
        }
        else
        {
            sb.AppendLine($"{indent}    // TODO: メソッド本体を実装");
        }

        sb.AppendLine($"{indent}}}");

        return sb.ToString();
    }

    /// <summary>
    /// コンストラクタコードを生成
    /// </summary>
    private string GenerateConstructorCode(MemberInfo member, string indent, string accessStr)
    {
        var parameters = string.Join(", ", member.Parameters.Select(p => $"{p.TypeName} {p.Name}"));
        var sb = new StringBuilder();

        sb.AppendLine($"{indent}{accessStr} {member.Name}({parameters})");
        sb.AppendLine($"{indent}{{");
        sb.AppendLine($"{indent}    // TODO: コンストラクタ本体を実装");
        sb.AppendLine($"{indent}}}");

        return sb.ToString();
    }

    /// <summary>
    /// アクセシビリティを文字列に変換
    /// </summary>
    private static string GetAccessibilityString(Models.Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedInternal => "protected internal",
            Accessibility.PrivateProtected => "private protected",
            _ => "public"
        };
    }

    /// <summary>
    /// RoslynのSyntaxTreeを使用してより高度なコード生成
    /// </summary>
    public CompilationUnitSyntax GenerateSyntaxTree(CodeObject obj)
    {
        // クラス宣言
        var classDeclaration = SyntaxFactory.ClassDeclaration(obj.Name)
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword)
            ))
            .AddMembers(GenerateMembers(obj));

        // 名前空間
        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(
            SyntaxFactory.ParseName(obj.Namespace))
            .AddMembers(classDeclaration);

        // usingディレクティブ
        var usingDirectives = new[]
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq"))
        };

        // コンパイルユニット
        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirectives)
            .AddMembers(namespaceDeclaration);

        return compilationUnit;
    }

    /// <summary>
    /// メンバーのSyntax配列を生成
    /// </summary>
    private MemberDeclarationSyntax[] GenerateMembers(CodeObject obj)
    {
        var members = new List<MemberDeclarationSyntax>();

        // メンバー情報から生成
        foreach (var member in obj.Members)
        {
            switch (member.Kind)
            {
                case MemberKind.Field:
                    members.Add(GenerateFieldSyntax(member));
                    break;
                case MemberKind.Property:
                    members.Add(GeneratePropertySyntax(member));
                    break;
                case MemberKind.Method:
                    members.Add(GenerateMethodSyntax(member));
                    break;
            }
        }

        // ブロックからメソッド生成
        var methods = GenerateMethodsFromBlocks(obj);
        members.AddRange(methods);

        return members.ToArray();
    }

    /// <summary>
    /// フィールドのSyntaxを生成
    /// </summary>
    private FieldDeclarationSyntax GenerateFieldSyntax(MemberInfo member)
    {
        var variableDeclaration = SyntaxFactory.VariableDeclaration(
            SyntaxFactory.ParseTypeName(member.ReturnType))
            .AddVariables(SyntaxFactory.VariableDeclarator(member.Name));

        var modifiers = new List<SyntaxToken>
        {
            SyntaxFactory.Token(SyntaxKind.PublicKeyword)
        };

        if (member.IsStatic)
        {
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        return SyntaxFactory.FieldDeclaration(variableDeclaration)
            .WithModifiers(SyntaxFactory.TokenList(modifiers));
    }

    /// <summary>
    /// プロパティのSyntaxを生成
    /// </summary>
    private PropertyDeclarationSyntax GeneratePropertySyntax(MemberInfo member)
    {
        var accessors = SyntaxFactory.AccessorList(
            SyntaxFactory.List(new[]
            {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            }));

        var modifiers = new List<SyntaxToken>
        {
            SyntaxFactory.Token(SyntaxKind.PublicKeyword)
        };

        if (member.IsStatic)
        {
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        return SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.ParseTypeName(member.ReturnType),
            member.Name)
            .WithModifiers(SyntaxFactory.TokenList(modifiers))
            .WithAccessors(accessors);
    }

    /// <summary>
    /// メソッドのSyntaxを生成
    /// </summary>
    private MethodDeclarationSyntax GenerateMethodSyntax(MemberInfo member)
    {
        var parameters = member.Parameters.Select(p =>
            SyntaxFactory.Parameter(SyntaxFactory.ParseToken(p.Name))
                .WithType(SyntaxFactory.ParseTypeName(p.TypeName))).ToArray();

        var modifiers = new List<SyntaxToken>
        {
            SyntaxFactory.Token(SyntaxKind.PublicKeyword)
        };

        if (member.IsStatic)
        {
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        var body = member.AssociatedBlock != null
            ? SyntaxFactory.ParseStatement(member.AssociatedBlock.CodeOutput(1))
            : SyntaxFactory.Block();

        return SyntaxFactory.MethodDeclaration(
            SyntaxFactory.ParseTypeName(member.ReturnType),
            member.Name)
            .WithModifiers(SyntaxFactory.TokenList(modifiers))
            .AddParameterListParameters(parameters)
            .WithBody(SyntaxFactory.Block(body as StatementSyntax));
    }

    /// <summary>
    /// ブロックからメソッドを生成
    /// </summary>
    private IEnumerable<MethodDeclarationSyntax> GenerateMethodsFromBlocks(CodeObject obj)
    {
        // 各ブロックチェーンの先頭を取得
        var rootBlocks = obj.Blocks.Where(b => b.PreviousBlock == null);

        foreach (var block in rootBlocks)
        {
            // メソッド定義ブロックからメソッドを生成
            if (block is MethodDefineBlock methodBlock)
            {
                yield return GenerateMethodFromBlock(methodBlock);
            }
        }
    }

    /// <summary>
    /// メソッド定義ブロックからメソッドSyntaxを生成
    /// </summary>
    private MethodDeclarationSyntax GenerateMethodFromBlock(MethodDefineBlock block)
    {
        var returnType = block.Parameters[0].GetValueAsString();
        var methodName = block.Parameters[1].GetValueAsString();
        var parameters = block.Parameters[2].GetValueAsString();
        var isStatic = block.Parameters[3].GetValueAsString() == "true";

        var modifiers = new List<SyntaxToken>
        {
            SyntaxFactory.Token(SyntaxKind.PublicKeyword)
        };

        if (isStatic)
        {
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        var bodyStatements = block.InnerBlocks
            .Select(b => SyntaxFactory.ParseStatement(b.CodeOutput(1)))
            .Where(s => s != null)
            .Cast<StatementSyntax>()
            .ToArray();

        return SyntaxFactory.MethodDeclaration(
            SyntaxFactory.ParseTypeName(returnType),
            methodName)
            .WithModifiers(SyntaxFactory.TokenList(modifiers))
            .WithBody(SyntaxFactory.Block(bodyStatements));
    }
}
