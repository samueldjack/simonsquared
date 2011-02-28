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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Flatlings
{
    class HelpScreen : Screen
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _helpScreenTexture;

        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public HelpScreen(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _helpScreenTexture = Game.Content.Load<Texture2D>("Images\\HelpScreen");

            base.LoadContent();
        }

        protected override void UpdateCore(GameTime gameTime)
        {
            while (TouchPanel.IsGestureAvailable)
            {
                var gestureSample = TouchPanel.ReadGesture();

                if (gestureSample.GestureType == GestureType.Tap)
                {
                    TransitionToScreen("MainMenu");
                }
            }
        }

        protected override void DrawCore(GameTime gameTime)
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            _spriteBatch.Begin();

            var x = GraphicsDevice.Viewport.Width/2 - 200;
            var y = GraphicsDevice.Viewport.Height/2 - 350;

            _spriteBatch.Draw(_helpScreenTexture, new Rectangle(x, y, 400, 700), Color.White);

            _spriteBatch.End();
        }
    }
}
