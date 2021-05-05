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
        public Action LastAction;
        public Action LastLastAction;
        public MyPaintBot(PaintBotConfig paintBotConfig, IPaintBotClient paintBotClient, IHearBeatSender hearBeatSender, ILogger logger) :
            base(paintBotConfig, paintBotClient, hearBeatSender, logger)
        {
            GameMode = paintBotConfig.GameMode;
            Name = paintBotConfig.Name ?? "Båtersh";
            LastAction = Action.Stay;
            LastLastAction = Action.Down;
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

        public MapCoordinate GetClosestPit(MapCoordinate myCoord, MapCoordinate[] pits)
        {
         
  

            var ClosestPit = new MapCoordinate(999, 999);
            var distanceToClosestPit = 9999;

            if (pits.Length != 0)
            {
                var prevDistancePow = 9999;
                foreach (var pit in pits)
                {
                    distanceToClosestPit = myCoord.GetManhattanDistanceTo(pit);

                    if (distanceToClosestPit < prevDistancePow)
                    {
                        prevDistancePow = distanceToClosestPit;
                        ClosestPit = pit;
                    }
                }
            }
            return ClosestPit;

        }


        public MapCoordinate GetClosestPlayerWithoutPowerUp(MapCoordinate myCoord, CharacterInfo self, CharacterInfo[] playerInfos)
        {

            var distanceToClosestPlayerWithoutPowerUp = 0;
            var prevDistancePlayerPow = 999;
            var closestPlayerWithPowerUp = new MapCoordinate(999, 999);
            foreach (var info in playerInfos)
            {
                if (info.Id == self.Id)
                {
                    continue;
                }

                distanceToClosestPlayerWithoutPowerUp = myCoord.GetManhattanDistanceTo(_mapUtils.GetCoordinateFrom(info.Position));

                if (distanceToClosestPlayerWithoutPowerUp < prevDistancePlayerPow)
                {
                    prevDistancePlayerPow = distanceToClosestPlayerWithoutPowerUp;
                    closestPlayerWithPowerUp = _mapUtils.GetCoordinateFrom(info.Position);
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
            var distanceToclosestPlayerWithoutPowerUp = myCoordinate.GetManhattanDistanceTo(closestPlayerWithoutPowerUp);
            distanceToClosestPlayerWithPowerUp = myCoordinate.GetManhattanDistanceTo(closestPlayerWithPowerUp);

            var closestPowerUp = new MapCoordinate(999, 999);




            if (distanceToClosestPlayerWithPowerUp < 6 || distanceToclosestPlayerWithoutPowerUp < 2)
            {
                // Debug.WriteLine("Danger:" + distanceToClosestPlayerWithPowerUp);
                var BestAction = Action.Up;
                var previousActionDistance = distanceToClosestPlayerWithPowerUp;

                validActionsThatPaintsNotOwnedTile = directions.Where(dir =>
                !Pits.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();

                foreach (var Action in validActionsThatPaintsNotOwnedTile)
                {
                    var TestCord = myCoordinate.MoveIn(Action);
                    var ActionDistance = TestCord.GetManhattanDistanceTo(closestPlayerWithPowerUp);
                    if (_mapUtils.GetTileAt(TestCord) == Tile.Obstacle || _mapUtils.GetTileAt(TestCord) == Tile.Character)
                    {
                        if (Action == Action.Up)
                        {
                            return BestAction = Action.Down;
                        }
                        if (Action == Action.Down)
                        {
                            return BestAction = Action.Up;
                        }
                        if (Action == Action.Left)
                        {
                            return BestAction = Action.Right;
                        }
                        if (Action == Action.Right)
                        {
                            return BestAction = Action.Left;
                        }
                        continue;
                    }

                    if (ActionDistance > 5)
                    {

                        if (ActionDistance > previousActionDistance)
                        {

                            previousActionDistance = TestCord.GetManhattanDistanceTo(closestPlayerWithPowerUp);
                            BestAction = Action;
                        }

                    }

                }
                // Debug.WriteLine("Safe:" + distanceToClosestPlayerWithPowerUp);
                return BestAction;


            }


            if (myCharacter.CarryingPowerUp)
            {
                var closestPlayer = GetClosestPlayerWithoutPowerUp(myCoordinate, myCharacter, playerInfos);
                var distanceToClosestPlayerNoPowerUp = myCoordinate.GetManhattanDistanceTo(closestPlayer);

                closestPowerUp = getClosestPowerUp(myCoordinate);
                distanceToClosestPowerUp = myCoordinate.GetManhattanDistanceTo(closestPowerUp);


                if (distanceToClosestPowerUp < 5 || distanceToClosestPlayerNoPowerUp < 5)
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

                if (powerUpCoordinates.Length > 0 || distanceToClosestPowerUp < 30)
                {
                    var ClosestPit = GetClosestPit(myCoordinate, Pits);
                    var distanceToClosestPit = myCoordinate.GetManhattanDistanceTo(ClosestPit);
                    validActionsThatPaintsNotOwnedTile = directions.Where(dir =>
                  !Pits.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();

                    if (distanceToClosestPit < 1 )
                    {
                        validActionsThatPaintsNotOwnedTile = directions.Where(dir =>
                      !Pits.Contains(myCoordinate.MoveIn(dir)) && !myColouredTiles.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();
                    }
                   

                    var BestAction = Action.Right;
                    var previousActionDistance = 9999;
                    foreach (var Action in validActionsThatPaintsNotOwnedTile)
                    {
                        var TestCord = myCoordinate.MoveIn(Action);


                        var TestDistance = TestCord.GetManhattanDistanceTo(closestPowerUp);

                        if (TestDistance < previousActionDistance)
                        {

                            previousActionDistance = TestDistance;
                            Debug.WriteLine(previousActionDistance);
                            BestAction = Action;
                        }


                        

                    }
                    if (distanceToclosestPlayerWithoutPowerUp <= 2 && distanceToClosestPowerUp < 2)
                    {
                        if (BestAction == Action.Up)
                        {
                            return Action.Down;
                        }
                        if (BestAction == Action.Down)
                        {
                            return Action.Up;
                        }
                        if (BestAction == Action.Left)
                        {
                            return Action.Right;
                        }
                        if (BestAction == Action.Right)
                        {
                            return Action.Left;
                        }

                    }
                    if (_mapUtils.GetTileAt(myCoordinate.MoveIn(BestAction)) == Tile.Obstacle/* || _mapUtils.GetTileAt(TestCord) == Tile.Character*/)
                    {
                        if (BestAction == Action.Up)
                        {
                            return Action.Right;
                        }
                        if (BestAction == Action.Down)
                        {
                            return Action.Left;
                        }
                        if (BestAction == Action.Left)
                        {
                            return Action.Up;
                        }
                        if (BestAction == Action.Right)
                        {
                            return Action.Down;
                        }
                        
                    }

                    return BestAction;
                }
                else
                {

                    validActionsThatPaintsNotOwnedTile = directions.Where(dir =>
             !myColouredTiles.Contains(myCoordinate.MoveIn(dir)) && !Pits.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();


                    var BestAction = Action.Left;
                    var distanceToCenter = 9999;
                    foreach (var Action in validActionsThatPaintsNotOwnedTile)
                    {
                        var TestCord = myCoordinate.MoveIn(Action);

                        //if (_mapUtils.GetTileAt(TestCord) == Tile.Obstacle || _mapUtils.GetTileAt(TestCord) == Tile.Character)
                        //{
                        //    if (Action == Action.Up)
                        //    {
                        //        return BestAction = Action.Down;
                        //    }
                        //    if (Action == Action.Down)
                        //    {
                        //        return BestAction = Action.Up;
                        //    }
                        //    if (Action == Action.Left)
                        //    {
                        //        return BestAction = Action.Right;
                        //    }
                        //    if (Action == Action.Right)
                        //    {
                        //        return BestAction = Action.Left;
                        //    }
                        //    continue;
                        //}
                        var TestDistance = TestCord.GetManhattanDistanceTo(new MapCoordinate(mapUpdated.Map.Width / 2, mapUpdated.Map.Height / 2));
                        if (TestDistance <= 3)
                        {

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

                                BestAction = Action;
                            }

                        }



                    }
                    return BestAction;

                }
            }
        }

    }




}
