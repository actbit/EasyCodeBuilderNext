using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.Blocks.Statements;
using EasyCodeBuilderNext.Core.Models;
using EasyCodeBuilderNext.Core.TypeSystem;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Core.PluginSystem;

/// <summary>
/// 型情報からブロックを生成するファクトリ
/// </summary>
public class BlockFactory
{
    private readonly TypeRegistry? _typeRegistry;
    private readonly PluginLoader? _pluginLoader;

    public BlockFactory() { }

    public BlockFactory(TypeRegistry typeRegistry, PluginLoader pluginLoader)
    {
        _typeRegistry = typeRegistry;
        _pluginLoader = pluginLoader;
    }

    /// <summary>
    /// 利用可能なブロックテンプレート一覧を取得
    /// </summary>
    public ObservableCollection<BlockTemplate> GetBlockTemplates()
    {
        var templates = new ObservableCollection<BlockTemplate>();

        // 標準ブロック
        AddStandardBlocks(templates);

        // プラグインからブロックを追加
        AddPluginBlocks(templates);

        return templates;
    }

    /// <summary>
    /// プラグインからブロックを追加
    /// </summary>
    private void AddPluginBlocks(ObservableCollection<BlockTemplate> templates)
    {
        if (_pluginLoader == null)
            return;

        foreach (var provider in _pluginLoader.BlockProviders)
        {
            try
            {
                foreach (var blockTemplate in provider.GetBlockTemplates())
                {
                    templates.Add(blockTemplate);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"プラグインブロック読み込みエラー: {ex.Message}");
            }
        }
    }

    private void AddStandardBlocks(ObservableCollection<BlockTemplate> templates)
    {
        // 変数カテゴリ
        templates.Add(new BlockTemplate(
            "VariableDeclare",
            "変数を宣言",
            BlockCategory.Variables,
            () => new VariableDeclareBlock()));

        templates.Add(new BlockTemplate(
            "VariableAssign",
            "変数に代入",
            BlockCategory.Variables,
            () => new VariableAssignBlock()));

        templates.Add(new BlockTemplate(
            "LocalVariable",
            "ローカル変数",
            BlockCategory.Variables,
            () => new LocalVariableBlock()));

        // 制御カテゴリ
        templates.Add(new BlockTemplate(
            "If",
            "もし～なら",
            BlockCategory.Control,
            () => new IfBlock()));

        templates.Add(new BlockTemplate(
            "IfElse",
            "もし～なら/そうでなければ",
            BlockCategory.Control,
            () => new IfElseBlock()));

        templates.Add(new BlockTemplate(
            "While",
            "～の間繰り返す",
            BlockCategory.Control,
            () => new WhileBlock()));

        templates.Add(new BlockTemplate(
            "For",
            "回数繰り返す",
            BlockCategory.Control,
            () => new ForBlock()));

        templates.Add(new BlockTemplate(
            "ForEach",
            "各要素について繰り返す",
            BlockCategory.Control,
            () => new ForEachBlock()));

        templates.Add(new BlockTemplate(
            "Break",
            "繰り返しを抜ける",
            BlockCategory.Control,
            () => new BreakBlock()));

        templates.Add(new BlockTemplate(
            "Continue",
            "次の繰り返しへ",
            BlockCategory.Control,
            () => new ContinueBlock()));

        // メソッドカテゴリ
        templates.Add(new BlockTemplate(
            "MethodDefine",
            "メソッドを定義",
            BlockCategory.Methods,
            () => new MethodDefineBlock()));

        templates.Add(new BlockTemplate(
            "MethodCall",
            "メソッド呼び出し",
            BlockCategory.Methods,
            () => new MethodCallBlock()));

        templates.Add(new BlockTemplate(
            "StaticMethodCall",
            "静的メソッド呼び出し",
            BlockCategory.Methods,
            () => new StaticMethodCallBlock()));

        templates.Add(new BlockTemplate(
            "Return",
            "戻り値",
            BlockCategory.Methods,
            () => new ReturnBlock()));

        // クラスカテゴリ
        templates.Add(new BlockTemplate(
            "ClassDefine",
            "クラスを定義",
            BlockCategory.Classes,
            () => new ClassDefineBlock()));

        templates.Add(new BlockTemplate(
            "FieldDefine",
            "フィールドを定義",
            BlockCategory.Classes,
            () => new FieldDefineBlock()));

        templates.Add(new BlockTemplate(
            "PropertyDefine",
            "プロパティを定義",
            BlockCategory.Classes,
            () => new PropertyDefineBlock()));

        templates.Add(new BlockTemplate(
            "PropertyAccess",
            "プロパティアクセス",
            BlockCategory.Classes,
            () => new PropertyAccessBlock()));

        templates.Add(new BlockTemplate(
            "PropertyAssign",
            "プロパティに代入",
            BlockCategory.Classes,
            () => new PropertyAssignBlock()));

        // 入出力カテゴリ
        templates.Add(new BlockTemplate(
            "ConsoleWrite",
            "出力する",
            BlockCategory.IO,
            () => new ConsoleWriteBlock()));

        templates.Add(new BlockTemplate(
            "ConsoleWriteLine",
            "出力して改行",
            BlockCategory.IO,
            () => new ConsoleWriteLineBlock()));

        templates.Add(new BlockTemplate(
            "ConsoleReadLine",
            "入力を受け取る",
            BlockCategory.IO,
            () => new ConsoleReadLineBlock()));

        templates.Add(new BlockTemplate(
            "ConsoleReadInt",
            "数値を入力",
            BlockCategory.IO,
            () => new ConsoleReadIntBlock()));
    }
}

/// <summary>
/// ブロックテンプレート
/// </summary>
public class BlockTemplate
{
    public string Id { get; }
    public string DisplayName { get; }
    public BlockCategory Category { get; }
    public Func<BlockBase> CreateBlock { get; }
    public string? Description { get; }
    public string? Icon { get; }

    public BlockTemplate(
        string id,
        string displayName,
        BlockCategory category,
        Func<BlockBase> createBlock,
        string? description = null,
        string? icon = null)
    {
        Id = id;
        DisplayName = displayName;
        Category = category;
        CreateBlock = createBlock;
        Description = description;
        Icon = icon;
    }

    public BlockBase Create()
    {
        return CreateBlock();
    }
}
