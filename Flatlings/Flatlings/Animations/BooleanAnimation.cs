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
    public class BooleanAnimation<TTarget> : SetterInvokingAnimation<bool, TTarget> where TTarget : class
    {
        private readonly bool _start;
        private readonly bool _end;
        private readonly Action<bool> _propertySetter;

        public BooleanAnimation(TTarget target, Action<TTarget, bool> propertySetter) : base(target, propertySetter)
        {
        }

        protected override bool GetValueForNormalisedTime(double normalisedTime)
        {
            return ((float)normalisedTime).IsWithinDeltaOf(1.0f) ? EndValue : StartValue;
        }

    }
}
