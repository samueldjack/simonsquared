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
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using RedBadger.Xpf;
using RedBadger.Xpf.Adapters.Xna.Graphics;
using RedBadger.Xpf.Adapters.Xna.Input;
using RedBadger.Xpf.Controls;
using RedBadger.Xpf.Media;

namespace Flatlings
{
    public class DialogScreen : Screen
    {
        private SpriteBatchAdapter _spriteBatchAdapter;
        private RootElement _rootElement;

        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public DialogScreen(Game game) : base(game)
        {
        }

        protected RootElement RootElement
        {
            get { return _rootElement; }
        }

        protected override void UpdateCore(GameTime gameTime)
        {
            RootElement.Update();
        }

        protected override void DrawCore(GameTime gameTime)
        {
            RootElement.Draw();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatchAdapter = new SpriteBatchAdapter(new SpriteBatch(GraphicsDevice));

            var primitivesService = new PrimitivesService(GraphicsDevice);
            var renderer = new Renderer(_spriteBatchAdapter, primitivesService);

            _rootElement = new RootElement(GraphicsDevice.Viewport.ToRect(), renderer, new InputManager());
        }

        protected FontManager FontManager
        {
            get { return Game.Services.GetService(typeof (FontManager)) as FontManager; }
        }

        protected void ShowMessageBox(string message, string title)
        {
            if (!Guide.IsVisible)
            {
                Guide.BeginShowMessageBox(title, message, new[] {"OK"}, 0, MessageBoxIcon.Alert,
                                          asyncResult => Guide.EndShowMessageBox(asyncResult), null);
            }
        }

        protected Button CreateButton(string caption, Action clickAction)
        {
            var whiteBrush = new SolidColorBrush(Colors.White);

            var button = new Button()
                             {
                                 Content = new Border()
                                               {
                                                   BorderBrush = whiteBrush,
                                                   BorderThickness = new Thickness(1),
                                                   Child = new TextBlock(FontManager[Fonts.Normal])
                                                               {
                                                                   Text = caption,
                                                                   Foreground = whiteBrush,
                                                                   Margin = new Thickness(5),
                                                                   HorizontalAlignment = HorizontalAlignment.Center,
                                                                   VerticalAlignment = VerticalAlignment.Center,
                                                               }
                                               },
                                 Margin = new Thickness(5),
                                 MinWidth = 80
                             };
            button.Click += delegate { clickAction(); };

            return button;
        }

        protected IElement CreateTextBlock(string text)
        {
            var textBlock = new TextBlock(FontManager[Fonts.Normal])
                                {
                                    Text = text,
                                    Foreground = new SolidColorBrush(Colors.White),
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                };

            return textBlock;
        }
    }
}
