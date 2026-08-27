using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SaveSystem {
    /// <summary>
    /// Marks a save-state class with a stable string key.
    /// You can rename the class freely — the save
    /// format stays stable as long as the key itself doesn't change.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class SaveStateAttribute : Attribute
    {
        public string Key { get; }
        public SaveStateAttribute(string key) => Key = key;
    }

    /// <summary>
    /// Maps [SaveState] keys to Types (and back), built once via reflection over
    /// loaded assemblies.
    /// </summary>
    public static class SaveTypeRegistry
    {
        private static readonly Dictionary<string, Type> keyToType = new();
        private static readonly Dictionary<Type, string> typeToKey = new();

        static SaveTypeRegistry()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = Array.FindAll(ex.Types, t => t != null);
                }

                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute<SaveStateAttribute>();
                    if (attr == null)
                        continue;

                    if (keyToType.TryGetValue(attr.Key, out var existingType))
                    {
                        Debug.LogError($"Duplicate [SaveState] key '{attr.Key}' on {type.FullName} and {existingType.FullName}. Keys must be unique.");
                        continue;
                    }

                    keyToType[attr.Key] = type;
                    typeToKey[type] = attr.Key;
                }
            }
        }

        public static bool TryGetKey(Type type, out string key) => typeToKey.TryGetValue(type, out key);

        public static bool TryGetType(string key, out Type type) => keyToType.TryGetValue(key, out type);
    }
}