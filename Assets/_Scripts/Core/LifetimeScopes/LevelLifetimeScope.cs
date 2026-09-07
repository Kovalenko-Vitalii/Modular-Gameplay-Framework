using VContainer;
using VContainer.Unity;

public class LevelLifetimeScope : LifetimeScope {
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterComponentInHierarchy<CinemachineController>(); 
        builder.RegisterComponentInHierarchy<PlayerSpawner>();

        builder.RegisterComponentInHierarchy<AmbientZone>();
    }
}
