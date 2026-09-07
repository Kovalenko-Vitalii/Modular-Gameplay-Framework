using SaveSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope {
    [SerializeField] private SaveConfig _saveConfig;
    [SerializeField] SceneDatabase _sceneDatabase;

    protected override void Configure(IContainerBuilder builder) {
        /// Register global systems
        builder.RegisterEntryPoint<GameFlowController>(Lifetime.Singleton).As<IGameFlowController>();
        builder.RegisterEntryPoint<CursorLockController>(Lifetime.Singleton); /// !!!
        builder.Register<GameStateManager>(Lifetime.Singleton);
        builder.Register<SceneLoader>(Lifetime.Singleton);
        builder.Register<SaveService>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<InputListener>().As<IInputListener>();
        builder.RegisterComponentInHierarchy<SoundManager>();

        /// Register UI panels 
        builder.RegisterComponentInHierarchy<LoadingPanel>();

        /// Register configs
        builder.RegisterInstance(_saveConfig);
        builder.RegisterInstance(_sceneDatabase);
    }
}
