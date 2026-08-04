using UnityEngine;

/// <summary>
/// This event only works for checking if an entity is playable. It should be interacted with triggers and cancelled if no ways of playing the entity are available
/// </summary>
public class CheckToPlayEvent : Event
{
    public Entity entity;

    public CheckToPlayEvent(Entity entity)
    {
        this.entity = entity;
    }
}
