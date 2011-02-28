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
using System.Text;
using Flatlings.UI;
using Microsoft.Phone.Reactive;
using Microsoft.Xna.Framework;
using RedBadger.Xpf;
using RedBadger.Xpf.Controls;
using RedBadger.Xpf.Media;
using SimonSquared.Online.DataContracts;

namespace Flatlings
{
    class ScoreboardScreen : DialogScreen
    {
        private TextBlock _roundNumber;
        private ResourceCleaner _resourceCleaner = new ResourceCleaner();
        private ItemsControl _scoresListView;
        private Button _startButton;
        private Button _cancelButton;
        private StackPanel _buttonsStackPanel;

        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public ScoreboardScreen(Game game)
            : base(game)
        {
        }

        protected override void OnShown()
        {
            var multiplayerGame = MultiplayerGameManager.CurrentGame;

            var layoutRoot = new Grid()
                                 {
                                     RowDefinitions =
                                         {
                                             new RowDefinition() {Height = GridLength.Auto},
                                             new RowDefinition() {Height = GridLength.Auto},
                                             new RowDefinition(),
                                             new RowDefinition() {Height = GridLength.Auto},
                                         }
                                 };

            const string ScreenTitle = "scores so far";

            var title = CreateTextBlock(
                ScreenTitle,
                Fonts.Title,
                (tb) =>
                    {
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        tb.Margin = new Thickness(0, 10, 0, 40);
                    });
            layoutRoot.AddToGrid(title, 0, 0);

            var stackPanel = new StackPanel() {Orientation = Orientation.Horizontal};
            layoutRoot.AddToGrid(stackPanel, 1, 0);

            var roundsLabel = CreateTextBlock("Rounds Played: ", Fonts.Normal,
                                              tb => tb.HorizontalAlignment = HorizontalAlignment.Right);
            _roundNumber = CreateTextBlock("Loading scores", Fonts.Normal);
            stackPanel.Children.Add(roundsLabel);
            stackPanel.Children.Add(_roundNumber);


            _scoresListView = new ItemsControl()
                                  {
                                      ItemsPanel = new StackPanel() {Margin = new Thickness(10, 0, 10, 0)},
                                      ItemTemplate = (item) => CreatePlayerScoreLine(item)
                                  };

            layoutRoot.AddToGrid(_scoresListView, 2, 0);

            _buttonsStackPanel = new StackPanel()
                                     {
                                         Orientation = Orientation.Horizontal,
                                         HorizontalAlignment = HorizontalAlignment.Center,
                                         Margin = new Thickness(0, 20, 0, 0)
                                     };
            layoutRoot.AddToGrid(_buttonsStackPanel, 3, 0);

            _startButton = CreateButton("play again", DoStartGame);

            _cancelButton = CreateButton("abandon", DoCancel);

            if (multiplayerGame.Role == GameRole.Initiator)
            {
                _buttonsStackPanel.Children.Add(_startButton);
            }
            _buttonsStackPanel.Children.Add(_cancelButton);

            RootElement.Content = layoutRoot;

            SubscribeToEvents();
        }

        private UIElement CreatePlayerScoreLine(object item)
        {
            var scoreDto = (item as ScoreDto);
            var stackPanel = new StackPanel()
                                 {
                                     Orientation = Orientation.Horizontal,
                                     Margin = new Thickness(0,5,0,0),
                                 };
            var nameTextBlock = CreateTextBlock(scoreDto.PlayerName + ":  ", Fonts.Normal);
            var scoreTextBlock = CreateTextBlock(scoreDto.Score.ToString(), Fonts.Normal);

            stackPanel.Children.Add(nameTextBlock);
            stackPanel.Children.Add(scoreTextBlock);

            return stackPanel;
        }

        private void ShowLetsPlayButton()
        {
            _startButton.MinWidth = 0;
            _startButton.MaxWidth = double.MaxValue;
            _startButton.Width = double.NaN;
        }

        private void HideLetsPlayButton()
        {
            _startButton.MinWidth = 0;
            _startButton.MaxWidth = 0;
            _startButton.Width = 0;
        }

        protected override void OnHidden()
        {
            _resourceCleaner.CleanUp();

            base.OnHidden();
        }

        private void SubscribeToEvents()
        {
            var gameManager = MultiplayerGameManager;

            var subscription = gameManager.CurrentGame.Scoreboard
                .ObserveOn(DispatcherScheduler.Current)
                .Subscribe(HandleScoresLoaded);

            _resourceCleaner.AddResourceRequiringCleanup((subscription));

            subscription = gameManager.CurrentGame.GameStateObservable
                .Where(state => state == MultiplayerGameState.BeginningRound)
                .Subscribe(state => Messenger.Send(new StartingMultiplayerGameMessage()));

            _resourceCleaner.AddResourceRequiringCleanup(subscription);
        }

        private void HandleScoresLoaded(ScoreboardDto scoreboard)
        {
            if (!Enabled || scoreboard == null)
            {
                return;
            }

            _scoresListView.ItemsSource = scoreboard.Scores;
            _roundNumber.Text = scoreboard.RoundsPlayed.ToString();
        }

        private TextBlock CreateTextBlock(string text, string font)
        {
            return CreateTextBlock(text, font, null);
        }

        private TextBlock CreateTextBlock(string text, string font, Action<TextBlock> configurator)
        {
            var textBlock = new TextBlock(FontManager[font])
                                {
                                    Text = text,
                                    Foreground = new SolidColorBrush(Colors.White),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                };

            if (configurator != null)
            {
                configurator(textBlock);
            }

            return textBlock;
        }

        private void DoCancel()
        {
            MultiplayerGameManager.CurrentGame.LeaveGame();
            TransitionToScreen("MainMenu");
        }

        private void DoStartGame()
        {
            MultiplayerGameManager.CurrentGame.StartGame();
        }
    }
}
