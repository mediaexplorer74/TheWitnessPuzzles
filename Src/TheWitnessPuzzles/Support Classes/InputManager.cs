using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameManager
{

    public static class InputManager
    {
        private static Game Game;

        private static MouseState _lastMouseState;
        public static bool MouseLeftClicked { get; private set; }
        public static bool MouseRightClicked { get; private set; }


        private static int moveStep = 5;    // used for keyboard movement
        private static bool mouseLocked = false; // if mouse is locked in center of window

        private static List<GestureSample> gestures;
        private static KeyboardState prevKB;
        private static MouseState prevMouse;

        public static bool IsFocused { get; set; }


        private static Vector2 _direction;
        public static Vector2 Direction
        {
            get
            {
                return _direction;
            }
        }

        public static bool Moving
        {
            get
            {
                return _direction != Vector2.Zero;
            }
        }


        public static void Initialize(Game game)
        {
            Game = game;
            IsFocused = game.IsActive;
            Update();
        }

        /*
        public static void Update1()
        {
            gestures = GetGestures().ToList();
            prevMouse = Mouse.GetState();
            prevKB = Keyboard.GetState();

            //    if (mouseLocked && IsFocused)
            //        ResetMouseToCenter();
        }
        */

        private static IEnumerable<GestureSample> GetGestures()
        {
            if (IsFocused)
            {
                while (TouchPanel.IsGestureAvailable)
                    yield return TouchPanel.ReadGesture();
            }           
        }

        public static void Update()
        {
            //Experimental ---------------------------

            gestures = GetGestures().ToList();
            prevMouse = Mouse.GetState();
            prevKB = Keyboard.GetState();

            //    if (mouseLocked && IsFocused)
            //        ResetMouseToCenter();

            // ---------------------------------------
            _direction = Vector2.Zero;

            // Kbd handle

            var keyboardState = Keyboard.GetState();

            //if (keyboardState.GetPressedKeyCount() > 0)
            if (keyboardState.GetPressedKeys().Length > 0)
            {
                if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                    _direction.X--;

                if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                    _direction.X++;

                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                    _direction.Y--;

                if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                    _direction.Y++;
            }

            // Mouse handle
            MouseLeftClicked = (Mouse.GetState().LeftButton == ButtonState.Pressed)
                && (_lastMouseState.LeftButton == ButtonState.Released);

            MouseRightClicked = (Mouse.GetState().RightButton == ButtonState.Pressed)
                && (_lastMouseState.RightButton == ButtonState.Released);
            _lastMouseState = Mouse.GetState();


        }

        public static Point? GetTapPosition()
        {
            if (IsFocused)
            {
                try
                {

                    // Touch screen first
                    foreach (var gesture in gestures)
                    {
                        if (gesture.GestureType == GestureType.Tap)
                            return gesture.Position.ToPoint();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ex] Gestures handling error: " + ex.Message);
                }

                // Then mouse
                if (prevMouse.LeftButton == ButtonState.Released
                    && Mouse.GetState().LeftButton == ButtonState.Pressed && Game.IsActive)
                {
                    Point mousePoint = Mouse.GetState().Position;
                    if (Game.GraphicsDevice.Viewport.Bounds.Contains(mousePoint))
                        return mousePoint;
                }
            }

            return null;
        }

        public static Vector2 GetDragVector()
        {
            Vector2 result = Vector2.Zero;

            if (IsFocused)
            {
                // gestures
                try
                {
                    foreach (var gesture in gestures.Where(x => x.GestureType == GestureType.FreeDrag))
                    {
                        result += gesture.Delta;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ex] Gestures handling error: " + ex.Message);
                }
            
                // kbd 
                if (Keyboard.GetState().IsKeyDown(Keys.Right)) result.X += moveStep;
                if (Keyboard.GetState().IsKeyDown(Keys.Left)) result.X -= moveStep;
                if (Keyboard.GetState().IsKeyDown(Keys.Down)) result.Y += moveStep;
                if (Keyboard.GetState().IsKeyDown(Keys.Up)) result.Y -= moveStep;

                // mouse
                if (mouseLocked)
                {
                    Point center = Game.Window.ClientBounds.Center - Game.Window.ClientBounds.Location;
                    Point mousePos = Mouse.GetState().Position;
                    result += (mousePos - center).ToVector2();
                }
            }

            return result * SettingsManager.Sensitivity;
        }

        public static void LockMouse()
        {
            mouseLocked = true;
            Game.IsMouseVisible = false;
            ResetMouseToCenter();
        }
        public static void UnlockMouse()
        {
            mouseLocked = false;
            Game.IsMouseVisible = true;
        }
        private static void ResetMouseToCenter()
        {
            Point center = Game.Window.ClientBounds.Center - Game.Window.ClientBounds.Location;
            Mouse.SetPosition(center.X, center.Y);
        }

        /// <summary>
        /// Returns True if key has just been pressed down. If not pressed or being held => False
        /// </summary>
        public static bool IsKeyPressed(Keys key)
        {
            return IsFocused ? prevKB.IsKeyUp(key) && Keyboard.GetState().IsKeyDown(key) : false;
        }
    }
}
