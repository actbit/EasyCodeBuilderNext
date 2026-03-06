using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.Models;
using System;

namespace EasyCodeBuilderNext.Controls;

/// <summary>
/// Scratch風パズルピース型ブロックコントロール
/// </summary>
public class PuzzleBlock : TemplatedControl
{
    #region 依存関係プロパティ

    public static readonly StyledProperty<BlockBase?> BlockProperty =
        AvaloniaProperty.Register<PuzzleBlock, BlockBase?>(nameof(Block));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<PuzzleBlock, bool>(nameof(IsSelected));

    public static readonly StyledProperty<bool> IsDraggingProperty =
        AvaloniaProperty.Register<PuzzleBlock, bool>(nameof(IsDragging));

    public static readonly StyledProperty<bool> IsSnapTargetProperty =
        AvaloniaProperty.Register<PuzzleBlock, bool>(nameof(IsSnapTarget));

    public static readonly StyledProperty<double> ConnectorSizeProperty =
        AvaloniaProperty.Register<PuzzleBlock, double>(nameof(ConnectorSize), 15.0);

    #endregion

    #region プロパティ

    public BlockBase? Block
    {
        get => GetValue(BlockProperty);
        set => SetValue(BlockProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public bool IsDragging
    {
        get => GetValue(IsDraggingProperty);
        set => SetValue(IsDraggingProperty, value);
    }

    public bool IsSnapTarget
    {
        get => GetValue(IsSnapTargetProperty);
        set => SetValue(IsSnapTargetProperty, value);
    }

    public double ConnectorSize
    {
        get => GetValue(ConnectorSizeProperty);
        set => SetValue(ConnectorSizeProperty, value);
    }

    #endregion

    static PuzzleBlock()
    {
        FocusableProperty.OverrideDefaultValue(typeof(PuzzleBlock), true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateVisualState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BlockProperty ||
            change.Property == IsSelectedProperty ||
            change.Property == IsDraggingProperty ||
            change.Property == IsSnapTargetProperty)
        {
            UpdateVisualState();
        }
    }

    private void UpdateVisualState()
    {
        if (IsDragging)
        {
            PseudoClasses.Add(":dragging");
        }
        else
        {
            PseudoClasses.Remove(":dragging");
        }

        if (IsSelected)
        {
            PseudoClasses.Add(":selected");
        }
        else
        {
            PseudoClasses.Remove(":selected");
        }

        if (IsSnapTarget)
        {
            PseudoClasses.Add(":snaptarget");
        }
        else
        {
            PseudoClasses.Remove(":snaptarget");
        }
    }

    /// <summary>
    /// ブロックの背景色を取得
    /// </summary>
    public IBrush GetBlockColor()
    {
        if (Block == null)
            return Brushes.Gray;

        var colorHex = Block.Category.GetColor();
        var color = Color.Parse(colorHex);

        // 選択時は明るく
        if (IsSelected)
        {
            color = new Color(
                color.A,
                (byte)Math.Min(255, color.R + 30),
                (byte)Math.Min(255, color.G + 30),
                (byte)Math.Min(255, color.B + 30));
        }

        return new SolidColorBrush(color);
    }

    /// <summary>
    /// ブロックのパズルピース形状を生成
    /// </summary>
    public PathGeometry CreatePuzzleShape(double width, double height)
    {
        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(0, ConnectorSize) };
        var segments = geometry.Figures = new PathFigures { figure };

        double cs = ConnectorSize; // コネクタサイズ
        bool hasTop = Block?.HasTopConnector ?? false;
        bool hasBottom = Block?.HasBottomConnector ?? false;

        // 左上
        figure.Segments!.Add(new LineSegment { Point = new Point(0, 0) });

        // 上部コネクタ（凹み）
        if (hasTop)
        {
            double cx = 15;
            figure.Segments.Add(new LineSegment { Point = new Point(cx - cs / 2, 0) });
            figure.Segments.Add(new LineSegment { Point = new Point(cx - cs / 2, -cs / 2) });
            figure.Segments.Add(new LineSegment { Point = new Point(cx + cs / 2, -cs / 2) });
            figure.Segments.Add(new LineSegment { Point = new Point(cx + cs / 2, 0) });
        }

        // 右上
        figure.Segments.Add(new LineSegment { Point = new Point(width, 0) });

        // 右下
        figure.Segments.Add(new LineSegment { Point = new Point(width, height) });

        // 下部コネクタ（凸）
        if (hasBottom)
        {
            double cx = 15;
            figure.Segments.Add(new LineSegment { Point = new Point(cx + cs / 2, height) });
            figure.Segments.Add(new LineSegment { Point = new Point(cx + cs / 2, height + cs / 2) });
            figure.Segments.Add(new LineSegment { Point = new Point(cx - cs / 2, height + cs / 2) });
            figure.Segments.Add(new LineSegment { Point = new Point(cx - cs / 2, height) });
        }

        // 左下
        figure.Segments.Add(new LineSegment { Point = new Point(0, height) });

        figure.IsClosed = true;

        return geometry;
    }
}
