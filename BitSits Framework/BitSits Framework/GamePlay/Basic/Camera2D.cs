using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BitSits_Framework
{
    /// <summary>
    /// Very basic sample program for demonstrating a 2D Camera
    /// Controls are WASD for movement, QE for rotation, and ZC for zooming.
    /// </summary>
    class Camera2D
    {
        Vector2 viewportSize;
        bool ManualCamera = false, isMovingUsingScreenAxis = true;

        Vector2 mousePos;

        public Vector2 Position, ScrollArea, ScrollBar, Origin;
        public float Rotation, Scale = 1, Speed = 0;

#if WINDOWS
        public static float PhoneScale = 1;
#endif
#if WINDOWS_PHONE
        public static float PhoneScale = .8f;
#endif

        public Camera2D()
        {
            this.viewportSize = ScrollArea = BitSitsGames.ViewportSize;

            Origin = Position = viewportSize / 2;
        }

        public Matrix Transform
        {
            get
            {
                return Matrix.CreateScale(new Vector3(PhoneScale, PhoneScale, 0))
                    * Matrix.CreateTranslation(new Vector3(-Position, 0))
                    * Matrix.CreateRotationZ(-Rotation) * Matrix.CreateScale(new Vector3(Scale, Scale, 0))
                    * Matrix.CreateTranslation(new Vector3(Origin, 0));
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void HandleInput(InputState input, PlayerIndex? controllingPlayer)
        {
            if (ManualCamera)
            {
                //translation controls, left stick xbox or WASD keyboard
                if (Keyboard.GetState().IsKeyDown(Keys.A)
                    || GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < 0) Position.X += Speed;
                if (Keyboard.GetState().IsKeyDown(Keys.D)
                    || GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > 0) Position.X -= Speed;
                if (Keyboard.GetState().IsKeyDown(Keys.S)
                    || GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < 0) Position.Y -= Speed;
                if (Keyboard.GetState().IsKeyDown(Keys.W)
                    || GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > 0) Position.Y += Speed;

                //rotation controls, right stick or QE keyboard
                if (Keyboard.GetState().IsKeyDown(Keys.Q)
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X < 0) Rotation += 0.01f;
                if (Keyboard.GetState().IsKeyDown(Keys.E)
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X > 0) Rotation -= 0.01f;

                //zoom/scale controls, left/right triggers or CZ keyboard
                if (Keyboard.GetState().IsKeyDown(Keys.C)
                    || GamePad.GetState(PlayerIndex.One).Triggers.Right > 0) Scale += 0.001f;
                if (Keyboard.GetState().IsKeyDown(Keys.Z)
                    || GamePad.GetState(PlayerIndex.One).Triggers.Left > 0) Scale -= 0.001f; 
            }

            mousePos = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y) / PhoneScale;

#if WINDOWS_PHONE
            //if (!input.IsMouseLeftButtonClick()) mousePos = viewportSize / 2 / PhoneScale;
#endif
        }

        public void Update(GameTime gameTime)
        {
            float s = Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (mousePos.X < ScrollBar.X) Position.X -= s;
            else if (mousePos.X > viewportSize.X / PhoneScale - ScrollBar.X) Position.X += s;

            if (mousePos.Y < ScrollBar.Y) Position.Y -= s;
            else if (mousePos.Y > viewportSize.Y / PhoneScale - ScrollBar.Y) Position.Y += s;

            // Clamp
            Position.X = MathHelper.Clamp(Position.X, viewportSize.X / 2 / Scale,
                (ScrollArea.X - viewportSize.X / 2 / Scale));
            Position.Y = MathHelper.Clamp(Position.Y, viewportSize.Y / 2 / Scale,
                (ScrollArea.Y - viewportSize.Y / 2 / Scale));
        }
    }
}
