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
using RedBadger.Xpf;
using RedBadger.Xpf.Controls;
using RedBadger.Xpf.Input;

namespace Flatlings.UI
{
    public class ListBoxItem : ContentControl, IInputElement
    {
        public static ReactiveProperty<bool> IsSelectedProperty = ReactiveProperty<bool>.Register("IsSelected",
                                                                                          typeof (ListBoxItem));
        public bool IsSelected
        {
            get { return GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public ListBoxItem()
        {
            
        }

        protected override void OnNextGesture(Gesture gesture)
        {
            if (gesture.Type == GestureType.LeftButtonDown)
            {
                IsSelected = true;
            }

            base.OnNextGesture(gesture);
        }
    }
}
