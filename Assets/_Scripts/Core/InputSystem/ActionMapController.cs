using UnityEngine.InputSystem;

public class ActionMapController { // !!! not proud of this approach but it does it`s job for now :-)
    readonly InputActionMap _global;
    readonly InputActionMap _player;
    readonly InputActionMap _ui;
    readonly InputActionMap _windows;

    public ActionMapController(InputActionAsset asset) {
        _global = asset.FindActionMap("Global");
        _player = asset.FindActionMap("Player");
        _ui = asset.FindActionMap("UI");
        _windows = asset.FindActionMap("Windows");
    }

    public void SetContext(InputContext context) {
        _global.Enable();

        _player.Disable();
        _ui.Disable();
        _windows.Disable();

        switch (context) {
            case InputContext.Gameplay:
                _player.Enable();
                _windows.Enable();
                break;

            case InputContext.UI:
                _ui.Enable();
                _windows.Enable();
                break;
            case InputContext.Cutscene:
                break;
        }
    }
}

public enum InputContext {
    Gameplay,
    UI,
    Cutscene
}