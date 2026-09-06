using SaveSystem;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope {
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterComponentInHierarchy<GameFlowController>();
        builder.RegisterComponentInHierarchy<GameStateManager>();
        builder.RegisterComponentInHierarchy<InputListener>();
        builder.RegisterComponentInHierarchy<SoundManager>();
        builder.Register<SceneLoader>(Lifetime.Singleton);
        builder.Register<SaveService>(Lifetime.Singleton);
        builder.RegisterEntryPoint<CursorLockController>(Lifetime.Singleton); /// !!!
    }
}
