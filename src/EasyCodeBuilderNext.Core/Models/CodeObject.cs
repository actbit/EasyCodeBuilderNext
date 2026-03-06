using CommunityToolkit.Mvvm.ComponentModel;
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
    private bool _IsAbstract;

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
            _IsAbstract = _IsAbstract,
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
