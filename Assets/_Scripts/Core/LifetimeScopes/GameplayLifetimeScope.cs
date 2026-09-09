using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope {
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterEntryPoint<GameplayFlowOrchestrator>(Lifetime.Singleton);
        builder.Register<GameplayStateMachine>(Lifetime.Singleton);

        builder.Register<GameplayContext>(Lifetime.Singleton);
        builder.Register<PlayingState>(Lifetime.Singleton).As<IGameplayState>();
        builder.Register<DeadState>(Lifetime.Singleton).As<IGameplayState>();
        builder.Register<CutsceneState>(Lifetime.Singleton).As<IGameplayState>();

        builder.RegisterComponentInHierarchy<AmbientManager>();
        builder.RegisterComponentInHierarchy<SurfaceResolver>();
        builder.RegisterComponentInHierarchy<TickSystem>();
        builder.Register<PauseService>(Lifetime.Singleton);
            
        builder.RegisterComponentInHierarchy<UIWindowManager>();
        builder.RegisterComponentInHierarchy<LoadSaveSlot>();
        builder.RegisterComponentInHierarchy<SaveToManual>();
        builder.RegisterComponentInHierarchy<UIExitToMenuButton>();
        builder.RegisterComponentInHierarchy<NewManualSave>();
        builder.RegisterComponentInHierarchy<LoadAutoSave>();
        builder.RegisterComponentInHierarchy<SettingsMenuController>();
    }
}
