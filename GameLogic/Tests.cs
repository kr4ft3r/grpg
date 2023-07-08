using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRPG.GameLogic
{
    [TestClass]
    public class LogicTests
    {
        // The graph should look like this.
        // x - full cover except from nearest (!CanSee)
        // c - half cover from everything except x (HasCover)
        // o - vanilla location
        // Nodes indexed left-right, top-bottom
        // Row->Col => From->To
        //              o
        //            /   \
        //      x - c       c - x
        //            \   /
        //              o
        public static readonly string[,] Graph = new string[,] {
            {"Sxx", "SMx", "xxx", "xxx", "xxx", "xxx"},
            {"SMx", "Sxx", "SMx", "SMx", "SxC", "xxx"},
            {"xxx", "SMC", "Sxx", "Sxx", "SMC", "xxx"},
            {"xxx", "SMC", "Sxx", "Sxx", "SMC", "xxx"},
            {"xxx", "SxC", "SMx", "SMx", "Sxx", "xxx"},
            {"xxx", "xxx", "xxx", "xxx", "SMx", "Sxx"},
        };

        public Mission Mission;
        public Actor Player;
        public Actor Monster;


        [TestInitialize]
        public void InitTest()
        {
            Mission = new Mission(Graph);
            Player = Mission.CreateActor("Player", new CharacterStats(), Team.Human, 0);
            Monster = Mission.CreateActor("Monster", new CharacterStats(), Team.AI, 5);
        }

        [TestMethod]
        public void HappyPathTest()
        {
            // 6 node graph, human goes first
            Assert.AreEqual(6, Mission.NumLocations);
            Assert.AreEqual(Team.Human, Mission.CurrentTeam);

            // Should be no APs before mission.Start()
            Assert.AreEqual(0, Mission.Actors[0].Resources[Resource.PrimaryAction]);

            Mission.Start();

            // Player should have 2AP, monster 0AP
            Assert.AreEqual(1, Player.Resources[Resource.PrimaryAction]);
            Assert.AreEqual(0, Monster.Resources[Resource.PrimaryAction]);

            // Monster shouldn't be able to act on player turn
            Assert.AreEqual(Monster.GetAvailableActions().Count, 0);

            // Player should have two available actions
            Assert.AreEqual(2, Player.GetAvailableActions().Count);

            // Player should only be able to move to second location (#1)
            var targets = Player.GetAvailableTargets(Action.Move);
            Assert.AreEqual(0, Player.Location);
            Assert.AreEqual(1, targets.Count());
            Assert.AreEqual(1, targets[0].Location);
            Assert.IsNull(targets[0].Actor);

            // Move to the same square and punch out of existence.
            Action.Move.Perform(Player, new ActionTarget(1));
            Mission.EndTurn();
            Action.Move.Perform(Monster, new ActionTarget(4));
            Mission.EndTurn();
            Action.Move.Perform(Player, new ActionTarget(3));
            Mission.EndTurn();
            Action.Move.Perform(Monster, new ActionTarget(3));

            Monster.Effects.Add(Effect.SureHit, 1);
            Action.Disintegrate.Perform(Monster, new ActionTarget(Player));
            Assert.AreEqual(1, Mission.Actors.Count);
        }
    }
}