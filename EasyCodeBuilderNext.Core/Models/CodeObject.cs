using CommunityToolkit.Mvvm.ComponentModel;
using EasyCodeBuilderNext.Core.Blocks;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Core.Models;

/// <summary>
/// コード上のオブジェクト（クラス）を表すモデル
/// Scratchのスプライトに相当
/// </summary>
public partial class CodeObject : ObservableObject
{
    /// <summary>
    /// オブジェクトの一意識別子
    /// </summary>
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    /// <summary>
    /// オブジェクト名（クラス名）
    /// </summary>
    [ObservableProperty]
    private string _name = "NewObject";

    /// <summary>
    /// 基底クラス名
    /// </summary>
    [ObservableProperty]
    private string? _baseClassName;

    /// <summary>
    /// 実装するインターフェースのリスト
    /// </summary>
    public ObservableCollection<string> ImplementedInterfaces { get; } = new();

    /// <summary>
    /// このオブジェクトのブロックコレクション
    /// </summary>
    public ObservableCollection<BlockBase> Blocks { get; } = new();

    /// <summary>
    /// このオブジェクトのメンバー（フィールド、プロパティ、メソッド）
    /// </summary>
    public ObservableCollection<MemberInfo> Members { get; } = new();

    /// <summary>
    /// クラスのアクセシビリティ
    /// </summary>
    [ObservableProperty]
    private Accessibility _accessibility = Accessibility.Public;

    /// <summary>
    /// 静的クラスかどうか
    /// </summary>
    [ObservableProperty]
    private bool _isStatic;

    /// <summary>
    /// シールクラスかどうか
    /// </summary>
    [ObservableProperty]
    private bool _isSealed;

    /// <summary>
    /// 抽象クラスかどうか
    /// </summary>
    [ObservableProperty]
    private bool _isAbstract;

    /// <summary>
    /// 名前空間
    /// </summary>
    [ObservableProperty]
    private string _namespace = "GeneratedCode";

    public CodeObject Clone()
    {
        var clone = new CodeObject
        {
            Name = Name,
            BaseClassName = BaseClassName,
            Accessibility = Accessibility,
            IsStatic = IsStatic,
            IsSealed = IsSealed,
            IsAbstract = IsAbstract,
            Namespace = Namespace
        };

        foreach (var iface in ImplementedInterfaces)
        {
            clone.ImplementedInterfaces.Add(iface);
        }

        return clone;
    }
}

/// <summary>
/// アクセシビリティ
/// </summary>
public enum Accessibility
{
    Public,
    Private,
    Protected,
    Internal,
    ProtectedInternal,
    PrivateProtected
}

/// <summary>
/// メンバーの種類
/// </summary>
public enum MemberKind
{
    Field,
    InstanceField,
    StaticField,
    Property,
    Method,
    InstanceMethod,
    StaticMethod,
    Event,
    Constructor,
    Destructor,
    Indexer,
    Operator
}

/// <summary>
/// MemberKindの拡張メソッド
/// </summary>
public static class MemberKindExtensions
{
    /// <summary>
    /// メンバー種類に対応するアイコンを取得
    /// </summary>
    public static string GetIcon(this MemberKind kind)
    {
        return kind switch
        {
            MemberKind.Field => "📝",
            MemberKind.InstanceField => "📝",
            MemberKind.StaticField => "⚡📝",
            MemberKind.Property => "🔷",
            MemberKind.Method => "▶",
            MemberKind.InstanceMethod => "▶",
            MemberKind.StaticMethod => "⚡▶",
            MemberKind.Event => "⚡",
            MemberKind.Constructor => "🔧",
            MemberKind.Destructor => "🗑",
            MemberKind.Indexer => "📋",
            MemberKind.Operator => "➕",
            _ => "📄"
        };
    }

    /// <summary>
    /// 彩度を下げるべきかどうか（静的メンバー用）
    /// </summary>
    public static bool ShouldDesaturate(this MemberKind kind)
    {
        // 静的メンバーの場合は彩度を下げる（ここではデフォルトでfalse）
        return false;
    }
}

/// <summary>
/// メンバー情報
/// </summary>
public partial class MemberInfo : ObservableObject
{
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private MemberKind _kind;

    [ObservableProperty]
    private string _returnType = "void";

    [ObservableProperty]
    private Accessibility _accessibility = Accessibility.Public;

    [ObservableProperty]
    private bool _isStatic;

    /// <summary>
    /// パラメータリスト（メソッド用）
    /// </summary>
    public ObservableCollection<ParameterInfo> Parameters { get; } = new();

    /// <summary>
    /// このメンバーに対応するブロック
    /// </summary>
    public BlockBase? AssociatedBlock { get; set; }
}

/// <summary>
/// パラメータ情報
/// </summary>
public partial class ParameterInfo : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _typeName = "object";

    [ObservableProperty]
    private string? _defaultValue;
}
