using UnityEngine;

public static class ValidationAPI {
    public static bool IsValid<T>(this T obj) where T : class, IIdentifiable {
        if (obj == null || string.IsNullOrEmpty(obj.Id)) {
            Debug.LogError($"Invalid {typeof(T).Name} (id: '{obj?.Id}')");
            return false;
        }
        return true;
    }
}