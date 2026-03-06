using CommunityToolkit.Mvvm.ComponentModel;
using EasyCodeBuilderNext.Core.Blocks;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Core.Models;

/// <summary>
/// プロジェクト全体を表すモデル
/// </summary>
public partial class Project : ObservableObject
{
    /// <summary>
    /// プロジェクト名
    /// </summary>
    [ObservableProperty]
    private string _name = "NewProject";

    /// <summary>
    /// プロジェクトの一意識別子
    /// </summary>
    [ObservableProperty]
    private Guid _id = Guid.NewGuid();

    /// <summary>
    /// デフォルトの名前空間
    /// </summary>
    [ObservableProperty]
    private string _defaultNamespace = "GeneratedCode";

    /// <summary>
    /// プロジェクト内のオブジェクト（クラス）リスト
    /// </summary>
    public ObservableCollection<CodeObject> Objects { get; } = new();

    /// <summary>
    /// 現在選択されているオブジェクト
    /// </summary>
    [ObservableProperty]
    private CodeObject? _currentObject;

    /// <summary>
    /// 参照DLLのリスト
    /// </summary>
    public ObservableCollection<string> References { get; } = new();

    /// <summary>
    /// usingディレクティブのリスト
    /// </summary>
    public ObservableCollection<string> Usings { get; } = new()
    {
        "System",
        "System.Collections.Generic",
        "System.Linq"
    };

    /// <summary>
    /// 新しいオブジェクトを追加
    /// </summary>
    public CodeObject AddObject(string name)
    {
        var obj = new CodeObject
        {
            Name = name,
            Namespace = DefaultNamespace
        };
        Objects.Add(obj);
        CurrentObject ??= obj;
        return obj;
    }

    /// <summary>
    /// オブジェクトを削除
    /// </summary>
    public void RemoveObject(CodeObject obj)
    {
        Objects.Remove(obj);
        if (CurrentObject == obj)
        {
            CurrentObject = Objects.FirstOrDefault();
        }
    }

    /// <summary>
    /// オブジェクト名を変更
    /// </summary>
    public void RenameObject(CodeObject obj, string newName)
    {
        obj.Name = newName;
    }

    /// <summary>
    /// 継承関係を設定
    /// </summary>
    public void SetInheritance(CodeObject child, string? baseClassName)
    {
        child.BaseClassName = baseClassName;
    }

    /// <summary>
    /// 継承ツリーを取得（指定したオブジェクトの祖先を取得）
    /// </summary>
    public IEnumerable<CodeObject> GetAncestors(CodeObject obj)
    {
        var current = obj;
        while (!string.IsNullOrEmpty(current.BaseClassName))
        {
            var baseObj = Objects.FirstOrDefault(o => o.Name == current.BaseClassName);
            if (baseObj == null) break;
            yield return baseObj;
            current = baseObj;
        }
    }

    /// <summary>
    /// 指定したオブジェクトの子孫を取得
    /// </summary>
    public IEnumerable<CodeObject> GetDescendants(CodeObject obj)
    {
        foreach (var child in Objects.Where(o => o.BaseClassName == obj.Name))
        {
            yield return child;
            foreach (var descendant in GetDescendants(child))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// 継承関係のあるオブジェクトペアを取得
    /// </summary>
    public IEnumerable<(CodeObject Parent, CodeObject Child)> GetInheritanceRelations()
    {
        foreach (var obj in Objects.Where(o => !string.IsNullOrEmpty(o.BaseClassName)))
        {
            var parent = Objects.FirstOrDefault(o => o.Name == obj.BaseClassName);
            if (parent != null)
            {
                yield return (parent, obj);
            }
        }
    }
}
