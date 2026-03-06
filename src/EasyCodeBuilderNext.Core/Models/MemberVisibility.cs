namespace EasyCodeBuilderNext.Core.Models;

/// <summary>
/// メンバーの種類
/// </summary>
public enum MemberKind
{
    /// <summary>
    /// インスタンスメソッド
    /// </summary>
    InstanceMethod,

    /// <summary>
    /// 静的メソッド
    /// </summary>
    StaticMethod,

    /// <summary>
    /// フィールド
    /// </summary>
    Field,

    /// <summary>
    /// プロパティ
    /// </summary>
    Property,

    /// <summary>
    /// インスタンスフィールド
    /// </summary>
    InstanceField,

    /// <summary>
    /// 静的フィールド
    /// </summary>
    StaticField
}

/// <summary>
/// メンバーの視覚的スタイル情報
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
            MemberKind.InstanceMethod => "▶",
            MemberKind.StaticMethod => "⚡",
            MemberKind.Field => "📝",
            MemberKind.Property => "🔷",
            MemberKind.InstanceField => "📝",
            MemberKind.StaticField => "⚡📝"
        };
    }

    /// <summary>
    /// 彩度を下げるかどうか（静的メンバー用）
    /// </summary>
    public static bool ShouldDesaturate(this MemberKind kind)
    {
        return kind is MemberKind.StaticMethod or MemberKind.StaticField;
    }

    /// <summary>
    /// 表示名を取得
    /// </summary>
    public static string GetDisplayName(this MemberKind kind)
    {
        return kind switch
        {
            MemberKind.InstanceMethod => "インスタンスメソッド",
            MemberKind.StaticMethod => "静的メソッド",
            MemberKind.Field => "フィールド",
            MemberKind.Property => "プロパティ",
            MemberKind.InstanceField => "インスタンスフィールド",
            MemberKind.StaticField => "静的フィールド"
        };
    }
}
