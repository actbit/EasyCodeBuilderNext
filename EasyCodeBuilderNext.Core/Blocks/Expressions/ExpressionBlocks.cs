using EasyCodeBuilderNext.Core.Models;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Core.Blocks.Expressions;

/// <summary>
/// 式ブロックの基底クラス
/// </summary>
public abstract class ExpressionBlockBase : BlockBase
{
    public override BlockType BlockType => BlockType.Expression;

    /// <summary>
    /// 戻り値の型
    /// </summary>
    public abstract string ReturnType { get; }
}
