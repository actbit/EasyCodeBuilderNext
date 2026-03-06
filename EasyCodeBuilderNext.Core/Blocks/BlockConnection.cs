using EasyCodeBuilderNext.Core.Models;

namespace EasyCodeBuilderNext.Core.Blocks;

/// <summary>
/// ブロック間の接続情報
/// </summary>
public class BlockConnection
{
    /// <summary>
    /// 接続元ブロックID
    /// </summary>
    public Guid SourceBlockId { get; set; }

    /// <summary>
    /// 接続先ブロックID
    /// </summary>
    public Guid TargetBlockId { get; set; }

    /// <summary>
    /// 接続タイプ
    /// </summary>
    public ConnectionType ConnectionType { get; set; }

    /// <summary>
    /// 接続位置（パラメータ接続の場合のインデックス）
    /// </summary>
    public int ConnectionIndex { get; set; }

    /// <summary>
    /// 接続の強さ（スナップ閾値）
    /// </summary>
    public const double SnapThreshold = 20.0;
}

/// <summary>
/// 接続タイプ
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// 次のブロックへの接続（上→下）
    /// </summary>
    Next,

    /// <summary>
    /// 内部ブロックへの接続（制御構造用）
    /// </summary>
    Inner,

    /// <summary>
    /// パラメータへの接続（式ブロック）
    /// </summary>
    Parameter,

    /// <summary>
    /// 戻り値への接続
    /// </summary>
    ReturnValue
}
