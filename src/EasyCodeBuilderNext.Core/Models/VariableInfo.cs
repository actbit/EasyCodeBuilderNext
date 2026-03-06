namespace EasyCodeBuilderNext.Core.Models;

/// <summary>
/// 変数情報
/// </summary>
public class VariableInfo
{
    /// <summary>
    /// 変数名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 型名
    /// </summary>
    public string TypeName { get; set; } = "var";

    /// <summary>
    /// スコープレベル（0: グローバル, 1: クラス, 2: メソッド, ...）
    /// </summary>
    public int ScopeLevel { get; set; }

    /// <summary>
    /// 属するオブジェクト名（クラス名）
    /// </summary>
    public string? ObjectName { get; set; }

    /// <summary>
    /// 静的変数かどうか
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// 初期値
    /// </summary>
    public string? InitialValue { get; set; }

    /// <summary>
    /// 完全修飾名を取得
    /// </summary>
    public string GetFullName()
    {
        if (!string.IsNullOrEmpty(ObjectName) && !IsStatic)
        {
            return $"{ObjectName}.{Name}";
        }
        return Name;
    }

    public VariableInfo Clone()
    {
        return new VariableInfo
        {
            Name = Name,
            TypeName = TypeName,
            ScopeLevel = ScopeLevel,
            ObjectName = ObjectName,
            IsStatic = IsStatic,
            InitialValue = InitialValue
        };
    }
}
