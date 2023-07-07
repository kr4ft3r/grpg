using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GRPG.GameLogic
{
    public class Connection
    {
        public bool CanSee;
        public bool CanMove;
        public bool HasCover;

        public Connection() {}
        public Connection(bool canSee, bool canMove, bool hasCover)
        {
            CanSee = canSee;
            CanMove = canMove;
            HasCover = hasCover;
        }
    };

    public class Mission
    {
        public Connection[,] Connections { get; private set; }
        public List<Actor> Actors = new List<Actor>();
        public List<Team> Teams = new List<Team>();
        public int TurnNumber { get; private set; }
        public int CurrentTeamIndex { get; private set; }

        public Team CurrentTeam { get { return Teams[CurrentTeamIndex]; } }
        public IEnumerable<Actor> GetTeamMembers(Team team) => Actors.Where(a => a.Team == team);

        public Mission(uint numNodes)
        {
            Connections = new Connection[numNodes, numNodes];
            Teams.Add(Team.Human);
            Teams.Add(Team.AI);
        }

        public void EndTurn()
        {
            CurrentTeamIndex += 1;
            foreach (var actor in this.GetTeamMembers(CurrentTeam)) actor.NewTurn();
            if (CurrentTeamIndex < this.Teams.Count) return;
            CurrentTeamIndex = 0;
            TurnNumber += 1;
        }
    }

}