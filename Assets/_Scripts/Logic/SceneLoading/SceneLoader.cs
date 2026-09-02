using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Additive scene loading manager.
/// </summary>
[DefaultExecutionOrder(-1500)]
public class SceneLoader : MonoBehaviour {
    string TAG = "SceneLoader";

    string currentContentScene;
    AsyncOperation activateLoadOperation; // Async operation for paralel loading
    private bool isBusy;

    public string CurrentContentScene => currentContentScene;
    public float Progress { get; private set; } = 0f;
    public bool IsBusy => isBusy;
    public bool IsLoading => activateLoadOperation != null && !activateLoadOperation.isDone; // Shows if scene loading at the moment

    /// <summary>
    /// Loaded scene name.
    /// </summary>
    public event Action<string> ContentLoaded;

    /// <summary>
    /// Load content scene and activate automatically when loaded.
    /// </summary>
    public Coroutine LoadContent(string sceneName) {
        if (isBusy) {
            GameLog.Warning(TAG, $"LoadContent ignored: already busy");
            return null;
        }

        if (currentContentScene == sceneName) { // early exit
            var scene = SceneManager.GetSceneByName(sceneName);

            if (scene.IsValid() && scene.isLoaded) {
                SceneManager.SetActiveScene(scene);
                Progress = 1f;
                ContentLoaded?.Invoke(sceneName);
                return null;
            }
        }
        
        GameLog.Log(TAG, $"LoadContent('{sceneName}') initiated ");

        Progress = 0f; // Resetting load progress
        isBusy = true;

        activateLoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive); // Adding loading process to async and activate it        
        activateLoadOperation.allowSceneActivation = true;

        return StartCoroutine(FinishSwapRoutine(sceneName));
    }

    /// <summary>
    /// Save as LoadContent but user can choose when to finally load scene
    /// </summary>
    public AsyncOperation LoadContentAsync(string sceneName, bool allowSceneActivation) {
        if (isBusy) {
            GameLog.Warning(TAG, $"LoadContentAsync ignored: already busy");
            return activateLoadOperation;
        }

        if (currentContentScene == sceneName) {
            var s = SceneManager.GetSceneByName(sceneName);

            if (s.IsValid() && s.isLoaded) {
                SceneManager.SetActiveScene(s);
                Progress = 1f;
                ContentLoaded?.Invoke(sceneName);
                return null;
            }
        }

        GameLog.Log(TAG, $"AsyncLoadContent('{sceneName}') initiated ");

        Progress = 0f;
        isBusy = true;

        activateLoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        activateLoadOperation.allowSceneActivation = allowSceneActivation;

        StartCoroutine(FinishSwapRoutine(sceneName));
        return activateLoadOperation;
    }

    /// <summary>
    /// This method used to wait for scene load and than switch it with current one
    /// </summary>
    private IEnumerator FinishSwapRoutine(string sceneName) {
        var operation = activateLoadOperation;

        while (operation != null && !operation.isDone) { // Waiting till loads
            float raw = operation.progress;
            Progress = Mathf.Clamp01(raw < 0.9f ? raw / 0.9f : 1f);
            yield return null;
        }

        GameLog.Log(TAG, $"FinishSwapRoutine BEGIN target='{sceneName}' prev='{currentContentScene}'");

        var loadedScene = SceneManager.GetSceneByName(sceneName); // Making this scene active

        if (loadedScene.IsValid()) { // Validating new scene
            SceneManager.SetActiveScene(loadedScene);
            GameLog.Log(TAG, $"SetActiveScene '{sceneName}'");
        }
        else
            GameLog.Error(TAG, $"Loaded scene '{sceneName}' is NOT valid");

        if (!string.IsNullOrEmpty(currentContentScene) && currentContentScene != sceneName) { // Unloading previous scene
            GameLog.Log(TAG, $"Unloading previous content '{currentContentScene}'");
            yield return SceneManager.UnloadSceneAsync(currentContentScene);
        }

        currentContentScene = sceneName; // Setting loaded scene as current, updating progress

        Progress = 1f;
        activateLoadOperation = null;
        isBusy = false;
        ContentLoaded?.Invoke(sceneName);

        GameLog.Log(TAG, $"FinishSwapRoutine END currentContent='{currentContentScene}' activeScene='{SceneManager.GetActiveScene().name}'");
    }
}