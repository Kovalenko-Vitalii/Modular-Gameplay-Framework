using System;
using UnityEngine;

public interface IInputService {
    event Action<InputActionId> Performed;
    event Action<InputActionId> Canceled;

    Vector2 Move { get; }
    Vector2 Look { get; }

    void RequestContext(string key, InputContext context);
    void ReleaseContext(string key);
}