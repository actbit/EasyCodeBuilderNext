using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.PluginSystem;
using EasyCodeBuilderNext.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace EasyCodeBuilderNext.Views;

public partial class MainView : UserControl
{
    private BlockTemplate? _draggingTemplate;
    private const double CanvasPadding = 200; // 余白
    private const double MinCanvasSize = 2000;

    public static readonly RoutedEvent<RoutedEventArgs> CanvasResizeNeededEvent =
        RoutedEvent.Register<MainView, RoutedEventArgs>(nameof(CanvasResizeNeeded), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? CanvasResizeNeeded
    {
        add => AddHandler(CanvasResizeNeededEvent, value);
        remove => RemoveHandler(CanvasResizeNeededEvent, value);
    }

    public MainView()
    {
        InitializeComponent();
        Debug.WriteLine("MainView constructor called");

        this.AddHandler(DragDrop.DropEvent, OnCanvasDrop);
        this.AddHandler(DragDrop.DragOverEvent, OnCanvasDragOver);

        // キャンバスリサイズイベントを監視
        this.AddHandler(CanvasResizeNeededEvent, OnCanvasResizeNeeded);
    }

    private void OnCanvasResizeNeeded(object? sender, RoutedEventArgs e)
    {
        UpdateCanvasSize();
    }

    public void UpdateCanvasSize()
    {
        var canvas = this.FindDescendantOfType<Canvas>();
        if (canvas == null) return;

        var vm = DataContext as MainViewModel;
        if (vm?.SelectedObject?.Blocks == null) return;

        double maxX = MinCanvasSize;
        double maxY = MinCanvasSize;

        foreach (var block in vm.SelectedObject.Blocks)
        {
            var blockRight = block.X + 250; // ブロック幅を考慮
            var blockBottom = block.Y + 150; // ブロック高さを考慮（制御構造の場合大きい）

            if (blockRight > maxX) maxX = blockRight;
            if (blockBottom > maxY) maxY = blockBottom;
        }

        // 余白を追加
        maxX += CanvasPadding;
        maxY += CanvasPadding;

        // 現在のサイズより大きい場合のみ更新
        if (maxX > canvas.Width || maxY > canvas.Height)
        {
            canvas.Width = maxX;
            canvas.Height = maxY;
            Debug.WriteLine($"Canvas resized to: {maxX} x {maxY}");
        }
    }

    private void OnBlockTapped(object? sender, TappedEventArgs e)
    {
        Debug.WriteLine($"OnBlockTapped called, sender: {sender?.GetType().Name}");

        if (sender is Border border)
        {
            Debug.WriteLine($"Border.Tag: {border.Tag?.GetType().Name ?? "null"}");

            if (border.Tag is BlockTemplate template)
            {
                Debug.WriteLine($"BlockTemplate: {template.DisplayName}");

                if (DataContext is MainViewModel vm)
                {
                    Debug.WriteLine("Executing AddBlockCommand");
                    vm.AddBlockCommand.Execute(template);
                }
                else
                {
                    Debug.WriteLine($"DataContext is not MainViewModel: {DataContext?.GetType().Name ?? "null"}");
                }
            }
        }
    }

    private void OnBlockPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Debug.WriteLine($"OnBlockPointerPressed called, sender: {sender?.GetType().Name}");

        if (sender is Border border)
        {
            Debug.WriteLine($"Border.Tag: {border.Tag?.GetType().Name ?? "null"}");

            if (border.Tag is BlockTemplate template)
            {
                Debug.WriteLine($"BlockTemplate: {template.DisplayName}");

                if (DataContext is MainViewModel vm)
                {
                    Debug.WriteLine("Executing AddBlockCommand");
                    vm.AddBlockCommand.Execute(template);
                }
                else
                {
                    Debug.WriteLine($"DataContext is not MainViewModel: {DataContext?.GetType().Name ?? "null"}");
                }
            }
        }
    }

    private void OnPaletteBlockTapped(object? sender, TappedEventArgs e)
    {
        Debug.WriteLine($"OnPaletteBlockTapped called");

        if (sender is Border border && border.Tag is BlockTemplate template)
        {
            Debug.WriteLine($"Template: {template.DisplayName}");

            if (DataContext is MainViewModel vm && vm.SelectedObject != null)
            {
                Debug.WriteLine("Adding block via tap");
                var block = template.Create();
                block.X = 50;
                block.Y = vm.SelectedObject.Blocks.Count * 60 + 50;
                block.OwnerObject = vm.SelectedObject;
                vm.SelectedObject.Blocks.Add(block);
                vm.SelectedBlock = block;

                Debug.WriteLine($"Block added: {block.DisplayName} at ({block.X}, {block.Y})");
                Debug.WriteLine($"Total blocks: {vm.SelectedObject.Blocks.Count}");
            }
            else
            {
                Debug.WriteLine($"Cannot add block: VM={DataContext != null}, SelectedObject={((MainViewModel?)DataContext)?.SelectedObject != null}");
            }
        }
    }

    private void OnPaletteBlockPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Debug.WriteLine($"OnPaletteBlockPointerPressed called");

        if (sender is Border border && border.Tag is BlockTemplate template)
        {
            Debug.WriteLine($"Starting drag for: {template.DisplayName}");
            _draggingTemplate = template;

            var dragData = new DataObject();
            dragData.Set("BlockTemplate", template);

            DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy).ContinueWith(_ =>
            {
                _draggingTemplate = null;
            });
        }
    }

    private void OnCanvasDragOver(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains("BlockTemplate"))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void OnCanvasDrop(object? sender, DragEventArgs e)
    {
        Debug.WriteLine("OnCanvasDrop called");

        if (e.Data.Contains("BlockTemplate") && DataContext is MainViewModel vm)
        {
            var template = e.Data.Get("BlockTemplate") as BlockTemplate;
            if (template != null)
            {
                // キャンバス上の位置を取得
                var canvas = this.FindDescendantOfType<Canvas>();
                if (canvas != null)
                {
                    var dropPosition = e.GetPosition(canvas);
                    Debug.WriteLine($"Drop position: ({dropPosition.X}, {dropPosition.Y})");

                    // ブロックを作成して位置を設定
                    if (vm.SelectedObject != null)
                    {
                        var block = template.Create();
                        block.X = dropPosition.X;
                        block.Y = dropPosition.Y;
                        block.OwnerObject = vm.SelectedObject;
                        vm.SelectedObject.Blocks.Add(block);
                        vm.SelectedBlock = block;

                        Debug.WriteLine($"Block added at position ({block.X}, {block.Y})");
                    }
                    else
                    {
                        Debug.WriteLine("No SelectedObject");
                    }
                }
            }
        }
    }
}