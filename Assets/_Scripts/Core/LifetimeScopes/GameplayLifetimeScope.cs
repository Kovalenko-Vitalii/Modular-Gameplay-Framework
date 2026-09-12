using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope {
    protected override void Configure(IContainerBuilder builder) {
        /// Services Registration
        builder.RegisterEntryPoint<GameplayFlowOrchestrator>(Lifetime.Scoped);
        builder.Register<GameplayStateMachine>(Lifetime.Scoped);
        builder.Register<GameplayContext>(Lifetime.Scoped);
        builder.Register<PlayingState>(Lifetime.Scoped).As<IGameplayState>();
        builder.Register<DeadState>(Lifetime.Scoped).As<IGameplayState>();
        builder.Register<CutsceneState>(Lifetime.Scoped).As<IGameplayState>();

        builder.RegisterComponentInHierarchy<AmbientManager>();
        builder.RegisterComponentInHierarchy<SurfaceResolver>();
        builder.RegisterComponentInHierarchy<TickSystem>();
        builder.Register<PauseService>(Lifetime.Scoped);

        /// UI Registration
        builder.RegisterComponentInHierarchy<UIWindowManager>();
        builder.RegisterComponentInHierarchy<UIWindowView>();

        builder.RegisterComponentInHierarchy<LoadSaveSlot>();
        builder.RegisterComponentInHierarchy<SaveToManual>();
        builder.RegisterComponentInHierarchy<UIExitToMenuButton>();
        builder.RegisterComponentInHierarchy<NewManualSave>();
        builder.RegisterComponentInHierarchy<LoadAutoSave>();
        builder.RegisterComponentInHierarchy<SettingsMenuController>();
    }
}
