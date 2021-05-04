namespace Paintbot.Tests
{
    using System;
    using AutoFixture;
    using PaintBot.Game.Map;
    using Xunit;
    using Action = PaintBot.Game.Action.Action;

    public class MapCoordinateTests
    {
        private readonly Fixture _fixture;

        public MapCoordinateTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void MoveIn_ShouldChangeCoordinateCorrectly_WhenCalledWithDownAction()
        {
            var x = _fixture.Create<int>();
            var y = _fixture.Create<int>();
            var sut = new MapCoordinate(x, y);

            var result = sut.MoveIn(Action.Down);

            Assert.Equal(x, result.X);
            Assert.Equal(y + 1, result.Y);
        }

        [Fact]
        public void MoveIn_ShouldChangeCoordinateCorrectly_WhenCalledWithUpAction()
        {
            var x = _fixture.Create<int>();
            var y = _fixture.Create<int>();
            var sut = new MapCoordinate(x, y);

            var result = sut.MoveIn(Action.Up);

            Assert.Equal(x, result.X);
            Assert.Equal(y - 1, result.Y);
        }

        [Fact]
        public void MoveIn_ShouldChangeCoordinateCorrectly_WhenCalledWithLeftAction()
        {
            var x = _fixture.Create<int>();
            var y = _fixture.Create<int>();
            var sut = new MapCoordinate(x, y);

            var result = sut.MoveIn(Action.Left);

            Assert.Equal(x - 1, result.X);
            Assert.Equal(y, result.Y);
        }

        [Fact]
        public void MoveIn_ShouldChangeCoordinateCorrectly_WhenCalledWithRightAction()
        {
            var x = _fixture.Create<int>();
            var y = _fixture.Create<int>();
            var sut = new MapCoordinate(x, y);

            var result = sut.MoveIn(Action.Right);

            Assert.Equal(x + 1, result.X);
            Assert.Equal(y, result.Y);
        }

        [Fact]
        public void MoveIn_ShouldNotChangeCoordinate_WhenCalledWithStayAction()
        {
            var x = _fixture.Create<int>();
            var y = _fixture.Create<int>();
            var sut = new MapCoordinate(x, y);

            var result = sut.MoveIn(Action.Stay);

            Assert.Equal(x, result.X);
            Assert.Equal(y, result.Y);
        }

        [Fact]
        public void MoveIn_ShouldNotChangeCoordinate_WhenCalledWithExplodeAction()
        {
            var x = _fixture.Create<int>();
            var y = _fixture.Create<int>();
            var sut = new MapCoordinate(x, y);

            var result = sut.MoveIn(Action.Explode);

            Assert.Equal(x, result.X);
            Assert.Equal(y, result.Y);
        }

        [Fact]
        public void GetManhattanDistanceTo_ShouldReturnCorrectDistance_WhenCalledWithValidCoordinate()
        {
            var sutX = _fixture.Create<int>();
            var sutY = _fixture.Create<int>();
            var sut = new MapCoordinate(sutX, sutY);
            var otherX = _fixture.Create<int>();
            var otherY = _fixture.Create<int>();
            var otherCoordinate = new MapCoordinate(otherX, otherY);

            var result = sut.GetManhattanDistanceTo(otherCoordinate);

            var expected = Math.Abs(sutX - otherX) + Math.Abs(sutY - otherY);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetManhattanDistanceTo_ShouldReturnZero_WhenCalledWithTheSameCoordinate()
        {
            var sutX = _fixture.Create<int>();
            var sutY = _fixture.Create<int>();
            var sut = new MapCoordinate(sutX, sutY);

            var result = sut.GetManhattanDistanceTo(sut);

            Assert.Equal(0, result);
        }

        [Fact]
        public void GetManhattanDistanceTo_ShouldThrowArgumentNullException_WhenCalledWithNullCoordinate()
        {
            var sut = _fixture.Create<MapCoordinate>();

            Assert.Throws<ArgumentNullException>(() => sut.GetManhattanDistanceTo(null));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenCalledWithNull()
        {
            var sut = _fixture.Create<MapCoordinate>();

            var result = sut.Equals(null);

            Assert.False(result);
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenCalledWithEqualReference()
        {
            var sut = _fixture.Create<MapCoordinate>();

            var result = sut.Equals(sut);

            Assert.True(result);
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenCalledWithAnotherObjectType()
        {
            var sut = _fixture.Create<MapCoordinate>();

            var result = sut.Equals(new Tile());

            Assert.False(result);
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenCalledWithCoordinateWithSameXAndY()
        {
            var sut = _fixture.Create<MapCoordinate>();
            var otherCoordinate = new MapCoordinate(sut.X, sut.Y);

            var result = sut.Equals(otherCoordinate);

            Assert.True(result);
        }

        [Fact]
        public void GetHashCode_ShouldReturnTheSameHashCode_WhenCalledWithEqualCoordinates()
        {
            var sut = _fixture.Create<MapCoordinate>();
            var otherCoordinate = new MapCoordinate(sut.X, sut.Y);

            var sutResult = sut.GetHashCode();
            var otherCoordinateResult = otherCoordinate.GetHashCode();

            Assert.Equal(sutResult, otherCoordinateResult);
        }

        [Fact]
        public void GetHashCode_ShouldReturnTheDifferentHashCode_WhenCalledWithDifferentCoordinates()
        {
            var sut = _fixture.Create<MapCoordinate>();
            var otherCoordinate = new MapCoordinate(sut.X + 1, sut.Y);

            var sutResult = sut.GetHashCode();
            var otherCoordinateResult = otherCoordinate.GetHashCode();

            Assert.NotEqual(sutResult, otherCoordinateResult);
        }
    }
}