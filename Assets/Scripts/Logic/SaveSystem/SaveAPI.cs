using System.Collections.Generic;
using SaveSystem;

/// <summary>
/// API class for save functionality. 
/// </summary>
public static class SaveApi
{
    public static string ActiveProfileId => SaveManager.Instance.ActiveProfileId;
    public static SaveProfile ActiveProfile => SaveManager.Instance.ActiveProfile;
    public static bool HasAutoSave => SaveManager.Instance.HasAutoSave;

    public static List<SaveProfile> ListProfiles() => SaveManager.Instance.ListProfiles();
    public static SaveProfile CreateProfile(string displayName) => SaveManager.Instance.CreateProfile(displayName);
    public static void RenameProfile(string profileId, string displayName) => SaveManager.Instance.RenameProfile(profileId, displayName);
    public static void DeleteProfile(string profileId) => SaveManager.Instance.DeleteProfile(profileId);

    public static void SaveAuto() => SaveManager.Instance.SaveAuto();
    public static string SaveManual(string displayName) => SaveManager.Instance.SaveManual(displayName);
    public static void OverwriteManual(string slotId) => SaveManager.Instance.OverwriteManual(slotId);
    public static void DeleteManualSave(string profileId, string slotId) => SaveManager.Instance.DeleteManualSave(profileId, slotId);

    public static void LoadSlot(string profileId, string slotId) => SaveManager.Instance.LoadSlot(profileId, slotId);
    public static void ContinueProfile(string profileId) => SaveManager.Instance.ContinueProfile(profileId);
    public static void NewGame(string sceneName) => SaveManager.Instance.NewGame(sceneName);

    public static bool CanContinue() => SaveManager.Instance.CanContinue();
    public static void ContinueLatestGame() => SaveManager.Instance.ContinueLatestGame();
}