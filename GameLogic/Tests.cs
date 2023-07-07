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

        // public static void Main(string[] args)
        // {
        //     System.Console.WriteLine("Hi.");
        // }

        [TestMethod]
        public void HappyPathTest()
        {
            // Setup mission
            var mission = new Mission(Graph);
            Assert.AreEqual(6, mission.NumLocations);
            Assert.AreEqual(Team.Human, mission.CurrentTeam);

            // Add some actors
            var player = new Actor(mission, "Player", new CharacterStats(), Team.Human, 0);
            var monster = new Actor(mission, "Monster", new CharacterStats(), Team.AI, 5);
            mission.Actors.Add(player);
            mission.Actors.Add(monster);

            // Should be no APs before mission.Start()
            Assert.AreEqual(0, mission.Actors[0].Resources[Resource.PrimaryAction]);

            mission.Start();

            // Player should have 2AP, monster 0AP
            Assert.AreEqual(2, player.Resources[Resource.PrimaryAction]);
            Assert.AreEqual(0, monster.Resources[Resource.PrimaryAction]);

            // Monster shouldn't be able to act on player turn
            Assert.AreEqual(monster.GetAvailableActions().Count, 0);

            // Player should have two available actions
            Assert.AreEqual(2, player.GetAvailableActions().Count);

            // Move to the same square and punch out of existence.
            Action.Move.Perform(mission, player, new ActionTarget(1));
            mission.EndTurn();
            Action.Move.Perform(mission, monster, new ActionTarget(4));
            mission.EndTurn();
            Action.Move.Perform(mission, player, new ActionTarget(3));
            mission.EndTurn();
            Action.Move.Perform(mission, monster, new ActionTarget(3));

            monster.Effects.Add(Effect.SureHit, 1);
            Action.Disintegrate.Perform(mission, monster, new ActionTarget(player));
            Assert.AreEqual(1, mission.Actors.Count);
        }
    }
}