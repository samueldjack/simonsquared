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
using RedBadger.Xpf.Adapters.Xna.Graphics;
using RedBadger.Xpf.Graphics;

namespace Flatlings.UI
{
    public class FontManager : IGameComponent
    {
        private readonly Game _game;
        Dictionary<string, ISpriteFont> _fonts = new Dictionary<string, ISpriteFont>();

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="game">Game that the game component should be attached to.</param>
        public FontManager(Game game)
        {
            _game = game;
        }

        public ISpriteFont this[string name]
        {
            get { return _fonts[name]; }
        }

        /// <summary>
        /// Called when the component should be initialized. This method can be used for tasks like querying for services the component needs and setting up non-graphics resources.
        /// </summary>
        public void Initialize()
        {
            AddFont("Small", "SegoeRegular14");
            AddFont("Normal", "SegoeRegular17");
            AddFont("Button", "SegoeRegular20");
            AddFont("Title", "TitleFont");
            AddFont("Large", "SegoeRegular22");
        }

        private void AddFont(string logicalName, string fileName)
        {
            var font = _game.Content.Load<SpriteFont>("Fonts\\" + fileName);
            var fontAdapter = new SpriteFontAdapter(font);

            _fonts.Add(logicalName, fontAdapter);
        }
    }
}
