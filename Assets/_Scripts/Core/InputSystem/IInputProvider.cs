using System;

public interface IInputProvider {
    event Action<InputActionId> Pressed;
    event Action<InputActionId> Released;
}