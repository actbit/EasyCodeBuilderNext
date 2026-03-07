using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.Models;
using EasyCodeBuilderNext.Views;
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

        // ブロックタイプに応じて適切なビルドメソッドを選択
        if (Block.BlockType == BlockType.ControlStructure)
        {
            BuildControlStructureBlock(mainPanel, color, darkerColor);
        }
        else if (Block.BlockType == BlockType.Hat || Block.BlockType == BlockType.Definition)
        {
            BuildHatBlock(mainPanel, color, darkerColor);
        }
        else
        {
            BuildStatementBlock(mainPanel, color, darkerColor);
        }

        Content = mainPanel;
    }

    private void BuildHatBlock(Grid mainPanel, Color color, Color darkerColor)
    {
        // Hat型（開始ブロック）- 上部コネクタなし、下部コネクタあり（凹み）
        var path = CreateHatPath(color, darkerColor);
        mainPanel.Children.Add(path);

        var contentPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(10, 5, 10, 5),
            Spacing = 5
        };

        AddParameterizedContent(contentPanel);
        mainPanel.Children.Add(contentPanel);

        Height = BlockHeight + ConnectorSize * 0.7; // 下部の凹み分を追加
        ActualBlockHeight = BlockHeight;
    }

    private Path CreateHatPath(Color color, Color darkerColor)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            double w = BlockWidth;
            double h = BlockHeight;
            double c = ConnectorSize;
            double cw = 25;
            double r = 6;

            // Hat型：上部は丸い（コネクタなし）、下部は凹み
            context.BeginFigure(new Point(0, r), true);
            context.ArcTo(new Point(r, 0), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 上辺（丸い帽子型）
            context.LineTo(new Point(w / 2 - 20, 0));
            context.ArcTo(new Point(w / 2, -8), new Size(20, 10), 0, false, SweepDirection.Clockwise);
            context.ArcTo(new Point(w / 2 + 20, 0), new Size(20, 10), 0, false, SweepDirection.Clockwise);
            context.LineTo(new Point(w - r, 0));
            context.ArcTo(new Point(w, r), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 右辺
            context.LineTo(new Point(w, h - r));
            context.ArcTo(new Point(w - r, h), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 下辺（凹み）- Statementブロックと同じ深さ
            context.LineTo(new Point(18 + cw, h));
            context.LineTo(new Point(15 + cw, h + c * 0.7));
            context.LineTo(new Point(18, h + c * 0.7));
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

    private void BuildStatementBlock(Grid mainPanel, Color color, Color darkerColor)
    {
        var path = CreateStatementPath(color, darkerColor);
        mainPanel.Children.Add(path);

        double offset = ConnectorSize * 0.7;

        // パラメータ付きのコンテンツを作成
        var contentPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(10, 5 + offset, 10, 5),
            Spacing = 5
        };

        // 表示名を追加（パラメータがある場合は分割して表示）
        AddParameterizedContent(contentPanel);

        mainPanel.Children.Add(contentPanel);

        Height = BlockHeight + ConnectorSize * 0.7 + ConnectorSize * 0.7; // 上部凸 + 下部凹
        ActualBlockHeight = BlockHeight;
    }

    private void AddParameterizedContent(StackPanel contentPanel)
    {
        if (Block == null) return;

        // パラメータがある場合は、「もし {条件} なら」のような形式で表示
        if (Block.Parameters.Count > 0)
        {
            // 表示名をパラメータ位置で分割
            var displayName = Block.DisplayName;
            var parts = SplitDisplayName(displayName, Block.Parameters.Count);

            for (int i = 0; i < parts.Count; i++)
            {
                // テキスト部分
                if (!string.IsNullOrEmpty(parts[i].Text))
                {
                    contentPanel.Children.Add(new TextBlock
                    {
                        Text = parts[i].Text,
                        Foreground = Brushes.White,
                        FontSize = 13,
                        FontWeight = FontWeight.Medium,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    });
                }

                // パラメータ入力部分
                if (i < Block.Parameters.Count && parts[i].HasParameter)
                {
                    var param = Block.Parameters[i];
                    var input = CreateParameterInput(param);
                    contentPanel.Children.Add(input);
                }
            }
        }
        else
        {
            // パラメータがない場合は単純に表示名
            contentPanel.Children.Add(new TextBlock
            {
                Text = Block.DisplayName,
                Foreground = Brushes.White,
                FontSize = 13,
                FontWeight = FontWeight.Medium,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
        }
    }

    private List<(string Text, bool HasParameter)> SplitDisplayName(string displayName, int paramCount)
    {
        var result = new List<(string, bool)>();

        // 「～」をパラメータ位置として扱う
        var tildeParts = displayName.Split(new[] { '～' }, StringSplitOptions.None);

        if (tildeParts.Length > 1)
        {
            for (int i = 0; i < tildeParts.Length; i++)
            {
                result.Add((tildeParts[i], i < paramCount));
            }
        }
        else
        {
            // 「～」がない場合は表示名の最後にパラメータを追加
            result.Add((displayName, paramCount > 0));
            for (int i = 1; i < paramCount; i++)
            {
                result.Add(("", true));
            }
        }

        return result;
    }

    private Control CreateParameterInput(BlockParameter param)
    {
        switch (param.InputType)
        {
            case ParameterInputType.Text:
            case ParameterInputType.Variable:
                var textBox = new TextBox
                {
                    Text = param.Value?.ToString() ?? param.DefaultValue?.ToString() ?? "",
                    MinWidth = 60,
                    MaxWidth = 100,
                    Padding = new Thickness(6, 2),
                    FontSize = 12,
                    Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                    BorderThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(4)
                };
                textBox.TextChanged += (s, e) => param.Value = textBox.Text;
                return textBox;

            case ParameterInputType.Number:
                var numBox = new TextBox
                {
                    Text = param.Value?.ToString() ?? param.DefaultValue?.ToString() ?? "0",
                    MinWidth = 50,
                    MaxWidth = 80,
                    Padding = new Thickness(6, 2),
                    FontSize = 12,
                    Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                    BorderThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(4)
                };
                numBox.TextChanged += (s, e) =>
                {
                    if (double.TryParse(numBox.Text, out var num))
                        param.Value = num;
                };
                return numBox;

            case ParameterInputType.Dropdown:
                var comboBox = new ComboBox
                {
                    ItemsSource = param.Options,
                    SelectedItem = param.Value ?? param.DefaultValue ?? param.Options.FirstOrDefault(),
                    MinWidth = 80,
                    MaxWidth = 120,
                    Padding = new Thickness(4, 2),
                    FontSize = 11
                };
                comboBox.SelectionChanged += (s, e) => param.Value = comboBox.SelectedItem;
                return comboBox;

            case ParameterInputType.Checkbox:
                var checkBox = new CheckBox
                {
                    IsChecked = param.Value as bool? ?? param.DefaultValue as bool? ?? false
                };
                checkBox.IsCheckedChanged += (s, e) => param.Value = checkBox.IsChecked;
                return checkBox;

            default:
                var defaultBox = new TextBox
                {
                    Text = param.Value?.ToString() ?? "",
                    MinWidth = 60,
                    Padding = new Thickness(6, 2),
                    FontSize = 12,
                    Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                    BorderThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(4)
                };
                defaultBox.TextChanged += (s, e) => param.Value = defaultBox.Text;
                return defaultBox;
        }
    }

    private void BuildControlStructureBlock(Grid mainPanel, Color color, Color darkerColor)
    {
        double innerHeight = 60;
        double totalHeight = BlockHeight + innerHeight + BlockHeight;
        double offset = ConnectorSize * 0.7;
        Height = totalHeight + offset + offset; // 上部凸 + 下部凹分
        ActualBlockHeight = totalHeight;

        var path = CreateControlStructurePath(color, darkerColor, innerHeight);
        mainPanel.Children.Add(path);

        // ヘッダー部分（パラメータ付き）
        var headerPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Thickness(25, 8 + offset, 10, 0)
        };

        AddParameterizedContent(headerPanel);
        mainPanel.Children.Add(headerPanel);

        // 内部ラベル
        var innerLabel = new TextBlock
        {
            Text = "ブロックを追加",
            Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
            FontSize = 11,
            FontStyle = FontStyle.Italic,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, BlockHeight + 10 + offset, 0, 0)
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
            double offset = c * 0.7; // 上部オフセット（凸の高さ分）

            // Scratch風パズルピース形状（上が凸、下が凹）
            // 左上から開始（オフセットを考慮）
            context.BeginFigure(new Point(0, r + offset), true);
            context.ArcTo(new Point(r, offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 上辺（凸部分 - 突起）
            context.LineTo(new Point(15, offset));
            context.LineTo(new Point(18, 0)); // 凸の先端（y=0）
            context.LineTo(new Point(15 + cw, 0));
            context.LineTo(new Point(18 + cw, offset));
            context.LineTo(new Point(w - r, offset));
            context.ArcTo(new Point(w, r + offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 右辺
            context.LineTo(new Point(w, h - r + offset));
            context.ArcTo(new Point(w - r, h + offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 下辺（凹み部分 - 受け）
            context.LineTo(new Point(18 + cw, h + offset));
            context.LineTo(new Point(15 + cw, h + c * 1.4 + offset)); // 凹の最深部
            context.LineTo(new Point(18, h + c * 1.4 + offset));
            context.LineTo(new Point(15, h + offset));
            context.LineTo(new Point(r, h + offset));
            context.ArcTo(new Point(0, h - r + offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 左辺
            context.LineTo(new Point(0, r + offset));

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
            double offset = c * 0.7;

            // 左上から開始（オフセット考慮）
            context.BeginFigure(new Point(0, r + offset), true);
            context.ArcTo(new Point(r, offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 上辺（凸 - 突起）
            context.LineTo(new Point(15, offset));
            context.LineTo(new Point(18, 0)); // 凸の先端（y=0）
            context.LineTo(new Point(15 + cw, 0));
            context.LineTo(new Point(18 + cw, offset));
            context.LineTo(new Point(w - r, offset));
            context.ArcTo(new Point(w, r + offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 右辺（ヘッダー部分）
            context.LineTo(new Point(w, headerH - r + offset));
            context.ArcTo(new Point(w - r, headerH + offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // ヘッダーから内側へ（インデント）
            context.LineTo(new Point(indent + r, headerH + offset));
            context.ArcTo(new Point(indent, headerH + r + offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 内部左辺
            context.LineTo(new Point(indent, headerH + innerH - r + offset));
            context.ArcTo(new Point(indent + r, headerH + innerH + offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 内部から右側へ
            context.LineTo(new Point(w - r, headerH + innerH + offset));
            context.ArcTo(new Point(w, headerH + innerH + r + offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 右辺（フッター部分）
            context.LineTo(new Point(w, totalH - r + offset));
            context.ArcTo(new Point(w - r, totalH + offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 下辺（凹み - 受け）
            context.LineTo(new Point(18 + cw, totalH + offset));
            context.LineTo(new Point(15 + cw, totalH + c * 0.7 + offset));
            context.LineTo(new Point(18, totalH + c * 0.7 + offset));
            context.LineTo(new Point(15, totalH + offset));
            context.LineTo(new Point(r, totalH + offset));
            context.ArcTo(new Point(0, totalH - r + offset), new Size(r, r), 0, false, SweepDirection.Clockwise);

            // 左辺
            context.LineTo(new Point(0, r + offset));

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

        // キャンバスのリサイズを要求
        RequestCanvasResize();

        _snapCandidate = null;
        _snapType = SnapType.None;
        e.Handled = true;
    }

    private void RequestCanvasResize()
    {
        var mainView = this.FindAncestorOfType<UserControl>();
        while (mainView == null && this.GetVisualParent() != null)
        {
            var parent = this.GetVisualParent();
            mainView = parent as UserControl;
            if (mainView == null)
            {
                // 親をたどってMainViewを探す
                var view = parent?.GetType();
                if (parent != null)
                {
                    try
                    {
                        var eventField = typeof(MainView).GetField("CanvasResizeNeededEvent",
                            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        if (eventField != null)
                        {
                            var routedEvent = eventField.GetValue(null) as RoutedEvent;
                            if (routedEvent != null)
                            {
                                RaiseEvent(new RoutedEventArgs(routedEvent));
                                return;
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        // 直接イベントを発生させる
        var canvasResizeEvent = typeof(MainView).GetField("CanvasResizeNeededEvent",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?.GetValue(null) as RoutedEvent;

        if (canvasResizeEvent != null)
        {
            RaiseEvent(new RoutedEventArgs(canvasResizeEvent));
        }
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
