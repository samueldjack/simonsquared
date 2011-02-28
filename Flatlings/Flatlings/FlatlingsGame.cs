#region License
// This file is part of Simon Squared
// 
// Simon Squared is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Simon Squared is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
// along with Simon Squared. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Flatlings.Animations;
using Flatlings.Messages;
using Flatlings.UI;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace Flatlings
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FlatlingsGame : Microsoft.Xna.Framework.Game, IScreenOrchestrator
    {
        GraphicsDeviceManager graphics;
        private Screen _currentScreen;
        private Dictionary<string, Screen> _screens = new Dictionary<string, Screen>();
        private Dispatcher _dispatcher;

        public FlatlingsGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferMultiSampling = true;
            graphics.IsFullScreen = true;

            TouchPanel.EnabledGestures = GestureType.DragComplete 
                | GestureType.FreeDrag
                | GestureType.DoubleTap
                | GestureType.Tap;

            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Services.AddService(typeof(IScreenOrchestrator), this);

            var messenger = new Messenger();
            Services.AddService(typeof(Messenger), messenger);
            Services.AddService(typeof(MultiplayerGameManager), new MultiplayerGameManager());
            Services.AddService(typeof(GameStateService), new GameStateService());
            _dispatcher = new Dispatcher();
            DispatcherScheduler.SetDispatcher(_dispatcher);
            Services.AddService(typeof(Dispatcher), _dispatcher);

            messenger.Register<TransitionToScreenMessage>(this, HandleTransitionToScreenMessage);
            messenger.Register<StartingMultiplayerGameMessage>(this, message => HandleStartingGameMessage());
            messenger.Register<StartingSinglePlayerGameMessage>(this, message => HandleStartingGameMessage());

            var commonContentManager = new CommonContentManager(this);
            commonContentManager.Initialize();
            Services.AddService(typeof (CommonContentManager), commonContentManager);

            var fontManager = new FontManager(this);
            fontManager.Initialize();
            Services.AddService(typeof (FontManager), fontManager);
            AddScreen("Game", new GameScreen(this));
            AddScreen("Help", new HelpScreen(this));
            AddScreen("MainMenu", new MainMenuScreen(this));
            AddScreen("StartMultiplayer", new StartMultiplayerGameScreen(this));
            AddScreen("WaitingForPlayers", new WaitingForPlayersScreen(this));
            AddScreen("JoinMultiplayer", new JoinMultiplayerGameScreen(this));
            AddScreen("GameStarting", new GameStartingScreen(this));
            AddScreen("Scoreboard", new ScoreboardScreen(this));

            base.Initialize();
        }

        private void HandleStartingGameMessage()
        {
            HandleTransitionToScreenMessage(new TransitionToScreenMessage() { ScreenName = "Game"});
        }

        private void AddScreen(string screenName, Screen sceen)
        {
            _screens.Add(screenName, sceen);
            Components.Add(sceen);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            TransitionToScreen("MainMenu");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            _dispatcher.ProcessQueue();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);
        }

        private void HandleTransitionToScreenMessage(TransitionToScreenMessage message)
        {
            _dispatcher.BeginInvoke(() =>TransitionToScreen(message.ScreenName));
        }

        public void TransitionToScreen(string screenName)
        {
            if (_screens.ContainsKey(screenName))
            {
                var screen = _screens[screenName];
                ActivateScreen(screen);
            }
        }

        private void ActivateScreen(Screen screen)
        {
            if (_currentScreen != null)
            {
                _currentScreen.Enabled = false;
            }

            screen.Enabled = true;

            _currentScreen = screen;
        }
    }
}
