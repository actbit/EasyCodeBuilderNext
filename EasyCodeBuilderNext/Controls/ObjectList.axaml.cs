using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EasyCodeBuilderNext.Core.Models;
using System;
using System.Collections.ObjectModel;

namespace EasyCodeBuilderNext.Controls;

/// <summary>
/// オブジェクト（クラス）リストコントロール
/// </summary>
public partial class ObjectList : UserControl
{
    public static readonly StyledProperty<ObservableCollection<CodeObject>?> ObjectsProperty =
        AvaloniaProperty.Register<ObjectList, ObservableCollection<CodeObject>?>(nameof(Objects));

    public static readonly StyledProperty<CodeObject?> SelectedObjectProperty =
        AvaloniaProperty.Register<ObjectList, CodeObject?>(nameof(SelectedObject));

    public ObservableCollection<CodeObject>? Objects
    {
        get => GetValue(ObjectsProperty);
        set => SetValue(ObjectsProperty, value);
    }

    public CodeObject? SelectedObject
    {
        get => GetValue(SelectedObjectProperty);
        set => SetValue(SelectedObjectProperty, value);
    }

    public event EventHandler<CodeObject?>? ObjectSelected;
    public event EventHandler? AddObjectRequested;
    public event EventHandler<CodeObject?>? RemoveObjectRequested;

    public ObjectList()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnObjectClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is ListBoxItem item && item.DataContext is CodeObject obj)
        {
            SelectedObject = obj;
            ObjectSelected?.Invoke(this, obj);
        }
    }

    private void OnAddClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AddObjectRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnRemoveClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SelectedObject != null)
        {
            RemoveObjectRequested?.Invoke(this, SelectedObject);
        }
    }
}
