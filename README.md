# paintbot-client-csharp

Welcome to the Paintbot C# Client. This is where you write awesome bots for the game Paintbot using C#
## Prerequisites
 - Make sure to have .NET 5 SDK installed. (It can be downloaded here https://dotnet.microsoft.com/download/dotnet/5.0)
## Getting started
 - Clone this repository
 - Open the project in whatever editor you feel comfortable with
 - Navigate to the `MyPaintBot.cs` class. In there, there's a `GetAction` method. This is the *only* method that you'll need to  change. As of now, there's a very simple bot in there which just acts as an example of what a bot could look like. You'll replace the content of this method with your own logic. Your goal is to take an action based on the current state of the board. Take a look at the `Action.cs` enum to get an idea of the possible actions. 
 - Take a look at the `IMapUtils` interface. This contains methods that let's you gather information about the current state of the game board and can be used as a basis of your decision on which action to take. We have created an instance of the `MapUtils` class at the top of the `GetAction` method for you.
 - Start writing your bot! 
## Running a game
 To get feedback on how your bot is faring you can run a game with it. 
### Game modes  
 - **Training** This lets you play through one game with your bot against random pre-defined bots. This mode is great to use during development.
 - **Tournament**.  This mode lets you compete with other bots in a tournament.

### Command line arguments
 Paintbot accepts three commandline arguments
   1. **Bot name** _REQUIRED_
   2. **Game mode** _OPTIONAL_ Default: Training
   3. **Game duration in seconds** _OPTIONAL_ Default: 180. Only applicable to training games

Example: `dotnet run SomeBot training 20` would start a 20 second long training game with the bot name SomeBot

### Running from commandline
1. From the repository root folder, CD into the `Painbot` folder
2. Run `dotnet run` followed by the command line arguments of your choice. 

### Run from Visual Studio
1. Right click the `Paintbot` project and choose `Properties`.
2. Go to the `Debug` tab. Add the command line arguments in the `Application Arguments`text area.

## Pitfalls
Beware the common mishaps:
- If two bots try to move to the same empty space, they will collide and stun each other. Once the stun ends, they risk doing the same thing again. And again, and again. Don't be the bot who runs into another bot the whole game! (`CanPlayerPerformAction` or `IsMovementPossibleTo` won't help you here, they only check if the tile is currently empty - which it is!)
