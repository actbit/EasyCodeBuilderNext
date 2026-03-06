using CommunityToolkit.Mvvm.ComponentModel;
using EasyCodeBuilderNext.Core.Models;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Core.Blocks;

/// <summary>
/// 全ブロックの抽象基底クラス
/// </summary>
public abstract partial class BlockBase : ObservableObject
{
    /// <summary>
    /// ブロックの一意識別子
    /// </summary>
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    /// <summary>
    /// ブロックの種類
    /// </summary>
    public abstract BlockType BlockType { get; }

    /// <summary>
    /// ブロックのカテゴリ
    /// </summary>
    public abstract BlockCategory Category { get; }

    /// <summary>
    /// 表示名
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// コード出力用のテンプレート
    /// </summary>
    public abstract string CodeTemplate { get; }

    /// <summary>
    /// 上位ブロック（接続されている前のブロック）
    /// </summary>
    [ObservableProperty]
    private BlockBase? _previousBlock;

    /// <summary>
    /// 下位ブロック（接続されている次のブロック）
    /// </summary>
    [ObservableProperty]
    private BlockBase? _nextBlock;

    /// <summary>
    /// 内部ブロック（制御構造用）
    /// </summary>
    public ObservableCollection<BlockBase> InnerBlocks { get; } = new();

    /// <summary>
    /// 内部ブロック（else節など、複数の内部ブロックを持つ場合）
    /// </summary>
    public ObservableCollection<ObservableCollection<BlockBase>> AdditionalInnerBlocks { get; } = new();

    /// <summary>
    /// パラメータコレクション
    /// </summary>
    public ObservableCollection<BlockParameter> Parameters { get; } = new();

    /// <summary>
    /// キャンバス上のX座標
    /// </summary>
    [ObservableProperty]
    private double _x;

    /// <summary>
    /// キャンバス上のY座標
    /// </summary>
    [ObservableProperty]
    private double _y;

    /// <summary>
    /// 幅
    /// </summary>
    [ObservableProperty]
    private double _width = 150;

    /// <summary>
    /// 高さ
    /// </summary>
    [ObservableProperty]
    private double _height = 40;

    /// <summary>
    /// 選択状態
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// ドラッグ中かどうか
    /// </summary>
    [ObservableProperty]
    private bool _isDragging;

    /// <summary>
    /// ハイライト状態（スナップ候補など）
    /// </summary>
    [ObservableProperty]
    private bool _isHighlighted;

    /// <summary>
    /// 無効状態
    /// </summary>
    [ObservableProperty]
    private bool _isDisabled;

    /// <summary>
    /// 親ブロック（制御構造内のブロックの場合）
    /// </summary>
    [ObservableProperty]
    private BlockBase? _parentBlock;

    /// <summary>
    /// 所属するオブジェクト（クラス）
    /// </summary>
    [ObservableProperty]
    private CodeObject? _ownerObject;

    /// <summary>
    /// 上部コネクタを持つかどうか
    /// </summary>
    public virtual bool HasTopConnector => BlockType is BlockType.Statement or BlockType.ControlStructure or BlockType.Terminal;

    /// <summary>
    /// 下部コネクタを持つかどうか
    /// </summary>
    public virtual bool HasBottomConnector => BlockType is BlockType.Statement or BlockType.ControlStructure;

    /// <summary>
    /// 内部コネクタを持つかどうか
    /// </summary>
    public virtual bool HasInnerConnector => BlockType == BlockType.ControlStructure;

    /// <summary>
    /// コード生成
    /// </summary>
    /// <param name="level">インデントレベル</param>
    /// <returns>生成されたコード</returns>
    public abstract string CodeOutput(int level);

    /// <summary>
    /// インデント文字列を生成
    /// </summary>
    protected static string GetIndent(int level)
    {
        return new string(' ', level * 4);
    }

    /// <summary>
    /// 利用可能な変数リストを取得
    /// </summary>
    public virtual List<VariableInfo> GetAvailableVariables()
    {
        var variables = new List<VariableInfo>();

        // 親ブロックから変数を収集
        if (ParentBlock != null)
        {
            variables.AddRange(ParentBlock.GetAvailableVariables());
        }

        // 上位ブロックから変数を収集
        if (PreviousBlock != null)
        {
            variables.AddRange(PreviousBlock.GetAvailableVariables());
        }

        // 自身が定義する変数を追加
        AddDefinedVariables(variables);

        return variables;
    }

    /// <summary>
    /// このブロックが定義する変数を追加
    /// </summary>
    protected virtual void AddDefinedVariables(List<VariableInfo> variables)
    {
        // 派生クラスでオーバーライドして実装
    }

    /// <summary>
    /// 次のブロックのコードを生成
    /// </summary>
    protected string GenerateNextBlockCode(int level)
    {
        if (NextBlock == null)
            return string.Empty;

        return "\n" + NextBlock.CodeOutput(level);
    }

    /// <summary>
    /// 内部ブロックのコードを生成
    /// </summary>
    protected string GenerateInnerBlocksCode(int level)
    {
        if (InnerBlocks.Count == 0)
            return GetIndent(level + 1) + "// 空のブロック";

        var sb = new System.Text.StringBuilder();
        foreach (var block in InnerBlocks)
        {
            sb.AppendLine(block.CodeOutput(level + 1));
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// ブロックの深層コピーを作成
    /// </summary>
    public virtual BlockBase Clone()
    {
        var clone = (BlockBase)MemberwiseClone();
        clone.Id = Guid.NewGuid();
        clone.PreviousBlock = null;
        clone.NextBlock = null;
        clone.ParentBlock = null;
        clone.OwnerObject = null;
        clone.IsSelected = false;
        clone.IsDragging = false;
        clone.IsHighlighted = false;

        // パラメータのコピー
        clone.Parameters.Clear();
        foreach (var param in Parameters)
        {
            clone.Parameters.Add(param.Clone());
        }

        // 内部ブロックのコピー
        clone.InnerBlocks.Clear();
        foreach (var inner in InnerBlocks)
        {
            var innerClone = inner.Clone();
            innerClone.ParentBlock = clone;
            clone.InnerBlocks.Add(innerClone);
        }

        return clone;
    }

    /// <summary>
    /// 指定位置に次のブロックを挿入
    /// </summary>
    public void InsertAfter(BlockBase block)
    {
        if (NextBlock != null)
        {
            var oldNext = NextBlock;
            oldNext.PreviousBlock = null;

            block.NextBlock = oldNext;
            oldNext.PreviousBlock = block;
        }

        NextBlock = block;
        block.PreviousBlock = this;
    }

    /// <summary>
    /// このブロックをチェーンから取り外す
    /// </summary>
    public void Detach()
    {
        if (PreviousBlock != null)
        {
            PreviousBlock.NextBlock = NextBlock;
        }

        if (NextBlock != null)
        {
            NextBlock.PreviousBlock = PreviousBlock;
        }

        PreviousBlock = null;
        NextBlock = null;
    }

    /// <summary>
    /// ブロックチェーンの最後を取得
    /// </summary>
    public BlockBase GetChainEnd()
    {
        var current = this;
        while (current.NextBlock != null)
        {
            current = current.NextBlock;
        }
        return current;
    }

    /// <summary>
    /// ブロックチェーンの先頭を取得
    /// </summary>
    public BlockBase GetChainStart()
    {
        var current = this;
        while (current.PreviousBlock != null)
        {
            current = current.PreviousBlock;
        }
        return current;
    }
}
