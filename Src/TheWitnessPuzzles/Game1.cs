// Game1 (Main)

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using TheWitnessPuzzles;
using System.Threading;

namespace GameManager
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        public static ScreenManager _screenManager;
        SpriteBatch _spriteBatch;
   
        Point defaultScreenSize = new Point(800, 480);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this) 
            {
                SynchronizeWithVerticalRetrace = true 
            };
            //_graphics = new GraphicsDeviceManager(this);
            
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            //Create a new instance of the Screen Manager

            _screenManager = new ScreenManager();//(this);

            //RnD
            //Components.Add(Game1._screenManager);

            IsMouseVisible = true;



            IsFixedTimeStep = false;
            
            //Content.RootDirectory = "Content";
            
            InputManager.Initialize(this);
            
            SettingsManager.LoadSettings();
            
            FileStorageManager.MigrateInternalToExternal();

//#if WINDOWS
            // This method call is present twice (here and in the Initialize method later), because of cross-platform magic
            // On Windows GraphicsDevice should me initialized in constructor, otherwise window size will be default and not corresponding to the backbuffer
            // But on Android GraphicsDevice is still null after ApplyChanges() if it's called in constructor
            InitializeGraphicsDevice();
//#endif

            Window.ClientSizeChanged += ResizeScreen;

            this.Activated += (object sender, EventArgs e) =>
            {
                InputManager.IsFocused = true;
            };
            
            this.Deactivated += (object sender, EventArgs e) =>
            {
                InputManager.IsFocused = false;
            };

//#if ANDROID
//            SettingsManager.OrientationLockChanged += () =>
//            {
//                SetScreenOrientation(Window.CurrentOrientation);
//                graphics.ApplyChanges();
//            };
//
//#endif
        }

        private void ResizeScreen(object sender, EventArgs e)
        {
#if ANDROID
            // When the game launches in the landscape mode it calls ResizeScreen before GraphicsDevice gets initialized
            if (GraphicsDevice == null || graphics == null)
                return;

            // On my andoird 4.0.3 i have weird behaviour when backbuffer is automatically being resized wrong
            // So i update it manually, TitleSafeArea has the right size of the free screeen area
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.TitleSafeArea.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.TitleSafeArea.Height;
            graphics.ApplyChanges();

            // Apply screen resize to active GameScreen
            ScreenManager.Instance.UpdateScreenSize(GraphicsDevice.DisplayMode.TitleSafeArea.Size);

            // Dirteh haxx here!
            // Touch panel behaves weirdly after screen rotation, like being stretched horizontally, 
            // so the coordinates of the touch point is not where your finger is, but slightly to the left.
            // But! If you call ApplyChanges again, then it gets back to normal.
            // But! You can't call it right away, there should be slight delay for hacky magic to happen.
            // I know, right?! (.__.)
            System.Threading.Tasks.Task.Run(() =>
            {
                Thread.Sleep(200);
                graphics.ApplyChanges();
            });
#else
            // But on PC it actually should be Window size, not the TitleSafeArea
            _screenManager.UpdateScreenSize(Window.ClientBounds.Size);
#endif

        }

        public void SetFullscreen(bool isFullscreen)
        {
#if ANDROID
            graphics.IsFullScreen = isFullscreen;
#else
            if (isFullscreen)
            {
                //Window.IsBorderless = true;
                //Window.Position = Point.Zero;
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                //Window.IsBorderless = false;
                _graphics.PreferredBackBufferWidth = defaultScreenSize.X;
                _graphics.PreferredBackBufferHeight = defaultScreenSize.Y;
            }
#endif
            _graphics.ApplyChanges();
            ResizeScreen(null, null);
        }
        
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if ANDROID
            // This method call is present twice (here and in the Constructor earlier), because of cross-platform magic
            // On Windows GraphicsDevice should me initialized in constructor, otherwise window size will be default and not corresponding to the backbuffer
            // But on Android GraphicsDevice is still null after ApplyChanges() if it's called in constructor
            InitializeGraphicsDevice();
#endif
            //RnD
            InitializeGraphicsDevice();

            _screenManager.Initialize(this, GraphicsDevice, Content);

            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            

            _screenManager.ScreenSize = new Point
                (_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            _graphics.ApplyChanges();

            Glob.Content = Content;

            base.Initialize();
        }

        private void InitializeGraphicsDevice()
        {
            //RnD
            _graphics.SupportedOrientations = 
                DisplayOrientation.Portrait 
                | DisplayOrientation.LandscapeLeft 
                | DisplayOrientation.LandscapeRight;

#if ANDROID

            graphics.IsFullScreen = SettingsManager.IsFullscreen;
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            SetScreenOrientation(SettingsManager.ScreenOrientation);
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();

#else

            if (SettingsManager.IsFullscreen)
            {
                //Window.IsBorderless = true;
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                //Window.Position = Point.Zero;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = defaultScreenSize.X;
                _graphics.PreferredBackBufferHeight = defaultScreenSize.Y;
            }

            Window.AllowUserResizing = true;
            IsMouseVisible = true;

            //RnD
            _graphics.IsFullScreen = SettingsManager.IsFullscreen;
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            //SetScreenOrientation(SettingsManager.ScreenOrientation);
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.ApplyChanges();      
#endif
        }

#if ANDROID
        private void SetScreenOrientation(DisplayOrientation orientation)
        {
            if (SettingsManager.IsOrientationLocked)
            {
                SettingsManager.ScreenOrientation = graphics.SupportedOrientations = orientation;
                switch (orientation)
                {
                    case DisplayOrientation.LandscapeLeft:
                        Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape; break;
                    case DisplayOrientation.LandscapeRight:
                        Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.ReverseLandscape; break;
                    case DisplayOrientation.Portrait:
                        Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait; break;
                }
            }
            else
            {
                graphics.SupportedOrientations = DisplayOrientation.Portrait | DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
                Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Sensor;
            }
        }
#endif

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Glob.SpriteBatch = _spriteBatch;

            _screenManager.LoadContent();
            
            SoundManager.LoadContent(Content);

            InitializeAfterContentIsLoaded();
        }

        protected virtual void InitializeAfterContentIsLoaded()
        {
            _screenManager.AddScreen<MenuGameScreen>();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Go to the previous screen when Back button is pressed. If we are at the last screen, then Exit.
            if (InputManager.IsFocused 
                && (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed 
                || Keyboard.GetState().IsKeyDown(Keys.Escape)))
            {
                bool? goBackResult = _screenManager.GoBack();
                if (goBackResult != null)
                {
                    SoundManager.PlayOnce(Sound.MenuEscape);
                    if (goBackResult == false)
                    {
//#if WINDOWS
                        Exit();
//#else
//                        // If you Exit() app in Android, it wont come back after restart until you manually kill the process
//                        // This is actually a bug of MonoGame 3.6
//                        //Activity.MoveTaskToBack(true);
//#endif
                    }
                }
            }

            //if (InputManager.IsKeyPressed(Keys.Enter))
            //    ScreenManager.Instance.AddScreen<PanelGameScreen>(true, true, DI.Get<PanelGenerator>().GeneratePanel());

            Glob.Update(gameTime);            
            _screenManager.Update(gameTime);            
            InputManager.Update();
            base.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _screenManager.Draw(_spriteBatch);            

            _spriteBatch.End();            

            base.Draw(gameTime);
        }
    }
}

