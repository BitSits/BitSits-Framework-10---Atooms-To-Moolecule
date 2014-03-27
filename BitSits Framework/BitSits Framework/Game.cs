using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BloomPostprocess;
using GameDataLibrary;

namespace BitSits_Framework
{
    /// <summary>
    /// Sample showing how to manage different game states, with transitions
    /// between menu screens, a loading screen, the game itself, and a pause
    /// menu. This main game class is extremely simple: all the interesting
    /// stuff happens in the ScreenManager component.
    /// </summary>
    public class BitSitsGames : Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

#if WINDOWS
        public static BloomComponent bloom;
#endif

        public static ScoreData ScoreData = new ScoreData();
        public static Settings Settings = new Settings();

        public static Vector2 ViewportSize;

        #endregion

        #region Initialization


        /// <summary>
        /// The main game constructor.
        /// </summary>
        public BitSitsGames()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

            ScoreData = ScoreData.Load(GameContent.MaxLevelIndex);
            Settings = Settings.Load();

#if WINDOWS
            graphics.IsFullScreen = Settings.IsFullScreen;
            ViewportSize = new Vector2(800, 600);
#endif
#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            ViewportSize = new Vector2(640, 480);
            graphics.IsFullScreen = true;
#endif
            graphics.PreferredBackBufferWidth = (int)ViewportSize.X;
            graphics.PreferredBackBufferHeight = (int)ViewportSize.Y;

            IsMouseVisible = true;

            // Create the screen manager component.
            screenManager = new ScreenManager(this, graphics);
            Components.Add(screenManager);

#if WINDOWS
            //graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;

            bloom = new BloomComponent(this);
            Components.Add(bloom);

            bloom.Settings = BloomSettings.PresetSettings[5 % BloomSettings.PresetSettings.Length];
#endif

#if DEBUG && WINDOWS
            Components.Add(new DebugComponent(this));

            //Level Menu
            LoadingScreen.Load(screenManager, false, null, new BackgroundScreen(), new MainMenuScreen());

            //LoadingScreen.Load(screenManager, false, PlayerIndex.One, new BackgroundScreen(), new MainMenuScreen()
            //    , new LabScreen());

            //LoadingScreen.Load(screenManager, false, PlayerIndex.One, new GameplayScreen());
#else
            //graphics.IsFullScreen = true;
            LoadingScreen.Load(screenManager, true, null, new BackgroundScreen(), new MainMenuScreen());
#endif
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            ScoreData.Save();
            Settings.Save();

            base.OnExiting(sender, args);
        }


        #endregion

        #region Draw


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
#if WINDOWS
            bloom.BeginDraw();
#endif

            graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (BitSitsGames game = new BitSitsGames())
            {
                game.Run();
            }
        }
    }

    #endregion
}
