namespace Paintbot.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Helpers;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using PaintBot;
    using PaintBot.Game.Configuration;
    using PaintBot.Messaging;
    using PaintBot.Messaging.Request;
    using PaintBot.Messaging.Request.HeartBeat;
    using PaintBot.Messaging.Response;
    using Serilog;
    using Xunit;

    public class PaintBotTests
    {
        private readonly Fixture _fixture;
        private readonly PaintBotConfig _config;

        public PaintBotTests()
        {
            _fixture = new Fixture();
            _config = new PaintBotConfig("testbot", GameMode.Training, 20);
        }

        [Fact]
        public async Task Run_ShouldLogExceptionAndCloseClient_WhenExceptionIsThrown()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            client.ConnectAsync(Arg.Any<GameMode>(), Arg.Any<CancellationToken>()).Throws(new Exception());
            var sut = new FakeBot(client, heartBeatSender, logger, _config);

            await sut.Run(CancellationToken.None);

            logger.Received(1).Error(Arg.Any<Exception>(), Arg.Any<string>());
            client.Received(1).Close();
        }

        [Fact]
        public async Task Run_ShouldConnectToClient_WhenCalled()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);

            await sut.Run(CancellationToken.None);

            await client.Received(1).ConnectAsync(sut.GameMode, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Run_ShouldRegisterPlayer_WhenCalled()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);

            await sut.Run(CancellationToken.None);

            await client.Received(1).SendAsync(Arg.Is<RegisterPlayer>(registerPlayer => registerPlayer.PlayerName == sut.Name),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Run_ShouldReceiveEventsFromClient_WhenCalled()
        {
            var client = Substitute.For<IPaintBotClient>();
            var logger = Substitute.For<ILogger>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);

            await sut.Run(CancellationToken.None);

            await foreach (var _ in client.Received(1).ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>())) { }
        }

        [Fact]
        public async Task Run_ShouldSendHeartBeat_OnPlayerRegisteredEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);
            var playerRegisteredEvent = _fixture.Create<PlayerRegistered>();
            var events = new List<Response>
            {
                playerRegisteredEvent
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            heartBeatSender.Received(1).SendHeartBeatFrom(playerRegisteredEvent.ReceivingPlayerId);
        }

        [Fact]
        public async Task Run_ShouldStartGame_OnPlayerRegisteredEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);
            var playerRegisteredEvent = _fixture.Create<PlayerRegistered>();
            playerRegisteredEvent.ReceivingPlayerId = _fixture.Create<Guid>().ToString();
            var events = new List<Response>
            {
                playerRegisteredEvent
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            await client.Received(1).SendAsync(Arg.Is<StartGame>(startGame => startGame.ReceivingPlayerId.Value.ToString() == playerRegisteredEvent.ReceivingPlayerId),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Run_ShouldSendBotMove_OnMapUpdatedEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            var logger = Substitute.For<ILogger>();
            var mapUpdatedEvent = _fixture.Create<MapUpdated>();
            mapUpdatedEvent.ReceivingPlayerId = _fixture.Create<Guid>().ToString();
            var events = new List<Response>
            {
                mapUpdatedEvent
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);

            await sut.Run(CancellationToken.None);

            await client.Received(1).SendAsync(Arg.Is<RegisterMove>(registerMove => 
                    registerMove.ReceivingPlayerId.Value.ToString() == mapUpdatedEvent.ReceivingPlayerId && 
                    registerMove.GameId == mapUpdatedEvent.GameId && 
                    registerMove.GameTick == mapUpdatedEvent.GameTick),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Run_ShouldLogGameLinkEvent_OnGameLinkEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);
            var gameLinkEvent = _fixture.Create<GameLink>();
            var events = new List<Response>
            {
                gameLinkEvent
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            logger.Received(1).Information(Arg.Is<string>(s => s.Contains(gameLinkEvent.Url)));
        }

        [Fact]
        public async Task Run_ShouldLogGameStartingEvent_OnGameStartingEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);
            var gameStartingEvent = _fixture.Create<GameStarting>();
            var events = new List<Response>
            {
                gameStartingEvent
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            logger.Received(1).Information(Arg.Is<string>(s => s.Contains(gameStartingEvent.GameId.ToString())));
        }

        [Fact]
        public async Task Run_ShouldLogGameResultEvent_WhenGameModeIsTraining()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);
            var events = new List<Response>
            {
                _fixture.Create<GameResult>()
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            logger.Received(1).Information(Arg.Any<string>());
        }

        [Fact]
        public async Task Run_ShouldNotLogGameResultEvent_WhenGameModeIsTournament()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, new PaintBotConfig("testbot", GameMode.Tournament, 20));
            var events = new List<Response>
            {
                _fixture.Create<GameResult>()
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            logger.DidNotReceive().Information(Arg.Any<string>());
        }

        [Fact]
        public async Task Run_ShouldLogCharacterStunnedEvent_OnCharacterStunnedEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);
            var characterStunnedEvent = _fixture.Create<CharacterStunned>();
            var events = new List<Response>
            {
                characterStunnedEvent
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            logger.Received(1).Information(Arg.Is<string>(s => s.Contains(characterStunnedEvent.GameId.ToString())));
        }

        [Fact]
        public async Task Run_ShouldSendHeartBeat_OnHeartBeatEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);
            var heartBeatEvent = _fixture.Create<HeartBeatResponse>();
            var playerRegisteredEvent = _fixture.Create<PlayerRegistered>();
            var events = new List<Response>
            {
                playerRegisteredEvent,
                heartBeatEvent
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            heartBeatSender.Received(2).SendHeartBeatFrom(playerRegisteredEvent.ReceivingPlayerId);
        }

        [Fact]
        public async Task Run_ShouldLogWinner_WhenGameEndedAndGameModeIsTraining()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, _config);
            var events = new List<Response>
            {
                _fixture.Create<GameEnded>()
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            logger.Received(1).Information(Arg.Is<string>(s => s.Contains("won!")));
        }

        [Fact]
        public async Task Run_ShouldNotLogWinnerAndShouldCloseClient_WhenGameEndedAndGameModeIsTournament()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, new PaintBotConfig("testbot", GameMode.Tournament, 20));
            var events = new List<Response>
            {
                _fixture.Create<GameEnded>()
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            logger.DidNotReceive().Information(Arg.Is<string>(s => s.Contains("won!")));
            client.Received(1).Close();
        }

        [Fact]
        public async Task Run_ShouldLogTournamentEndedAndCloseClient_OnTournamentEndedEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, new PaintBotConfig("testbot", GameMode.Tournament, 20));
            var events = new List<Response>
            {
                _fixture.Create<TournamentEnded>()
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            logger.Received(1).Information(Arg.Is<string>(s => s.Contains("The tournament has ended")));
            client.Received(1).Close();
        }

        [Fact]
        public async Task Run_ShouldLogReason_OnInvalidPlayerEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var logger = Substitute.For<ILogger>();
            var sut = new FakeBot(client, heartBeatSender, logger, new PaintBotConfig("testbot", GameMode.Tournament, 20));
            var invalidPlayerEvent = _fixture.Create<InvalidPlayerName>();
            var events = new List<Response>
            {
                invalidPlayerEvent
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));

            await sut.Run(CancellationToken.None);

            logger.Received(1).Information(Arg.Is<string>(s => s.Contains(invalidPlayerEvent.ReasonCode.ToString())));
        }

        private static async IAsyncEnumerable<Response> GetTestValues(IEnumerable<Response> events)
        {
            foreach (var e in events) yield return e;

            await Task.CompletedTask;
        }
    }
}