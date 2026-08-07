using MoonSharp.Interpreter;
using System.Threading.Tasks;

[MoonSharpUserData]
public class CreateEntityEvent : Event
{
    public EntityData entityData;
    public string zoneCategory;
    public string zoneOwner;
    
    public CreateEntityEvent(EntityData entityData, string zoneCategory, string zoneOwner)
    {
        this.entityData = entityData;
        this.zoneCategory = zoneCategory;
        this.zoneOwner = zoneOwner;
        eventType = "create_entity";
        SetOutput();
    }

    protected override Task Execute(Game game)
    {
        Entity createdEntity = game.CreateEntity(entityData, zoneCategory, zoneOwner);
        output["entity"] = createdEntity;
        SetOutput();
        return base.Execute(game);
    }

    public override void SetOutput()
    {
        base.SetOutput();
        output["entityData"] = zoneCategory;
        output["zoneCategory"] = zoneCategory;
        output["zoneOwner"] = zoneCategory;
    }
}
