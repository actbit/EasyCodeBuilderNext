using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Core.CodeGeneration;

/// <summary>
/// Roslynを使用したコード生成クラス
/// </summary>
public class RoslynCodeGenerator
{
    /// <summary>
    /// プロジェクトから完全なC#コードを生成
    /// </summary>
    public string GenerateCode(Project project)
    {
        var usings = project.Usings.Select(u => SyntaxFactory.UsingDirective(
            SyntaxFactory.ParseName(u))).ToArray();

        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(
            SyntaxFactory.ParseName(project.DefaultNamespace));

        foreach (var obj in project.Objects)
        {
            var classDeclaration = GenerateClass(obj);
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);
        }

        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usings)
            .AddMembers(namespaceDeclaration);

        return compilationUnit.NormalizeWhitespace().ToFullString();
    }

    /// <summary>
    /// 単一のオブジェクト（クラス）からコードを生成
    /// </summary>
    public string GenerateCode(CodeObject obj)
    {
        var usings = new[]
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq"))
        };

        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(
            SyntaxFactory.ParseName(obj.Namespace))
            .AddMembers(GenerateClass(obj));

        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usings)
            .AddMembers(namespaceDeclaration);

        return compilationUnit.NormalizeWhitespace().ToFullString();
    }

    /// <summary>
    /// クラス宣言を生成
    /// </summary>
    private ClassDeclarationSyntax GenerateClass(CodeObject obj)
    {
        var classDeclaration = SyntaxFactory.ClassDeclaration(obj.Name)
            .AddModifiers(GetAccessibilityToken(obj.Accessibility));

        if (obj.IsStatic)
        {
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        if (obj.IsAbstract)
        {
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.AbstractKeyword));
        }

        if (obj.IsSealed)
        {
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword));
        }

        // 基底クラス
        if (!string.IsNullOrEmpty(obj.BaseClassName))
        {
            classDeclaration = classDeclaration.WithBaseList(
                SyntaxFactory.BaseList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(obj.BaseClassName!))
                    })));
        }

        // インターフェース実装
        if (obj.ImplementedInterfaces.Count > 0)
        {
            var baseTypes = obj.ImplementedInterfaces
                .Select(i => SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(i)))
                .Cast<BaseTypeSyntax>()
                .ToList();

            if (classDeclaration.BaseList != null)
            {
                baseTypes.InsertRange(0, classDeclaration.BaseList.Types.Select(t => (BaseTypeSyntax)t));
            }

            classDeclaration = classDeclaration.WithBaseList(
                SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(baseTypes)));
        }

        // メンバーを追加
        foreach (var member in obj.Members)
        {
            var memberDeclaration = GenerateMember(member);
            if (memberDeclaration != null)
            {
                classDeclaration = classDeclaration.AddMembers(memberDeclaration);
            }
        }

        // ブロックからメンバーを生成
        foreach (var block in obj.Blocks)
        {
            var memberFromBlock = GenerateMemberFromBlock(block);
            if (memberFromBlock != null)
            {
                classDeclaration = classDeclaration.AddMembers(memberFromBlock);
            }
        }

        return classDeclaration;
    }

    /// <summary>
    /// アクセシビリティからSyntaxTokenを取得
    /// </summary>
    private SyntaxToken GetAccessibilityToken(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            Accessibility.Private => SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
            Accessibility.Protected => SyntaxFactory.Token(SyntaxFactory.ProtectedKeyword),
            Accessibility.Internal => SyntaxFactory.Token(SyntaxKind.InternalKeyword),
            Accessibility.ProtectedInternal => SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
            Accessibility.PrivateProtected => SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
            _ => SyntaxFactory.Token(SyntaxKind.PublicKeyword)
        };
    }

    /// <summary>
    /// メンバー宣言を生成
    /// </summary>
    private MemberDeclarationSyntax? GenerateMember(MemberInfo member)
    {
        return member.Kind switch
        {
            MemberKind.InstanceMethod or MemberKind.StaticMethod => GenerateMethod(member),
            MemberKind.Field or MemberKind.InstanceField or MemberKind.StaticField => GenerateField(member),
            MemberKind.Property => GenerateProperty(member),
            _ => null
        };
    }

    /// <summary>
    /// メソッド宣言を生成
    /// </summary>
    private MethodDeclarationSyntax? GenerateMethod(MemberInfo member)
    {
        var method = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.ParseTypeName(member.ReturnType),
            member.Name)
            .AddModifiers(GetAccessibilityToken(member.Accessibility));

        if (member.IsStatic)
        {
            method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        // パラメータ
        foreach (var param in member.Parameters)
        {
            method = method.AddParameterListParameters(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier(param.Name))
                    .WithType(SyntaxFactory.ParseTypeName(param.TypeName)));
        }

        // 関連ブロックから本体を生成
        if (member.AssociatedBlock != null)
        {
            var bodyCode = member.AssociatedBlock.CodeOutput(1);
            var statements = SyntaxFactory.ParseStatement(bodyCode);
            method = method.WithBody(SyntaxFactory.Block(statements));
        }
        else
        {
            method = method.WithBody(SyntaxFactory.Block());
        }

        return method;
    }

    /// <summary>
    /// フィールド宣言を生成
    /// </summary>
    private FieldDeclarationSyntax? GenerateField(MemberInfo member)
    {
        var field = SyntaxFactory.FieldDeclaration(
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.ParseTypeName(member.ReturnType))
            .AddVariables(SyntaxFactory.VariableDeclarator(member.Name)))
            .AddModifiers(GetAccessibilityToken(member.Accessibility));

        if (member.IsStatic)
        {
            field = field.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        return field;
    }

    /// <summary>
    /// プロパティ宣言を生成
    /// </summary>
    private PropertyDeclarationSyntax? GenerateProperty(MemberInfo member)
    {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.ParseTypeName(member.ReturnType),
            member.Name)
            .AddModifiers(GetAccessibilityToken(member.Accessibility))
            .AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

        return property;
    }

    /// <summary>
    /// ブロックからメンバーを生成
    /// </summary>
    private MemberDeclarationSyntax? GenerateMemberFromBlock(BlockBase block)
    {
        if (block is Statements.ClassDefineBlock classBlock)
        {
            // クラス定義ブロックは別のクラスとして生成
            var classObj = new CodeObject
            {
                Name = classBlock.Parameters[0].GetValueAsString(),
                BaseClassName = classBlock.Parameters[1].GetValueAsString()
            };

            foreach (var innerBlock in classBlock.InnerBlocks)
            {
                var innerMember = GenerateMemberFromBlock(innerBlock);
                if (innerMember != null)
                {
                    // 内部ブロックのメンバーを追加する処理が必要
                }
            }

            return null; // クラスはトップレベルで生成
        }

        if (block is Statements.MethodDefineBlock methodBlock)
        {
            var returnType = methodBlock.Parameters[0].GetValueAsString();
            var methodName = methodBlock.Parameters[1].GetValueAsString();
            var parameters = methodBlock.Parameters[2].GetValueAsString();
            var isStatic = methodBlock.Parameters[3].Value as bool? ?? false;

            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(returnType),
                methodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (isStatic)
            {
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            }

            // パラメータを解析
            if (!string.IsNullOrEmpty(parameters))
            {
                var paramList = SyntaxFactory.ParseParameterList($"({parameters})");
                method = method.WithParameterList(paramList);
            }

            // 本体を生成
            var bodyStatements = new List<StatementSyntax>();
            foreach (var innerBlock in methodBlock.InnerBlocks)
            {
                var code = innerBlock.CodeOutput(0);
                var stmt = SyntaxFactory.ParseStatement(code);
                if (stmt != null)
                {
                    bodyStatements.Add(stmt);
                }
            }

            method = method.WithBody(SyntaxFactory.Block(bodyStatements));

            return method;
        }

        if (block is Statements.FieldDefineBlock fieldBlock)
        {
            var type = fieldBlock.Parameters[0].GetValueAsString();
            var name = fieldBlock.Parameters[1].GetValueAsString();
            var isStatic = fieldBlock.Parameters[3].Value as bool? ?? false;

            var field = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(type))
                .AddVariables(SyntaxFactory.VariableDeclarator(name)))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (isStatic)
            {
                field = field.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            }

            return field;
        }

        if (block is Statements.PropertyDefineBlock propBlock)
        {
            var type = propBlock.Parameters[0].GetValueAsString();
            var name = propBlock.Parameters[1].GetValueAsString();

            var property = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(type),
                name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            return property;
        }

        return null;
    }

    /// <summary>
    /// Mainメソッドを含むエントリーポイントクラスを生成
    /// </summary>
    public string GenerateExecutableCode(Project project, string mainClassName = "Program")
    {
        var code = GenerateCode(project);

        // ProgramクラスとMainメソッドが存在するか確認
        if (!project.Objects.Any(o => o.Name == mainClassName))
        {
            var programObj = new CodeObject
            {
                Name = mainClassName,
                Namespace = project.DefaultNamespace
            };

            var mainMethod = new MemberInfo
            {
                Name = "Main",
                Kind = MemberKind.StaticMethod,
                ReturnType = "void",
                IsStatic = true
            };

            programObj.Members.Add(mainMethod);
            project.Objects.Add(programObj);
        }

        return code;
    }
}
