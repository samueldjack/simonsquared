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
using Flatlings.Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Flatlings
{
    public class Shape
    {
        private const double MovementAnimationTime = 0.2;
        private readonly Storyboard _storyboard;
        private Vector2 _sizeInTiles;
        private float _tileSize;
        private readonly float _baseZPosition;
        private Matrix? _worldMatrix;
        private Animation _hoverAnimation;

        private Shape(List<Tile> tiles, Storyboard storyboard, Vector2 sizeInTiles, float tileSize, Vector2 offsetInPuzzle, float baseZPosition)
        {
            _storyboard = storyboard;
            _sizeInTiles = sizeInTiles;
            _tileSize = tileSize;
            _unexplodedZPosition = baseZPosition - 5;
            _baseZPosition = baseZPosition;
            Tiles = tiles;
            InitialOffset = offsetInPuzzle*_tileSize;
            Offset = InitialOffset;
            ZPosition = baseZPosition + 5;

            _hoverAnimation =
                new AnimationFactory().Animate(this, (Shape target, float value) => target.ZPosition = value)
                    .From(0)
                    .To(5)
                    .In(TimeSpan.FromSeconds(1))
                    .RepeatForever()
                    .AutoReverse();

        }

        public static Shape Create(int[][] tilePositions, Vector2 offset, Storyboard storyboard, Model tileModel, Palette palette, float baseZPosition)
        {
            var tileSize = 10.0f;
            var tiles = new List<Tile>();
            var widthInTiles = 0;

            for (int row = 0; row < tilePositions.Length; row++)
            {
                widthInTiles = Math.Max(tilePositions[row].Length, widthInTiles);

                for (int column = 0; column < tilePositions[row].Length; column++)
                {
                    var tileColor = tilePositions[row][column];
                    bool hasTile = tileColor > 0;
                    if (!hasTile)
                    {
                        continue;
                    }

                    var xOffset = column*tileSize;
                    var yOffset = row*tileSize;

                    var position = new Vector2(xOffset, yOffset);

                    tiles.Add(new Tile(tileModel, position, palette.GetTileColor(tileColor)));
                }
            }

            var shape = new Shape(tiles, storyboard, new Vector2(widthInTiles, tilePositions.Length), tileSize, offset, baseZPosition);
            
            return shape;
        }

        protected Vector2 InitialOffset { get; set; }

        protected Vector2 ExplodedPosition { get; set; }

        public bool IsInExplodedPosition { get; private set; }

        public bool IsAnimating { get; private set; }

        protected void MarkAsAnimating()
        {
            IsAnimating = true;
            _storyboard.Plan()
                .Begin(b => b.Animate(this, (Shape target, bool value) => target.IsAnimating = value)
                    .From(true)
                    .To(false)
                    .In(TimeSpan.FromSeconds(MovementAnimationTime)));
        }

        public void FlipX()
        {
            if (!IsInExplodedPosition)
            {
                return;
            }

            _storyboard.Plan()
                .Begin(b => b.Animate(this, (Shape target, float value) => target.RotationX = value)
                                .From(RotationX)
                                .To(RotationX + MathHelper.Pi)
                                .In(TimeSpan.FromSeconds(MovementAnimationTime)));
            MarkAsAnimating();

            _parent.NotifyShapeMoved();
        }

        public void FlipY()
        {
            if (!IsInExplodedPosition)
            {
                return;
            }

            _storyboard.Plan()
                .Begin(b => b.Animate(this, (Shape target, float value) => target.RotationY = value)
                                .From(RotationY)
                                .To(RotationY + MathHelper.Pi)
                                .In(TimeSpan.FromSeconds(MovementAnimationTime)));
            MarkAsAnimating();

            _parent.NotifyShapeMoved();
        }

        public void RotateClockwise()
        {
            if (!IsInExplodedPosition)
            {
                return;
            }

            if (!IsBackShowing())
            {
                PerformZRotation(MathHelper.PiOver2);
            }
            else
            {
                PerformZRotation(-MathHelper.PiOver2);
            }
        }

        public void RotateAntiClockwise()
        {
            if (!IsInExplodedPosition)
            {
                return;
            }

            if (!IsBackShowing())
            {
                PerformZRotation(-MathHelper.PiOver2);
            }
            else
            {
                PerformZRotation(MathHelper.PiOver2);
            }
        }

        private void PerformZRotation(float angle)
        {
            _storyboard.Plan()
                .Begin(b => b.Animate(this, (Shape target, float value) => target.RotationZ = value)
                                .From(RotationZ)
                                .To(RotationZ + angle)
                                .In(TimeSpan.FromSeconds(MovementAnimationTime)));
            MarkAsAnimating();

            _parent.NotifyShapeMoved();
        }

        public void MoveToExplodedPosition()
        {
            _storyboard.Plan()
                .Begin(b => b.Animate(this, (target, value) => target.Offset = value)
                                .From(Offset)
                                .To(ExplodedPosition)
                                .In(TimeSpan.FromSeconds(MovementAnimationTime)))
                .Begin(b => b.Animate(this, (Shape target, float value) => target.ZPosition = value)
                                .From(ZPosition)
                                .To(0)
                                .In(TimeSpan.FromSeconds(MovementAnimationTime)));
            MarkAsAnimating();

            IsInExplodedPosition = true;
        }

        public void StartHovering()
        {
            _storyboard.Plan()
                .Begin(_hoverAnimation);
        }

        public void MoveToHomePosition()
        {
            if (CanMoveToHomePosition())
            {
                _storyboard.Plan()
                    .Begin(b => b.Animate(this, (target, value) => target.Offset = value)
                                    .From(Offset)
                                    .To(InitialOffset)
                                    .In(TimeSpan.FromSeconds(MovementAnimationTime)))
                    .Begin(b => b.Animate(this, (Shape target, float value) => target.ZPosition = value)
                                    .From(ZPosition)
                                    .To(_baseZPosition)
                                    .In(TimeSpan.FromSeconds(MovementAnimationTime)));

                _storyboard.RemoveAnimation(_hoverAnimation);
                MarkAsAnimating();

                IsInExplodedPosition = false;
                _parent.NotifyShapeMovedHome();
            }
        }

        private bool CanMoveToHomePosition()
        {
            var testVector = new Vector3(1, 1, 2);
            var transformedVector = Vector3.Transform(testVector, GetMatrixRepresentingRotations());

            return transformedVector.X.IsWithinDeltaOf(testVector.X) 
                && transformedVector.Y.IsWithinDeltaOf(testVector.Y)
                && transformedVector.Z.IsWithinDeltaOf(testVector.Z);
        }

        private bool IsBackShowing()
        {
            var testVector = new Vector3(0, 0, 1);
            var transformedVector = Vector3.Transform(testVector, GetMatrixRepresentingRotations());

            return transformedVector.Z < 0;
        }

        public bool HitTest(Ray inputRay)
        {
            var inputRayInWorldCoordinates = inputRay.Transform(WorldMatrixInverse);

            var boundingSphere = CalculateBoundingSphere();

            var distance = boundingSphere.Intersects(inputRayInWorldCoordinates);

            return distance.HasValue;
        }

        private BoundingSphere CalculateBoundingSphere()
        {
            var centre = SizeInPixels/2;

            // centreX and centreY also happen to be the distances for centre to edges
            var radius = Math.Max(centre.X, centre.Y) + 10;

            return new BoundingSphere(new Vector3(centre.X, centre.Y, 0), radius);
        }

        public Vector2 GetCenterInWorldCoordinates(Viewport viewPort, Matrix viewMatrix, Matrix projectionMatrix)
        {
            var worldMatrix = WorldMatrix * _parent.WorldMatrix;
            var center = viewPort.Project(new Vector3(SizeInPixels / 2, 0), projectionMatrix, viewMatrix, WorldMatrix);

            return new Vector2(center.X, center.Y);
        }

        public BoundingBox GetBoundingBox(Viewport viewPort, Matrix viewMatrix, Matrix projectionMatrix)
        {
            var worldMatrix = WorldMatrix * _parent.WorldMatrix;

            var bottomLeft = Vector3.Zero;
            var topRight = new Vector3(SizeInPixels, 0);

            var points = new[] { 
                viewPort.Project(bottomLeft, projectionMatrix, viewMatrix, worldMatrix), 
                viewPort.Project(topRight, projectionMatrix, viewMatrix, worldMatrix) };

            var boundingBox = BoundingBox.CreateFromPoints(points);

            return boundingBox;
        }

        private BoundingBox CalculateBoundingBox()
        {
            return new BoundingBox(new Vector3(0, 0, 0), new Vector3(SizeInPixels, 0));
        }

        private float _zPosition;
        public float ZPosition
        {
            get { return _zPosition; }
            set
            {
                _zPosition = value;
                InvalidateWorldMatrix();
            }
        }

        private Vector2 _offset;

        public Vector2 Offset
        {
            get { return _offset; }
            set { _offset = value;
                InvalidateWorldMatrix();
            }
        }

        public Vector2 SizeInPixels
        {
            get { return _sizeInTiles*_tileSize; }
        }

        private void InvalidateWorldMatrix()
        {
            _worldMatrix = null;
            _worldMatrixInverse = null;
        }

        private float _rotationX;

        protected float RotationX
        {
            get { return _rotationX; }
            set { _rotationX = value;
                InvalidateWorldMatrix();
            }
        }

        private float _rotationY;

        protected float RotationY
        {
            get { return _rotationY; }
            set { _rotationY = value;
                InvalidateWorldMatrix();
            }
        }

        private float _rotationZ;
        private Matrix? _unexplodedWorldMatrix;
        private Puzzle _parent;
        private Matrix? _worldMatrixInverse;
        private float _unexplodedZPosition;

        protected float RotationZ
        {
            get { return _rotationZ; }
            set { _rotationZ = value;
                InvalidateWorldMatrix();
            }
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix, Matrix parentTransform)
        {
            var combinedWorldMatrix = WorldMatrix * parentTransform;

            foreach (var tile in Tiles)
            {
                tile.Draw(viewMatrix, projectionMatrix, 
                    combinedWorldMatrix);
            }
        }

        public void DrawUnexploded(Matrix viewMatrix, Matrix projectionMatrix, Matrix parentTransform)
        {
            var combinedWorldMatrix = UnexplodedWorldMatrix * parentTransform;

            foreach (var tile in Tiles)
            {
                tile.DrawUnexploded(viewMatrix, projectionMatrix,
                    combinedWorldMatrix);
            }
        }

        private Matrix WorldMatrix
        {
            get
            {
                if (_worldMatrix == null)
                {
                    var centre = SizeInPixels/2;

                    _worldMatrix = Matrix.CreateTranslation(-centre.X, -centre.Y, 0) *
                            GetMatrixRepresentingRotations()*
                           Matrix.CreateTranslation(centre.X, centre.Y, 0)
                           *Matrix.CreateTranslation(Offset.X, Offset.Y, ZPosition);
                }

                return _worldMatrix.Value;
            }
        }

        private Matrix WorldMatrixInverse
        {
            get
            {
                if (_worldMatrixInverse == null)
                {
                    _worldMatrixInverse = Matrix.Invert(WorldMatrix);
                }

                return _worldMatrixInverse.Value;
            }
        }
        private Matrix GetMatrixRepresentingRotations()
        {
            return Matrix.CreateRotationZ(RotationZ)*
                   Matrix.CreateRotationX(RotationX)*
                   Matrix.CreateRotationY(RotationY);
        }

        /// <summary>
        /// Call the bomb squad
        /// </summary>
        private Matrix UnexplodedWorldMatrix
        {
            get
            {
                if (_unexplodedWorldMatrix == null)
                {
                    _unexplodedWorldMatrix = Matrix.CreateTranslation(InitialOffset.X, InitialOffset.Y, _unexplodedZPosition);
                }

                return _unexplodedWorldMatrix.Value;
            }
        }

        protected IList<Tile> Tiles { get; set; }

        public void CenterExplodedShapeAt(Vector2 newCenter)
        {
            var centre = SizeInPixels/2;

            ExplodedPosition = newCenter - centre;
        }

        public void OffsetInitialPositionBy(Vector2 offset)
        {
            InitialOffset = InitialOffset - offset;
            Offset = InitialOffset;
        }

        public void SetParent(Puzzle puzzle)
        {
            _parent = puzzle;
        }
    }
}
