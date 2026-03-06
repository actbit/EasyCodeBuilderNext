namespace EasyCodeBuilderNext.Core.Models;

/// <summary>
/// ブロックの種類
/// </summary>
public enum BlockType
{
    /// <summary>
    /// ステートメント（文）- 上部と下部にコネクタを持つ
    /// </summary>
    Statement,

    /// <summary>
    /// 式 - 値を返すブロック
    /// </summary>
    Expression,

    /// <summary>
    /// 制御構造（if, while等）- 内部にブロックを含む
    /// </summary>
    ControlStructure,

    /// <summary>
    /// 終了ブロック（return, break等）
    /// </summary>
    Terminal,

    /// <summary>
    /// イベントハンドラ開始ブロック
    /// </summary>
    Hat,

    /// <summary>
    /// 定義ブロック（クラス、メソッド等）
    /// </summary>
    Definition,

    /// <summary>
    /// 値ブロック（数値、文字列リテラル等）
    /// </summary>
    Value
}

/// <summary>
/// コネクタの種類
/// </summary>
public enum ConnectorType
{
    /// <summary>
    /// なし
    /// </summary>
    None,

    /// <summary>
    /// 上部コネクタ（凹み）
    /// </summary>
    Top,

    /// <summary>
    /// 下部コネクタ（凸）
    /// </summary>
    Bottom,

    /// <summary>
    /// 内部コネクタ（制御構造用）
    /// </summary>
    Inner,

    /// <summary>
    /// 左側コネクタ（式用）
    /// </summary>
    Left,

    /// <summary>
    /// 右側コネクタ（式用）
    /// </summary>
    Right
}
