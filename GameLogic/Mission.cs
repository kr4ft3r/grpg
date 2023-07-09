using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
        public Connection(string props)
        {
            if (props.Contains('S')) CanSee = true;
            if (props.Contains('M')) CanMove = true;
            if (props.Contains('C')) HasCover = true;
        }
    }

    public delegate void ActionPerformedDelegate(ActionResult result);
    public delegate void ActorHasMovedDelegate(Actor actor, int from, int to);
    public delegate void ActorHasAttackedDelegate(Actor actor, Actor target, bool success);

    public class Mission
    {
        public int NumLocations { get; private set; }
        public Connection[,] Connections { get; private set; }
        public List<Actor> Actors = new List<Actor>();
        public List<Team> Teams = new List<Team>();
        public int TurnNumber { get; private set; }
        public int CurrentTeamIndex { get; private set; }
        public bool IsActive { get { return TurnNumber > 0; } }

        public Team CurrentTeam { get { return Teams[CurrentTeamIndex]; } }
        public IEnumerable<Actor> GetTeamMembers(Team team) => Actors.Where(a => a.Team == team);

        // Events
        public ActionPerformedDelegate AfterActionPerformed;
        public ActorHasMovedDelegate AfterActorMoves;
        public ActorHasAttackedDelegate AfterActorAttacks;

        public Mission(string[,] graph)
        {
            AfterActionPerformed = HandleAction;
            AfterActorMoves = HandleMove;
            AfterActorAttacks = HandleAttack;
            MakeGraph(graph);
            Teams.Add(Team.Human);
            Teams.Add(Team.AI);
        }

        private void MakeGraph(string[,] graph)
        {
            NumLocations = (int) Math.Sqrt(graph.Length);
            // Can't I just map a string matrix to a Conn matrix? :(
            Connections = new Connection[NumLocations, NumLocations];
            for (int i = 0; i < NumLocations; i++)
                for (int j = 0; j < NumLocations; j++)
                    Connections[i, j] = new Connection(graph[i, j]);
        }

        public Actor CreateActor(string name, Character character, Team team, int location)
        {
            var actor = new Actor(this, name, character, team, location);
            Actors.Add(actor);
            return actor;
        }

        public void Start()
        {
            TurnNumber += 1;
            foreach (var actor in this.GetTeamMembers(CurrentTeam)) actor.NewTurn();
        }

        public void EndTurn()
        {
            CurrentTeamIndex += 1;
            if (CurrentTeamIndex >= this.Teams.Count) {
                CurrentTeamIndex = 0;
                TurnNumber += 1;
            }
            foreach (var actor in this.GetTeamMembers(CurrentTeam)) actor.NewTurn();
        }

        protected void HandleMove(Actor actor, int from, int to)
        {
            FFS.Log("Moved: {0} {1}->{2}", actor.Name, from, to);
        }

        protected void HandleAttack(Actor actor, Actor target, bool success)
        {
            FFS.Log("Attacked: {0} -> {1} ({2})", actor.Name, target.Name, success);
        }

        protected void HandleAction(ActionResult result)
        {
            FFS.Log("Action: {0}", result.Action.Name);
        }
    }

}