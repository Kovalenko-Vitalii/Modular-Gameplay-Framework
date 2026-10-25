using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadService {
    string TAG = "SceneLoadService";

    readonly SortedDictionary<SceneType, string> _loadedScenes = new();
    readonly CancellationTokenSource _lifetimeCts = new();

    public float Progress { get; private set; } = 0f;
    public bool IsBusy { get; private set; }

    public event Action<SceneType, string> SceneLoaded;

    #region Public API
    /// <returns> Name of the scene loaded in the specified slot. </returns>
    public string GetLoaded(SceneType sceneType) {
        if (_loadedScenes.TryGetValue(sceneType, out var name)) 
            return name;
        return null;
    }
    
    public string GetLoadedLevel() => GetLoaded(SceneType.Level);

    public UniTask<bool> LoadShell(string sceneName) => Load(SceneType.Shell, sceneName);

    public UniTask<bool> LoadLevel(string sceneName) => Load(SceneType.Level, sceneName);
        
    #endregion

    #region Private
    /// <summary> Loads scene into specified slot. </summary>
    /// <returns> Success of the operation. </returns>
    async public UniTask<bool> Load(SceneType slot, string sceneName) {
        if (IsBusy) { GameLog.Warning(TAG, $"Load({slot}, '{sceneName}') ignored: already busy"); return false; }

        if (GetLoaded(slot) == sceneName) { // early exit
            var scene = SceneManager.GetSceneByName(sceneName);

            if (scene.IsValid() && scene.isLoaded) {
                SceneManager.SetActiveScene(scene);
                Progress = 1f;
                SceneLoaded?.Invoke(slot, sceneName);
                return true;
            }
        }

        var token = _lifetimeCts.Token;

        IsBusy = true;
        Progress = 0f;

        try {
            var deeperSlots = _loadedScenes.Keys.Where(key => key > slot)
                                                .OrderByDescending(key => key)
                                                .ToList();
            foreach (var key in deeperSlots)
                await UnloadSlot(key, token);

            string previousInSameSlot = GetLoaded(slot);

            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null) { GameLog.Error(TAG, $"Scene '{sceneName}' not found in Build Settings."); return false; }

            while (!operation.isDone) {
                float raw = operation.progress;
                Progress = Mathf.Clamp01(raw < 0.9f ? raw / 0.9f : 1f);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName)); // Set the newly loaded scene as active

            if (!string.IsNullOrEmpty(previousInSameSlot) && previousInSameSlot != sceneName) 
                await SceneManager.UnloadSceneAsync(previousInSameSlot).ToUniTask(cancellationToken: token);
            
            _loadedScenes[slot] = sceneName;
            Progress = 1f;

            SceneLoaded?.Invoke(slot, sceneName);
            return true;
        } finally {
            IsBusy = false;
        }
    }

    async UniTask UnloadSlot(SceneType slot, CancellationToken ct) {
        if (!_loadedScenes.TryGetValue(slot, out var name) || string.IsNullOrEmpty(name))
            return;

        await SceneManager.UnloadSceneAsync(name).ToUniTask(cancellationToken: ct);
        _loadedScenes.Remove(slot);
    }
    #endregion
}