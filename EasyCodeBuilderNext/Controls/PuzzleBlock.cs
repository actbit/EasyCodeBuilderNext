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
    private PuzzleBlock? _snapCandidate;
    private bool _snapToBottom; // true: 相手の下に接続, false: 相手の内側に接続

    private const double SnapThreshold = 40;
    private const double BlockWidth = 180;
    private const double BlockHeight = 40;
    private const double ConnectorSize = 12;
    private const double InnerIndent = 20;

    // ブロックの実際のサイズ（制御構造は動的に変わる）
    public double ActualBlockHeight { get; private set; } = BlockHeight;

    public PuzzleBlock()
    {
        Width = BlockWidth;
        Height = BlockHeight;
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BlockProperty)
        {
            BuildVisualTree();
            UpdatePosition();

            if (change.OldValue is BlockBase oldBlock)
            {
                oldBlock.PropertyChanged -= Block_PropertyChanged;
            }

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
            // 再描画をトリガー
            InvalidateVisual();
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
        var darkerColor = DarkenColor(color, 0.15f);
        var lighterColor = Color.FromRgb(
            (byte)Math.Min(255, color.R + 30),
            (byte)Math.Min(255, color.G + 30),
            (byte)Math.Min(255, color.B + 30)
        );

        var mainPanel = new Grid();

        if (Block.BlockType == BlockType.ControlStructure)
        {
            // 制御構造（if, while, for）- C字型
            BuildControlStructureBlock(mainPanel, color, darkerColor, lighterColor);
        }
        else
        {
            // 通常のステートメントブロック
            BuildStatementBlock(mainPanel, color, darkerColor, lighterColor);
        }

        Content = mainPanel;
    }

    private void BuildStatementBlock(Grid mainPanel, Color color, Color darkerColor, Color lighterColor)
    {
        var path = CreateStatementPath(color, darkerColor, lighterColor);
        mainPanel.Children.Add(path);

        // テキスト
        var textBlock = new TextBlock
        {
            Text = Block!.DisplayName,
            Foreground = Brushes.White,
            FontSize = 13,
            FontWeight = FontWeight.Medium,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(10, 5, 10, 5),
            TextWrapping = TextWrapping.Wrap
        };
        mainPanel.Children.Add(textBlock);

        Height = BlockHeight;
        ActualBlockHeight = BlockHeight;
    }

    private void BuildControlStructureBlock(Grid mainPanel, Color color, Color darkerColor, Color lighterColor)
    {
        // 内部ブロックの数に応じて高さを計算
        double innerHeight = 60; // 最小の内部高さ
        if (Block!.InnerBlocks.Count > 0)
        {
            innerHeight = 40; // 各内部ブロックの高さ分
        }

        double totalHeight = BlockHeight + innerHeight + BlockHeight; // ヘッダー + 内部 + フッター
        Height = totalHeight;
        ActualBlockHeight = totalHeight;

        var path = CreateControlStructurePath(color, darkerColor, lighterColor, innerHeight);
        mainPanel.Children.Add(path);

        // ヘッダーテキスト
        var headerText = new TextBlock
        {
            Text = Block.DisplayName,
            Foreground = Brushes.White,
            FontSize = 13,
            FontWeight = FontWeight.Medium,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Thickness(25, 8, 10, 0)
        };
        mainPanel.Children.Add(headerText);

        // 内部エリアのラベル
        var innerLabel = new TextBlock
        {
            Text = "ここにブロックを追加",
            Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
            FontSize = 11,
            FontStyle = FontStyle.Italic,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, BlockHeight + 10, 0, 0)
        };
        mainPanel.Children.Add(innerLabel);
    }

    private Path CreateStatementPath(Color color, Color darkerColor, Color lighterColor)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            double w = BlockWidth;
            double h = BlockHeight;
            double c = ConnectorSize;
            double r = 6; // 角丸

            // パズルピースの形状（上に凹み、下に凸）
            context.BeginFigure(new Point(0, c), true);
            context.LineTo(new Point(15, c));
            context.LineTo(new Point(20, 0)); // 上部の凹み
            context.LineTo(new Point(35, 0));
            context.LineTo(new Point(40, c));
            context.LineTo(new Point(w - r, c));
            context.ArcTo(new Point(w, c + r), new Size(r, r), 0, false, SweepDirection.Clockwise);
            context.LineTo(new Point(w, h - c - r));
            context.ArcTo(new Point(w - r, h - c), new Size(r, r), 0, false, SweepDirection.Clockwise);
            context.LineTo(new Point(40, h - c));
            context.LineTo(new Point(35, h)); // 下部の凸
            context.LineTo(new Point(20, h));
            context.LineTo(new Point(15, h - c));
            context.LineTo(new Point(r, h - c));
            context.ArcTo(new Point(0, h - c - r), new Size(r, r), 0, false, SweepDirection.Clockwise);
            context.LineTo(new Point(0, c + r));
            context.ArcTo(new Point(r, c), new Size(r, r), 0, false, SweepDirection.Clockwise);
            context.EndFigure(true);
        }

        return new Path
        {
            Data = geometry,
            Fill = new SolidColorBrush(color),
            Stroke = new SolidColorBrush(darkerColor),
            StrokeThickness = 1.5
        };
    }

    private Path CreateControlStructurePath(Color color, Color darkerColor, Color lighterColor, double innerHeight)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            double w = BlockWidth;
            double headerH = BlockHeight;
            double innerH = innerHeight;
            double footerH = BlockHeight;
            double totalH = headerH + innerH + footerH;
            double c = ConnectorSize;
            double indent = InnerIndent;
            double r = 6;

            // C字型のパズルブロック
            context.BeginFigure(new Point(0, c), true);

            // 上部ヘッダー
            context.LineTo(new Point(15, c));
            context.LineTo(new Point(20, 0)); // 上部の凹み
            context.LineTo(new Point(35, 0));
            context.LineTo(new Point(40, c));
            context.LineTo(new Point(w - r, c));
            context.ArcTo(new Point(w, c + r), new Size(r, r), 0, false, SweepDirection.Clockwise);
            context.LineTo(new Point(w, headerH - r));
            context.ArcTo(new Point(w - r, headerH), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 右側から内側へ（内部ブロックエリアの開始）
            context.LineTo(new Point(indent + r, headerH));
            context.ArcTo(new Point(indent, headerH + r), new Size(r, r), 0, false, SweepDirection.Clockwise);
            context.LineTo(new Point(indent, headerH + innerH - r));
            context.ArcTo(new Point(indent + r, headerH + innerH), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // フッター（内部ブロックエリアの終わり）
            context.LineTo(new Point(w - r, headerH + innerH));
            context.ArcTo(new Point(w, headerH + innerH + r), new Size(r, r), 0, false, SweepDirection.Clockwise);
            context.LineTo(new Point(w, totalH - c - r));
            context.ArcTo(new Point(w - r, totalH - c), new Size(r, r), 0, false, SweepDirection.Clockwise);

            context.LineTo(new Point(40, totalH - c));
            context.LineTo(new Point(35, totalH)); // 下部の凸
            context.LineTo(new Point(20, totalH));
            context.LineTo(new Point(15, totalH - c));
            context.LineTo(new Point(r, totalH - c));
            context.ArcTo(new Point(0, totalH - c - r), new Size(r, r), 0, false, SweepDirection.Clockwise);
            context.LineTo(new Point(0, c + r));
            context.ArcTo(new Point(r, c), new Size(r, r), 0, false, SweepDirection.Clockwise);

            context.EndFigure(true);
        }

        return new Path
        {
            Data = geometry,
            Fill = new SolidColorBrush(color),
            Stroke = new SolidColorBrush(darkerColor),
            StrokeThickness = 1.5
        };
    }

    private void UpdatePosition()
    {
        if (Block == null) return;

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

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // ハイライト表示
        if (Block?.IsHighlighted == true)
        {
            var highlightBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0));
            context.FillRectangle(highlightBrush, new Rect(0, 0, Bounds.Width, Bounds.Height), 8);
            context.DrawRectangle(new Pen(Brushes.Yellow, 3), new Rect(0, 0, Bounds.Width, Bounds.Height), 8);
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

        Block.Detach();

        ZIndex = 1000;
        Opacity = 0.85;
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
        _snapToBottom = false;

        foreach (var child in canvas.Children)
        {
            if (child is PuzzleBlock otherBlock && otherBlock.Block != null && otherBlock != this)
            {
                var other = otherBlock.Block;

                // 1. このブロックを他のブロックの下に接続
                if (Block.HasTopConnector && other.HasBottomConnector)
                {
                    var otherBottomY = other.Y + otherBlock.ActualBlockHeight;
                    var dx = Math.Abs(Block.X - other.X);
                    var dy = Math.Abs(Block.Y - otherBottomY);

                    if (dx < SnapThreshold && dy < SnapThreshold)
                    {
                        otherBlock.Block.IsHighlighted = true;
                        _snapCandidate = otherBlock;
                        _snapToBottom = true;
                        return;
                    }
                }

                // 2. このブロックの下に他のブロックを接続
                if (Block.HasBottomConnector && other.HasTopConnector)
                {
                    var myBottomY = Block.Y + ActualBlockHeight;
                    var dx = Math.Abs(Block.X - other.X);
                    var dy = Math.Abs(myBottomY - other.Y);

                    if (dx < SnapThreshold && dy < SnapThreshold)
                    {
                        otherBlock.Block.IsHighlighted = true;
                        _snapCandidate = otherBlock;
                        _snapToBottom = false;
                        return;
                    }
                }

                // 3. 制御構造の内部に接続
                if (other.BlockType == BlockType.ControlStructure && Block.HasTopConnector)
                {
                    double innerStartY = other.Y + BlockHeight;
                    double innerEndY = other.Y + otherBlock.ActualBlockHeight - BlockHeight;
                    double innerX = other.X + InnerIndent;

                    var dx = Math.Abs(Block.X - innerX);
                    var dy = Math.Abs(Block.Y - innerStartY);

                    if (dx < SnapThreshold && dy < SnapThreshold && Block.Y < innerEndY)
                    {
                        otherBlock.Block.IsHighlighted = true;
                        _snapCandidate = otherBlock;
                        _snapToBottom = false;
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

        if (_snapCandidate != null && Block != null && _snapCandidate.Block != null)
        {
            PerformSnap();
        }

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

        // このブロックを他のブロックの下に接続
        if (_snapToBottom && myBlock.HasTopConnector && otherBlock.HasBottomConnector)
        {
            var otherBottomY = otherBlock.Y + _snapCandidate.ActualBlockHeight;
            myBlock.X = otherBlock.X;
            myBlock.Y = otherBottomY;

            otherBlock.NextBlock = myBlock;
            myBlock.PreviousBlock = otherBlock;

            Debug.WriteLine($"Connected below: {otherBlock.DisplayName} -> {myBlock.DisplayName}");
        }
        // このブロックの下に他のブロックを接続
        else if (!_snapToBottom && myBlock.HasBottomConnector && otherBlock.HasTopConnector)
        {
            // 制御構造の内部に接続する場合
            if (myBlock.BlockType == BlockType.ControlStructure)
            {
                otherBlock.X = myBlock.X + InnerIndent;
                otherBlock.Y = myBlock.Y + BlockHeight;

                myBlock.InnerBlocks.Add(otherBlock);
                otherBlock.ParentBlock = myBlock;

                Debug.WriteLine($"Connected inside: {myBlock.DisplayName} <- {otherBlock.DisplayName}");
            }
            else
            {
                var myBottomY = myBlock.Y + ActualBlockHeight;
                otherBlock.X = myBlock.X;
                otherBlock.Y = myBottomY;

                myBlock.NextBlock = otherBlock;
                otherBlock.PreviousBlock = myBlock;

                Debug.WriteLine($"Connected: {myBlock.DisplayName} -> {otherBlock.DisplayName}");
            }
        }
    }
}
