using System;

public interface IInputListener {
    event Action<InputAction> Pressed;
    event Action<InputAction> Released;
}