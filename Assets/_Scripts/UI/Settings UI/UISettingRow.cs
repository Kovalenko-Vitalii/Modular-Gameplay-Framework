using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISettingRow : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text valueText;

    [Header("Cycle mode")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Rebind mode")]
    [SerializeField] private Button middleButton;
    [SerializeField] private Button revertButton;

    private ISettingRow setting;

    public void Setup(ISettingRow setting)
    {
        if (this.setting != null)
            this.setting.Changed -= Refresh;

        this.setting = setting;
        nameText.text = setting.Name;

        ConfigureMode(setting.Mode);
        BindListeners(setting.Mode);

        setting.Changed += Refresh;

        Refresh();
    }

    private void ConfigureMode(SettingRowMode mode)
    {
        bool isCycle = mode == SettingRowMode.Cycle;

        leftButton.gameObject.SetActive(isCycle);
        rightButton.gameObject.SetActive(isCycle);

        middleButton.gameObject.SetActive(!isCycle);
        revertButton.gameObject.SetActive(!isCycle);
    }

    private void BindListeners(SettingRowMode mode)
    {
        leftButton.onClick.RemoveListener(OnLeftClicked);
        rightButton.onClick.RemoveListener(OnRightClicked);
        middleButton.onClick.RemoveListener(OnMiddleClicked);
        revertButton.onClick.RemoveListener(OnRevertClicked);

        if (mode == SettingRowMode.Cycle)
        {
            leftButton.onClick.AddListener(OnLeftClicked);
            rightButton.onClick.AddListener(OnRightClicked);
        }
        else
        {
            middleButton.onClick.AddListener(OnMiddleClicked);
            revertButton.onClick.AddListener(OnRevertClicked);
        }
    }

    private void OnLeftClicked() => setting.SecondaryAction();
    private void OnRightClicked() => setting.PrimaryAction();
    private void OnMiddleClicked() => setting.PrimaryAction();
    private void OnRevertClicked() => setting.SecondaryAction();

    private void Refresh()
    {
        valueText.text = setting.CurrentValue;
        if (setting.Mode == SettingRowMode.Cycle)
        {
            leftButton.interactable = setting.SecondaryEnabled;
        }
        else
        {
            revertButton.interactable = setting.SecondaryEnabled;
            middleButton.interactable = true;
        }
    }

    private void OnDestroy()
    {
        if (setting != null)
            setting.Changed -= Refresh;

        leftButton.onClick.RemoveListener(OnLeftClicked);
        rightButton.onClick.RemoveListener(OnRightClicked);
        middleButton.onClick.RemoveListener(OnMiddleClicked);
        revertButton.onClick.RemoveListener(OnRevertClicked);

        if (setting is RebindSettingDefinition rebind)
            rebind.CancelIfActive();
    }
}