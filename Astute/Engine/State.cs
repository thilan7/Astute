﻿using System.Collections.Generic;
using System.Linq;
using Astute.Communication.Replies;
using Astute.Entity;

namespace Astute.Engine
{
    public class State
    {
        public State(IEnumerable<Point> brickWalls, IEnumerable<Point> stoneWalls, IEnumerable<Point> waters)
        {
            GridItems = new IGridItem[20, 20];

            BrickWalls = new List<BrickWall>();
            foreach (var point in brickWalls)
            {
                var gridItem = new BrickWall(4, point);
                GridItems[point.X, point.Y] = gridItem;
                BrickWalls.Add(gridItem);
            }

            StoneWalls = new List<StoneWall>();
            foreach (var point in stoneWalls)
            {
                var gridItem = new StoneWall(point);
                GridItems[point.X, point.Y] = gridItem;
                StoneWalls.Add(gridItem);
            }

            Waters = new List<Water>();
            foreach (var point in waters)
            {
                var gridItem = new Water(point);
                GridItems[point.X, point.Y] = gridItem;
                Waters.Add(gridItem);
            }

            Tanks = new List<Tank>();

            Coinpacks = new List<Coinpack>();
            Lifepacks = new List<Lifepack>();
        }

        public IGridItem[,] GridItems { get; }
        public List<BrickWall> BrickWalls { get; }
        public List<StoneWall> StoneWalls { get; }
        public List<Water> Waters { get; }
        public List<Tank> Tanks { get; }
        public List<Coinpack> Coinpacks { get; }
        public List<Lifepack> Lifepacks { get; }


        public void Update(IEnumerable<BroadcastMessage.PlayerDetails> playersDetails,
            IEnumerable<BroadcastMessage.DamageDetails> damagesDetails)
        {
            foreach (var playerDetails in playersDetails)
            {
                var tanks = Tanks.Where(t => t.PlayerNumber == playerDetails.PlayerNumber).ToList();
                Tank tank;

                if (!tanks.Any())
                {
                    tank = new Tank(playerDetails.Location, playerDetails.Health, playerDetails.FacingDirection,
                        playerDetails.Points, playerDetails.Coins, playerDetails.PlayerNumber);
                }
                else
                {
                    tank = tanks.First();
                    GridItems[tank.Location.X, tank.Location.Y] = null;
                }

                tank.Location = playerDetails.Location;
                tank.Health = playerDetails.Health;
                tank.Coins = playerDetails.Coins;
                tank.Direction = playerDetails.FacingDirection;
                tank.Points = playerDetails.Points;

                GridItems[tank.Location.X, tank.Location.Y] = tank;
            }

            foreach (var damageDetails in damagesDetails)
            {
                var brick = BrickWalls.First(b => b.Location == damageDetails.Location);
                brick.Health = 4 - damageDetails.DamageLevel;
                if (brick.Health != 0) continue;

                // Brick is broken. 
                BrickWalls.Remove(brick);
                GridItems[brick.Location.X, brick.Location.Y] = null;
            }

            // Run clock for TimeVariant Items.
            // We do NOT use a client based clock, but rather beleive on the once per second broadcasts. 
            Coinpacks.ForEach(coinpack =>
            {
                if (!coinpack.Tick()) return;
                GridItems[coinpack.Location.X, coinpack.Location.Y] = null;
                Coinpacks.Remove(coinpack);
            });
            Lifepacks.ForEach(lifepack =>
            {
                if (!lifepack.Tick()) return;
                GridItems[lifepack.Location.X, lifepack.Location.Y] = null;
                Lifepacks.Remove(lifepack);
            });
        }

        public void ShowLifepack(Point location, int remainingTime)
        {
            var lifepack = new Lifepack(location, 5, remainingTime);
            Lifepacks.Add(lifepack);
            GridItems[location.X, location.Y] = lifepack;
        }

        public void ShowCoinpack(Point location, int coinValue, int remainingTime)
        {
            var coinpack = new Coinpack(location, coinValue, remainingTime);
            Coinpacks.Add(coinpack);
            GridItems[location.X, location.Y] = coinpack;
        }

        public void SetMyTank(Tank myTank)
        {
            Tanks.Add(myTank);
            GridItems[myTank.Location.X, myTank.Location.Y] = myTank;
        }
    }
}