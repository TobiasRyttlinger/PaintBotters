namespace PaintBot
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using System.Runtime;
    using Game.Action;
    using Game.Configuration;
    using Game.Map;
    using Messaging;
    using Messaging.Request.HeartBeat;
    using Messaging.Response;
    using Serilog;

    public class MyPaintBot : PaintBot
    {
        private IMapUtils _mapUtils;

        public int counter;
        public MyPaintBot(PaintBotConfig paintBotConfig, IPaintBotClient paintBotClient, IHearBeatSender hearBeatSender, ILogger logger) :
            base(paintBotConfig, paintBotClient, hearBeatSender, logger)
        {
            GameMode = paintBotConfig.GameMode;
            Name = paintBotConfig.Name ?? "Båtersh";
            counter = 0;
        }

        public override GameMode GameMode { get; }

        public override string Name { get; }

        public MapCoordinate getClosestPowerUp(MapCoordinate myCoord)
        {
            var powerUpCoordinates = _mapUtils.GetPowerUpCoordinates();
            var prevDistancePow = 9999;

            var closestPowerUp = new MapCoordinate(999, 999);
            var distanceToClosestPowerUp = 9999;

            if (powerUpCoordinates.Length != 0)
            {
                prevDistancePow = 9999;
                foreach (var PowerUpCord in powerUpCoordinates)
                {
                    distanceToClosestPowerUp = myCoord.GetManhattanDistanceTo(PowerUpCord);

                    if (distanceToClosestPowerUp < prevDistancePow)
                    {
                        prevDistancePow = distanceToClosestPowerUp;
                        closestPowerUp = PowerUpCord;
                    }
                }
            }
            return closestPowerUp;
        }

        public MapCoordinate GetClosestPlayerWithPowerUp(MapCoordinate myCoord, CharacterInfo self, CharacterInfo[] playerInfos)
        {

            var distanceToClosestPlayerWithPowerUp = 999;
            var prevDistancePlayerPow = 999;
            var closestPlayerWithPowerUp = new MapCoordinate(999, 999);
            foreach (var info in playerInfos)
            {
                if (info.Id == self.Id)
                {
                    continue;
                }


                if (info.CarryingPowerUp)
                {
                    distanceToClosestPlayerWithPowerUp = myCoord.GetManhattanDistanceTo(_mapUtils.GetCoordinateFrom(info.Position));

                    if (distanceToClosestPlayerWithPowerUp < prevDistancePlayerPow)
                    {
                        prevDistancePlayerPow = distanceToClosestPlayerWithPowerUp;
                        closestPlayerWithPowerUp = _mapUtils.GetCoordinateFrom(info.Position);
                    }

                }

            }
            return closestPlayerWithPowerUp;
        }



        public MapCoordinate GetClosestPlayerWithoutPowerUp(MapCoordinate myCoord, CharacterInfo self, CharacterInfo[] playerInfos)
        {

            var distanceToClosestPlayerWithPowerUp = 999;
            var prevDistancePlayerPow = 999;
            var closestPlayerWithPowerUp = new MapCoordinate(999, 999);
            foreach (var info in playerInfos)
            {
                if (info.Id == self.Id)
                {
                    continue;
                }


                if (info.CarryingPowerUp)
                {
                    distanceToClosestPlayerWithPowerUp = myCoord.GetManhattanDistanceTo(_mapUtils.GetCoordinateFrom(info.Position));

                    if (distanceToClosestPlayerWithPowerUp < prevDistancePlayerPow)
                    {
                        prevDistancePlayerPow = distanceToClosestPlayerWithPowerUp;
                        closestPlayerWithPowerUp = _mapUtils.GetCoordinateFrom(info.Position);
                    }

                }

            }
            return closestPlayerWithPowerUp;
        }

        public override Action GetAction(MapUpdated mapUpdated)
        {
            _mapUtils = new MapUtils(mapUpdated.Map); // Keep this

            // Implement your bot here!

            //1. Find Closest powerup
            //2. move to closest powerup
            //3. Move three more steps.
            //



            // The following is a simple example bot. It tries to
            // 1. Explode PowerUp
            // 2. Move to a tile that it is not currently owning
            // 3. Move in the direction where it can move for the longest time. 

            var directions = new List<Action> { Action.Down, Action.Right, Action.Left, Action.Up };
            var playerInfos = mapUpdated.Map.CharacterInfos;
            var myCharacter = _mapUtils.GetCharacterInfoFor(mapUpdated.ReceivingPlayerId);
            var myCoordinate = _mapUtils.GetCoordinateFrom(myCharacter.Position);
            var myColouredTiles = _mapUtils.GetCoordinatesFrom(myCharacter.ColouredPositions);

            var Pits = _mapUtils.GetObstacleCoordinates();

            var powerUpCoordinates = _mapUtils.GetPowerUpCoordinates();
            var prevDistancePow = 999999;
            var distanceToClosestPowerUp = 999;
            var validActionsThatPaintsNotOwnedTile = directions.Where(dir =>
                            !myColouredTiles.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();


            var distanceToClosestPlayerWithPowerUp = 999;
            var closestPlayerWithoutPowerUp = new MapCoordinate(999, 999);
            var closestPlayerWithPowerUp = GetClosestPlayerWithPowerUp(myCoordinate, myCharacter, playerInfos);

            distanceToClosestPlayerWithPowerUp = myCoordinate.GetManhattanDistanceTo(closestPlayerWithPowerUp);

            var closestPowerUp = new MapCoordinate(999, 999);

            if (distanceToClosestPlayerWithPowerUp < 5)
            {

                var BestAction = Action.Up;
                var previousActionDistance = 9999;
                validActionsThatPaintsNotOwnedTile = directions.Where(dir => 
                !Pits.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();

                foreach (var Action in validActionsThatPaintsNotOwnedTile)
                {
                    var TestCord = myCoordinate.MoveIn(Action);
                    if (_mapUtils.GetTileAt(TestCord) == Tile.Obstacle || _mapUtils.GetTileAt(TestCord) == Tile.Character)
                    {
                        continue;
                    }

                    if (TestCord.GetManhattanDistanceTo(closestPlayerWithPowerUp) > 5)
                    {

                        if (TestCord.GetManhattanDistanceTo(closestPlayerWithPowerUp) > previousActionDistance)
                        {

                            previousActionDistance = TestCord.GetManhattanDistanceTo(closestPlayerWithPowerUp);
                            BestAction = Action;
                        }

                    }

                }

                return BestAction;


            }


            if (myCharacter.CarryingPowerUp)
            {
                var closestPlayer = GetClosestPlayerWithoutPowerUp(myCoordinate, myCharacter, playerInfos);
                var distanceToClosestPlayerNoPowerUp = myCoordinate.GetManhattanDistanceTo(closestPlayer);
                
                closestPowerUp = getClosestPowerUp(myCoordinate);
                distanceToClosestPowerUp = myCoordinate.GetManhattanDistanceTo(closestPowerUp);

                return Action.Explode;

                if (distanceToClosestPowerUp < 5 || distanceToClosestPlayerNoPowerUp < 5 )
                {
                    return Action.Explode;
                }
                else
                {
                  
                    var BestActionPlayer = Action.Down;


                    validActionsThatPaintsNotOwnedTile = directions.Where(dir =>
                     !Pits.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();


                    var previousActionDistancePlayer = 9999;

                    foreach (var Action in validActionsThatPaintsNotOwnedTile)
                    {
                        var TestCord = myCoordinate.MoveIn(Action);
                        if (_mapUtils.GetTileAt(TestCord) == Tile.Obstacle || _mapUtils.GetTileAt(TestCord) == Tile.Character)
                        {
                            continue;
                        }
                        var distanceToPlayer = TestCord.GetManhattanDistanceTo(closestPlayerWithoutPowerUp);

                        if (distanceToClosestPlayerNoPowerUp <= distanceToClosestPowerUp)
                        {

                            if (TestCord.GetManhattanDistanceTo(closestPlayer) < distanceToClosestPlayerNoPowerUp)
                            {
                                if (TestCord.GetManhattanDistanceTo(closestPlayer) < previousActionDistancePlayer)
                                {
                                    previousActionDistancePlayer = TestCord.GetManhattanDistanceTo(closestPlayer);
                                    BestActionPlayer = Action;
                                }
                            }
                        }
                        else
                        {
                            if (TestCord.GetManhattanDistanceTo(closestPowerUp) < distanceToClosestPowerUp)
                            {
                                if (TestCord.GetManhattanDistanceTo(closestPowerUp) < previousActionDistancePlayer)
                                {
                                    previousActionDistancePlayer = TestCord.GetManhattanDistanceTo(closestPowerUp);
                                    BestActionPlayer = Action;
                                }
                            }
                        }
                    }
                    return BestActionPlayer;
                }
            }
            else
            {

                closestPowerUp = getClosestPowerUp(myCoordinate);
                distanceToClosestPowerUp = myCoordinate.GetManhattanDistanceTo(closestPowerUp);

                if (powerUpCoordinates.Length > 0)
                {
                    //Move to closest power up
                    validActionsThatPaintsNotOwnedTile = directions.Where(dir =>
               !Pits.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();

                    var BestAction = Action.Left;
                    var previousActionDistance = 9999;
                    foreach (var Action in validActionsThatPaintsNotOwnedTile)
                    {
                        var TestCord = myCoordinate.MoveIn(Action);
                        if (_mapUtils.GetTileAt(TestCord) == Tile.Obstacle || _mapUtils.GetTileAt(TestCord) == Tile.Character)
                        {
                            continue;
                        }

                        var TestDistance = TestCord.GetManhattanDistanceTo(closestPowerUp);
                        if (TestDistance < distanceToClosestPowerUp)
                        {
                            if (TestDistance < previousActionDistance)
                            {

                                previousActionDistance = TestDistance;
                                Debug.WriteLine(previousActionDistance);
                                BestAction = Action;
                            }

                        }

                    }


                    return BestAction;
                }
                else
                {
                    validActionsThatPaintsNotOwnedTile = directions.Where(dir =>
            !Pits.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();
                    var actionen = Action.Down;
                    var distanceToCenter = 9999;
                    foreach (var Action in validActionsThatPaintsNotOwnedTile)
                    {
                        var TestCord = myCoordinate.MoveIn(Action);

                        if (_mapUtils.GetTileAt(TestCord) == Tile.Obstacle || _mapUtils.GetTileAt(TestCord) == Tile.Character)
                        {
                            continue;
                        }
                        var TestDistance = TestCord.GetManhattanDistanceTo(new MapCoordinate(mapUpdated.Map.Width / 2, mapUpdated.Map.Height / 2));
                        if (TestDistance < 3)
                        {
                            validActionsThatPaintsNotOwnedTile = directions.Where(dir =>
               !Pits.Contains(myCoordinate.MoveIn(dir)) && !myColouredTiles.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();
                            if (validActionsThatPaintsNotOwnedTile.Any())
                            {
                                return validActionsThatPaintsNotOwnedTile.First();
                            }
                        }
                        if (TestDistance < distanceToCenter)
                        {
                            if (TestDistance < distanceToCenter)
                            {

                                distanceToCenter = TestDistance;
                                Debug.WriteLine(distanceToCenter);
                                actionen = Action;
                            }

                        }



                    }
                    return actionen;

                }
            }
        }

    }




}
