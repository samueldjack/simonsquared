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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Flatlings.UI;
using Microsoft.Phone.Reactive;
using Microsoft.Xna.Framework;
using RedBadger.Xpf;
using RedBadger.Xpf.Controls;
using RedBadger.Xpf.Data;
using RedBadger.Xpf.Media;
using SimonSquared.Online.DataContracts;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace Flatlings
{
    public class JoinMultiplayerGameScreen : DialogScreen
    {
        private GameViewModel _selectedViewModel;
        private TextBox _nameTextBox;
        private ItemsControl _gamesListView;

        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public JoinMultiplayerGameScreen(Game game) : base(game)
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

            const string ScreenTitle = "join a game";

            var title = new TextBlock(FontManager[Fonts.Title])
            {
                Text = ScreenTitle,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 10, 0, 40)
            };
            layoutRoot.AddToGrid(title, 0, 0);

            var inputGrid = new Grid() { ColumnDefinitions =
                                                   {
                                                       new ColumnDefinition() { Width = GridLength.Auto},
                                                       new ColumnDefinition() 
                                                   }};

            layoutRoot.AddToGrid(inputGrid, 1,0);

            var nameLabel = new TextBlock(FontManager[Fonts.Normal])
                                {
                                    Text = "Your name:",
                                    Foreground = new SolidColorBrush(Colors.White),
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Margin = new Thickness(0, 0, 5,0)
                                };
            inputGrid.AddToGrid(nameLabel, 0, 0);

            _nameTextBox = new TextBox(FontManager[Fonts.Normal])
                               {
                                   GuideTitle = "Simon Squared",
                                   GuideDescription = "Please enter your name",
                                   Margin = new Thickness(5,0,5,0)
                               };
            inputGrid.AddToGrid(_nameTextBox, 0,1);

            var signedUpLabel = new TextBlock(FontManager[Fonts.Normal])
            {
                Text = "games looking for players:",
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0,8,0,0)
            };
            layoutRoot.AddToGrid(signedUpLabel, 2, 0);

            _gamesListView = new ItemsControl()
                                 {
                                     ItemsPanel = new StackPanel() { Margin = new Thickness(10, 0, 10, 0) },
                                     ItemTemplate = (item) => CreateListBoxItem(item as GameViewModel)
                                 };

            layoutRoot.AddToGrid(_gamesListView, 3, 0);

            var buttonsStackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 10)
            };
            layoutRoot.AddToGrid(buttonsStackPanel, 4, 0);

            var startButton = CreateButton("join", DoJoinGame);
            var cancelButton = CreateButton("cancel", DoCancel);

            buttonsStackPanel.Children.Add(startButton);
            buttonsStackPanel.Children.Add(cancelButton);

            RootElement.Content = layoutRoot;

            _selectedViewModel = null;

            RefreshGamesList();
        }

        private void RefreshGamesList()
        {
            var gameManager = GetService<MultiplayerGameManager>();

            gameManager.ListAvailableGames().Subscribe(HandleGamesListAvailable);
        }

        private void HandleGamesListAvailable(List<GameDto> gamesResources)
        {
            var viewModels = PrepareGameViewModelsCollection(gamesResources);
            _gamesListView.ItemsSource = viewModels;

            _selectedViewModel = null;
        }

        private IElement CreateListBoxItem(GameViewModel content)
        {
            var textBlock = new TextBlock(FontManager[Fonts.Normal])
                                {
                                    Text = content.GameName,
                                    Foreground = new SolidColorBrush(Colors.White),
                                    Margin = new Thickness(0,10,0,10),
                                };

            var border = new Border()
                             {
                                 Child = textBlock
                             };
            var listBoxItem = new ListBoxItem()
                                  {
                                      Content = border
                                  };

            textBlock.Bind(TextBlock.ForegroundProperty,
                           listBoxItem.GetObservable<bool, ListBoxItem>(ListBoxItem.IsSelectedProperty)
                               .Select(
                                   isSelected =>
                                   (Brush)
                                   (isSelected ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White))));
            border.Bind(Border.BackgroundProperty,
                        listBoxItem.GetObservable<bool, ListBoxItem>(ListBoxItem.IsSelectedProperty)
                            .Select(
                                isSelected =>
                                (Brush)
                                (isSelected ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.Black))));

            content.PropertyChanged += delegate { listBoxItem.IsSelected = content.IsSelected; };

            listBoxItem.Bind(ListBoxItem.IsSelectedProperty, BindingFactory.CreateTwoWay(content, c => c.IsSelected));
            return listBoxItem;
        }

        private void DoCancel()
        {
            TransitionToScreen("MainMenu");
        }

        private void DoJoinGame()
        {
            if (string.IsNullOrEmpty(_nameTextBox.Text))
            {
                ShowMessageBox("Please enter you name", "Simon Squared");
                return;
            }

            if (_selectedViewModel == null)
            {
                ShowMessageBox("Please select a game", "Simon Squared");
                return;
            }

            var gameManager = GetService<MultiplayerGameManager>();

            gameManager.SetPlayerName(_nameTextBox.Text);
            gameManager.JoinGame(_selectedViewModel.GameId);

            TransitionToScreen("WaitingForPlayers");
        }

        private IList<GameViewModel> PrepareGameViewModelsCollection(IEnumerable<GameDto> games)
        {
            if (games == null)
            {
                return new GameViewModel[0];
            }

            var viewModels = games.Select(g => new GameViewModel() {GameName = g.Name, GameId = g.Id}).ToList();

            foreach (var gameViewModel in viewModels)
            {
                gameViewModel.PropertyChanged += HandleGameViewModelSelectionChanged;
            }

            return viewModels;
        }

        private void HandleGameViewModelSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            var senderViewModel = (sender as GameViewModel);
            if (!senderViewModel.IsSelected)
            {
                return;
            }

            if (_selectedViewModel != null)
            {
                _selectedViewModel.IsSelected = false;
            }

            _selectedViewModel = senderViewModel;
        }

        protected override void OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            DoCancel();
        }

        public class GameViewModel : ViewModel
        {
            private bool _isSelected;

            public string GameName { get; set; }

            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    _isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }

            public string GameId { get; set; }
        }


    }
}
