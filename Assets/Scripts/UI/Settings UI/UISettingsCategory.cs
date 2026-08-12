using System.Collections.Generic;
using UnityEngine;

public class UISettingsCategory : MonoBehaviour
{
    [SerializeField] private UISettingRow rowPrefab;
    [SerializeField] private Transform content;

    private readonly List<UISettingRow> spawnedRows = new();

    public void Build(IReadOnlyList<ISettingRow> settings)
    {
        Clear();

        foreach (var setting in settings)
        {
            UISettingRow row = Instantiate(rowPrefab, content);
            row.Setup(setting);

            spawnedRows.Add(row);
        }
    }

    private void Clear()
    {
        foreach (var row in spawnedRows)
        {
            if (row != null)
                Destroy(row.gameObject);
        }

        spawnedRows.Clear();
    }
}