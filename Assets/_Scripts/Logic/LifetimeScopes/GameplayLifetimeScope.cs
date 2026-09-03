using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope {
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterComponentInHierarchy<AmbientManager>();
        builder.RegisterComponentInHierarchy<SurfaceResolver>();
        builder.RegisterComponentInHierarchy<TickSystem>();

        builder.RegisterComponentInHierarchy<AmbientZone>();
    }
}
