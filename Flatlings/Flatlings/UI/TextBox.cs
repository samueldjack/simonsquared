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
using Microsoft.Xna.Framework.GamerServices;
using RedBadger.Xpf;
using RedBadger.Xpf.Controls;
using RedBadger.Xpf.Data;
using RedBadger.Xpf.Graphics;
using RedBadger.Xpf.Input;
using RedBadger.Xpf.Media;
using Flatlings.Reactive;
using Microsoft.Phone.Reactive;

namespace Flatlings.UI
{
    public class TextBox : ContentControl, IInputElement
    {
        public event EventHandler<EventArgs> TextChanged;

        private TextBlock _textBlock;

        private IDisposable _clickSubscription;

        public static ReactiveProperty<string> TextProperty = ReactiveProperty<string>.Register("Text", typeof (TextBox));

        public static ReactiveProperty<string> GuideTitleProperty = ReactiveProperty<string>.Register("GuideTitle", typeof(TextBox));

        public static ReactiveProperty<string> GuideDescriptionProperty = ReactiveProperty<string>.Register("GuideDescription", typeof(TextBox));

        public TextBox(ISpriteFont spriteFont)
        {
            _textBlock = new TextBlock(spriteFont)
                             {
                                 Margin = new Thickness(3,6),
                                 Foreground = new SolidColorBrush(Colors.Black),
                             };

            _textBlock.Bind(TextBlock.TextProperty, BindingFactory.CreateOneWay(this, TextProperty));

            Content = new Border()
                          {
                              Background = new SolidColorBrush(Colors.Gray),
                              BorderThickness = new Thickness(1),
                              Child = _textBlock,
                          };

            _clickSubscription = Gestures
                .Where(gesture => gesture.Type == GestureType.LeftButtonDown)
                .Subscribe(HandleClick);
        }

        public string Text
        {
            get { return GetValue(TextProperty); }
            set { SetValue(TextProperty, value);}
        }

        public string GuideTitle
        {
            get { return GetValue(GuideTitleProperty); }
            set { SetValue(GuideTitleProperty, value); }
        }

        public string GuideDescription
        {
            get { return GetValue(GuideDescriptionProperty); }
            set { SetValue(GuideDescriptionProperty, value); }
        }

        private void HandleClick(Gesture gesture)
        {
            if (_clickSubscription != null) _clickSubscription.Dispose();

            if (!Guide.IsVisible)
            {
                Guide.BeginShowKeyboardInput(
                    PlayerIndex.One,
                    GuideTitle,
                    GuideDescription,
                    Text, HandleKeyboardInputComplete, null);
            }
        }

        private void HandleKeyboardInputComplete(IAsyncResult asyncResult)
        {
            var text = Guide.EndShowKeyboardInput(asyncResult);
            Text = text;

            _clickSubscription = Gestures.Subscribe(HandleClick);

            InvokeTextChanged(EventArgs.Empty);
        }

        protected void InvokeTextChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = TextChanged;
            if (handler != null) handler(this, e);
        }
    }
}
