using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EasyCodeBuilderNext.Controls;

public class PuzzleBlock : ContentControl
{
    public static readonly StyledProperty<BlockBase?> BlockProperty =
        AvaloniaProperty.Register<PuzzleBlock, BlockBase?>(nameof(Block));

    public BlockBase? Block
    {
        get => GetValue(BlockProperty);
        set => SetValue(BlockProperty, value);
    }

    private bool _isDragging;
    private Point _dragStart;
    private double _offsetX;
    private double _offsetY;
    private Border? _border;
    private TextBlock? _text;
    private Border? _topConnector;
    private Border? _bottomConnector;
    private PuzzleBlock? _snapCandidate;

    // スナップ距離の閾値
    private const double SnapThreshold = 30;

    public PuzzleBlock()
    {
        Width = 150;
        Height = 60;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BlockProperty)
        {
            // Blockプロパティが変更されたらビジュアルを再構築
            BuildVisualTree();
            UpdatePosition();

            // 古いBlockのイベント購読を解除
            if (change.OldValue is BlockBase oldBlock)
            {
                oldBlock.PropertyChanged -= Block_PropertyChanged;
            }

            // 新しいBlockのイベントを購読
            if (change.NewValue is BlockBase newBlock)
            {
                newBlock.PropertyChanged += Block_PropertyChanged;
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        BuildVisualTree();
        UpdatePosition();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (Block != null)
        {
            Block.PropertyChanged -= Block_PropertyChanged;
        }
    }

    private void Block_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BlockBase.X) || e.PropertyName == nameof(BlockBase.Y))
        {
            UpdatePosition();
        }
        else if (e.PropertyName == nameof(BlockBase.IsHighlighted))
        {
            UpdateHighlightState();
        }
    }

    private void UpdateHighlightState()
    {
        if (_border == null || Block == null) return;

        if (Block.IsHighlighted)
        {
            _border.BorderThickness = new Thickness(3);
            _border.BorderBrush = Brushes.Yellow;
        }
        else
        {
            _border.BorderThickness = new Thickness(0);
            _border.BorderBrush = Brushes.Transparent;
        }
    }

    private static Color DarkenColor(Color color, float amount)
    {
        return new Color(
            color.A,
            (byte)Math.Max(0, color.R - (255 * amount)),
            (byte)Math.Max(0, color.G - (255 * amount)),
            (byte)Math.Max(0, color.B - (255 * amount))
        );
    }

    private void BuildVisualTree()
    {
        if (Block == null) return;

        var color = Color.Parse(Block.Category.GetColor());
        var darkerColor = DarkenColor(color, 0.2f);

        // メインコンテナ
        var mainPanel = new Grid();

        // コネクタ表示用のオーバーレイ
        var connectorPanel = new Canvas();

        // 上部コネクタ（凹み）
        if (Block.HasTopConnector)
        {
            _topConnector = new Border
            {
                Width = 30,
                Height = 8,
                Background = new SolidColorBrush(darkerColor),
                CornerRadius = new CornerRadius(0, 0, 4, 4),
                IsVisible = true
            };
            Canvas.SetLeft(_topConnector, 20);
            Canvas.SetTop(_topConnector, 0);
            connectorPanel.Children.Add(_topConnector);
        }

        // 下部コネクタ（凸）
        if (Block.HasBottomConnector)
        {
            _bottomConnector = new Border
            {
                Width = 30,
                Height = 8,
                Background = new SolidColorBrush(darkerColor),
                CornerRadius = new CornerRadius(4, 4, 0, 0),
                IsVisible = true
            };
            Canvas.SetLeft(_bottomConnector, 20);
            Canvas.SetBottom(_bottomConnector, 0);
            connectorPanel.Children.Add(_bottomConnector);
        }

        // メインボディ
        _border = new Border
        {
            Background = new SolidColorBrush(color),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 10, 12, 10),
            Cursor = Cursor.Parse("Hand"),
            Margin = new Thickness(0, Block.HasTopConnector ? 6 : 0, 0, Block.HasBottomConnector ? 6 : 0)
        };

        _text = new TextBlock
        {
            Text = Block.DisplayName,
            Foreground = Brushes.White,
            FontSize = 13,
            FontWeight = FontWeight.Medium,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };

        _border.Child = _text;

        mainPanel.Children.Add(_border);
        mainPanel.Children.Add(connectorPanel);

        Content = mainPanel;

        // ハイライト状態を更新
        UpdateHighlightState();
    }

    private void UpdatePosition()
    {
        if (Block == null) return;

        // Canvasの直接の子要素を見つける
        Visual? visual = this;
        Visual? canvasChild = null;

        while (visual != null)
        {
            var parent = visual.GetVisualParent();
            if (parent is Canvas)
            {
                canvasChild = visual;
                break;
            }
            visual = parent;
        }

        if (canvasChild != null)
        {
            Canvas.SetLeft(canvasChild, Block.X);
            Canvas.SetTop(canvasChild, Block.Y);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (Block == null) return;

        var canvas = this.FindAncestorOfType<Canvas>();
        if (canvas == null) return;

        _isDragging = true;
        _dragStart = e.GetPosition(canvas);
        _offsetX = Block.X - _dragStart.X;
        _offsetY = Block.Y - _dragStart.Y;

        // 既存の接続を解除
        Block.Detach();

        ZIndex = 1000;
        Opacity = 0.8;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_isDragging || Block == null) return;

        var canvas = this.FindAncestorOfType<Canvas>();
        if (canvas == null) return;

        var pos = e.GetPosition(canvas);
        Block.X = pos.X + _offsetX;
        Block.Y = pos.Y + _offsetY;

        // スナップ候補を探す
        FindSnapCandidate();

        e.Handled = true;
    }

    private void FindSnapCandidate()
    {
        if (Block == null) return;

        var canvas = this.FindAncestorOfType<Canvas>();
        if (canvas == null) return;

        // 全てのハイライトを解除
        foreach (var child in canvas.Children)
        {
            if (child is PuzzleBlock otherBlock && otherBlock.Block != null && otherBlock != this)
            {
                otherBlock.Block.IsHighlighted = false;
            }
        }

        _snapCandidate = null;

        // このブロックの下部コネクタ位置
        var myBottomY = Block.Y + Bounds.Height;
        var myLeftX = Block.X;

        // 他のブロックをチェック
        foreach (var child in canvas.Children)
        {
            if (child is PuzzleBlock otherBlock && otherBlock.Block != null && otherBlock != this)
            {
                var other = otherBlock.Block;

                // 上部に接続（このブロックの下に他のブロックを接続）
                if (Block.HasBottomConnector && otherBlock.Block.HasTopConnector)
                {
                    var otherTopY = other.Y;
                    var otherLeftX = other.X;

                    var dx = Math.Abs(myLeftX - otherLeftX);
                    var dy = Math.Abs(myBottomY - otherTopY);

                    if (dx < SnapThreshold && dy < SnapThreshold)
                    {
                        otherBlock.Block.IsHighlighted = true;
                        _snapCandidate = otherBlock;
                        return;
                    }
                }

                // 下部に接続（このブロックを他のブロックの下に接続）
                if (Block.HasTopConnector && otherBlock.Block.HasBottomConnector)
                {
                    var otherBottomY = other.Y + otherBlock.Bounds.Height;
                    var otherLeftX = other.X;

                    var dx = Math.Abs(myLeftX - otherLeftX);
                    var dy = Math.Abs(Block.Y - otherBottomY);

                    if (dx < SnapThreshold && dy < SnapThreshold)
                    {
                        otherBlock.Block.IsHighlighted = true;
                        _snapCandidate = otherBlock;
                        return;
                    }
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_isDragging) return;

        _isDragging = false;
        Opacity = 1.0;
        ZIndex = 0;
        e.Pointer.Capture(null);

        // スナップ実行
        if (_snapCandidate != null && Block != null && _snapCandidate.Block != null)
        {
            PerformSnap();
        }

        // 全てのハイライトを解除
        var canvas = this.FindAncestorOfType<Canvas>();
        if (canvas != null)
        {
            foreach (var child in canvas.Children)
            {
                if (child is PuzzleBlock otherBlock && otherBlock.Block != null)
                {
                    otherBlock.Block.IsHighlighted = false;
                }
            }
        }

        _snapCandidate = null;
        e.Handled = true;
    }

    private void PerformSnap()
    {
        if (Block == null || _snapCandidate?.Block == null) return;

        var myBlock = Block;
        var otherBlock = _snapCandidate.Block;

        // このブロックの下に他のブロックを接続する場合
        if (myBlock.HasBottomConnector && otherBlock.HasTopConnector)
        {
            var myBottomY = myBlock.Y + Bounds.Height;
            var dx = Math.Abs(myBlock.X - otherBlock.X);
            var dy = Math.Abs(myBottomY - otherBlock.Y);

            if (dx < SnapThreshold && dy < SnapThreshold)
            {
                // 位置をスナップ
                otherBlock.X = myBlock.X;
                otherBlock.Y = myBottomY;

                // 接続関係を設定
                myBlock.NextBlock = otherBlock;
                otherBlock.PreviousBlock = myBlock;

                Debug.WriteLine($"Connected: {myBlock.DisplayName} -> {otherBlock.DisplayName}");
                return;
            }
        }

        // このブロックを他のブロックの下に接続する場合
        if (myBlock.HasTopConnector && otherBlock.HasBottomConnector)
        {
            var otherBottomY = otherBlock.Y + _snapCandidate.Bounds.Height;
            var dx = Math.Abs(myBlock.X - otherBlock.X);
            var dy = Math.Abs(myBlock.Y - otherBottomY);

            if (dx < SnapThreshold && dy < SnapThreshold)
            {
                // 位置をスナップ
                myBlock.X = otherBlock.X;
                myBlock.Y = otherBottomY;

                // 接続関係を設定
                otherBlock.NextBlock = myBlock;
                myBlock.PreviousBlock = otherBlock;

                Debug.WriteLine($"Connected: {otherBlock.DisplayName} -> {myBlock.DisplayName}");
            }
        }
    }
}
