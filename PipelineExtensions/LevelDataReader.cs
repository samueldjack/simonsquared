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
using System.IO;
using System.Linq;
using System.Text;
using SimonSquared.Online.DataContracts;

namespace PipelineExtensions
{
    class LevelDataReader
    {
        public LevelTemplate[] ReadLevelTemplates(string filename)
        {
            var fileLines = File.ReadAllLines(filename);
            var lineProvider = new CsvLineProvider(fileLines);

            var levels = new List<LevelTemplate>();
            lineProvider.MoveToNextLine();

            while (lineProvider.HasLine)
            {
                var levelData = ParseLevel(lineProvider);
                var levelTemplate = ConvertLevelDataToLevelTemplate(levelData);
                levels.Add(levelTemplate);
            }

            return levels.ToArray();
        }

        private LevelTemplate ConvertLevelDataToLevelTemplate(Level levelData)
        {
            var shapes = new List<ShapeData>();

            for (int shapeId = 1; shapeId <= levelData.ShapeCount; shapeId++)
            {
                var shapeData = ExtractShapeDataFromShapeMask(shapeId, levelData.ShapeMasks, levelData.TileColors);

                shapes.Add(shapeData);
            }

            return new LevelTemplate()
                       {
                           Shapes = shapes.ToArray()
                       };
        }

        private Level ParseLevel(CsvLineProvider lineProvider)
        {
            var level = new Level();

            ConsumeTagLine(lineProvider, "Level");
            ConsumeTagCell(lineProvider, "ShapeCount");

            level.ShapeCount = lineProvider.Cells.CurrentCellAsInt();
            lineProvider.MoveToNextLine();

            ConsumeTagLine(lineProvider, "TileColors");

            level.TileColors = ParseIntArray(lineProvider);

            ConsumeTagLine(lineProvider, "ShapeMasks");

            level.ShapeMasks = ParseIntArray(lineProvider);

            ConsumeTagLine(lineProvider, "ShapeMoves");
            for (int i = 0; i < level.ShapeCount; i++)
            {
                lineProvider.MoveToNextLine();
            }

            return level;
        }

        private int[][] ParseIntArray(CsvLineProvider lineProvider)
        {
            var lines = new List<int[]>();

            while (lineProvider.HasLine && !lineProvider.Cells.CurrentCellIsTag())
            {
                var cells = new List<int>();

                while (lineProvider.Cells.HasCell && lineProvider.Cells.CurrentCell != "")
                {
                    cells.Add(lineProvider.Cells.CurrentCellAsInt());
                    lineProvider.Cells.MoveToNextCell();
                }
                lines.Add(cells.ToArray());

                lineProvider.MoveToNextLine();
            }

            return lines.ToArray();
        }

        private void ConsumeTagCell(CsvLineProvider lineProvider, string tag)
        {
            AssertCurrentCellHasTag(lineProvider, tag);
            lineProvider.Cells.MoveToNextCell();
        }

        private void ConsumeTagLine(CsvLineProvider lineProvider, string tag)
        {
            AssertCurrentCellHasTag(lineProvider, tag);
            lineProvider.MoveToNextLine();
        }

        private void AssertCurrentCellHasTag(CsvLineProvider lineProvider, string tag)
        {
            if (lineProvider.Cells.CurrentCell != MakeTagString(tag))
            {
                throw new Exception("Unexpected tag " + tag);
            }
        }

        private ShapeData ExtractShapeDataFromShapeMask(int shapeId, int[][] shapeMasks, int[][] tileColors)
        {
            var tileData = new List<int[]>();

            var shapeBounds = GetBoundingRectangleForShape(shapeId, shapeMasks);

            for (int row = shapeBounds.FirstRow; row <= shapeBounds.LastRow; row++)
            {
                var rowData = new List<int>();

                for (int column = shapeBounds.FirstColumn; column <= shapeBounds.LastColumn; column++)
                {
                    if (shapeMasks[row][column] == shapeId)
                    {
                        rowData.Add(tileColors[row][column]);
                    }
                    else
                    {
                        rowData.Add(0);
                    }
                }

                tileData.Add(rowData.ToArray());
            }

            tileData.Reverse();
            var puzzleHeight = shapeMasks.Count(row => row.Any(i => i > 0));

            var offset = Tuple.Create(shapeBounds.FirstColumn, (puzzleHeight - 1) - shapeBounds.LastRow);

            return new ShapeData()
                       {
                           TilePositions = tileData.ToArray(),
                           OffsetX = offset.Item1,
                           OffsetY = offset.Item2,
                       };
        }

        private ShapeBounds GetBoundingRectangleForShape(int shapeId, int[][] shapeMasks)
        {
            int firstRow = int.MaxValue;
            int lastRow = int.MinValue;
            int firstColumn = int.MaxValue;
            int lastColumn = int.MinValue;

            for (int row = 0; row < shapeMasks.Length; row++)
            {
                for (int column = 0; column < shapeMasks[row].Length; column++)
                {
                    if (shapeMasks[row][column] == shapeId)
                    {
                        firstRow = Math.Min(firstRow, row);
                        firstColumn = Math.Min(firstColumn, column);
                        lastRow = Math.Max(lastRow, row);
                        lastColumn = Math.Max(lastColumn, column);
                    }
                }
            }

            return new ShapeBounds() { FirstColumn = firstColumn, FirstRow = firstRow, LastColumn = lastColumn, LastRow = lastRow };
        }

        private string MakeTagString(string tag)
        {
            return "$" + tag + "$";
        }

        private class CsvLineProvider
        {
            private List<string> _lines;
            private int _currentLineIndex = -1;
            private CsvCellProvider _cellsProvider;

            public CsvLineProvider(IEnumerable<string> lines)
            {
                _lines = lines.ToList();
            }

            public bool MoveToNextLine()
            {
                _currentLineIndex++;

                if (HasLine)
                {
                    _cellsProvider = new CsvCellProvider(_lines[_currentLineIndex]);
                    _cellsProvider.MoveToNextCell();
                }

                return HasLine;
            }

            public CsvCellProvider Cells
            {
                get { return _cellsProvider; }
            }

            public bool HasLine
            {
                get { return _currentLineIndex < _lines.Count; }
            }
        }

        private class CsvCellProvider
        {
            private List<string> _cells;
            private int _currentCellIndex = -1;

            public CsvCellProvider(string line)
            {
                _cells = line.Split(',').ToList();
            }

            public bool MoveToNextCell()
            {
                _currentCellIndex++;
                return HasCell;
            }

            public bool HasCell
            {
                get { return _currentCellIndex < _cells.Count; }
            }

            public string CurrentCell
            {
                get { return _cells[_currentCellIndex]; }
            }

            public int CurrentCellAsInt()
            {
                return int.Parse(CurrentCell);
            }

            public bool CurrentCellIsTag()
            {
                return CurrentCell.StartsWith("$");
            }
        }

        private class Level
        {
            public int ShapeCount { get; set; }
            public int[][] TileColors { get; set; }
            public int[][] ShapeMasks { get; set; }
        }

        internal class ShapeBounds
        {
            public int FirstColumn { get; set; }

            public int FirstRow { get; set; }

            public int LastColumn { get; set; }

            public int LastRow { get; set; }
        }
    }
   
}
