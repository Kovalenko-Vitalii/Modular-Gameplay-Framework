using MessagePipe;
using SaveSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope {
    [SerializeField] private SaveConfig _saveConfig;
    [SerializeField] SceneDatabase _sceneDatabase;
    [SerializeField] private InputActionAsset _inputActions;

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterMessagePipe();

        /// Register global systems
        builder.RegisterEntryPoint<GameFlowOrchestrator>(Lifetime.Singleton).As<IGameFlowOrchestrator>();
        builder.Register<CursorLockController>(Lifetime.Singleton);
        builder.Register<GameModeProvider>(Lifetime.Singleton);
        builder.Register<SceneLoadService>(Lifetime.Singleton);
        builder.Register<SaveService>(Lifetime.Singleton);
        builder.Register<InputService>(Lifetime.Singleton).As<IInputService>();
        builder.RegisterComponentInHierarchy<SoundManager>();

        /// Register UI panels 
        builder.RegisterComponentInHierarchy<LoadingPanel>();

        /// Register configs
        builder.RegisterInstance(_saveConfig);
        builder.RegisterInstance(_sceneDatabase);
        builder.RegisterInstance(_inputActions);
    }
}
