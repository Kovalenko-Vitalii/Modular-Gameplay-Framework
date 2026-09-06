using SaveSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope {
    [SerializeField] private SaveConfig saveConfig;

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance(saveConfig);

        builder.RegisterComponentInHierarchy<GameFlowController>();
        builder.RegisterComponentInHierarchy<InputListener>();
        builder.RegisterComponentInHierarchy<SoundManager>();

        builder.Register<GameStateManager>(Lifetime.Singleton);
        builder.Register<SceneLoader>(Lifetime.Singleton);
        builder.Register<SaveService>(Lifetime.Singleton);

        builder.RegisterEntryPoint<CursorLockController>(Lifetime.Singleton); /// !!!
    }
}
