using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    string TAG = "SceneLoader";
    public static SceneLoader Instance { get; private set; }

    string _currentContentScene; // Name of current loaded in content scene

    AsyncOperation _activeLoadOp; // Async operation for paralel loading

    private bool _isBusy;
    public bool IsBusy => _isBusy;

    public string CurrentContentScene => _currentContentScene;

    public float Progress { get; private set; } = 0f; // Float that indicated loading progress, used mainly for UI

    public bool IsLoading => _activeLoadOp != null && !_activeLoadOp.isDone; // Shows if scene loading at the moment

    public event Action<string> ContentLoaded;

    private void Awake()
    {
        GameLog.Log(TAG, $"Awake() initiated");

        if (Instance != null && Instance != this)
        {
            GameLog.Warning(TAG, $"Duplicate SceneLoader -> destroying id={GetInstanceID()}");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameLog.Log(TAG, $"Awake() finished. Singleton set");
    }

    // Load content scene and activate automatically when loaded
    public Coroutine LoadContent(string sceneName)
    {
        if (_currentContentScene == sceneName)
        {
            var s = SceneManager.GetSceneByName(sceneName);
            if (s.IsValid() && s.isLoaded)
            {
                SceneManager.SetActiveScene(s);
                Progress = 1f;
                ContentLoaded?.Invoke(sceneName);
                return null;
            }
        }

        if (_isBusy)
        {
            GameLog.Warning(TAG, $"LoadContent ignored: already busy");
            return null;
        }
        GameLog.Log(TAG, $"LoadContent('{sceneName}') initiated ");

        Progress = 0f; // Resetting load progress
        _isBusy = true;

        _activeLoadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive); // Adding loading process to async and activate it        
        _activeLoadOp.allowSceneActivation = true;

        return StartCoroutine(FinishSwapRoutine(sceneName));
    }

    // Save as LoadContent but user can choose when to finally load scene
    public AsyncOperation LoadContentAsync(string sceneName, bool allowSceneActivation)
    {
        if (_currentContentScene == sceneName)
        {
            var s = SceneManager.GetSceneByName(sceneName);
            if (s.IsValid() && s.isLoaded)
            {
                SceneManager.SetActiveScene(s);
                Progress = 1f;
                ContentLoaded?.Invoke(sceneName);
                return null;
            }
        }

        if (_isBusy)
        {
            GameLog.Warning(TAG, $"LoadContentAsync ignored: already busy");
            return _activeLoadOp;
        }

        GameLog.Log(TAG, $"AsyncLoadContent('{sceneName}') initiated ");

        Progress = 0f;
        _isBusy = true;

        _activeLoadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _activeLoadOp.allowSceneActivation = allowSceneActivation;

        StartCoroutine(FinishSwapRoutine(sceneName));
        return _activeLoadOp;
    }

    // Trigger scene activation
    // DEPRECIATED
    public void ActivateLoadedScene()
    {
        if (_activeLoadOp != null)
        {
            GameLog.Log(TAG, "ActivateLoadedScene()");
            _activeLoadOp.allowSceneActivation = true;
        }
        else
            GameLog.Warning(TAG, "ActivateLoadedScene called but _activeLoadOp is null");
    }

    // This method used to wait for scene load and than switch it with current one
    private IEnumerator FinishSwapRoutine(string sceneName)
    {
        var op = _activeLoadOp;

        while (op != null && !op.isDone)
        {
            float raw = op.progress;
            Progress = Mathf.Clamp01(raw < 0.9f ? raw / 0.9f : 1f);
            yield return null;
        }

        GameLog.Log(TAG, $"FinishSwapRoutine BEGIN target='{sceneName}' prev='{_currentContentScene}'");

        var loadedScene = SceneManager.GetSceneByName(sceneName); // Making this scene active

        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
            GameLog.Log(TAG, $"SetActiveScene '{sceneName}'");
        }
        else
            GameLog.Error(TAG, $"Loaded scene '{sceneName}' is NOT valid");

        // Unloading previous scene
        if (!string.IsNullOrEmpty(_currentContentScene) && _currentContentScene != sceneName)
        {
            GameLog.Log(TAG, $"Unloading previous content '{_currentContentScene}'");
            yield return SceneManager.UnloadSceneAsync(_currentContentScene);
        }

        _currentContentScene = sceneName; // Setting loaded scene as current, updating progress

        Progress = 1f;
        _activeLoadOp = null;
        _isBusy = false;
        ContentLoaded?.Invoke(sceneName);

        GameLog.Log(TAG, $"FinishSwapRoutine END currentContent='{_currentContentScene}' activeScene='{SceneManager.GetActiveScene().name}'");
    }
}