using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.Models;

namespace EasyCodeBuilderNext.Plugins.Abstractions;

/// <summary>
/// ブロックプロバイダインターフェース
/// プラグインがこのインターフェースを実装してカスタムブロックを提供する
/// </summary>
public interface IBlockProvider
{
    /// <summary>
    /// プロバイダ名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// プロバイダの説明
    /// </summary>
    string Description { get; }

    /// <summary>
    /// バージョン
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// 提供するブロックタイプのコレクションを取得
    /// </summary>
    IEnumerable<BlockTypeInfo> GetBlockTypes();

    /// <summary>
    /// 指定されたブロックタイプのインスタンスを作成
    /// </summary>
    BlockBase CreateBlock(string blockTypeId);

    /// <summary>
    /// 初期化処理
    /// </summary>
    void Initialize();
}

/// <summary>
/// ブロックタイプ情報
/// </summary>
public class BlockTypeInfo
{
    /// <summary>
    /// ブロックタイプの一意識別子
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 表示名
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// カテゴリ
    /// </summary>
    public BlockCategory Category { get; set; } = BlockCategory.Custom;

    /// <summary>
    /// 説明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// アイコン（Unicodeまたは画像パス）
    /// </summary>
    public string? Icon { get; set; }
}
