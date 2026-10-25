using UnityEngine;

public static class ValidationAPI {
    public static bool IsValid<T>(this T obj) where T : class, IIdentifiable {
        if (obj == null || string.IsNullOrEmpty(obj.Id)) 
            return false; 
        else
            return true;
    }
}