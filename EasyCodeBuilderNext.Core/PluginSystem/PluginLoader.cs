using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Plugins.Abstractions;
using System.Reflection;
using System.Runtime.Loader;

namespace EasyCodeBuilderNext.Core.PluginSystem;

/// <summary>
/// DLLからプラグインを読み込むクラス
/// </summary>
public class PluginLoader : IDisposable
{
    private readonly List<AssemblyLoadContext> _loadContexts = new();
    private readonly List<IBlockProvider> _blockProviders = new();
    private readonly List<ITypeProvider> _typeProviders = new();

    /// <summary>
    /// 読み込み済みのブロックプロバイダ一覧
    /// </summary>
    public IReadOnlyList<IBlockProvider> BlockProviders => _blockProviders;

    /// <summary>
    /// 読み込み済みの型プロバイダ一覧
    /// </summary>
    public IReadOnlyList<ITypeProvider> TypeProviders => _typeProviders;

    /// <summary>
    /// DLLからプラグインを読み込む
    /// </summary>
    public PluginLoadResult LoadPlugin(string assemblyPath)
    {
        var result = new PluginLoadResult();

        try
        {
            var alc = new AssemblyLoadContext(Path.GetFileNameWithoutExtension(assemblyPath), true);
            _loadContexts.Add(alc);

            var assembly = alc.LoadFromAssemblyPath(assemblyPath);

            // IBlockProviderを実装する型を検索
            var blockProviderTypes = assembly.GetTypes()
                .Where(t => typeof(IBlockProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in blockProviderTypes)
            {
                if (Activator.CreateInstance(type) is IBlockProvider provider)
                {
                    provider.Initialize();
                    _blockProviders.Add(provider);
                    result.BlockProvider = provider;
                }
            }

            // ITypeProviderを実装する型を検索
            var typeProviderTypes = assembly.GetTypes()
                .Where(t => typeof(ITypeProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in typeProviderTypes)
            {
                if (Activator.CreateInstance(type) is ITypeProvider provider)
                {
                    _typeProviders.Add(provider);
                    result.TypeProvider = provider;
                }
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Exception = ex;
        }

        return result;
    }

    /// <summary>
    /// 指定されたディレクトリからすべてのプラグインを読み込む
    /// </summary>
    public IEnumerable<PluginLoadResult> LoadPluginsFromDirectory(string directoryPath)
    {
        var results = new List<PluginLoadResult>();

        if (!Directory.Exists(directoryPath))
            return results;

        foreach (var dllFile in Directory.GetFiles(directoryPath, "*.dll"))
        {
            results.Add(LoadPlugin(dllFile));
        }

        return results;
    }

    /// <summary>
    /// すべてのプロバイダをアンロード
    /// </summary>
    public void UnloadAll()
    {
        _blockProviders.Clear();
        _typeProviders.Clear();

        foreach (var alc in _loadContexts)
        {
            alc.Unload();
        }

        _loadContexts.Clear();
    }

    public void Dispose()
    {
        UnloadAll();
        GC.SuppressFinalize(this);
    }
}
