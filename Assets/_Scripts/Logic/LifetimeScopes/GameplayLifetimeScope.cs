using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope {
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterComponentInHierarchy<AmbientManager>();
        builder.RegisterComponentInHierarchy<SurfaceResolver>();
        builder.RegisterComponentInHierarchy<TickSystem>();

        builder.RegisterComponentInHierarchy<UIWindowManager>();
        builder.RegisterComponentInHierarchy<LoadSaveSlot>();
        builder.RegisterComponentInHierarchy<SaveToManual>();
        builder.RegisterComponentInHierarchy<UIExitToMenuButton>();
        builder.RegisterComponentInHierarchy<NewManualSave>();
        builder.RegisterComponentInHierarchy<LoadAutoSave>();
        builder.RegisterComponentInHierarchy<SettingsMenuController>();

        builder.Register<GameplayStateController>(Lifetime.Singleton);
    }
}
