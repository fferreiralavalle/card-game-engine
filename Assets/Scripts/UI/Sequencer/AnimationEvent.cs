using MoonSharp.Interpreter;
using System;
using System.Threading.Tasks;

[MoonSharpUserData]
public class AnimationEvent
{
    public Func<Task> animation;
    public float priority = 0;

    public AnimationEvent() { }

    public AnimationEvent(Func<Task> animation)
    {
        this.animation = animation;
    }

    public AnimationEvent(Func<Task> animation, float priority)
    {
        this.animation = animation;
        this.priority = priority;
    }

    public virtual async Task Play()
    {
        await animation.Invoke();
    }
}
