using EasyCodeBuilderNext.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Runtime.Loader;

namespace EasyCodeBuilderNext.Core.CodeGeneration;

/// <summary>
/// Roslynを使用したコンパイラ
/// </summary>
public class RoslynCompiler
{
    private readonly RoslynCodeGenerator _generator = new();

    /// <summary>
    /// デフォルトの参照アセンブリ
    /// </summary>
    private static readonly IEnumerable<MetadataReference> DefaultReferences = new[]
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Text.StringBuilder).Assembly.Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location),
        MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location)
    };

    /// <summary>
    /// プロジェクトをコンパイルしてメモリ内アセンブリを生成
    /// </summary>
    public CompilationResult Compile(Project project)
    {
        var code = _generator.GenerateExecutableCode(project);

        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references = DefaultReferences.ToList();

        // 追加の参照を追加
        foreach (var reference in project.References)
        {
            try
            {
                references.Add(MetadataReference.CreateFromFile(reference));
            }
            catch
            {
                // 参照の読み込みに失敗した場合はスキップ
            }
        }

        var compilation = CSharpCompilation.Create(
            project.Name,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                .WithOptimizationLevel(OptimizationLevel.Debug));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (result.Success)
        {
            return new CompilationResult
            {
                Success = true,
                AssemblyBytes = ms.ToArray(),
                GeneratedCode = code
            };
        }

        var errors = result.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.ToString())
            .ToList();

        return new CompilationResult
        {
            Success = false,
            Errors = errors,
            GeneratedCode = code
        };
    }

    /// <summary>
    /// コードをコンパイルしてメモリ内アセンブリを生成
    /// </summary>
    public CompilationResult Compile(string code, string assemblyName = "GeneratedAssembly")
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            DefaultReferences,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                .WithOptimizationLevel(OptimizationLevel.Debug));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (result.Success)
        {
            return new CompilationResult
            {
                Success = true,
                AssemblyBytes = ms.ToArray(),
                GeneratedCode = code
            };
        }

        var errors = result.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.ToString())
            .ToList();

        return new CompilationResult
        {
            Success = false,
            Errors = errors,
            GeneratedCode = code
        };
    }

    /// <summary>
    /// コンパイル結果からEXEファイルを生成
    /// </summary>
    public bool SaveExecutable(CompilationResult result, string outputPath)
    {
        if (!result.Success || result.AssemblyBytes == null)
            return false;

        try
        {
            File.WriteAllBytes(outputPath, result.AssemblyBytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// コンパイル済みアセンブリを実行
    /// </summary>
    public int Execute(CompilationResult result, string[]? args = null)
    {
        if (!result.Success || result.AssemblyBytes == null)
            return -1;

        try
        {
            using var alc = new AssemblyLoadContext("ExecutionContext", true);
            using var ms = new MemoryStream(result.AssemblyBytes);
            var assembly = alc.LoadFromStream(ms);

            var entryPoint = assembly.EntryPoint;
            if (entryPoint == null)
                return -1;

            var instance = entryPoint.IsStatic ? null : Activator.CreateInstance(entryPoint.DeclaringType!);
            var returnValue = entryPoint.Invoke(instance, new object?[] { args ?? Array.Empty<string>() });

            return returnValue as int? ?? 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"実行エラー: {ex.Message}");
            return -1;
        }
    }
}

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
    /// 生成されたアセンブリのバイト配列
    /// </summary>
    public byte[]? AssemblyBytes { get; set; }

    /// <summary>
    /// 生成されたコード
    /// </summary>
    public string GeneratedCode { get; set; } = string.Empty;

    /// <summary>
    /// コンパイルエラーのリスト
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 警告のリスト
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}
