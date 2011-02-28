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
    class GameStartingScreen : DialogScreen
    {
        private TextBlock _textBlock;

        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public GameStartingScreen(Game game) : base(game)
        {
        }

        protected override void OnShown()
        {
            base.OnShown();

            _textBlock = new TextBlock(FontManager[Fonts.Title])
                             {
                                 Foreground = new SolidColorBrush(Colors.White),
                                 HorizontalAlignment = HorizontalAlignment.Center,
                                 VerticalAlignment = VerticalAlignment.Center,
                             };

            RootElement.Content = _textBlock;
        }

        protected override void UpdateCore(GameTime gameTime)
        {
            base.UpdateCore(gameTime);

            var gameState = MultiplayerGameManager.CurrentGame.GameState;

            if (gameState == null)
            {
                return;
            }

            var timeUntilStart = MultiplayerGameManager.CurrentGame.StartTimeUtc - DateTime.UtcNow;

            _textBlock.Text = timeUntilStart.ToString();
            _textBlock.Text = (DateTimeOffset.UtcNow + MultiplayerGameManager.CurrentGame.ServerClockSkew).ToString("G");

            if (timeUntilStart.TotalMilliseconds <= 0)
            {
                //Messenger.Send(new StartingMultiplayerGameMessage());
            }
        }

       
    }
}
