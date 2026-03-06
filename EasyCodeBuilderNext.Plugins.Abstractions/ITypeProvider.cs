namespace EasyCodeBuilderNext.Plugins.Abstractions;

/// <summary>
/// 型情報プロバイダインターフェース
/// 外部DLLから型情報を読み込んでブロックを生成する
/// </summary>
public interface ITypeProvider
{
    /// <summary>
    /// プロバイダ名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 読み込み済みの型一覧を取得
    /// </summary>
    IEnumerable<PluginTypeInfo> GetLoadedTypes();

    /// <summary>
    /// 指定されたDLLから型を読み込む
    /// </summary>
    IEnumerable<PluginTypeInfo> LoadTypesFromAssembly(string assemblyPath);

    /// <summary>
    /// 型を検索
    /// </summary>
    IEnumerable<PluginTypeInfo> SearchTypes(string query);

    /// <summary>
    /// 指定された型のメンバー情報を取得
    /// </summary>
    IEnumerable<PluginMemberInfo> GetTypeMembers(string fullTypeName);
}

/// <summary>
/// プラグイン用型情報
/// </summary>
public class PluginTypeInfo
{
    /// <summary>
    /// 完全修飾型名
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 型名（名前空間なし）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 名前空間
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// アセンブリ名
    /// </summary>
    public string? AssemblyName { get; set; }

    /// <summary>
    /// 基底型
    /// </summary>
    public string? BaseType { get; set; }

    /// <summary>
    /// 実装インターフェース
    /// </summary>
    public List<string> Interfaces { get; set; } = new();

    /// <summary>
    /// 型の種類
    /// </summary>
    public PluginTypeKind Kind { get; set; }

    /// <summary>
    /// パブリックかどうか
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// 静的型かどうか
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// 抽象型かどうか
    /// </summary>
    public bool IsAbstract { get; set; }

    /// <summary>
    /// 列挙型かどうか
    /// </summary>
    public bool IsEnum { get; set; }

    /// <summary>
    /// 値型かどうか
    /// </summary>
    public bool IsValueType { get; set; }
}

/// <summary>
/// プラグイン用型の種類
/// </summary>
public enum PluginTypeKind
{
    Class,
    Interface,
    Struct,
    Enum,
    Delegate
}

/// <summary>
/// プラグイン用メンバー情報
/// </summary>
public class PluginMemberInfo
{
    /// <summary>
    /// メンバー名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// メンバーの種類
    /// </summary>
    public PluginMemberKind Kind { get; set; }

    /// <summary>
    /// 戻り値の型（メソッド・プロパティ用）
    /// </summary>
    public string? ReturnType { get; set; }

    /// <summary>
    /// パラメータ（メソッド用）
    /// </summary>
    public List<PluginParameterInfo> Parameters { get; set; } = new();

    /// <summary>
    /// 静的メンバーかどうか
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// 読み取り専用かどうか（プロパティ・フィールド用）
    /// </summary>
    public bool IsReadOnly { get; set; }
}

/// <summary>
/// プラグイン用パラメータ情報
/// </summary>
public class PluginParameterInfo
{
    /// <summary>
    /// パラメータ名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 型名
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// デフォルト値があるかどうか
    /// </summary>
    public bool HasDefaultValue { get; set; }

    /// <summary>
    /// デフォルト値
    /// </summary>
    public object? DefaultValue { get; set; }
}

/// <summary>
/// プラグイン用メンバー種類
/// </summary>
public enum PluginMemberKind
{
    Method,
    Property,
    Field,
    Event
}
