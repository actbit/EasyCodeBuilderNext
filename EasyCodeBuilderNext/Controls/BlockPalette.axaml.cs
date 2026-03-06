using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.Models;
using EasyCodeBuilderNext.Core.PluginSystem;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Controls;

/// <summary>
/// ブロックパレット - 利用可能なブロックの一覧を表示
/// </summary>
public partial class BlockPalette : UserControl
{
    public static readonly StyledProperty<ObservableCollection<BlockTemplate>?> BlockTemplatesProperty =
        AvaloniaProperty.Register<BlockPalette, ObservableCollection<BlockTemplate>?>(nameof(BlockTemplates));

    public static readonly StyledProperty<BlockCategory?> SelectedCategoryProperty =
        AvaloniaProperty.Register<BlockPalette, BlockCategory?>(nameof(SelectedCategory));

    public ObservableCollection<BlockTemplate>? BlockTemplates
    {
        get => GetValue(BlockTemplatesProperty);
        set => SetValue(BlockTemplatesProperty, value);
    }

    public BlockCategory? SelectedCategory
    {
        get => GetValue(SelectedCategoryProperty);
        set => SetValue(SelectedCategoryProperty, value);
    }

    public BlockPalette()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnBlockPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && control.DataContext is BlockTemplate template)
        {
            var block = template.Create();

            var dragData = new DataObject();
            dragData.Set("Block", block);
            dragData.Set("IsNew", true);

            DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
        }
    }

    private void OnCategoryClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is BlockCategory category)
        {
            SelectedCategory = SelectedCategory == category ? null : category;
        }
    }

    public void FilterByCategory(BlockCategory? category)
    {
        SelectedCategory = category;
    }

    public void SearchBlocks(string query)
    {
        // 検索ロジックはViewModelで実装
    }
}
