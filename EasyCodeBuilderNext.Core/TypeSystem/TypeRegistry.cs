using System.Collections.ObjectModel;
using System.Reflection;

namespace EasyCodeBuilderNext.Core.TypeSystem;

/// <summary>
/// 登録された型情報
/// </summary>
public class RegisteredType
{
    /// <summary>
    /// 型の完全名
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 型の表示名
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 型のカテゴリ（プリミティブ、コレクション、カスタムなど）
    /// </summary>
    public string Category { get; set; } = "Custom";

    /// <summary>
    /// アセンブリ名
    /// </summary>
    public string? AssemblyName { get; set; }

    /// <summary>
    /// System.Type情報
    /// </summary>
    public Type? Type { get; set; }

    /// <summary>
    /// メンバー情報
    /// </summary>
    public ObservableCollection<TypeMemberInfo> Members { get; } = new();

    /// <summary>
    /// この型が生成したカスタムブロック
    /// </summary>
    public ObservableCollection<string> CustomBlockIds { get; } = new();
}

/// <summary>
/// 型のメンバー情報
/// </summary>
public class TypeMemberInfo
{
    /// <summary>
    /// メンバー名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// メンバーの種類
    /// </summary>
    public TypeMemberKind Kind { get; set; }

    /// <summary>
    /// 戻り値の型
    /// </summary>
    public string? ReturnType { get; set; }

    /// <summary>
    /// パラメータ（メソッド用）
    /// </summary>
    public ObservableCollection<ParameterInfo> Parameters { get; } = new();

    /// <summary>
    /// 静的メンバーかどうか
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// 読み取り専用かどうか（プロパティ用）
    /// </summary>
    public bool IsReadOnly { get; set; }
}

/// <summary>
/// メンバーの種類
/// </summary>
public enum TypeMemberKind
{
    Method,
    Property,
    Field,
    Event
}

/// <summary>
/// パラメータ情報
/// </summary>
public class ParameterInfo
{
    /// <summary>
    /// パラメータ名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 型名
    /// </summary>
    public string TypeName { get; set; } = "object";

    /// <summary>
    /// デフォルト値
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 省略可能かどうか
    /// </summary>
    public bool IsOptional { get; set; }
}

/// <summary>
/// 型レジストリ
/// 登録された型を管理し、ブロック生成に必要な情報を提供
/// </summary>
public class TypeRegistry
{
    /// <summary>
    /// 登録された型のコレクション
    /// </summary>
    public ObservableCollection<RegisteredType> RegisteredTypes { get; } = new();

    /// <summary>
    /// 型名での高速アクセス用辞書
    /// </summary>
    private readonly Dictionary<string, RegisteredType> _typeCache = new();

    public TypeRegistry()
    {
        // 基本的な型を登録
        RegisterPrimitiveTypes();
        RegisterCommonTypes();
    }

    /// <summary>
    /// プリミティブ型を登録
    /// </summary>
    private void RegisterPrimitiveTypes()
    {
        var primitives = new[]
        {
            ("int", "int", "整数"),
            ("long", "long", "長整数"),
            ("short", "short", "短整数"),
            ("byte", "byte", "バイト"),
            ("float", "float", "単精度浮動小数点"),
            ("double", "double", "倍精度浮動小数点"),
            ("decimal", "decimal", "10進数"),
            ("bool", "bool", "真偽値"),
            ("char", "char", "文字"),
            ("string", "string", "文字列"),
            ("object", "object", "オブジェクト"),
            ("void", "void", "なし")
        };

        foreach (var (fullName, displayName, category) in primitives)
        {
            RegisterType(new RegisteredType
            {
                FullName = fullName,
                DisplayName = displayName,
                Category = category,
                AssemblyName = "mscorlib"
            });
        }
    }

    /// <summary>
    /// よく使用される型を登録
    /// </summary>
    private void RegisterCommonTypes()
    {
        // Console型を登録
        RegisterType(typeof(Console));

        // List型を登録
        RegisterType(typeof(List<>));

        // Dictionary型を登録
        RegisterType(typeof(Dictionary<,>));

        // Array型を登録
        RegisterType(typeof(Array));

        // Math型を登録
        RegisterType(typeof(Math));

        // DateTime型を登録
        RegisterType(typeof(DateTime));

        // TimeSpan型を登録
        RegisterType(typeof(TimeSpan));

        // Guid型を登録
        RegisterType(typeof(Guid));
    }

    /// <summary>
    /// 型を登録
    /// </summary>
    public void RegisterType(RegisteredType type)
    {
        if (string.IsNullOrEmpty(type.FullName))
            return;

        if (_typeCache.ContainsKey(type.FullName))
        {
            // 既存の型を更新
            var existing = _typeCache[type.FullName];
            existing.DisplayName = type.DisplayName;
            existing.Category = type.Category;
            existing.AssemblyName = type.AssemblyName;
            existing.Type = type.Type;

            foreach (var member in type.Members)
            {
                if (!existing.Members.Any(m => m.Name == member.Name && m.Kind == member.Kind))
                {
                    existing.Members.Add(member);
                }
            }
        }
        else
        {
            // 新しい型を追加
            _typeCache[type.FullName] = type;
            RegisteredTypes.Add(type);
        }
    }

    /// <summary>
    /// System.Typeから型を登録
    /// </summary>
    public void RegisterType(Type type)
    {
        if (type == null)
            return;

        var registeredType = new RegisteredType
        {
            FullName = type.FullName ?? type.Name,
            DisplayName = type.Name,
            Category = GetTypeCategory(type),
            AssemblyName = type.Assembly?.GetName().Name,
            Type = type
        };

        // パブリックメソッドを追加
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName)) // get_, set_ などを除外
        {
            registeredType.Members.Add(new TypeMemberInfo
            {
                Name = method.Name,
                Kind = TypeMemberKind.Method,
                ReturnType = method.ReturnType.Name,
                IsStatic = method.IsStatic
            });
        }

        // パブリックプロパティを追加
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
        {
            registeredType.Members.Add(new TypeMemberInfo
            {
                Name = property.Name,
                Kind = TypeMemberKind.Property,
                ReturnType = property.PropertyType.Name,
                IsStatic = property.GetMethod?.IsStatic ?? false,
                IsReadOnly = !property.CanWrite
            });
        }

        // パブリックフィールドを追加
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
        {
            registeredType.Members.Add(new TypeMemberInfo
            {
                Name = field.Name,
                Kind = TypeMemberKind.Field,
                ReturnType = field.FieldType.Name,
                IsStatic = field.IsStatic
            });
        }

        RegisterType(registeredType);
    }

    /// <summary>
    /// 型のカテゴリを取得
    /// </summary>
    private static string GetTypeCategory(Type type)
    {
        if (type.IsPrimitive)
            return "プリミティブ";
        if (type.IsEnum)
            return "列挙型";
        if (type.IsInterface)
            return "インターフェース";
        if (type.IsAbstract)
            return "抽象クラス";
        if (type.IsGenericType)
            return "ジェネリック";
        if (type.IsValueType)
            return "値型";
        if (type == typeof(string))
            return "文字列";

        return "クラス";
    }

    /// <summary>
    /// アセンブリから全てのパブリック型を登録
    /// </summary>
    public void RegisterAssembly(Assembly assembly)
    {
        try
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                RegisterType(type);
            }
        }
        catch (Exception ex)
        {
            // 一部の型が読み込めなくても続行
            System.Diagnostics.Debug.WriteLine($"アセンブリ読み込みエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// DLLファイルから型を登録
    /// </summary>
    public void RegisterAssemblyFromPath(string dllPath)
    {
        if (!File.Exists(dllPath))
            return;

        try
        {
            var assembly = Assembly.LoadFrom(dllPath);
            RegisterAssembly(assembly);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DLL読み込みエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 型名から登録された型を取得
    /// </summary>
    public RegisteredType? GetType(string typeName)
    {
        if (_typeCache.TryGetValue(typeName, out var type))
            return type;

        // 部分一致も試す
        return RegisteredTypes.FirstOrDefault(t =>
            t.FullName == typeName ||
            t.DisplayName == typeName ||
            t.FullName?.EndsWith($".{typeName}") == true);
    }

    /// <summary>
    /// 指定した型のメンバー一覧を取得
    /// </summary>
    public IEnumerable<TypeMemberInfo> GetMembers(string typeName, TypeMemberKind? kind = null)
    {
        var type = GetType(typeName);
        if (type == null)
            return Enumerable.Empty<TypeMemberInfo>();

        if (kind.HasValue)
        {
            return type.Members.Where(m => m.Kind == kind.Value);
        }

        return type.Members;
    }

    /// <summary>
    /// 指定した型のメソッド一覧を取得
    /// </summary>
    public IEnumerable<TypeMemberInfo> GetMethods(string typeName, bool? staticOnly = null)
    {
        var methods = GetMembers(typeName, TypeMemberKind.Method);

        if (staticOnly.HasValue)
        {
            methods = methods.Where(m => m.IsStatic == staticOnly.Value);
        }

        return methods;
    }

    /// <summary>
    /// 指定した型のプロパティ一覧を取得
    /// </summary>
    public IEnumerable<TypeMemberInfo> GetProperties(string typeName)
    {
        return GetMembers(typeName, TypeMemberKind.Property);
    }

    /// <summary>
    /// 型名一覧を取得（フィルタリング可能）
    /// </summary>
    public IEnumerable<string> GetTypeNames(string? categoryFilter = null, string? searchQuery = null)
    {
        var types = RegisteredTypes.AsEnumerable();

        if (!string.IsNullOrEmpty(categoryFilter))
        {
            types = types.Where(t => t.Category == categoryFilter);
        }

        if (!string.IsNullOrEmpty(searchQuery))
        {
            types = types.Where(t =>
                t.DisplayName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                t.FullName?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) == true);
        }

        return types.Select(t => t.DisplayName).OrderBy(n => n);
    }

    /// <summary>
    /// 登録された型をクリア
    /// </summary>
    public void Clear()
    {
        RegisteredTypes.Clear();
        _typeCache.Clear();

        // 基本型を再登録
        RegisterPrimitiveTypes();
        RegisterCommonTypes();
    }
}
