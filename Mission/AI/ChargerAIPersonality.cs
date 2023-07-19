using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GRPG.GameLogic;

public class ChargerAIPersonality : AIPersonality
{
    public override (Action,ActionTarget) GetNextAction()
    {
        //TODO helpers to simplify all this shit

        List<Action> actions = _actor.GetAvailableActions()
            .Where(act => _actor.GetAvailableTargets(act).Count > 0)
            .ToList();

        if (actions.Count == 0) return (Action.Null, new ActionTarget(-1));

        List<Action> aggressiveActions = actions.Where(act => act.Name != "Move").ToList();//.DefaultIfEmpty(Action.Null).First();
        //Got agro action?
        if (aggressiveActions.Count > 0)
        {
            int rnd = Random.Range(0, aggressiveActions.Count);
            List<ActionTarget> targets = _actor.GetAvailableTargets(aggressiveActions[rnd]);
            
            return (aggressiveActions[rnd], targets[Random.Range(0, targets.Count)]);
        }

        Action moveAction = actions.Where(act => act.Name == "Move").DefaultIfEmpty(Action.Null).First(); // Need a better way for actions of TYPE move
        if (moveAction.IsNull())
        {
            return (Action.Null, new ActionTarget(-1));
        }
        Dictionary<Actor, List<int>> paths = Util.GetPathToActorsOnTeam(_actor, _enemyTeam);
        int nextLocation = -1;
        int shortestLength = 1000;
        foreach(KeyValuePair<Actor, List<int>> kv in paths) {
            if (kv.Value.Count == 0) return (Action.Null, new ActionTarget(-1)); //stay put i guess
            if (kv.Value.Count < shortestLength) { shortestLength = kv.Value.Count; nextLocation = kv.Value[0]; } // go
        }
        ActionTarget moveTarget = _actor.GetAvailableTargets(moveAction)
            .Where(t => t.Location == nextLocation)
            .DefaultIfEmpty(new ActionTarget(-1)).ToList().First();
        if (moveTarget.Location == -1)
        {
            return (Action.Null, new ActionTarget(-1));
        }

        return (moveAction, moveTarget);
    }
}
