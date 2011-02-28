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

namespace Flatlings.Animations
{
    public abstract class SetterInvokingAnimation<TValue, TTarget> : Animation where TTarget : class 
    {
        public TValue StartValue { get; set; }

        public TValue EndValue { get; set; }

        private WeakReference _target;
        private readonly Action<TTarget, TValue> _setter;

        public SetterInvokingAnimation(TTarget target, Action<TTarget, TValue> setter)
        {
            _setter = setter;
            _target = new WeakReference(target);
        }

        public override bool HasExpired(TimeSpan currentElapsedTime)
        {
            return !_target.IsAlive || base.HasExpired(currentElapsedTime);
        }

        protected override void SetValueForTime(double normalisedTime)
        {
            var value = GetValueForNormalisedTime(normalisedTime);
            var target = _target.Target as TTarget;
            if (target != null)
            {
                _setter(target, value);
            }
        }

        protected abstract TValue GetValueForNormalisedTime(double normalisedTime);
    }
}
