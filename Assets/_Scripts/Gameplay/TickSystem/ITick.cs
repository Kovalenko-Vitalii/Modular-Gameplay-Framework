// <summary>
// Interface for objects that want to be ticked every frame by the PlayerTickSystem
// </summary>
public interface ITick
{
    void Tick(float dt);
}