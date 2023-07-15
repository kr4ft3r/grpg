using System.Collections;
using System.Collections.Generic;

using GRPG.GameLogic;

public class Damage
{
    public Actor Victim;
    public Actor Attacker;
    public Action Action;
    public int DamageAmount;

    /// <summary>
    /// Damage from attacker performin a damaging action
    /// </summary>
    /// <param name="victim"></param>
    /// <param name="attacker"></param>
    /// <param name="action"></param>
    /// <param name="damage"></param>
    public Damage(Actor victim, Actor attacker, Action action, int damage)
    {
        Victim = victim;
        Attacker = attacker;
        Action = action;
        DamageAmount = damage;
    }

    /// <summary>
    /// Damage from environment and effects
    /// </summary>
    /// <param name="victim"></param>
    /// <param name="damage"></param>
    public Damage(Actor victim, int damage)
    {
        Victim = victim;
        Attacker = null;
        Action = Action.Null;
        DamageAmount = damage;
    }

    /// <summary>
    /// Set damage scale, or negative scale to heal
    /// </summary>
    /// <param name="modifier"></param>
    public void ScaleDamage(float modifier)
    {
        DamageAmount =  (int)(DamageAmount * modifier);
    }

    /// <summary>
    /// Called after ActorIsDamaged events
    /// </summary>
    public void Apply()
    {
        if (DamageAmount == 0) return; // Not sure if it's good to hardcode this

        if (!Victim.Resources.Contains(Resource.HitPoints)) return; // Cannot kill that which has no structure

        int hp = Victim.Resources[Resource.HitPoints];
        hp = System.Math.Clamp(hp - DamageAmount, 0, Victim.Stats.PerBattleResources[Resource.HitPoints]);

        if (hp == 0 && Victim.Status != ActorStatus.Downed) {
            Victim.Status = ActorStatus.Downed; // TODO onactorkilled event
            //No.. do not. Victim.Location = -1;
            if (Victim.Mission.ActorWasDowned != null) Victim.Mission.ActorWasDowned(Victim);
        }
        Victim.Resources[Resource.HitPoints] = hp;
    }
    
}
