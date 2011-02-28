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

namespace Flatlings.Animations
{
    public class AnimationFactory
    {
        public AnimationBuilder<TTarget, float> Animate<TTarget>(TTarget target, Action<TTarget, float> setter) where TTarget : class 
        {
            return new AnimationBuilder<TTarget, float>(new FloatAnimation<TTarget>(target, setter));
        }

        public AnimationBuilder<TTarget, Vector2> Animate<TTarget>(TTarget target, Action<TTarget, Vector2> setter) where TTarget : class
        {
            return new AnimationBuilder<TTarget, Vector2>(new Vector2Animation<TTarget>(target, setter));
        }

        public AnimationBuilder<TTarget, int> Animate<TTarget>(TTarget target, Action<TTarget, int> setter) where TTarget : class
        {
            return new AnimationBuilder<TTarget, int>(new Int32Animation<TTarget>(target, setter));
        }

        public AnimationBuilder<TTarget, bool> Animate<TTarget>(TTarget target, Action<TTarget, bool> setter) where TTarget : class
        {
            return new AnimationBuilder<TTarget, bool>(new BooleanAnimation<TTarget>(target, setter));
        }
    }
}
