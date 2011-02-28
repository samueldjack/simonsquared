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
using Microsoft.Xna.Framework.Input.Touch;
using RedBadger.Xpf;
using RedBadger.Xpf.Input;
using GestureType = Microsoft.Xna.Framework.Input.Touch.GestureType;

namespace Flatlings
{
    class InputManager : IInputManager
    {
        Subject<Gesture> _gestures = new Subject<Gesture>();

        public InputManager()
        {
            TouchPanel.EnabledGestures |= GestureType.Tap;
            TouchPanel.EnabledGestures |= GestureType.FreeDrag;
        }

        public void Update()
        {
            while (TouchPanel.IsGestureAvailable)
            {
                var touchPanelGesture = TouchPanel.ReadGesture();
                if (touchPanelGesture.GestureType == GestureType.Tap)
                {
                    _gestures.OnNext(new Gesture(RedBadger.Xpf.Input.GestureType.LeftButtonDown, new Point(touchPanelGesture.Position.X, touchPanelGesture.Position.Y)));
                    _gestures.OnNext(new Gesture(RedBadger.Xpf.Input.GestureType.LeftButtonUp, new Point(touchPanelGesture.Position.X, touchPanelGesture.Position.Y)));
                }
                else if (touchPanelGesture.GestureType ==GestureType.FreeDrag)
                {
                    _gestures.OnNext(new Gesture(RedBadger.Xpf.Input.GestureType.FreeDrag, new Point(touchPanelGesture.Position.X, touchPanelGesture.Position.Y), new Vector(touchPanelGesture.Delta.X, touchPanelGesture.Delta.Y)));
                }
            }
        }

        public IObservable<Gesture> Gestures
        {
            get { return _gestures; }
        }
    }
}
