using VContainer;
using VContainer.Unity;

public class MainMenuLifetimeScope : LifetimeScope {
    protected override void Configure(IContainerBuilder builder) {
        /// UI Registration
        builder.RegisterComponentInHierarchy<NewGame>();
        builder.RegisterComponentInHierarchy<ResumeGameButton>();
        builder.RegisterComponentInHierarchy<LoadSaveProfile>();
    }
}
