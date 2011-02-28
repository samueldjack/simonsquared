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
    public class Tile
    {
        private readonly Model _tileModel;
        private readonly Vector2 _position;
        private Vector3 _tileColor;

        public Tile(Model tileModel, Vector2 position, Color color)
        {
            _tileModel = tileModel;
            _position = position;
            _tileColor = color.ToVector3();
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix, Matrix parentTransform)
        {
            var transforms = new Matrix[_tileModel.Bones.Count];
            _tileModel.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (var mesh in _tileModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Alpha = 1;
                    effect.DiffuseColor = _tileColor;
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(_position.X, _position.Y, 0) * parentTransform;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }

                mesh.Draw();
            }
        }

        public void DrawUnexploded(Matrix viewMatrix, Matrix projectionMatrix, Matrix parentTransform)
        {
            var transforms = new Matrix[_tileModel.Bones.Count];
            _tileModel.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (var mesh in _tileModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Alpha = 0.5f;
                    effect.DiffuseColor = _tileColor;
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(_position.X, _position.Y, 0) * parentTransform;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }

                mesh.Draw();
            }
        }
    }
}
