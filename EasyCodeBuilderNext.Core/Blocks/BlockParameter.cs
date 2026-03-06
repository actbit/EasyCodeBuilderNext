using CommunityToolkit.Mvvm.ComponentModel;
using EasyCodeBuilderNext.Core.Models;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Core.Blocks;

/// <summary>
/// ブロックのパラメータ定義
/// </summary>
public partial class BlockParameter : ObservableObject
{
    /// <summary>
    /// パラメータ名
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// パラメータの型
    /// </summary>
    [ObservableProperty]
    private string _typeName = "object";

    /// <summary>
    /// 表示ラベル
    /// </summary>
    [ObservableProperty]
    private string _label = string.Empty;

    /// <summary>
    /// デフォルト値
    /// </summary>
    [ObservableProperty]
    private object? _defaultValue;

    /// <summary>
    /// 現在の値
    /// </summary>
    [ObservableProperty]
    private object? _value;

    /// <summary>
    /// 入力タイプ
    /// </summary>
    [ObservableProperty]
    private ParameterInputType _inputType = ParameterInputType.Text;

    /// <summary>
    /// 選択肢（ドロップダウン用）
    /// </summary>
    public ObservableCollection<string> Options { get; } = new();

    /// <summary>
    /// 接続された式ブロック（ブロック入力の場合）
    /// </summary>
    [ObservableProperty]
    private BlockBase? _connectedBlock;

    /// <summary>
    /// 必須かどうか
    /// </summary>
    [ObservableProperty]
    private bool _isRequired = true;

    /// <summary>
    /// 説明文
    /// </summary>
    [ObservableProperty]
    private string? _description;

    /// <summary>
    /// 値をコード出力用文字列として取得
    /// </summary>
    public string GetValueAsString()
    {
        if (ConnectedBlock != null)
        {
            return ConnectedBlock.CodeOutput(0);
        }

        if (Value == null)
        {
            return DefaultValue?.ToString() ?? string.Empty;
        }

        // 文字列の場合は引用符で囲む
        if (Value is string strValue && TypeName == "string")
        {
            return $"\"{strValue}\"";
        }

        return Value.ToString() ?? string.Empty;
    }

    public BlockParameter Clone()
    {
        var clone = new BlockParameter
        {
            Name = Name,
            TypeName = TypeName,
            Label = Label,
            DefaultValue = DefaultValue,
            Value = Value,
            InputType = InputType,
            IsRequired = IsRequired,
            Description = Description
        };

        foreach (var option in Options)
        {
            clone.Options.Add(option);
        }

        return clone;
    }
}

/// <summary>
/// パラメータの入力タイプ
/// </summary>
public enum ParameterInputType
{
    /// <summary>
    /// テキスト入力
    /// </summary>
    Text,

    /// <summary>
    /// 数値入力
    /// </summary>
    Number,

    /// <summary>
    /// ドロップダウン選択
    /// </summary>
    Dropdown,

    /// <summary>
    /// ブロック接続
    /// </summary>
    Block,

    /// <summary>
    /// チェックボックス
    /// </summary>
    Checkbox,

    /// <summary>
    /// 変数選択
    /// </summary>
    Variable,

    /// <summary>
    /// 型選択
    /// </summary>
    TypeSelector
}
