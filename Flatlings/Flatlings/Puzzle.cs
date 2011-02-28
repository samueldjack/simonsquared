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
using SimonSquared.Online.DataContracts;

namespace Flatlings
{
    public class PuzzleCreationContext
    {
        public Storyboard Storyboard { get; set; }
        public Model TileModel { get; set; }
        public Palette Palette { get; set; }
        public Vector2 LayoutSize { get; set; }

        public double Hardness { get; set; }
    }

    public enum PuzzleState
    {
        Incomplete,
        Solved,
    }

    public class Puzzle
    {
        private readonly Storyboard _storyboard;

        private readonly IList<Shape> _shapes;
        private readonly IList<ShapeDeformation> _deformations;
        private PuzzleState _state = PuzzleState.Incomplete;
        private Matrix? _worldMatrix;

        protected Puzzle(Storyboard storyboard, IList<Shape> shapes, IList<ShapeDeformation> deformations)
        {
            _storyboard = storyboard;
            _shapes = shapes;
            _deformations = deformations;
            Size =
                CalculatePuzzleSize(_shapes);

            foreach (var shape in shapes)
            {
                shape.SetParent(this);
            }
        }

        protected Vector2 Size { get; private set; }

        private Vector2 _offset;

        private int _playerMoveCount;

        private Matrix? _worldMatrixInverse;

        protected Vector2 Offset
        {
            get { return _offset; }
            set { 
                _offset = value;
                InvalidateWorldMatrix();
            }
        }

        private void InvalidateWorldMatrix()
        {
            _worldMatrix = null;
            _worldMatrixInverse = null;
        }

        public Matrix WorldMatrix
        {
            get
            {
                if (_worldMatrix == null)
                {
                    _worldMatrix =Matrix.CreateTranslation(Offset.X, Offset.Y, 0);
                }

                return _worldMatrix.Value;
            }
        }

        public Matrix WorldMatrixInverse
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

        public int PlayerMoveCount
        {
            get { return _playerMoveCount; }
        }

        public int TargetMoveCount { get; private set; }

        public void ScriptDeformation()
        {
            var secondsBetweenMoves = 0.2;

            _storyboard.Plan().AfterDelay(TimeSpan.FromSeconds(secondsBetweenMoves));
            

            foreach (var shape in _shapes)
            {
                shape.MoveToExplodedPosition();
            }

            _storyboard.Plan().AfterDelay(TimeSpan.FromSeconds(secondsBetweenMoves));

            foreach (var shapeDeformation in _deformations)
            {
                shapeDeformation.Deformation.Apply(shapeDeformation.Shape);

                _storyboard.Plan().AfterDelay(TimeSpan.FromSeconds(secondsBetweenMoves));
            }

            foreach (var shape in _shapes)
            {
                _storyboard.Plan().AfterDelay(TimeSpan.FromSeconds(0.1));
                shape.StartHovering();
            }

            _playerMoveCount = 0;
            TargetMoveCount = _deformations.Count;
        }

        private static IEnumerable<ShapeDeformation> GetDeformationSequence(IList<Shape> shapes)
        {
            Dictionary<Shape, IList<Deformation>> availableDeformations = shapes.ToDictionary(shape => shape,
                                                                                              shape =>
                                                                                              GetDeformationList());
            var random = new Random();
            int loopsWithoutFindingHit = 0;

            var shuffledShapes = shapes.ToList().Shuffle();

            foreach (var shape in shuffledShapes.Repeat())
            {
                var shapeDeformations = availableDeformations[shape];
                if (shapeDeformations.Count > 0)
                {
                    var deformation = shapeDeformations[random.Next(0, shapeDeformations.Count)];
                    shapeDeformations.Remove(deformation);
                    yield return new ShapeDeformation() {Shape = shape, Deformation = deformation};
                }
                else
                {
                    loopsWithoutFindingHit++;
                    if (loopsWithoutFindingHit >10)
                    {
                        yield break;
                    }
                }
            }
        }

        private static IList<Deformation> GetDeformationList()
        {
            return new List<Deformation>()
                       {
                           new FlipXDeformation(),
                           new FlipYDeformation(),
                           new RotateAntiClockwiseDeformation(),
                           new RotateClockwiseDeformation(), 
                       };
        }

        public static Puzzle CreateFromPuzzleData(PuzzleDto puzzleData, PuzzleCreationContext context)
        {
            var shapes = new List<Shape>();

            foreach (var shapeData in puzzleData.LevelTemplate.Shapes)
            {
                var shape = Shape.Create(shapeData.TilePositions, new Vector2(shapeData.OffsetX, shapeData.OffsetY), context.Storyboard, context.TileModel, context.Palette, -100);
                shapes.Add(shape);
            }

            AdjustInitialPositionsToCentreShape(shapes);
            AssignExplodedPositions(shapes, context.LayoutSize);

            var deformations = puzzleData.Deformations.Select(
                deformation => new ShapeDeformation()
                                   {
                                       Shape = shapes[deformation.ShapeIndex],
                                       Deformation = GetDeformationClassFromDto(deformation.Transformation),
                                   }).ToList();

            return new Puzzle(context.Storyboard, shapes, deformations);
        }

        private static Deformation GetDeformationClassFromDto(ShapeTransformation transformation)
        {
            switch (transformation)
            {
                case ShapeTransformation.FlipVertical:
                    return new FlipXDeformation();
                case ShapeTransformation.FlipHorizontal:
                    return new FlipYDeformation();
                case ShapeTransformation.RotateClockwise:
                    return new RotateClockwiseDeformation();
                case ShapeTransformation.RotateAnticlockwise:
                    return new RotateAntiClockwiseDeformation();
                default:
                    throw new ArgumentOutOfRangeException("transformation");
            }
        }

        public static Puzzle CreateFromLevelTemplate(LevelTemplate levelTemplate, PuzzleCreationContext context)
        {
            var shapes = new List<Shape>();

            foreach (var shapeData in levelTemplate.Shapes)
            {
                var shape = Shape.Create(shapeData.TilePositions, new Vector2(shapeData.OffsetX, shapeData.OffsetY), context.Storyboard, context.TileModel, context.Palette, -100);
                shapes.Add(shape);
            }

            AdjustInitialPositionsToCentreShape(shapes);
            AssignExplodedPositions(shapes, context.LayoutSize);

            var numberOfMoves = Math.Min(shapes.Count + (int)Math.Ceiling(context.Hardness * shapes.Count) - 1, 2 * shapes.Count);
            var deformations = GetDeformationSequence(shapes).Take(numberOfMoves).ToList();

            return new Puzzle(context.Storyboard, shapes, deformations);
        }

        private static void AdjustInitialPositionsToCentreShape(List<Shape> shapes)
        {
            var size = CalculatePuzzleSize(shapes);
            var offset = size/2;

            foreach (var shape in shapes)
            {
                shape.OffsetInitialPositionBy(offset);
            }
        }

        private static void AssignExplodedPositions(List<Shape> shapes, Vector2 layoutSize)
        {
            var separationAngle = MathHelper.TwoPi/shapes.Count;

            var ellipseA = layoutSize.X/2;
            var ellipseB = layoutSize.Y/2;

            var currentAngle = MathHelper.PiOver2;

            foreach (var shape in shapes)
            {
                var ellipseR = CalculateEllipseR(ellipseA, ellipseB, currentAngle);
                var x = ellipseR*Math.Cos(currentAngle);
                var y = ellipseR*Math.Sin(currentAngle);

                shape.CenterExplodedShapeAt(new Vector2((float)x, (float)y));

                currentAngle -= separationAngle;
            }
        }

        private static Vector2 CalculatePuzzleSize(IList<Shape> shapes)
        {
            return new Vector2(Math.Max(shapes.Select(s => s.Offset.X + s.SizeInPixels.X).Max(),
                                        shapes.Select(s => Math.Abs(s.Offset.Y) + s.SizeInPixels.Y).Max()));
        }

        private static double CalculateEllipseR(float a, float b, double theta)
        {
            var aSquared = a*a;
            var bSquared = b*b;

            var cosTheta = Math.Cos(theta);
            var cosThetaSquared = cosTheta*cosTheta;

            var r = (a*b)/(Math.Sqrt(aSquared - (aSquared - bSquared)*(cosThetaSquared)));

            return r;
        }


        public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach (var shape in _shapes)
            {
                shape.DrawUnexploded(viewMatrix, projectionMatrix, WorldMatrix);
            }

            foreach (var shape in _shapes)
            {
                shape.Draw(viewMatrix, projectionMatrix, WorldMatrix);
            }    
        }

        public Shape HitTest(Ray inputRay)
        {
            var inputRayInWorldCoords = inputRay.Transform(WorldMatrixInverse);

            return _shapes.Where(shape => shape.IsInExplodedPosition).FirstOrDefault(shape => shape.HitTest(inputRayInWorldCoords));
        }

        public bool IsSolved()
        {
            return _state == PuzzleState.Solved;
        }

        public void ScriptCompletedAnimation()
        {
            _storyboard.Plan()
                .Begin(b => b.Animate(this, (target, value) => target.Offset = value)
                                .From(Offset)
                                .To(new Vector2(0, -200))
                                .In(TimeSpan.FromSeconds(0.5)));           
        }

        public void ScriptEntry()
        {
            Offset = new Vector2(0, 200);
            _storyboard.Plan()
                .Begin(b => b.Animate(this, (target, value) => target.Offset = value)
                                .From(Offset)
                                .To(Vector2.Zero)
                                .In(TimeSpan.FromSeconds(0.5)));
        }

        public abstract class Deformation
        {
            public abstract void Apply(Shape shape);
        }

        private class FlipXDeformation : Deformation
        {
            public override void Apply(Shape shape)
            {
                shape.FlipX();
            }
        }

        private  class FlipYDeformation : Deformation
        {
            public override void Apply(Shape shape)
            {
                shape.FlipY();
            }
        }

        private class RotateClockwiseDeformation : Deformation
        {
            public override void Apply(Shape shape)
            {
                shape.RotateClockwise();
            }
        }

        private class RotateClockwiseTwiceDeformation : Deformation
        {
            public override void Apply(Shape shape)
            {
                shape.RotateClockwise();
                shape.RotateClockwise();
            }
        }

        private class RotateAntiClockwiseDeformation : Deformation
        {
            public override void Apply(Shape shape)
            {
                shape.RotateAntiClockwise();
            }
        }

        public class ShapeDeformation
        {
            public Shape Shape;
            public Deformation Deformation;
        }

        public void NotifyShapeMoved()
        {
            _playerMoveCount = PlayerMoveCount + 1;
            if (_shapes.All(shape => !shape.IsInExplodedPosition))
            {
                _state = PuzzleState.Solved;
            }
        }

        public void NotifyShapeMovedHome()
        {
            if (_shapes.All(shape => !shape.IsInExplodedPosition))
            {
                _state = PuzzleState.Solved;
            }
        }
    }
}
