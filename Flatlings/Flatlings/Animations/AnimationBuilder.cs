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

namespace Flatlings.Animations
{
    public abstract class AnimationBuilder
    {
        public abstract Animation Build();

        public static implicit operator Animation(AnimationBuilder builder)
        {
            return builder.Build();
        }
    }

    public class AnimationBuilder<TTarget, TValue> : AnimationBuilder where TTarget : class 
    {
        private readonly SetterInvokingAnimation<TValue, TTarget> _animation;

        public AnimationBuilder(SetterInvokingAnimation<TValue, TTarget> animation)
        {
            _animation = animation;
        }

        public AnimationBuilder<TTarget, TValue> From(TValue startValue)
        {
            _animation.StartValue = startValue;
            return this;
        }

        public AnimationBuilder<TTarget, TValue> To(TValue endValue)
        {
            _animation.EndValue = endValue;
            return this;
        }

        public AnimationBuilder<TTarget, TValue> In(TimeSpan duration)
        {
            _animation.Duration = duration;
            return this;
        }

        public AnimationBuilder<TTarget, TValue> RepeatForever()
        {
            _animation.RepeatMode = RepeatMode.Forever;
            return this;
        }

        public AnimationBuilder<TTarget, TValue> AutoReverse()
        {
            _animation.AutoReverse = true;
            return this;
        }

        public override Animation Build()
        {
            return _animation;
        }
    }

    public interface IAnimationBuilder<TTarget, TValue>
    {
        IAnimationBuilder<TTarget, TValue> From(TValue startValue);
        IAnimationBuilder<TTarget, TValue> To(TValue endValue);
        IAnimationBuilder<TTarget, TValue> In(TimeSpan duration);
    }
}
