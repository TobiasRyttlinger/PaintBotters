namespace Paintbot.Tests.Helpers
{
    using PaintBot;
    using PaintBot.Game.Action;
    using PaintBot.Game.Configuration;
    using PaintBot.Messaging;
    using PaintBot.Messaging.Request.HeartBeat;
    using PaintBot.Messaging.Response;
    using Serilog;

    public class FakeBot : PaintBot
    {
        public FakeBot(IPaintBotClient paintBotClient, IHearBeatSender heartBeatSender, ILogger logger, 
            PaintBotConfig config) : base(config, paintBotClient, heartBeatSender, logger)
        {
            GameMode = config.GameMode;
        }

        public override GameMode GameMode { get; }
        public override string Name => "FakeBot";

        public override Action GetAction(MapUpdated mapUpdated)
        {
            return Action.Stay;
        }
    }
}