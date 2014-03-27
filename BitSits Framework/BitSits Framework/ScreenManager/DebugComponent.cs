using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using BloomPostprocess;

namespace BitSits_Framework
{
    class DebugComponent : DrawableGameComponent
    {
        Camera2D camera = new Camera2D();

        public static bool DebugMode = false;

        public static bool ShowBonds = false;

        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont font;

        Point mousePos;

        int bloomSettingsIndex;

        KeyboardState prevKeyboardState;

        public DebugComponent(Game game)
            : base(game)
        {
            content = game.Content;
            DebugMode = true;
        }

        protected override void LoadContent()
        {
            font = content.Load<SpriteFont>("Fonts/DebugFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            MouseState mouseState = Mouse.GetState();
            Vector2 v = new Vector2(mouseState.X, mouseState.Y) / Camera2D.PhoneScale;
            mousePos = new Point((int)v.X, (int)v.Y);

            //if (keyboardState.IsKeyDown(Keys.F1) && prevKeyboardState.IsKeyUp(Keys.F1))
            //    DebugMode = !DebugMode;

#if WINDOWS
            if (keyboardState.IsKeyDown(Keys.F2) && prevKeyboardState.IsKeyUp(Keys.F2))
            {
                BitSitsGames.bloom.Visible = !BitSitsGames.bloom.Visible;
                BitSitsGames.bloom.Settings = BloomSettings.PresetSettings[5];
            }

            if (keyboardState.IsKeyDown(Keys.F3) && prevKeyboardState.IsKeyUp(Keys.F3))
            {
                bloomSettingsIndex = (bloomSettingsIndex + 1) %
                                     BloomSettings.PresetSettings.Length;

                BitSitsGames.bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
            }
#endif

            if (keyboardState.IsKeyDown(Keys.Space) && prevKeyboardState.IsKeyDown(Keys.Space))
                ShowBonds = !ShowBonds;

            prevKeyboardState = keyboardState;
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.Transform);

            float fps = (1000.0f / (float)gameTime.ElapsedGameTime.TotalMilliseconds);
            spriteBatch.DrawString(font, "fps : " + fps.ToString("00"), Vector2.Zero, Color.White);

            spriteBatch.DrawString(font, "X = " + mousePos.X + " Y = " + mousePos.Y, new Vector2(0, 20),
                Color.White);
            
#if WINDOWS
            string text = "F2 toggle bloom = " + (BitSitsGames.bloom.Visible ? "on" : "off") + "\n" +
                "F3 bloom settings = " + BitSitsGames.bloom.Settings.Name + "\n" +
                "Space toggle ShowBond = " + (ShowBonds ? "on" : "off");

            spriteBatch.DrawString(font, text, new Vector2(0, 40), Color.White);
#endif

            spriteBatch.End();
        }
    }
}
