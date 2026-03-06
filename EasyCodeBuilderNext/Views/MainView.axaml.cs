using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EasyCodeBuilderNext.Core.PluginSystem;
using EasyCodeBuilderNext.ViewModels;

namespace EasyCodeBuilderNext.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnBlockTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.Tag is BlockTemplate template)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.AddBlockCommand.Execute(template);
            }
        }
    }
}