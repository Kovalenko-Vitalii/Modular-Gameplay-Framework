// <summary>
// API for interacting with the UI system outside of the UI system itself
// </summary>
public static class UIApi
{
    public static void Open(UIWindowDefinition window) => UIScreenManager.Instance.Current?.Windows.Open(window);
    public static void Close(UIWindowDefinition window) => UIScreenManager.Instance.Current?.Windows.Close(window);
    public static void Toggle(UIWindowDefinition window) => UIScreenManager.Instance.Current?.Windows.Toggle(window);
    public static bool IsOpen(UIWindowDefinition window) => UIScreenManager.Instance.Current?.Windows.IsOpen(window) ?? false;
}