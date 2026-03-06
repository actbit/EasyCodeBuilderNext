using EasyCodeBuilderNext.Core.Blocks;
using EasyCodeBuilderNext.Core.Models;
using System.Collections.ObjectModel;
using System.Reflection;

namespace EasyCodeBuilderNext.Core.PluginSystem;

/// <summary>
/// 型レジストリ - 外部DLLから読み込んだ型を管理
/// </summary>
public class TypeRegistry
{
    private readonly Dictionary<string, TypeInfo> _types = new();
    private readonly Dictionary<string, Assembly> _assemblies = new();

    /// <summary>
    /// 登録済みの型一覧
    /// </summary>
    public IReadOnlyDictionary<string, TypeInfo> RegisteredTypes => _types;

    /// <summary>
    /// アセンブリを登録
    /// </summary>
    public IEnumerable<TypeInfo> RegisterAssembly(Assembly assembly)
    {
        var types = new List<TypeInfo>();

        _assemblies[assembly.FullName ?? assembly.GetName().Name ?? Guid.NewGuid().ToString()] = assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsPublic && !type.IsNested)
            {
                var typeInfo = CreateTypeInfo(type);
                _types[typeInfo.FullName] = typeInfo;
                types.Add(typeInfo);
            }
        }

        return types;
    }

    /// <summary>
    /// アセンブリファイルから型を登録
    /// </summary>
    public IEnumerable<TypeInfo> RegisterAssembly(string assemblyPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);
        return RegisterAssembly(assembly);
    }

    /// <summary>
    /// 型を検索
    /// </summary>
    public IEnumerable<TypeInfo> SearchTypes(string query)
    {
        return _types.Values
            .Where(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                       t.FullName.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 完全修飾名で型を取得
    /// </summary>
    public TypeInfo? GetType(string fullName)
    {
        return _types.TryGetValue(fullName, out var type) ? type : null;
    }

    /// <summary>
    /// 指定した型のメンバーを取得
    /// </summary>
    public IEnumerable<MemberInfo> GetTypeMembers(string fullName)
    {
        if (!_assemblies.Values
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == fullName) is { } type)
        {
            return Enumerable.Empty<MemberInfo>();
        }

        var members = new List<MemberInfo>();

        // メソッド
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            if (method.DeclaringType == typeof(object)) continue;

            members.Add(new MemberInfo
            {
                Name = method.Name,
                Kind = method.IsStatic ? MemberKind.StaticMethod : MemberKind.InstanceMethod,
                ReturnType = method.ReturnType.Name,
                IsStatic = method.IsStatic
            });

            foreach (var param in method.GetParameters())
            {
                members.Last().Parameters.Add(new ParameterInfo
                {
                    Name = param.Name ?? "",
                    TypeName = param.ParameterType.Name,
                    DefaultValue = param.HasDefaultValue ? param.DefaultValue?.ToString() : null
                });
            }
        }

        // プロパティ
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            if (prop.DeclaringType == typeof(object)) continue;

            members.Add(new MemberInfo
            {
                Name = prop.Name,
                Kind = MemberKind.Property,
                ReturnType = prop.PropertyType.Name,
                IsStatic = prop.GetMethod?.IsStatic ?? false
            });
        }

        // フィールド
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            if (field.DeclaringType == typeof(object)) continue;

            members.Add(new MemberInfo
            {
                Name = field.Name,
                Kind = field.IsStatic ? MemberKind.StaticField : MemberKind.InstanceField,
                ReturnType = field.FieldType.Name,
                IsStatic = field.IsStatic
            });
        }

        return members;
    }

    private TypeInfo CreateTypeInfo(Type type)
    {
        return new TypeInfo
        {
            Name = type.Name,
            FullName = type.FullName ?? type.Name,
            Namespace = type.Namespace,
            BaseType = type.BaseType?.FullName,
            IsPublic = type.IsPublic,
            IsStatic = type.IsAbstract && type.IsSealed,
            IsAbstract = type.IsAbstract,
            IsEnum = type.IsEnum,
            IsValueType = type.IsValueType
        };
    }
}
