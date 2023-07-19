using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GRPG.GameLogic;

public delegate void ActorGainedActionDelegate(Actor actor, Action action);
public delegate void ActorLostActionDelegate(Actor actor, Action action);

public class ActorManager
{
    public static ActorManager Instance;

    private Dictionary<string, Actor> _actors;

    public ActorGainedActionDelegate ActorGainedAction;
    public ActorLostActionDelegate ActorLostAction;

    public ActorManager(Dictionary<string, Actor> actors)
    {
        _actors = actors;

        Instance = this;
    }

    public Actor GetActorByName(string name)
    {
        if (_actors.ContainsKey(name))
        {
            return _actors[name];
        }

        return null;
    }

    public void ActorEquipAction(string actorName, Action action)
    {
        Actor actor = GetActorByName(actorName);
        if (actor != null) ActorEquipAction(actor, action);
    }

    public void ActorEquipAction(Actor actor, Action action)
    {
        if (actor.HasActionEquipped(action)) return;

        actor.Stats.Actions.Add(action);
        if (ActorGainedAction != null) ActorGainedAction(actor, action);
    }

    public void ActorUnequipAction(string actorName, Action action)
    {
        Actor actor = GetActorByName(actorName);
        if (actor != null) ActorUnequipAction(actor, action);
    }

    public void ActorUnequipAction(Actor actor, Action action)
    {
        if (!actor.HasActionEquipped(action)) return;

        actor.Stats.Actions.Remove(action);
        if (ActorLostAction != null) ActorLostAction(actor, action);
    }
}
