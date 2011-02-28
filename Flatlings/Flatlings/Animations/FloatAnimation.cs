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
    public class FloatAnimation<TTarget> : SetterInvokingAnimation<float,TTarget> where TTarget : class 
    {
        public FloatAnimation(TTarget target, Action<TTarget, float> propertySetter) : base(target, propertySetter)
        {
        }

        protected override float GetValueForNormalisedTime(double normalisedTime)
        {
            var delta = EndValue - StartValue;
            var value = StartValue + delta * normalisedTime;

            return (float)value;
        }
    }
}
