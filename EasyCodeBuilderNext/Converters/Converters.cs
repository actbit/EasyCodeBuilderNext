using Avalonia.Data.Converters;
using Avalonia.Media;
using EasyCodeBuilderNext.Core.Models;
using System;
using System.Globalization;

namespace EasyCodeBuilderNext.Converters;

/// <summary>
/// BlockCategoryを色に変換するコンバーター
/// </summary>
public class CategoryToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BlockCategory category)
        {
            var colorHex = category.GetColor();
            return new SolidColorBrush(Color.Parse(colorHex));
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// ブール値を可視性に変換するコンバーター
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? true : false;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// MemberKindをアイコンに変換するコンバーター
/// </summary>
public class MemberKindToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MemberKind kind)
        {
            return kind.GetIcon();
        }
        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// MemberKindを彩度調整用の不透明度に変換するコンバーター
/// </summary>
public class MemberKindToOpacityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MemberKind kind)
        {
            return kind.ShouldDesaturate() ? 0.7 : 1.0;
        }
        return 1.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
