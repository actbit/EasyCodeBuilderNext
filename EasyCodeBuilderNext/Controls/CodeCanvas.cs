using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using EasyCodeBuilderNext.Core.Blocks;
using System;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Controls;

/// <summary>
/// ブロックを配置するキャンバス
/// ドラッグ＆ドロップとスナップ機能を提供
/// </summary>
public class CodeCanvas : Canvas
{
    #region 依存関係プロパティ

    public static readonly StyledProperty<ObservableCollection<BlockBase>?> BlocksProperty =
        AvaloniaProperty.Register<CodeCanvas, ObservableCollection<BlockBase>?>(nameof(Blocks));

    public static readonly StyledProperty<BlockBase?> SelectedBlockProperty =
        AvaloniaProperty.Register<CodeCanvas, BlockBase?>(nameof(SelectedBlock));

    public static readonly StyledProperty<double> SnapThresholdProperty =
        AvaloniaProperty.Register<CodeCanvas, double>(nameof(SnapThreshold), 20.0);

    #endregion

    #region プロパティ

    public ObservableCollection<BlockBase>? Blocks
    {
        get => GetValue(BlocksProperty);
        set => SetValue(BlocksProperty, value);
    }

    public BlockBase? SelectedBlock
    {
        get => GetValue(SelectedBlockProperty);
        set => SetValue(SelectedBlockProperty, value);
    }

    public double SnapThreshold
    {
        get => GetValue(SnapThresholdProperty);
        set => SetValue(SnapThresholdProperty, value);
    }

    #endregion

    private Point _dragStartPoint;
    private Point _dragOffset;
    private bool _isDragging;
    private BlockBase? _draggingBlock;
    private PuzzleBlock? _draggingControl;

    public CodeCanvas()
    {
        Background = Brushes.Transparent;

        // イベントハンドラ
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetPosition(this);

        // クリックされたブロックを探す
        var block = FindBlockAtPoint(point);
        if (block != null)
        {
            SelectedBlock = block;
            _draggingBlock = block;
            _dragStartPoint = point;
            _dragOffset = new Point(block.X - point.X, block.Y - point.Y);
            _isDragging = true;
            block.IsDragging = true;

            e.Pointer.Capture(this);
            e.Handled = true;
        }
        else
        {
            SelectedBlock = null;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _draggingBlock == null)
            return;

        var point = e.GetPosition(this);
        var newX = point.X + _dragOffset.X;
        var newY = point.Y + _dragOffset.Y;

        _draggingBlock.X = newX;
        _draggingBlock.Y = newY;

        // スナップ判定
        CheckSnap(_draggingBlock);

        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_draggingBlock != null)
        {
            _draggingBlock.IsDragging = false;
            _draggingBlock.IsHighlighted = false;

            // スナップ実行
            PerformSnap(_draggingBlock);

            _draggingBlock = null;
        }

        _isDragging = false;
        e.Pointer.Capture(null);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (e.Data.Get("Block") is BlockBase block)
        {
            var point = e.GetPosition(this);

            if (e.Data.Get("IsNew") is bool isNew && isNew)
            {
                // 新しいブロックを追加
                var newBlock = block.Clone();
                newBlock.X = point.X;
                newBlock.Y = point.Y;
                Blocks?.Add(newBlock);
                SelectedBlock = newBlock;
            }
            else
            {
                // 既存ブロックを移動
                block.X = point.X;
                block.Y = point.Y;
            }

            e.DragEffects = DragDropEffects.Move;
        }
    }

    private BlockBase? FindBlockAtPoint(Point point)
    {
        if (Blocks == null) return null;

        // 逆順で探す（上にあるものを優先）
        for (int i = Blocks.Count - 1; i >= 0; i--)
        {
            var block = Blocks[i];
            if (point.X >= block.X && point.X <= block.X + block.Width &&
                point.Y >= block.Y && point.Y <= block.Y + block.Height)
            {
                return block;
            }
        }

        return null;
    }

    private void CheckSnap(BlockBase draggingBlock)
    {
        if (Blocks == null) return;

        // 全ブロックのスナップ状態をリセット
        foreach (var block in Blocks)
        {
            block.IsHighlighted = false;
        }

        // 最も近いスナップ先を探す
        BlockBase? nearestBlock = null;
        double nearestDistance = double.MaxValue;
        bool snapToBottom = true;

        foreach (var block in Blocks)
        {
            if (block == draggingBlock) continue;

            // 下部へのスナップ
            if (draggingBlock.HasTopConnector && block.HasBottomConnector)
            {
                var bottomSnapPoint = new Point(block.X, block.Y + block.Height);
                var distance = GetDistance(
                    new Point(draggingBlock.X, draggingBlock.Y),
                    bottomSnapPoint);

                if (distance < SnapThreshold && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestBlock = block;
                    snapToBottom = true;
                }
            }

            // 上部へのスナップ
            if (draggingBlock.HasBottomConnector && block.HasTopConnector)
            {
                var topSnapPoint = new Point(block.X, block.Y);
                var distance = GetDistance(
                    new Point(draggingBlock.X, draggingBlock.Y + draggingBlock.Height),
                    topSnapPoint);

                if (distance < SnapThreshold && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestBlock = block;
                    snapToBottom = false;
                }
            }
        }

        if (nearestBlock != null)
        {
            nearestBlock.IsHighlighted = true;
        }
    }

    private void PerformSnap(BlockBase block)
    {
        if (Blocks == null) return;

        foreach (var targetBlock in Blocks)
        {
            if (!targetBlock.IsHighlighted) continue;
            if (targetBlock == block) continue;

            // スナップ実行
            if (block.HasTopConnector && targetBlock.HasBottomConnector)
            {
                var snapPoint = new Point(targetBlock.X, targetBlock.Y + targetBlock.Height);
                var distance = GetDistance(new Point(block.X, block.Y), snapPoint);

                if (distance < SnapThreshold)
                {
                    // チェーン接続
                    targetBlock.InsertAfter(block);
                    block.X = targetBlock.X;
                    block.Y = targetBlock.Y + targetBlock.Height;
                    targetBlock.IsHighlighted = false;
                    return;
                }
            }

            if (block.HasBottomConnector && targetBlock.HasTopConnector)
            {
                var snapPoint = new Point(targetBlock.X, targetBlock.Y);
                var distance = GetDistance(
                    new Point(block.X, block.Y + block.Height),
                    snapPoint);

                if (distance < SnapThreshold)
                {
                    // チェーン接続
                    block.InsertAfter(targetBlock);
                    targetBlock.X = block.X;
                    targetBlock.Y = block.Y + block.Height;
                    targetBlock.IsHighlighted = false;
                    return;
                }
            }
        }
    }

    private static double GetDistance(Point p1, Point p2)
    {
        var dx = p1.X - p2.X;
        var dy = p1.Y - p2.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 選択されたブロックを削除
    /// </summary>
    public void DeleteSelectedBlock()
    {
        if (SelectedBlock != null && Blocks != null)
        {
            SelectedBlock.Detach();
            Blocks.Remove(SelectedBlock);
            SelectedBlock = null;
        }
    }

    /// <summary>
    /// 全ブロックをクリア
    /// </summary>
    public void ClearBlocks()
    {
        Blocks?.Clear();
        SelectedBlock = null;
    }
}
