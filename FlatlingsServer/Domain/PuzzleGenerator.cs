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
using System.Web;
using SimonSquared.Online.DataContracts;

namespace FlatlingsServer.Domain
{
    public class PuzzleGenerator
    {
        private readonly LevelDataRepository _levelDataRepository;

        public PuzzleGenerator(LevelDataRepository levelDataRepository)
        {
            _levelDataRepository = levelDataRepository;
        }

        public Round GenerateRound(int roundsPlayed)
        {
            var puzzles = new List<PuzzleDto>();

            for (int i = 0; i < 5; i++)
            {
                puzzles.Add(GenerateNewPuzzle(roundsPlayed * 5 + i));
            }

            return new Round() {Puzzles = puzzles};
        }

        private PuzzleDto GenerateNewPuzzle(int levelNumber)
        {
            var levelData = _levelDataRepository.GetLevel(levelNumber);

            var numberOfShapes = levelData.Shapes.Length;

            var numberOfMoves = numberOfShapes + Math.Min((int)(((double) levelNumber/15)*numberOfShapes), 2*numberOfShapes);
            var puzzle = new PuzzleDto()
                             {
                                 LevelTemplate = levelData,
                                 Deformations = GetDeformationSequence(numberOfShapes).Take(numberOfMoves).ToList(),
                             };

            return puzzle;
        }

        private static IEnumerable<Deformation> GetDeformationSequence(int shapeCount)
        {
            Dictionary<int, IList<ShapeTransformation>> availableDeformations = Enumerable.Range(0, shapeCount).ToDictionary(shape => shape,
                                                                                              shape =>
                                                                                              GetDeformationList());
            var random = new Random();
            int loopsWithoutFindingHit = 0;

            var shuffledShapes = Enumerable.Range(0, shapeCount).ToList().Shuffle();

            foreach (var shape in shuffledShapes.Repeat())
            {
                var shapeDeformations = availableDeformations[shape];
                if (shapeDeformations.Count > 0)
                {
                    var deformation = shapeDeformations[random.Next(0, shapeDeformations.Count)];
                    shapeDeformations.Remove(deformation);
                    yield return new Deformation() { ShapeIndex = shape, Transformation = deformation };
                }
                else
                {
                    loopsWithoutFindingHit++;
                    if (loopsWithoutFindingHit > 10)
                    {
                        yield break;
                    }
                }
            }
        }

        private static IList<ShapeTransformation> GetDeformationList()
        {
            return new List<ShapeTransformation>
                       {
                           ShapeTransformation.FlipHorizontal,
                           ShapeTransformation.FlipVertical,
                           ShapeTransformation.RotateAnticlockwise,
                           ShapeTransformation.RotateClockwise,
                       };
        }
    }
}