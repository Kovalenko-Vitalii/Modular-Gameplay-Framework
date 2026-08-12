using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsSettingsProvider : ISettingsCategoryProvider
{
    public string CategoryName => "Controls";

    private readonly InputActionAsset actions;
    private readonly System.Action onAnyRebind;

    public ControlsSettingsProvider(InputActionAsset actions, System.Action onAnyRebind = null)
    {
        this.actions = actions;
        this.onAnyRebind = onAnyRebind;
    }

    public List<ISettingRow> BuildSettings()
    {
        var rows = new List<ISettingRow>();

        foreach (var map in actions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];

                    if (binding.isComposite)
                        continue;

                    string label = binding.isPartOfComposite
                        ? $"{action.name} - {binding.name}"
                        : action.name;

                    rows.Add(new RebindSettingDefinition(label, action, i, onAnyRebind));
                }
            }
        }

        return rows;
    }
}

public static class InputRebindPersistence
{
    private const string PrefsKey = "InputBindingOverrides";

    public static void Save(InputActionAsset actions)
    {
        PlayerPrefs.SetString(PrefsKey, actions.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    public static void Load(InputActionAsset actions)
    {
        if (PlayerPrefs.HasKey(PrefsKey))
            actions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PrefsKey));
    }
}