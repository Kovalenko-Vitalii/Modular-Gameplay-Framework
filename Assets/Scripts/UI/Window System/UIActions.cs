using UnityEngine;

public class UIActions : MonoBehaviour
{
    public void ResumeGame() => GameStateManager.Instance.SetPaused(false);

    public void OpenInventory() => UIWindowManager.Instance.OpenWindow(UIWindowId.Inventory);

    public void OpenEsc() => UIWindowManager.Instance.OpenWindow(UIWindowId.Esc);

    public void OpenMainMenu() => UIWindowManager.Instance.OpenWindow(UIWindowId.MainMenu);

    public void SaveGame()
    {
        GameLog.Log("UIActions", "Save requested");
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}