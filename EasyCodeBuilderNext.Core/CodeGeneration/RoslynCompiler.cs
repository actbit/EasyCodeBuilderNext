using EasyCodeBuilderNext.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Runtime.Loader;

namespace EasyCodeBuilderNext.Core.CodeGeneration;

/// <summary>
/// コンパイル結果
/// </summary>
public class CompilationResult
{
    /// <summary>
    /// コンパイルが成功したかどうか
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// エラーメッセージのリスト
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 警告メッセージのリスト
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// コンパイルされたアセンブリ
    /// </summary>
    public Assembly? Assembly { get; set; }

    /// <summary>
    /// 生成されたバイトコード
    /// </summary>
    public byte[]? ByteCode { get; set; }

    /// <summary>
    /// エントリーポイント（Mainメソッド）
    /// </summary>
    public MethodInfo? EntryPoint { get; set; }
}

/// <summary>
/// Roslynを使用したコンパイラ
/// </summary>
public class RoslynCompiler : IDisposable
{
    private AssemblyLoadContext? _loadContext;
    private readonly List<MetadataReference> _references;
    private readonly RoslynCodeGenerator _codeGenerator;

    public RoslynCompiler()
    {
        _codeGenerator = new RoslynCodeGenerator();
        _references = new List<MetadataReference>();

        // 基本的な参照を追加
        AddDefaultReferences();
    }

    /// <summary>
    /// デフォルトの参照を追加
    /// </summary>
    private void AddDefaultReferences()
    {
        // System.Runtime
        var runtimeAssembly = typeof(object).Assembly;
        AddReference(runtimeAssembly.Location);

        // System.Console
        var consoleAssembly = typeof(Console).Assembly;
        AddReference(consoleAssembly.Location);

        // System.Linq
        var linqAssembly = typeof(Enumerable).Assembly;
        AddReference(linqAssembly.Location);

        // System.Collections.Generic
        var collectionsAssembly = typeof(List<>).Assembly;
        AddReference(collectionsAssembly.Location);

        // netstandard
        var netstandard = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "netstandard");
        if (netstandard != null)
        {
            AddReference(netstandard.Location);
        }

        // System.Runtime.dll
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (runtimeDir != null)
        {
            var assembliesToReference = new[]
            {
                "System.Runtime.dll",
                "System.Console.dll",
                "System.Linq.dll",
                "System.Collections.dll",
                "System.Linq.Expressions.dll",
                "System.ObjectModel.dll",
                "mscorlib.dll"
            };

            foreach (var assemblyName in assembliesToReference)
            {
                var path = Path.Combine(runtimeDir, assemblyName);
                if (File.Exists(path))
                {
                    AddReference(path);
                }
            }
        }
    }

    /// <summary>
    /// 参照アセンブリを追加
    /// </summary>
    public void AddReference(string assemblyPath)
    {
        if (File.Exists(assemblyPath) && _references.All(r => r.Display != assemblyPath))
        {
            _references.Add(MetadataReference.CreateFromFile(assemblyPath));
        }
    }

    /// <summary>
    /// プロジェクトをコンパイル
    /// </summary>
    public CompilationResult Compile(Project project)
    {
        var code = _codeGenerator.GenerateCode(project);
        return Compile(code, project.Name);
    }

    /// <summary>
    /// コードをコンパイル
    /// </summary>
    public CompilationResult Compile(string code, string assemblyName = "GeneratedAssembly")
    {
        var result = new CompilationResult();

        try
        {
            // 構文木を作成
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            // コンパイルオプション
            var options = new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                .WithOptimizationLevel(OptimizationLevel.Debug)
                .WithAllowUnsafe(true);

            // コンパイル作成
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                _references,
                options);

            // メモリストリームに出力
            using var memoryStream = new MemoryStream();
            var emitResult = compilation.Emit(memoryStream);

            if (emitResult.Success)
            {
                result.Success = true;
                result.ByteCode = memoryStream.ToArray();

                // アセンブリをロード
                memoryStream.Position = 0;
                _loadContext = new AssemblyLoadContext(assemblyName, isCollectible: true);
                result.Assembly = _loadContext.LoadFromStream(memoryStream);

                // エントリーポイントを取得
                result.EntryPoint = compilation.GetEntryPoint(System.Globalization.CultureInfo.InvariantCulture)?
                    .DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodSyntax
                    ? result.Assembly.GetType(methodSyntax.Identifier.Text)?.GetMethod("Main", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    : null;
            }
            else
            {
                result.Success = false;

                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    var message = $"Line {diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}: {diagnostic.GetMessage()}";

                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        result.Errors.Add(message);
                    }
                    else if (diagnostic.Severity == DiagnosticSeverity.Warning)
                    {
                        result.Warnings.Add(message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"コンパイルエラー: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// コンパイル結果を実行
    /// </summary>
    public void Execute(CompilationResult result)
    {
        if (!result.Success || result.Assembly == null)
        {
            throw new InvalidOperationException("コンパイルが失敗しているか、アセンブリが生成されていません");
        }

        // エントリーポイントを探す
        var entryPoint = result.Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .FirstOrDefault(m => m.Name == "Main");

        if (entryPoint == null)
        {
            throw new InvalidOperationException("エントリーポイント（Mainメソッド）が見つかりません");
        }

        // Mainメソッドを実行
        var parameters = entryPoint.GetParameters();
        if (parameters.Length == 0)
        {
            entryPoint.Invoke(null, null);
        }
        else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]))
        {
            entryPoint.Invoke(null, new object[] { Array.Empty<string>() });
        }
        else
        {
            throw new InvalidOperationException("Mainメソッドのシグネチャが無効です");
        }
    }

    /// <summary>
    /// EXEファイルを出力
    /// </summary>
    public bool ExportExe(Project project, string outputPath)
    {
        var code = _codeGenerator.GenerateCode(project);
        return ExportExe(code, outputPath);
    }

    /// <summary>
    /// EXEファイルを出力
    /// </summary>
    public bool ExportExe(string code, string outputPath)
    {
        var result = Compile(code, Path.GetFileNameWithoutExtension(outputPath));

        if (!result.Success || result.ByteCode == null)
        {
            return false;
        }

        try
        {
            File.WriteAllBytes(outputPath, result.ByteCode);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// DLLファイルを出力
    /// </summary>
    public bool ExportDll(Project project, string outputPath)
    {
        var code = _codeGenerator.GenerateCode(project);
        return ExportDll(code, outputPath);
    }

    /// <summary>
    /// DLLファイルを出力
    /// </summary>
    public bool ExportDll(string code, string outputPath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Release);

        var compilation = CSharpCompilation.Create(
            Path.GetFileNameWithoutExtension(outputPath),
            new[] { syntaxTree },
            _references,
            options);

        using var memoryStream = new MemoryStream();
        var emitResult = compilation.Emit(memoryStream);

        if (emitResult.Success)
        {
            try
            {
                File.WriteAllBytes(outputPath, memoryStream.ToArray());
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// アンロード（アセンブリを解放）
    /// </summary>
    public void Unload()
    {
        _loadContext?.Unload();
        _loadContext = null;
    }

    public void Dispose()
    {
        Unload();
    }
}
