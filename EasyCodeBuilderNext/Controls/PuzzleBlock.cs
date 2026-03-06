using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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
    private SnapType _snapType;

    private const double SnapThreshold = 50;
    private const double BlockWidth = 180;
    private const double BlockHeight = 40;
    private const double ConnectorSize = 12;
    private const double InnerIndent = 20;

    public double ActualBlockHeight { get; private set; } = BlockHeight;

    private enum SnapType
    {
        None,
        BelowOther,      // 他のブロックの下に接続
        AboveOther,      // 他のブロックの上に接続
        InsideControl    // 制御構造の内側に接続
    }

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

        var mainPanel = new Grid();

        if (Block.BlockType == BlockType.ControlStructure)
        {
            BuildControlStructureBlock(mainPanel, color, darkerColor);
        }
        else
        {
            BuildStatementBlock(mainPanel, color, darkerColor);
        }

        Content = mainPanel;
    }

    private void BuildStatementBlock(Grid mainPanel, Color color, Color darkerColor)
    {
        var path = CreateStatementPath(color, darkerColor);
        mainPanel.Children.Add(path);

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

    private void BuildControlStructureBlock(Grid mainPanel, Color color, Color darkerColor)
    {
        double innerHeight = 60;
        double totalHeight = BlockHeight + innerHeight + BlockHeight;
        Height = totalHeight;
        ActualBlockHeight = totalHeight;

        var path = CreateControlStructurePath(color, darkerColor, innerHeight);
        mainPanel.Children.Add(path);

        var headerText = new TextBlock
        {
            Text = Block!.DisplayName,
            Foreground = Brushes.White,
            FontSize = 13,
            FontWeight = FontWeight.Medium,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Thickness(25, 8, 10, 0)
        };
        mainPanel.Children.Add(headerText);

        var innerLabel = new TextBlock
        {
            Text = "ブロックを追加",
            Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
            FontSize = 11,
            FontStyle = FontStyle.Italic,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, BlockHeight + 10, 0, 0)
        };
        mainPanel.Children.Add(innerLabel);
    }

    private Path CreateStatementPath(Color color, Color darkerColor)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            double w = BlockWidth;
            double h = BlockHeight;
            double c = ConnectorSize; // コネクタの高さ
            double cw = 25; // コネクタの幅
            double r = 6; // 角丸

            // Scratch風パズルピース形状
            // 左上から開始
            context.BeginFigure(new Point(0, r), true);
            context.ArcTo(new Point(r, 0), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 上辺（凹み部分）
            context.LineTo(new Point(15, 0));
            context.LineTo(new Point(18, c * 0.7));
            context.LineTo(new Point(15 + cw, c * 0.7));
            context.LineTo(new Point(18 + cw, 0));
            context.LineTo(new Point(w - r, 0));
            context.ArcTo(new Point(w, r), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 右辺
            context.LineTo(new Point(w, h - r));
            context.ArcTo(new Point(w - r, h), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 下辺（凸部分）
            context.LineTo(new Point(18 + cw, h));
            context.LineTo(new Point(15 + cw, h - c * 0.7));
            context.LineTo(new Point(18, h - c * 0.7));
            context.LineTo(new Point(15, h));
            context.LineTo(new Point(r, h));
            context.ArcTo(new Point(0, h - r), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 左辺
            context.LineTo(new Point(0, r));

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

    private Path CreateControlStructurePath(Color color, Color darkerColor, double innerHeight)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            double w = BlockWidth;
            double headerH = BlockHeight;
            double innerH = innerHeight;
            double totalH = headerH + innerH + headerH;
            double c = ConnectorSize;
            double cw = 25;
            double indent = InnerIndent;
            double r = 6;

            // 左上から開始
            context.BeginFigure(new Point(0, r), true);
            context.ArcTo(new Point(r, 0), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 上辺（凹み）
            context.LineTo(new Point(15, 0));
            context.LineTo(new Point(18, c * 0.7));
            context.LineTo(new Point(15 + cw, c * 0.7));
            context.LineTo(new Point(18 + cw, 0));
            context.LineTo(new Point(w - r, 0));
            context.ArcTo(new Point(w, r), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 右辺（ヘッダー部分）
            context.LineTo(new Point(w, headerH - r));
            context.ArcTo(new Point(w - r, headerH), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // ヘッダーから内側へ（インデント）
            context.LineTo(new Point(indent + r, headerH));
            context.ArcTo(new Point(indent, headerH + r), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 内部左辺
            context.LineTo(new Point(indent, headerH + innerH - r));
            context.ArcTo(new Point(indent + r, headerH + innerH), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 内部から右側へ
            context.LineTo(new Point(w - r, headerH + innerH));
            context.ArcTo(new Point(w, headerH + innerH + r), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 右辺（フッター部分）
            context.LineTo(new Point(w, totalH - r));
            context.ArcTo(new Point(w - r, totalH), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 下辺（凸）
            context.LineTo(new Point(18 + cw, totalH));
            context.LineTo(new Point(15 + cw, totalH - c * 0.7));
            context.LineTo(new Point(18, totalH - c * 0.7));
            context.LineTo(new Point(15, totalH));
            context.LineTo(new Point(r, totalH));
            context.ArcTo(new Point(0, totalH - r), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 左辺
            context.LineTo(new Point(0, r));

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

        var container = GetCanvasContainer();
        if (container != null)
        {
            Canvas.SetLeft(container, Block.X);
            Canvas.SetTop(container, Block.Y);
        }
    }

    private Control? GetCanvasContainer()
    {
        // ItemsControl内では、ContentPresenterがコンテナ
        Visual? current = this;
        while (current != null)
        {
            var parent = current.GetVisualParent();
            if (parent is Canvas)
            {
                return current as Control;
            }
            current = parent;
        }
        return null;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Block?.IsHighlighted == true)
        {
            var highlightBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 0));
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

        Debug.WriteLine($"[Drag Start] {Block.DisplayName} at ({Block.X}, {Block.Y})");
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
        var allBlocks = GetAllPuzzleBlocks(canvas);
        foreach (var block in allBlocks)
        {
            if (block.Block != null && block != this)
            {
                block.Block.IsHighlighted = false;
            }
        }

        _snapCandidate = null;
        _snapType = SnapType.None;

        double bestDistance = SnapThreshold;
        PuzzleBlock? bestCandidate = null;
        SnapType bestSnapType = SnapType.None;

        foreach (var otherBlock in allBlocks)
        {
            if (otherBlock == this || otherBlock.Block == null) continue;

            var other = otherBlock.Block;

            // 1. このブロックを他のブロックの下に接続
            if (Block.HasTopConnector && other.HasBottomConnector)
            {
                var otherBottomY = other.Y + otherBlock.ActualBlockHeight;
                var dx = Block.X - other.X;
                var dy = Block.Y - otherBottomY;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCandidate = otherBlock;
                    bestSnapType = SnapType.BelowOther;
                }
            }

            // 2. このブロックの下に他のブロックを接続
            if (Block.HasBottomConnector && other.HasTopConnector)
            {
                var myBottomY = Block.Y + ActualBlockHeight;
                var dx = other.X - Block.X;
                var dy = other.Y - myBottomY;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCandidate = otherBlock;
                    bestSnapType = SnapType.AboveOther;
                }
            }

            // 3. 制御構造の内部に接続
            if (other.BlockType == BlockType.ControlStructure && Block.HasTopConnector)
            {
                double innerStartY = other.Y + BlockHeight;
                double innerX = other.X + InnerIndent;

                var dx = Block.X - innerX;
                var dy = Block.Y - innerStartY;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCandidate = otherBlock;
                    bestSnapType = SnapType.InsideControl;
                }
            }
        }

        if (bestCandidate != null && bestCandidate.Block != null)
        {
            _snapCandidate = bestCandidate;
            _snapType = bestSnapType;
            bestCandidate.Block.IsHighlighted = true;
            Debug.WriteLine($"[Snap Candidate] {bestCandidate.Block.DisplayName} ({bestSnapType})");
        }
    }

    private List<PuzzleBlock> GetAllPuzzleBlocks(Canvas canvas)
    {
        var result = new List<PuzzleBlock>();

        foreach (var child in canvas.Children)
        {
            // ContentPresenterの中にPuzzleBlockがある
            if (child is ContentPresenter presenter)
            {
                var puzzleBlock = presenter.Child as PuzzleBlock;
                if (puzzleBlock != null)
                {
                    result.Add(puzzleBlock);
                }
            }
        }

        return result;
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
            foreach (var block in GetAllPuzzleBlocks(canvas))
            {
                if (block.Block != null)
                {
                    block.Block.IsHighlighted = false;
                }
            }
        }

        _snapCandidate = null;
        _snapType = SnapType.None;
        e.Handled = true;
    }

    private void PerformSnap()
    {
        if (Block == null || _snapCandidate?.Block == null) return;

        var myBlock = Block;
        var otherBlock = _snapCandidate.Block;

        switch (_snapType)
        {
            case SnapType.BelowOther:
                // このブロックを他のブロックの下に接続
                myBlock.X = otherBlock.X;
                myBlock.Y = otherBlock.Y + _snapCandidate.ActualBlockHeight;

                // 既存の接続を解除して再接続
                if (otherBlock.NextBlock != null)
                {
                    var oldNext = otherBlock.NextBlock;
                    otherBlock.NextBlock = null;
                    oldNext.PreviousBlock = null;
                }

                otherBlock.NextBlock = myBlock;
                myBlock.PreviousBlock = otherBlock;

                Debug.WriteLine($"[Connected] {otherBlock.DisplayName} -> {myBlock.DisplayName}");
                break;

            case SnapType.AboveOther:
                // このブロックの下に他のブロックを接続
                otherBlock.X = myBlock.X;
                otherBlock.Y = myBlock.Y + ActualBlockHeight;

                // 既存の接続を解除して再接続
                if (myBlock.NextBlock != null)
                {
                    var oldNext = myBlock.NextBlock;
                    myBlock.NextBlock = null;
                    oldNext.PreviousBlock = null;
                }

                myBlock.NextBlock = otherBlock;
                otherBlock.PreviousBlock = myBlock;

                Debug.WriteLine($"[Connected] {myBlock.DisplayName} -> {otherBlock.DisplayName}");
                break;

            case SnapType.InsideControl:
                // 制御構造の内部に接続
                otherBlock.InnerBlocks.Add(myBlock);
                myBlock.ParentBlock = otherBlock;
                myBlock.X = otherBlock.X + InnerIndent;
                myBlock.Y = otherBlock.Y + BlockHeight;

                Debug.WriteLine($"[Connected Inside] {otherBlock.DisplayName} <- {myBlock.DisplayName}");
                break;
        }
    }
}
