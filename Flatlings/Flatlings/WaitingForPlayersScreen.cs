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
    class WaitingForPlayersScreen : DialogScreen
    {
        private TextBlock _gameNameLabel;
        private ResourceCleaner _resourceCleaner = new ResourceCleaner();
        private ItemsControl _playerListView;
        private Button _startButton;
        private Button _cancelButton;
        private StackPanel _buttonsStackPanel;

        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public WaitingForPlayersScreen(Game game) : base(game)
        {
        }

        protected override void OnShown()
        {
            var layoutRoot = new Grid()
                                 {
                                     RowDefinitions =
                                         {
                                             new RowDefinition() { Height = GridLength.Auto},
                                             new RowDefinition() { Height = GridLength.Auto},
                                             new RowDefinition() { Height = GridLength.Auto},
                                             new RowDefinition(),
                                             new RowDefinition() { Height = GridLength.Auto},
                                         }
                                 };

            const string ScreenTitle = "waiting for players";

            var title = CreateTextBlock(
                ScreenTitle,
                Fonts.Title,
                (tb) =>
                    {
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        tb.Margin = new Thickness(0, 10, 0, 40);
                    });
            layoutRoot.AddToGrid(title, 0,0);

            var stackPanel = new StackPanel() {Orientation = Orientation.Horizontal};
            layoutRoot.AddToGrid(stackPanel, 1, 0);

            var nameLabel = CreateTextBlock("Game: ", Fonts.Normal, 
                tb => tb.HorizontalAlignment = HorizontalAlignment.Right);
            _gameNameLabel = CreateTextBlock("Wait for game to be set up", Fonts.Normal);
            stackPanel.Children.Add(nameLabel);
            stackPanel.Children.Add(_gameNameLabel);

            var signedUpLabel = new TextBlock(FontManager[Fonts.Normal])
                                    {
                                        Text = "signed up so far:",
                                        Foreground = new SolidColorBrush(Colors.White),
                                        HorizontalAlignment = HorizontalAlignment.Left
                                    };
            layoutRoot.AddToGrid(signedUpLabel, 2,0);

            _playerListView = new ItemsControl()
                                  {
                                      ItemsPanel = new StackPanel() { Margin = new Thickness(10,0,10,0)},
                                      ItemTemplate = (item) => CreateTextBlock((item as PlayerDto).Name, Fonts.Title, (tb) =>
                                                                                                                          {
                                                                                                                              tb.HorizontalAlignment = HorizontalAlignment.Center;
                                                                                                                              tb.Margin = new Thickness(0, 5, 0, 5);
                                                                                                                          })
                                  };

            layoutRoot.AddToGrid(_playerListView, 3,0);

            _buttonsStackPanel = new StackPanel()
                                     {
                                         Orientation = Orientation.Horizontal,
                                         HorizontalAlignment = HorizontalAlignment.Center,
                                         Margin = new Thickness(0, 20, 0, 0)
                                     };
            layoutRoot.AddToGrid(_buttonsStackPanel, 4, 0);

            _startButton = CreateButton("let's play!", DoStartGame);
            
            _cancelButton = CreateButton("abandon", DoCancel);

            _buttonsStackPanel.Children.Add(_startButton);
            _buttonsStackPanel.Children.Add(_cancelButton);

            RootElement.Content = layoutRoot;

            SubscribeToEvents();
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

            var subscription = gameManager.CurrentGameObservable
                .ObserveOn(DispatcherScheduler.Current)
                .Subscribe(HandleGameChanged);

            _resourceCleaner.AddResourceRequiringCleanup((subscription));
        }

        private void HandleGameChanged(MultiplayerGame game)
        {
            if (!Enabled || game == null)
            {
                return;
            }

            _playerListView.ItemsSource = game.Players;
            _gameNameLabel.Text = game.Name;

            var subscription = game.GameStateObservable
                .Where(state => state == MultiplayerGameState.BeginningRound)
                .Subscribe(state => Messenger.Send(new StartingMultiplayerGameMessage()));

            _resourceCleaner.AddResourceRequiringCleanup(subscription);
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
                                    Margin = new Thickness(0, 10, 0, 40)
                                };

            if (configurator != null)
            {
                configurator(textBlock);
            }

            return textBlock;
        }

        private void DoCancel()
        {
            MultiplayerGameManager.LeaveGame();
            TransitionToScreen("MainMenu");
        }

        protected override void OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            DoCancel();
        }

        private void DoStartGame()
        {
            MultiplayerGameManager.CurrentGame.StartGame();
        }
    }
}
