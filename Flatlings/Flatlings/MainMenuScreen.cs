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
using RedBadger.Xpf.Adapters.Xna.Graphics;
using RedBadger.Xpf.Controls;
using RedBadger.Xpf.Input;
using RedBadger.Xpf.Media;
using RedBadger.Xpf.Media.Imaging;
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Phone.Reactive;

namespace Flatlings
{
    class MainMenuScreen : DialogScreen
    {
        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public MainMenuScreen(Game game) : base(game)
        {
        }

        protected override void OnHidden()
        {
            if (RootElement != null)
            {
                RootElement.Content = null;
            }
        }

        protected override void OnShown()
        {
            base.OnShown();

            var stackPanel = new StackPanel()
                                 {
                                     VerticalAlignment = VerticalAlignment.Center, 
                                     HorizontalAlignment = HorizontalAlignment.Center
                                 };

            var imageSource = new TextureImage(new Texture2DAdapter(GetService<CommonContentManager>().Logo));
            var image = new Image()
                            {
                                Source = imageSource,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Stretch = Stretch.None,
                                Margin = new Thickness(0,0,0, 40)
                            };

            stackPanel.Children.Add(image);

            AddButton(stackPanel, "new game", () => Messenger.Send(new StartingSinglePlayerGameMessage()));
            AddButton(stackPanel, "start a multiplayer game", () => TransitionToScreen("StartMultiplayer"));
            AddButton(stackPanel, "join a multiplayer game", () => TransitionToScreen("JoinMultiplayer"));
            AddButton(stackPanel, "see help", () => TransitionToScreen("Help"));

            RootElement.Content = stackPanel;
        }

        private void AddButton(Panel parent, string buttonText, Action clickAction)
        {
            var button = new SimpleButton()
                             {
                                 Content =
                                     new TextBlock(FontManager[Fonts.Large])
                                         {
                                             Text = buttonText,
                                             Foreground = new SolidColorBrush(Colors.White)
                                         },
                                         Margin = new Thickness(0,4,0,4),
                             };

            button.Click += delegate { clickAction(); };

            parent.Children.Add(button);
        }

        protected override void OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            var gameState = GetService<GameStateService>();

            if (gameState.IsGameInProgress)
            {
                TransitionToScreen("Game");
            }
            else
            {
                Game.Exit();
            }
        }
    }
}
