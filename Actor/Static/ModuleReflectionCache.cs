using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Sackrany.Actor.Modules;

namespace Sackrany.Actor.Static
{
    public static class ModuleReflectionCache
    {
        public readonly struct ModuleMetadata
        {
            public readonly DependencyField[] Dependencies;
            public readonly TemplateField Template;
            public readonly int UpdateOrder;
            public ModuleMetadata(DependencyField[] dependencies, TemplateField template, int updateOrder)
            {
                Dependencies = dependencies;
                Template = template;
                UpdateOrder = updateOrder;
            }
        }

        public readonly struct DependencyField
        {
            public readonly FieldInfo Field;
            public readonly Type FieldType;
            public readonly Type ElementType;
            public readonly bool IsArray;
            public readonly bool IsOptional;
            public DependencyField(FieldInfo field, Type fieldType, Type elementType, bool isArray, bool isOptional)
            {
                Field = field;
                FieldType = fieldType;
                ElementType = elementType;
                IsArray = isArray;
                IsOptional = isOptional;
            }
        }

        public readonly struct TemplateField
        {
            public readonly FieldInfo Field;
            public readonly Type FieldType;
            public TemplateField(FieldInfo field, Type fieldType)
            {
                Field = field;
                FieldType = fieldType;
            }
        }

        static readonly Dictionary<Type, ModuleMetadata> _cache = new();

        public static ModuleMetadata GetMetadata(Type moduleType)
        {
            if (_cache.TryGetValue(moduleType, out var meta))
                return meta;

            meta = BuildMetadata(moduleType);
            _cache[moduleType] = meta;
            return meta;
        }

        static ModuleMetadata BuildMetadata(Type type)
        {
            var fields = type.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            var deps = new List<DependencyField>();
            var templates = new List<TemplateField>();
            int order = 0;

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<DependencyAttribute>() is { } dep)
                {
                    bool isArray = field.FieldType.IsArray;
                    deps.Add(new DependencyField(
                        field: field, 
                        fieldType: field.FieldType,
                        elementType: isArray ? field.FieldType.GetElementType() : null, 
                        isArray: isArray,
                        isOptional: dep.Optional)
                    );
                }
                else if (field.GetCustomAttribute<TemplateAttribute>() != null)
                {
                    templates.Add(new TemplateField(
                        field: field, 
                        fieldType: field.FieldType)
                    );
                }
            }

            if (type.GetCustomAttribute<UpdateOrderAttribute>() is { } orderAttr)
                order = orderAttr._order;

            return new ModuleMetadata(
                dependencies: deps.ToArray(), 
                template: templates.FirstOrDefault(), 
                updateOrder: order);
        }
    }
}