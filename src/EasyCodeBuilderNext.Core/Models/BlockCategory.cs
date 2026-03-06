namespace EasyCodeBuilderNext.Core.Models;

/// <summary>
/// ブロックのカテゴリ（Scratch風色分け）
/// </summary>
public enum BlockCategory
{
    /// <summary>
    /// 変数定義・代入 (オレンジ)
    /// </summary>
    Variables,

    /// <summary>
    /// if/while/for (黄色)
    /// </summary>
    Control,

    /// <summary>
    /// メソッド定義・呼び出し (紫)
    /// </summary>
    Methods,

    /// <summary>
    /// クラス定義 (ピンク)
    /// </summary>
    Classes,

    /// <summary>
    /// コンソール入出力 (水色)
    /// </summary>
    IO,

    /// <summary>
    /// 配列・リスト・辞書 (赤)
    /// </summary>
    Data,

    /// <summary>
    /// プラグインブロック (ティール)
    /// </summary>
    Custom,

    /// <summary>
    /// 演算子 (緑)
    /// </summary>
    Operators,

    /// <summary>
    /// イベント (茶色)
    /// </summary>
    Events
}

/// <summary>
/// ブロックカテゴリの拡張メソッド
/// </summary>
public static class BlockCategoryExtensions
{
    /// <summary>
    /// カテゴリに対応する色を取得（16進数形式）
    /// </summary>
    public static string GetColor(this BlockCategory category)
    {
        return category switch
        {
            BlockCategory.Variables => "#FF8C1A",  // オレンジ
            BlockCategory.Control => "#FFBF00",    // 黄色
            BlockCategory.Methods => "#9966FF",    // 紫
            BlockCategory.Classes => "#FF66B2",    // ピンク
            BlockCategory.IO => "#4C97FF",         // 水色
            BlockCategory.Data => "#FF6666",       // 赤
            BlockCategory.Custom => "#0BD1D1",     // ティール
            BlockCategory.Operators => "#59C059",  // 緑
            BlockCategory.Events => "#CC9933",     // 茶色
            _ => "#999999"
        };
    }

    /// <summary>
    /// カテゴリの表示名を取得
    /// </summary>
    public static string GetDisplayName(this BlockCategory category)
    {
        return category switch
        {
            BlockCategory.Variables => "変数",
            BlockCategory.Control => "制御",
            BlockCategory.Methods => "メソッド",
            BlockCategory.Classes => "クラス",
            BlockCategory.IO => "入出力",
            BlockCategory.Data => "データ",
            BlockCategory.Custom => "カスタム",
            BlockCategory.Operators => "演算子",
            BlockCategory.Events => "イベント",
            _ => "その他"
        };
    }
}
