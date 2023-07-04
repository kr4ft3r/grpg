using System.Collections;
using System.Collections.Generic;

public class Team
{
    public enum Owner { Human, AI }

    public readonly string name;
    public readonly List<Actor> members;
    public readonly Team.Owner type;

    public int alive { get; private set; }

    public Team (string teamName, List<Actor> teamMembers, Owner teamOwner)
    {
        name = teamName;
        members = teamMembers;
        type = teamOwner;

        alive = teamMembers.Count;
    }
}
