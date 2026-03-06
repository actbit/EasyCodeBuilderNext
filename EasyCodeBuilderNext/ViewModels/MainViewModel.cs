using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.CodeGeneration;
using EasyCodeBuilderNext.Core.Models;
using EasyCodeBuilderNext.Core.PluginSystem;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EasyCodeBuilderNext.ViewModels;

/// <summary>
/// メインウィンドウのViewModel
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly RoslynCodeGenerator _codeGenerator = new();
    private readonly RoslynCompiler _compiler = new();
    private readonly BlockFactory _blockFactory;
    private readonly TypeRegistry _typeRegistry = new();
    private readonly PluginLoader _pluginLoader = new();

    [ObservableProperty]
    private Project _project = new();

    [ObservableProperty]
    private CodeObject? _selectedObject;

    [ObservableProperty]
    private BlockBase? _selectedBlock;

    [ObservableProperty]
    private ObservableCollection<BlockTemplate> _blockTemplates = new();

    [ObservableProperty]
    private string _generatedCode = "// コードを生成してください";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private BlockCategory? _selectedCategory;

    [ObservableProperty]
    private bool _isClassDiagramVisible;

    [ObservableProperty]
    private bool _isCodePreviewVisible = true;

    public MainViewModel()
    {
        _blockFactory = new BlockFactory(_typeRegistry, _pluginLoader);
        LoadBlockTemplates();

        // デフォルトのオブジェクトを追加
        var programClass = Project.AddObject("Program");
        SelectedObject = programClass;
    }

    private void LoadBlockTemplates()
    {
        BlockTemplates = _blockFactory.GetBlockTemplates();
    }

    [RelayCommand]
    private void AddObject()
    {
        var newObject = Project.AddObject($"Object{Project.Objects.Count + 1}");
        SelectedObject = newObject;
    }

    [RelayCommand]
    private void RemoveObject()
    {
        if (SelectedObject != null)
        {
            Project.RemoveObject(SelectedObject);
        }
    }

    [RelayCommand]
    private void SelectObject(CodeObject obj)
    {
        SelectedObject = obj;
    }

    [RelayCommand]
    private void AddBlock(BlockTemplate template)
    {
        if (SelectedObject != null)
        {
            var block = template.Create();
            block.X = 50;
            block.Y = SelectedObject.Blocks.Count * 50 + 50;
            block.OwnerObject = SelectedObject;
            SelectedObject.Blocks.Add(block);
            SelectedBlock = block;
        }
    }

    [RelayCommand]
    private void DeleteBlock()
    {
        if (SelectedBlock != null && SelectedObject != null)
        {
            SelectedBlock.Detach();
            SelectedObject.Blocks.Remove(SelectedBlock);
            SelectedBlock = null;
        }
    }

    [RelayCommand]
    private void GenerateCode()
    {
        if (SelectedObject != null)
        {
            GeneratedCode = _codeGenerator.GenerateCode(SelectedObject);
        }
        else
        {
            GeneratedCode = _codeGenerator.GenerateCode(Project);
        }
    }

    [RelayCommand]
    private async void RunCode()
    {
        GenerateCode();

        var result = _compiler.Compile(Project);

        if (result.Success)
        {
            try
            {
                _compiler.Execute(result);
            }
            catch (Exception ex)
            {
                GeneratedCode = $"実行エラー:\n{ex.Message}\n\n{GeneratedCode}";
            }
        }
        else
        {
            GeneratedCode = $"コンパイルエラー:\n{string.Join("\n", result.Errors)}\n\n{GeneratedCode}";
        }
    }

    [RelayCommand]
    private void SaveProject()
    {
        // TODO: プロジェクト保存機能を実装
    }

    [RelayCommand]
    private void LoadProject()
    {
        // TODO: プロジェクト読み込み機能を実装
    }

    [RelayCommand]
    private void ShowClassDiagram()
    {
        IsClassDiagramVisible = !IsClassDiagramVisible;
    }

    [RelayCommand]
    private void ToggleClassDiagram()
    {
        IsClassDiagramVisible = !IsClassDiagramVisible;
    }

    [RelayCommand]
    private void ToggleCodePreview()
    {
        IsCodePreviewVisible = !IsCodePreviewVisible;
    }

    [RelayCommand]
    private void RefreshDiagram()
    {
        // クラスダイアグラムの更新
        OnPropertyChanged(nameof(Project));
    }

    [RelayCommand]
    private void NewProject()
    {
        Project = new Project();
        var programClass = Project.AddObject("Program");
        SelectedObject = programClass;
        GeneratedCode = "// 新規プロジェクト";
    }

    [RelayCommand]
    private void Exit()
    {
        // アプリケーション終了
        System.Environment.Exit(0);
    }

    [RelayCommand]
    private void Undo()
    {
        // TODO: Undo機能を実装
    }

    [RelayCommand]
    private void Redo()
    {
        // TODO: Redo機能を実装
    }

    [RelayCommand]
    private void ExportExe()
    {
        // TODO: EXE出力機能を実装
    }

    [RelayCommand]
    private void LoadDll()
    {
        // TODO: DLL読み込み機能を実装
    }

    [RelayCommand]
    private void About()
    {
        // TODO: バージョン情報ダイアログを表示
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilterBlocks();
    }

    partial void OnSelectedCategoryChanged(BlockCategory? value)
    {
        FilterBlocks();
    }

    private void FilterBlocks()
    {
        var allTemplates = _blockFactory.GetBlockTemplates();

        var filtered = allTemplates.Where(t =>
        {
            // カテゴリフィルタ
            if (SelectedCategory.HasValue && t.Category != SelectedCategory.Value)
                return false;

            // 検索フィルタ
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                return t.DisplayName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                       (t.Description?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            return true;
        });

        BlockTemplates = new ObservableCollection<BlockTemplate>(filtered);
    }
}
