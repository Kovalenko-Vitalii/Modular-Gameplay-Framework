using System;
using UnityEngine;

// temporary solution !!!
[CreateAssetMenu(menuName = "Game/Core/Scene Database")]
public class SceneDatabase : ScriptableObject {
    public SceneRecord MainMenuSchell;
    public SceneRecord GameplayShell;
    public SceneRecord[] Levels;
}

[Serializable]
public class SceneRecord {
    public SceneType _sceneType;
    public string sceneName;
}

public enum SceneType {
    Shell,
    Level
}