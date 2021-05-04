namespace Paintbot.Tests
{
    using System;
    using System.Linq;
    using AutoFixture;
    using PaintBot.Game.Map;
    using Xunit;
    using Action = PaintBot.Game.Action.Action;

    public class MapUtilTests
    {
        public Fixture _fixture;

        public MapUtilTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnTrue_GivenPlayerInGameAndStayAction()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);
            var playerInGame = map.CharacterInfos.First().Id;

            var result = sut.CanPlayerPerformAction(playerInGame, Action.Stay);

            Assert.True(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnFalse_GivenPlayerNotInGame()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);
            var randomAction = _fixture.Create<Action>();

            var result = sut.CanPlayerPerformAction("player-not-in-game", randomAction);

            Assert.False(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnTrue_GivenExplodeActionAndPlayerCarryingPowerUp()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.CarryingPowerUp = true;

            var result = sut.CanPlayerPerformAction(playerInGame.Id, Action.Explode);

            Assert.True(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnFalse_GivenExplodeActionAndPlayerNotCarryingPowerUp()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.CarryingPowerUp = false;

            var result = sut.CanPlayerPerformAction(playerInGame.Id, Action.Explode);

            Assert.False(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnFalse_GivenLeftMoveWhenPlayerIsAtTheLeftEdge()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.Position = 100;

            var result = sut.CanPlayerPerformAction(playerInGame.Id, Action.Left);

            Assert.False(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnFalse_GivenRightMoveWhenPlayerIsAtTheRightEdge()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.Position = 99;

            var result = sut.CanPlayerPerformAction(playerInGame.Id, Action.Right);

            Assert.False(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnFalse_GivenDownMoveWhenPlayerIsAtTheBottom()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.Position = 10000;

            var result = sut.CanPlayerPerformAction(playerInGame.Id, Action.Down);

            Assert.False(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnFalse_GivenUpMoveWhenPlayerIsAtTheTop()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.Position = 99;

            var result = sut.CanPlayerPerformAction(playerInGame.Id, Action.Up);

            Assert.False(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnTrue_GivenValidLeftMove()
        {
            var map = CreateMap(100, 100);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.Position = 101;
            var sut = new MapUtils(map);

            var result = sut.CanPlayerPerformAction(playerInGame.Id, Action.Left);

            Assert.True(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnTrue_GivenValidRightMove()
        {
            var map = CreateMap(100, 100);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.Position = 101;
            var sut = new MapUtils(map);

            var result = sut.CanPlayerPerformAction(playerInGame.Id, Action.Right);

            Assert.True(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnTrue_GivenValidDownMove()
        {
            var map = CreateMap(100, 100);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.Position = 101;
            var sut = new MapUtils(map);

            var result = sut.CanPlayerPerformAction(playerInGame.Id, Action.Down);

            Assert.True(result);
        }

        [Fact]
        public void CanPerformPlayerAction_ShouldReturnTrue_GivenValidUpMove()
        {
            var map = CreateMap(100, 100);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.Position = 101;
            var sut = new MapUtils(map);

            var result = sut.CanPlayerPerformAction(playerInGame.Id, Action.Up);

            Assert.True(result);
        }

        [Fact]
        public void GetPlayerColoredPositions_ShouldThrowException_GivenPlayerNotInGame()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            Assert.Throws<Exception>(() => sut.GetPlayerColoredPositions("player-not-in-game"));
        }

        [Fact]
        public void GetPlayerColoredPositions_ShouldReturnColoredCoordinates_GivenPlayerInGame()
        {
            var map = CreateMap(100, 100);
            var playerInGame = map.CharacterInfos.First();
            playerInGame.Position = 101;
            playerInGame.ColouredPositions = new[] { 102, 103, 104 };
            var sut = new MapUtils(map);

            var result = sut.GetPlayerColoredPositions(playerInGame.Id);

            Assert.Equal(2, result.First().X);
            Assert.Equal(3, result.ElementAt(1).X);
            Assert.Equal(4, result.Last().X);
            Assert.Equal(1, result.First().Y);
            Assert.Equal(1, result.ElementAt(1).Y);
            Assert.Equal(1, result.Last().Y);
        }

        [Fact]
        public void GetPowerUpCoordinates_ShouldReturnEmptyArray_GivenNoPowerUps()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            var result = sut.GetPowerUpCoordinates();

            Assert.Empty(result);
        }

        [Fact]
        public void GetPowerUpCoordinates_ShouldReturnPowerUpCoordinates_GivenPowerUps()
        {
            var map = CreateMap(100, 100);
            map.PowerUpPositions = new[] {210, 211, 212};
            var sut = new MapUtils(map);

            var result = sut.GetPowerUpCoordinates();

            Assert.Equal(10, result.First().X);
            Assert.Equal(11, result.ElementAt(1).X);
            Assert.Equal(12, result.Last().X);
            Assert.Equal(2, result.First().Y);
            Assert.Equal(2, result.ElementAt(1).Y);
            Assert.Equal(2, result.Last().Y);
        }

        [Fact]
        public void GetObstacleCoordinates_ShouldReturnEmptyArray_GivenNoObstacles()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            var result = sut.GetObstacleCoordinates();

            Assert.Empty(result);
        }

        [Fact]
        public void GetObstacleCoordinates_ShouldReturnObstacleCoordinates_GivenObstacles()
        {
            var map = CreateMap(100, 100);
            map.ObstaclePositions = new[] { 340, 341, 342 };
            var sut = new MapUtils(map);

            var result = sut.GetObstacleCoordinates();

            Assert.Equal(40, result.First().X);
            Assert.Equal(41, result.ElementAt(1).X);
            Assert.Equal(42, result.Last().X);
            Assert.Equal(3, result.First().Y);
            Assert.Equal(3, result.ElementAt(1).Y);
            Assert.Equal(3, result.Last().Y);
        }

        [Fact]
        public void GetCoordinateOf_ShouldThrowException_GivenPlayerIsNotInGame()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            Assert.Throws<Exception>(() => sut.GetCoordinateOf("player-not-in-game"));
        }

        [Fact]
        public void GetCoordinateOf_ShouldReturnPositionOfPlayer_GivenPlayerIsInGame()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);
            var playerInGame = map.CharacterInfos.First();

            var result = sut.GetCoordinateOf(playerInGame.Id);

            Assert.Equal(playerInGame.Position, sut.GetPositionFrom(result));
        }

        [Fact]
        public void GetPositionFrom_ShouldThrowArgumentNullException_GivenNullCoordinate()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            Assert.Throws<ArgumentNullException>(() => sut.GetPositionFrom(null));
        }

        [Fact]
        public void GetTileAt_ShouldThrowArgumentNullException_GivenNullCoordinate()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            Assert.Throws<ArgumentNullException>(() => sut.GetTileAt(null));
        }

        [Fact]
        public void GetTileAt_ShouldThrowException_GivenPositionOutOfBounds()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            Assert.Throws<Exception>(() => sut.GetTileAt(new MapCoordinate(101, 100)));
        }

        [Fact]
        public void GetTileAt_ShouldReturnCorrectTile_GivenValidPosition()
        {
            var map = CreateMap(100, 100);
            const int obstaclePosition = 100;
            const int powerUpPosition = 200;
            const int characterPosition = 300;
            map.ObstaclePositions = new[] {obstaclePosition};
            map.PowerUpPositions = new[] {powerUpPosition};
            var character = _fixture.Create<CharacterInfo>();
            character.Position = characterPosition;
            map.CharacterInfos = new[] {character};
            var sut = new MapUtils(map);

            var actualTileAtObstaclePosition = sut.GetTileAt(obstaclePosition);
            var actualTileAtPowerUpPosition = sut.GetTileAt(powerUpPosition);
            var actualTileAtCharacterPosition = sut.GetTileAt(characterPosition);
            var actualTileAtEmptyPosition = sut.GetTileAt(0);

            Assert.Equal(Tile.Obstacle, actualTileAtObstaclePosition);
            Assert.Equal(Tile.PowerUp, actualTileAtPowerUpPosition);
            Assert.Equal(Tile.Character, actualTileAtCharacterPosition);
            Assert.Equal(Tile.Empty, actualTileAtEmptyPosition);
        }

        [Fact]
        public void IsMovementPossibleToCoordinate_ShouldArgumentNullException_GivenNullCoordinate()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            Assert.Throws<ArgumentNullException>(() => sut.IsMovementPossibleTo(null));
        }

        [Fact]
        public void IsMovementPossibleToCoordinate_ShouldReturnFalse_GivenCoordinateOutOfBounds()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            var result = sut.IsMovementPossibleTo(new MapCoordinate(100, 101));

            Assert.False(result);
        }

        [Fact]
        public void IsMovementPossibleToCoordinate_ShouldReturnFalse_GivenAnotherCharacterIsAtTheGivenCoordinate()
        {
            var map = CreateMap(100, 100);
            var character = _fixture.Create<CharacterInfo>();
            character.Position = 5001;
            map.CharacterInfos = new[] {character};
            var sut = new MapUtils(map);

            var result = sut.IsMovementPossibleTo(new MapCoordinate(1, 50));

            Assert.False(result);
        }

        [Fact]
        public void IsMovementPossibleToCoordinate_ShouldReturnFalse_GivenAnObstacleIsAtTheGivenCoordinate()
        {
            var map = CreateMap(100, 100);
            map.ObstaclePositions = new[] {4005};
            var sut = new MapUtils(map);

            var result = sut.IsMovementPossibleTo(new MapCoordinate(5, 40));

            Assert.False(result);
        }

        [Fact]
        public void IsMovementPossibleToCoordinate_ShouldReturnTrue_GivenTileIsFree()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            var result = sut.IsMovementPossibleTo(new MapCoordinate(1, 1));

            Assert.True(result);
        }

        [Fact]
        public void IsMovementPossibleToPosition_ShouldReturnFalse_GivenPositionIsOutOfBounds()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            var result = sut.IsMovementPossibleTo(10001);

            Assert.False(result);
        }

        [Fact]
        public void IsCoordinateOutOfBounds_ShouldThrowArgumentNullException_GivenNullCoordinate()
        {
            var map = CreateMap(100, 100);
            var sut = new MapUtils(map);

            Assert.Throws<ArgumentNullException>(() => sut.IsCoordinateOutOfBounds(null));
        }

        [Fact]
        public void MapUtilsConstructor_ShouldNotThrowException_GivenNullPositions()
        {
            var map = _fixture.Create<Map>();
            map.PowerUpPositions = null;
            map.ObstaclePositions = null;

            new MapUtils(map);
        }

        private Map CreateMap(int width, int height)
        {
            var map = _fixture.Create<Map>();
            map.Width = width;
            map.Height = height;
            map.ObstaclePositions = Array.Empty<int>();
            map.PowerUpPositions = Array.Empty<int>();
            var character = _fixture.Create<CharacterInfo>();
            character.Position = width;
            map.CharacterInfos = new[] {character};

            return map;
        }
    }
}