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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Flatlings.Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using SimonSquared.Online.DataContracts;
using Microsoft.Phone.Reactive;

namespace Flatlings
{
    class GameScreen : Screen
    {
        SpriteBatch _spriteBatch;
        private Vector3 _cameraPosition;
        private float _aspectRatio;
        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;
        private Storyboard _storyboard;
        private Puzzle _currentPuzzle;
        private Puzzle _oldPuzzle;
        private SpriteFont _kootenayFont;
        private Texture2D _helpButtonTexture;
        private Rectangle _helpButtonRectangle;
        private Rectangle _menuButtonRectangle;
        private IList<Vector2> _dragPoints = new List<Vector2>();
        private GameController _controller;
        private ResourceCleaner _gameControllerSubscriptions = new ResourceCleaner();
        private Texture2D _menuButtonTexture;
        private SpriteFont _titleFont;
        private string _statusText = string.Empty;
        private float _statusOpacity = 0;

        /// <summary>
        /// Creates a new instance of DrawableGameComponent.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public GameScreen(Game game) : base(game)
        {
        }

        public void StartSinglePlayerGame()
        {
            var controller = new SinglePlayerGameController(_storyboard, GetService<CommonContentManager>(), Messenger);
            ChangeController(controller);
        }

        private void StartMultiplayerGame()
        {
            var controller = new MultiPlayerGameController(GetService<MultiplayerGameManager>(), _storyboard, GetService<CommonContentManager>(), Messenger);
            ChangeController(controller);
        }

        private void ChangeController(GameController controller)
        {
            _gameControllerSubscriptions.CleanUp();

            _currentPuzzle = null;
            _oldPuzzle = null;

            if (_controller != null)
            {
                _controller.End();
            }

            _controller = controller;

            SubscribeToGameEvents(_controller);
        }

        protected override void OnShown()
        {
            base.OnShown();

            if (_controller == null)
            {
                return;
            }

            if (_controller.IsPlaying)
            {
                _controller.Resume();
            }
            else
            {
                _controller.Begin();

                var stateService = GetService<GameStateService>();
                stateService.ReportGameInProgress();
            }
        }

        private void SubscribeToGameEvents(GameController controller)
        {
            var eventsOnDispatcher = controller.GameEvents.ObserveOn(DispatcherScheduler.Current);

            var subscription = eventsOnDispatcher.Where(e => e == GameEvents.StartingNewGame)
                .Subscribe(e => HandleStartingNewGame());
            _gameControllerSubscriptions.AddResourceRequiringCleanup(subscription);

            subscription = eventsOnDispatcher.Where(e => e == GameEvents.Won)
                .Subscribe(e => HandleWonGame());
            _gameControllerSubscriptions.AddResourceRequiringCleanup(subscription);

            subscription = eventsOnDispatcher.Where(e => e == GameEvents.Lost)
                .Subscribe(e => HandleLostGame());
            _gameControllerSubscriptions.AddResourceRequiringCleanup(subscription);
        }

        private void HandleLostGame()
        {
            using (_storyboard.BeginPlanning())
            {
                _storyboard.Plan().AfterDelay(TimeSpan.FromSeconds(0.5));
                _currentPuzzle.ScriptCompletedAnimation();
            }

            using (_storyboard.BeginPlanning())
            {
                _statusText = "Loser!";
                AnimateStatusOpacity();               
            }

            _oldPuzzle = _currentPuzzle;
            _currentPuzzle = null;
        }

        private void AnimateStatusOpacity()
        {
            _statusOpacity = 1;
            _storyboard.Plan()
                .AfterDelay(TimeSpan.FromSeconds(1))
                .Begin(b => b.Animate(this, (GameScreen s, float value) => s._statusOpacity = value)
                                .From(1)
                                .To(0)
                                .In(TimeSpan.FromSeconds(0.25)));
        }

        private void HandleWonGame()
        {
            using (_storyboard.BeginPlanning())
            {
                _storyboard.Plan().AfterDelay(TimeSpan.FromSeconds(0.5));
                _currentPuzzle.ScriptCompletedAnimation();
            }

            using (_storyboard.BeginPlanning())
            {
                _statusText = "Champion!";
                AnimateStatusOpacity();
            }

            _oldPuzzle = _currentPuzzle;
            _currentPuzzle = null;
        }

        private void HandleStartingNewGame()
        {
            _currentPuzzle = _controller.GetNextPuzzle();

            using (_storyboard.BeginPlanning())
            {
                _storyboard.Plan().AfterDelay(TimeSpan.FromSeconds(1));
                _currentPuzzle.ScriptEntry();
                _storyboard.Plan().AfterDelay(TimeSpan.FromSeconds(1));
                _currentPuzzle.ScriptDeformation();
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {           
            _helpButtonRectangle = new Rectangle(10, 750, 40, 40);
            _menuButtonRectangle = new Rectangle(440, 750, 40, 40);

            Messenger.Register<StartingMultiplayerGameMessage>(this, message => StartMultiplayerGame());
            Messenger.Register<StartingSinglePlayerGameMessage>(this, message => StartSinglePlayerGame());
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _aspectRatio = Game.GraphicsDevice.Viewport.AspectRatio;

            _cameraPosition = new Vector3(0.0f, 0.0f, 220.0f);
            _viewMatrix = Matrix.CreateLookAt(_cameraPosition, Vector3.Zero, Vector3.Up);
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), _aspectRatio, 1, 1000);

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _storyboard = new Storyboard();

            _kootenayFont = Game.Content.Load<SpriteFont>("Fonts\\Kootenay");
            _titleFont = Game.Content.Load<SpriteFont>("Fonts\\TitleFont");
            _helpButtonTexture = Game.Content.Load<Texture2D>("Images\\RoundHelpButton");
            _menuButtonTexture = Game.Content.Load<Texture2D>("Images\\MenuButton");
        }

       
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void UpdateCore(GameTime gameTime)
        {
            _storyboard.AdvanceTimeTo(gameTime.TotalGameTime);

            ProcessTouchInput();

            if (_currentPuzzle != null)
            {
                if (_currentPuzzle.IsSolved())
                {
                    _controller.PuzzleCompleted();
                }
            }

            _storyboard.Update();
        }

        private void ProcessTouchInput()
        {
            while (TouchPanel.IsGestureAvailable)
            {
                var gestureSample = TouchPanel.ReadGesture();

                if (gestureSample.GestureType == GestureType.FreeDrag)
                {
                    _dragPoints.Add(gestureSample.Position);
                }
                else if (gestureSample.GestureType == GestureType.DragComplete)
                {
                    ProcessDrag(_dragPoints);
                    _dragPoints.Clear();
                }
                else if (gestureSample.GestureType == GestureType.DoubleTap)
                {
                    ProcessDoubleTap(gestureSample);
                }
                else if (gestureSample.GestureType == GestureType.Tap)
                {
                    if (_helpButtonRectangle.Contains((int) gestureSample.Position.X, (int) gestureSample.Position.Y))
                    {
                        TransitionToScreen("Help");
                    }
                    if (_menuButtonRectangle.Contains((int)gestureSample.Position.X, (int)gestureSample.Position.Y))
                    {
                        TransitionToScreen("MainMenu");
                    }
                }
            }
        }

        private void ProcessDoubleTap(GestureSample gestureSample)
        {
            var selectedShape = GetShapeNearPoint(gestureSample.Position);
            if (selectedShape != null && !selectedShape.IsAnimating)
            {
                selectedShape.MoveToHomePosition();
            }

           
        }

        private enum DragDirection
        {
            Up,
            Down,
            Left,
            Right,
            DiagonalDown,
            DiagonalUp,
        }

        private void ProcessDrag(IList<Vector2> dragPoints)
        {
            if (dragPoints.Count < 2 || _currentPuzzle == null)
            {
                return;
            }

            const float dragTolerance = 35;
            const float minimumDragDistance = 7;

            var intersectionInfo = FindIntersectedShape(dragPoints);
            if (!intersectionInfo.Intersects)
            {
                return;
            }

            var selectedShape = intersectionInfo.Shape;
            if (selectedShape.IsAnimating)
            {
                return;
            }

            var quadrant = GetRectangleFromPoints(dragPoints);
            if (quadrant.Width < minimumDragDistance && quadrant.Height < minimumDragDistance)
            {
                // we don't have a proper drag
                return;
            }

            var shapeCenter = selectedShape.GetCenterInWorldCoordinates(GraphicsDevice.Viewport, _viewMatrix, _projectionMatrix);

            var indexOfFirstPoint = intersectionInfo.IndexOfFirstHitPoint == dragPoints.Count - 1
                                        ? intersectionInfo.IndexOfFirstHitPoint - 1
                                        : intersectionInfo.IndexOfFirstHitPoint;

            var firstHitPoint = dragPoints[indexOfFirstPoint];
            var secondHitPoint = dragPoints[indexOfFirstPoint + 1];

            var firstPointRelativeToShape = new Vector3(firstHitPoint - shapeCenter, 0);
            var secondPointRelativeToShape = new Vector3(secondHitPoint - shapeCenter, 0);

            var crossProduct = Vector3.Cross(firstPointRelativeToShape, secondPointRelativeToShape);
         
            // need to decide what kind of drag: Vertical, Horizontal, or Diagonal
            DragDirection dragDirection;

            if (quadrant.Width < dragTolerance)
            {
                dragDirection = DragDirection.Up;
            }
            else if (quadrant.Height < dragTolerance)
            {
                dragDirection = DragDirection.Left;
            }
            else if (crossProduct.Z < 0)
            {
                dragDirection = DragDirection.DiagonalUp;
            }
            else
            {
                dragDirection = DragDirection.DiagonalDown;
            }

            

            switch (dragDirection)
            {
                case DragDirection.Up:
                    selectedShape.FlipX();
                    break;
                case DragDirection.Down:
                    selectedShape.FlipX();
                    break;
                case DragDirection.Left:
                    selectedShape.FlipY();
                    break;
                case DragDirection.Right:
                    selectedShape.FlipY();
                    break;
                case DragDirection.DiagonalDown:
                    selectedShape.RotateAntiClockwise();
                    break;
                case DragDirection.DiagonalUp:
                    selectedShape.RotateClockwise();
                    break;
                default:
                    break;
            }
        }

        private Shape GetShapeNearPoint(Vector2 touchPosition)
        {
            // calculate a ray that goes from the eye of the camera through the touch Position
            var rayThroughScreenPosition = touchPosition.CalculateRayThroughScreenPosition(Matrix.Identity,
                                                                                           _projectionMatrix,
                                                                                           _viewMatrix,
                                                                                           GraphicsDevice.Viewport);

            return _currentPuzzle.HitTest(rayThroughScreenPosition);
        }

        

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void DrawCore(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            if (_controller == null)
            {
                return;
            }

            if (_controller.LastGameEvent == GameEvents.StartingNewGame
                || _controller.LastGameEvent == GameEvents.Won
                || _controller.LastGameEvent == GameEvents.Lost)
            {
                if (_currentPuzzle != null)
                {
                    _currentPuzzle.Draw(_viewMatrix, _projectionMatrix);
                }

                if (_oldPuzzle != null)
                {
                    _oldPuzzle.Draw(_viewMatrix, _projectionMatrix);
                }
            }

            _spriteBatch.Begin();

            _spriteBatch.Draw(_helpButtonTexture, _helpButtonRectangle, Color.White);
            _spriteBatch.Draw(_menuButtonTexture, _menuButtonRectangle, Color.White);

            _spriteBatch.DrawString(_kootenayFont, _controller.Score.ToString(), new Vector2(0, 0), new Color(255, 0, 0));

            if (_statusOpacity > 0)
            {
                DrawStringCentered(_spriteBatch, _statusText, _titleFont, new Color(1, 1, 1, _statusOpacity), Game.GraphicsDevice.Viewport, 500);
            }
            if (_controller.LastGameEvent == GameEvents.Countdown)
            {
                DrawStringCentered(_spriteBatch, _controller.CountdownTimeRemaining.TotalSeconds.ToString("0"), _titleFont , Color.White, Game.GraphicsDevice.Viewport);
            }
            else if (_controller.LastGameEvent == GameEvents.Waiting)
            {
                DrawStringCentered(_spriteBatch, "Loading", _titleFont, Color.White, Game.GraphicsDevice.Viewport);
            }

            _spriteBatch.End();
        }

        private void DrawStringCentered(SpriteBatch spriteBatch, string text, SpriteFont font, Color color, Viewport viewport, int height)
        {
            var size = font.MeasureString(text);
            var textLeft = viewport.Width / 2 - size.X / 2;
            var textTop = height;

            spriteBatch.DrawString(font, text, new Vector2(textLeft, textTop), color);
        }

        private void DrawStringCentered(SpriteBatch spriteBatch, string text, SpriteFont font, Color color, Viewport viewport)
        {
            var size = font.MeasureString(text);
            var textLeft = viewport.Width/2 - size.X/2;
            var textTop = viewport.Height/2 - size.Y/2;

            spriteBatch.DrawString(font, text, new Vector2(textLeft, textTop), color);
        }

        private Quadrant GetRectangleFromPoints(IList<Vector2> points)
        {
            var quadrant = new Quadrant()
                               {
                                   LeftMost = new Vector2(float.MaxValue, 0),
                                   RightMost = new Vector2(float.MinValue, 0),
                                   TopMost = new Vector2(0, float.MinValue),
                                   BottomMost = new Vector2(0, float.MaxValue),
                               };

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];

                if (point.X < quadrant.LeftMost.X) { quadrant.LeftMost = point; }
                if (point.X > quadrant.RightMost.X) { quadrant.RightMost = point;  }
                if (point.Y > quadrant.TopMost.Y) { quadrant.TopMost = point;  }
                if (point.Y < quadrant.BottomMost.Y) { quadrant.BottomMost = point; }
            }

            return quadrant;
        }

        [DebuggerDisplay("Left {LeftMost} Top {TopMost} Right {RightMost} Bottom {BottomMost} Center {Center} Width {Width} Height {Height}")]
        private class Quadrant
        {
            public Vector2 TopMost;
            public Vector2 BottomMost;
            public Vector2 LeftMost;
            public Vector2 RightMost;

            public Vector2 Center
            {
                get
                {
                    return new Vector2(
                        LeftMost.X + (Width) / 2,
                        BottomMost.Y + (Height) / 2);
                }
            }

            public float Height
            {
                get { return TopMost.Y - BottomMost.Y; }
            }

            public float Width
            {
                get { return RightMost.X - LeftMost.X; }
            }
        }

        private IntersectionInfo FindIntersectedShape(IList<Vector2> dragPath)
        {
            for (int i = 0; i < dragPath.Count; i++)
            {
                var point = dragPath[i];
                var shape = GetShapeNearPoint(point);

                if (shape != null)
                {
                    return new IntersectionInfo() { IndexOfFirstHitPoint = i, Shape = shape};
                }
            }    

            return new IntersectionInfo();
        }

        private struct IntersectionInfo
        {
            public int IndexOfFirstHitPoint;
            public Shape Shape;

            public bool Intersects
            {
                get { return Shape != null; }
            }
        }
    }
}
