namespace EasyCodeBuilderNext.Plugins.Abstractions;

/// <summary>
/// プラグインメタデータ
/// </summary>
public class PluginMetadata
{
    /// <summary>
    /// プラグインID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// プラグイン名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 説明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 作成者
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// バージョン
    /// </summary>
    public Version Version { get; set; } = new Version(1, 0, 0);

    /// <summary>
    /// 最小互換バージョン
    /// </summary>
    public Version? MinHostVersion { get; set; }

    /// <summary>
    /// プラグインDLLのパス
    /// </summary>
    public string? AssemblyPath { get; set; }

    /// <summary>
    /// 依存するアセンブリ
    /// </summary>
    public List<string> Dependencies { get; set; } = new();
}

/// <summary>
/// プラグイン読み込み結果
/// </summary>
public class PluginLoadResult
{
    /// <summary>
    /// 読み込みが成功したかどうか
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 読み込まれたプロバイダ
    /// </summary>
    public IBlockProvider? BlockProvider { get; set; }

    /// <summary>
    /// 読み込まれた型プロバイダ
    /// </summary>
    public ITypeProvider? TypeProvider { get; set; }

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 例外（エラーがある場合）
    /// </summary>
    public Exception? Exception { get; set; }
}
