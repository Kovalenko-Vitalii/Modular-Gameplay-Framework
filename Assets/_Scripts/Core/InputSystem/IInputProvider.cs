using System;

public interface IInputProvider {
    event Action<InputAction> Pressed;
    event Action<InputAction> Released;
}