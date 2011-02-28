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
using Flatlings.Messages;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Flatlings
{
    public class Screen : DrawableGameComponent
    {
        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public Screen(Game game) : base(game)
        {
            Enabled = false;
        }


        public sealed override void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                UpdateCore(gameTime);
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                {
                    OnBackButtonPressed();
                }
            }

            base.Update(gameTime);
        }

        protected virtual void OnBackButtonPressed()
        {
            
        }

        protected  virtual void UpdateCore(GameTime gameTime)
        {
            
        }

        public sealed override void Draw(GameTime gameTime)
        {
            if (Enabled)
            {
                DrawCore(gameTime);
            }

            base.Draw(gameTime);
        }

        protected virtual void DrawCore(GameTime gameTime)
        {

        }

        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            if (Enabled)
            {
                OnShown();
            }
            else
            {
                OnHidden();
            }

            base.OnEnabledChanged(sender, args);
        }

        protected virtual void OnShown()
        {
            
        }

        protected virtual void OnHidden()
        {

        }

        protected void TransitionToScreen(string screenName)
        {
            Messenger.Send(new TransitionToScreenMessage() { ScreenName = screenName});
        }

        protected Messenger Messenger
        {
            get { return Game.Services.GetService(typeof (Messenger)) as Messenger; }
        }

        protected MultiplayerGameManager MultiplayerGameManager
        {
            get { return Game.Services.GetService(typeof (MultiplayerGameManager)) as MultiplayerGameManager; }
        }

        protected T GetService<T>()
        {
            return (T)Game.Services.GetService(typeof (T));
        }
    }
}
