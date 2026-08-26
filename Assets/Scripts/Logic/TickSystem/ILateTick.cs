// <summary>
// Interface for objects that want to be late-ticked every frame by the PlayerTickSystem
// </summary>
public interface ILateTick
{
    void LateTick(float dt);
}