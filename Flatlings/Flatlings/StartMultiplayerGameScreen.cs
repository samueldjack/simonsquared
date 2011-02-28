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
using Microsoft.Xna.Framework;
using RedBadger.Xpf;
using RedBadger.Xpf.Controls;
using RedBadger.Xpf.Media;

namespace Flatlings
{
    public class StartMultiplayerGameScreen : DialogScreen
    {
        private TextBox _nameTextBox;
        private TextBox _gameNameTextBox;

        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public StartMultiplayerGameScreen(Game game) : base(game)
        {
        }

        protected override void OnShown()
        {
            var layoutRoot = new StackPanel() { };

            const string ScreenTitle = "start a multi-player game";

            var title = new TextBlock(FontManager[Fonts.Title])
                            {
                                Text = ScreenTitle,
                                Foreground = new SolidColorBrush(Colors.White),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Margin = new Thickness(0,10,0,40)
                            };
            layoutRoot.Children.Add(title);

            var inputGrid = new Grid()
                                {
                                    RowDefinitions =
                                        {
                                            new RowDefinition() {Height = GridLength.Auto},
                                            new RowDefinition() {Height = GridLength.Auto}
                                        },
                                    ColumnDefinitions =
                                        {
                                            new ColumnDefinition() {Width = GridLength.Auto},
                                            new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)}
                                        }
                                };
            
            layoutRoot.Children.Add(inputGrid);

            var nameLabel = new TextBlock(FontManager[Fonts.Normal])
                                {
                                    Text = "Your name:",
                                    Foreground = new SolidColorBrush(Colors.White),
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    Margin = new Thickness(5,3,2,3) 
                                };
            _nameTextBox = new TextBox(FontManager[Fonts.Normal])
                               {
                                   GuideTitle = ScreenTitle,
                                   GuideDescription = "Please enter you name",
                                   Margin = new Thickness(3,3,5,3) 
                               };
            inputGrid.Children.Add(nameLabel);
            inputGrid.Children.Add(_nameTextBox);
            Grid.SetColumn(_nameTextBox, 1);

            var gameNameLabel = new TextBlock(FontManager[Fonts.Normal])
            {
                Text = "Game's name:",
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5,3,2,3) 
            };
            _gameNameTextBox = new TextBox(FontManager[Fonts.Normal])
                                   {
                                       GuideTitle = ScreenTitle,
                                       GuideDescription = "Please enter a name for your game, to help others find it",
                                       Margin = new Thickness(3, 3, 5, 3),
                                   };

            inputGrid.Children.Add(gameNameLabel);
            inputGrid.Children.Add(_gameNameTextBox);
            Grid.SetRow(gameNameLabel, 1);
            Grid.SetRow(_gameNameTextBox, 1);
            Grid.SetColumn(_gameNameTextBox, 1);


            var buttonsStackPanel = new StackPanel()
                                        {
                                            Orientation = Orientation.Horizontal,
                                            HorizontalAlignment = HorizontalAlignment.Center,
                                            Margin = new Thickness(0,20,0,0)
                                        };
            layoutRoot.Children.Add(buttonsStackPanel);

            var startButton = CreateButton("start", DoStartGame);
            var cancelButton = CreateButton("cancel", DoCancel);

            buttonsStackPanel.Children.Add(startButton);
            buttonsStackPanel.Children.Add(cancelButton);

            _nameTextBox.TextChanged += delegate
                                            {
                                                if (string.IsNullOrEmpty(_gameNameTextBox.Text))
                                                {
                                                    _gameNameTextBox.Text = _nameTextBox.Text + "'s game";
                                                }
                                            };

            RootElement.Content = layoutRoot;
        }

        private void DoCancel()
        {
            TransitionToScreen("MainMenu");
        }

        private void DoStartGame()
        {
            if (string.IsNullOrEmpty(_nameTextBox.Text) || string.IsNullOrEmpty(_gameNameTextBox.Text))
            {
                ShowMessageBox("Please give your name, and a name for your game", "Simon Squared");
                return;
            }

            var manager = Game.Services.GetService(typeof (MultiplayerGameManager)) as MultiplayerGameManager;

            manager.SetPlayerName(_nameTextBox.Text);
            manager.StartNewGame(_gameNameTextBox.Text);

            TransitionToScreen("WaitingForPlayers");
        }

        protected override void OnHidden()
        {
            if (RootElement != null)
            {
                RootElement.Content = null;
            }
        }

        protected override void OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            DoCancel();
        }
    }
}
