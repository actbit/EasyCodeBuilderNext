using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using EasyCodeBuilderNext.Core.Models;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Controls;

/// <summary>
/// クラスダイアグラムビュー - 継承関係をUMLライクに表示
/// </summary>
public partial class ClassDiagramView : UserControl
{
    public static readonly StyledProperty<ObservableCollection<CodeObject>?> ObjectsProperty =
        AvaloniaProperty.Register<ClassDiagramView, ObservableCollection<CodeObject>?>(nameof(Objects));

    public static readonly StyledProperty<Project?> ProjectProperty =
        AvaloniaProperty.Register<ClassDiagramView, Project?>(nameof(Project));

    public ObservableCollection<CodeObject>? Objects
    {
        get => GetValue(ObjectsProperty);
        set => SetValue(ObjectsProperty, value);
    }

    public Project? Project
    {
        get => GetValue(ProjectProperty);
        set => SetValue(ProjectProperty, value);
    }

    private Canvas? _diagramCanvas;
    private const double NodeWidth = 150;
    private const double NodeHeight = 100;
    private const double HorizontalSpacing = 200;
    private const double VerticalSpacing = 150;

    public ClassDiagramView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _diagramCanvas = this.FindControl<Canvas>("DiagramCanvas");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ObjectsProperty || change.Property == ProjectProperty)
        {
            InvalidateDiagram();
        }
    }

    public void InvalidateDiagram()
    {
        if (_diagramCanvas == null || Objects == null || Objects.Count == 0)
            return;

        _diagramCanvas.Children.Clear();

        // レイアウト計算
        var positions = CalculateLayout();

        // クラスボックスを描画
        foreach (var obj in Objects)
        {
            if (positions.TryGetValue(obj, out var pos))
            {
                DrawClassBox(obj, pos.X, pos.Y);
            }
        }

        // 継承線を描画
        DrawInheritanceLines(positions);
    }

    private Dictionary<CodeObject, Point> CalculateLayout()
    {
        var positions = new Dictionary<CodeObject, Point>();

        if (Objects == null || Project == null)
            return positions;

        // ルートクラス（基底クラスを持たないクラス）を特定
        var roots = Objects.Where(o => string.IsNullOrEmpty(o.BaseClassName) ||
                                       !Objects.Any(p => p.Name == o.BaseClassName)).ToList();

        // 各ルートからツリーを構築して配置
        double currentX = 50;

        foreach (var root in roots)
        {
            LayoutSubtree(root, positions, currentX, 50, 0);
            currentX += GetSubtreeWidth(root) * HorizontalSpacing + HorizontalSpacing;
        }

        // 孤立したクラスを配置
        foreach (var obj in Objects.Where(o => !positions.ContainsKey(o)))
        {
            positions[obj] = new Point(currentX, 50);
            currentX += HorizontalSpacing;
        }

        return positions;
    }

    private void LayoutSubtree(CodeObject obj, Dictionary<CodeObject, Point> positions, double x, double y, int level)
    {
        if (Objects == null || Project == null)
            return;

        positions[obj] = new Point(x, y);

        // 子クラスを取得
        var children = Objects.Where(o => o.BaseClassName == obj.Name).ToList();

        if (children.Count == 0)
            return;

        // 子クラスを配置
        double childX = x - ((children.Count - 1) * HorizontalSpacing) / 2;

        foreach (var child in children)
        {
            LayoutSubtree(child, positions, childX, y + VerticalSpacing, level + 1);
            childX += HorizontalSpacing;
        }
    }

    private double GetSubtreeWidth(CodeObject obj)
    {
        if (Objects == null || Project == null)
            return 1;

        var children = Objects.Where(o => o.BaseClassName == obj.Name).ToList();

        if (children.Count == 0)
            return 1;

        return children.Sum(GetSubtreeWidth);
    }

    private void DrawClassBox(CodeObject obj, double x, double y)
    {
        if (_diagramCanvas == null)
            return;

        var border = new Border
        {
            Width = NodeWidth,
            Height = NodeHeight,
            Background = Brushes.White,
            BorderBrush = obj.IsAbstract ? Brushes.DodgerBlue : Brushes.Gray,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(4),
            Classes = { "class-box" }
        };

        // タイトルエリア
        var titlePanel = new Border
        {
            Background = obj.IsAbstract ? Brushes.DodgerBlue : Brushes.DimGray,
            Padding = new Thickness(8, 4)
        };

        var titleText = new TextBlock
        {
            Text = obj.Name,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        if (obj.IsAbstract)
        {
            titleText.FontStyle = FontStyle.Italic;
        }

        titlePanel.Child = titleText;

        // メンバー表示
        var membersPanel = new StackPanel
        {
            Margin = new Thickness(4)
        };

        // フィールドとプロパティ
        foreach (var member in obj.Members.Where(m => m.Kind is MemberKind.Field or
                                                       MemberKind.InstanceField or
                                                       MemberKind.StaticField or
                                                       MemberKind.Property))
        {
            var icon = member.Kind.GetIcon();
            membersPanel.Children.Add(new TextBlock
            {
                Text = $"{icon} {member.Name}: {member.ReturnType}",
                FontSize = 10,
                Foreground = Brushes.DarkGray,
                TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis
            });
        }

        // メソッド
        foreach (var member in obj.Members.Where(m => m.Kind is MemberKind.InstanceMethod or MemberKind.StaticMethod))
        {
            var icon = member.Kind.GetIcon();
            var params_str = string.Join(", ", member.Parameters.Select(p => $"{p.TypeName} {p.Name}"));
            membersPanel.Children.Add(new TextBlock
            {
                Text = $"{icon} {member.Name}({params_str})",
                FontSize = 10,
                Foreground = Brushes.DarkGray,
                TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis
            });
        }

        var content = new Grid RowDefinitions="Auto,*">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        Grid.SetRow(titlePanel, 0);
        Grid.SetRow(membersPanel, 1);

        content.Children.Add(titlePanel);
        content.Children.Add(membersPanel);

        border.Child = content;

        Canvas.SetLeft(border, x - NodeWidth / 2);
        Canvas.SetTop(border, y);
        Panel.SetZIndex(border, 10);

        _diagramCanvas.Children.Add(border);
    }

    private void DrawInheritanceLines(Dictionary<CodeObject, Point> positions)
    {
        if (_diagramCanvas == null || Objects == null || Project == null)
            return;

        foreach (var (parent, child) in Project.GetInheritanceRelations())
        {
            if (!positions.TryGetValue(parent, out var parentPos) ||
                !positions.TryGetValue(child, out var childPos))
                continue;

            // 親の下部から子の上部へ線を引く
            var line = new Avalonia.Controls.Shapes.Line
            {
                StartPoint = new Point(parentPos.X, parentPos.Y + NodeHeight / 2),
                EndPoint = new Point(childPos.X, childPos.Y - NodeHeight / 2),
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 2,
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 2 }
            };

            Panel.SetZIndex(line, 1);
            _diagramCanvas.Children.Add(line);

            // 矢印（継承マーク）を描画
            DrawInheritanceArrow(childPos.X, childPos.Y - NodeHeight / 2);
        }
    }

    private void DrawInheritanceArrow(double x, double y)
    {
        if (_diagramCanvas == null)
            return;

        // 白抜き三角（UML継承マーク）
        var triangle = new Avalonia.Controls.Shapes.Polygon
        {
            Points = new Avalonia.Collections.AvaloniaList<Point>
            {
                new Point(x, y),
                new Point(x - 8, y - 12),
                new Point(x + 8, y - 12)
            },
            Fill = Brushes.White,
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = 2
        };

        Panel.SetZIndex(triangle, 5);
        _diagramCanvas.Children.Add(triangle);
    }
}
