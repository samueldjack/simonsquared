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
using Microsoft.Phone.Reactive;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RedBadger.Xpf;
using RedBadger.Xpf.Adapters.Xna.Graphics;
using RedBadger.Xpf.Adapters.Xna.Input;
using RedBadger.Xpf.Controls;
using RedBadger.Xpf.Input;
using RedBadger.Xpf.Media;

namespace Flatlings.UI
{
    public class TestScreen : DrawableGameComponent
    {
        private SpriteBatchAdapter _spriteBatchAdapter;
        private RootElement _rootElement;

        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public TestScreen(Game game) : base(game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            _rootElement.Update();
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _rootElement.Draw();
            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatchAdapter = new SpriteBatchAdapter(new SpriteBatch(GraphicsDevice));

            var primitivesService = new PrimitivesService(GraphicsDevice);
            var renderer = new Renderer(_spriteBatchAdapter, primitivesService);

            _rootElement = new RootElement(GraphicsDevice.Viewport.ToRect(), renderer, new RedBadger.Xpf.Adapters.Xna.Input.InputManager());

            var spriteFont = Game.Content.Load<SpriteFont>("Fonts\\Kootenay");
            var spriteFontAdapter = new SpriteFontAdapter(spriteFont);

            var grid = new Grid()
                           {
                               Background = new SolidColorBrush(Colors.White),
                               RowDefinitions = {new RowDefinition(), new RowDefinition()},
                               ColumnDefinitions = { new ColumnDefinition()},
                           };

            _rootElement.Content = grid;

            var button = new Button()
                             {
                                 Content = new TextBlock(spriteFontAdapter)
                                               {
                                                   Text = "Hello World",
                                                   Foreground = new SolidColorBrush(Colors.Black),
                                                   HorizontalAlignment = HorizontalAlignment.Center,
                                                   VerticalAlignment = VerticalAlignment.Center,
                                               },
                                 HorizontalAlignment = HorizontalAlignment.Stretch,
                                 VerticalAlignment = VerticalAlignment.Stretch,
                             };

            var textBox = new TextBox(spriteFontAdapter);

            button.Click += delegate { (button.Content as TextBlock).Text += "1"; };
            button.Gestures.Subscribe(obj=> Console.WriteLine("Hi"));
            Grid.SetRow(button, 0);
            Grid.SetColumn(button, 0);
            Grid.SetRow(textBox, 1);

            grid.Children.Add(button);
            grid.Children.Add(textBox);
        }
    }
}
