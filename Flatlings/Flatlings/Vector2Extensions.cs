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

namespace Flatlings
{
    public static class Vector2Extensions
    {
        // CalculateRayThroughScreenPosition Calculates a world space ray starting at the camera's
        // "eye" and pointing in the direction of the given screenPosition. 
        public static Ray CalculateRayThroughScreenPosition(this Vector2 screenPosition, Matrix worldMatrix, Matrix projectionMatrix, Matrix viewMatrix, Viewport viewport)
        {
            // create 2 positions in screenspace using the screen position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3(screenPosition, viewport.MinDepth);
            Vector3 farSource = new Vector3(screenPosition, viewport.MaxDepth);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space.
            Vector3 nearPoint = viewport.Unproject(nearSource,
                projectionMatrix, viewMatrix, worldMatrix);

            Vector3 farPoint = viewport.Unproject(farSource,
                projectionMatrix, viewMatrix, worldMatrix);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }

        public static double AngleWith(this Vector2 vector, Vector2 otherVector)
        {
            return Math.Acos(Vector2.Dot(vector, otherVector)/(vector.Length() * otherVector.Length()));
        }
    }
}
