using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace BitSits_Framework
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    abstract class MenuScreen : GameScreen
    {
        #region Fields


        protected Camera2D camera = new Camera2D();

        List<MenuEntry> menuEntries = new List<MenuEntry>();
        int selectedEntry = 0;
        protected bool isMouseOver;

        protected float titleSize = 50;
        protected string titleString = string.Empty;

        protected Texture2D titleTexture;

        protected Vector2 titlePosition = Vector2.Zero;
        

        #endregion

        #region Properties


        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<MenuEntry> MenuEntries
        {
            get { return menuEntries; }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen()
        {
            // menus generally only need Tap for menu selection
            EnabledGestures = GestureType.Tap;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            // Move to the previous menu entry?
            if (input.IsMenuUp(ControllingPlayer))
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = menuEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (input.IsMenuDown(ControllingPlayer))
            {
                selectedEntry++;

                if (selectedEntry >= menuEntries.Count)
                    selectedEntry = 0;
            }

            isMouseOver = false;
            Vector2 mousePos = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y) / Camera2D.PhoneScale;

            camera.HandleInput(input, ControllingPlayer);

            mousePos = ((camera.Position / Camera2D.PhoneScale - ScreenManager.GameContent.PlayArea / 2 / camera.Scale)
                + mousePos / camera.Scale);

            for (int i = 0; i < menuEntries.Count; i++)
            {
                Point m = new Point((int)mousePos.X, (int)mousePos.Y);
                if (menuEntries[i].BoundingRectangle.Contains(m))
                {
                    isMouseOver = true;
                    selectedEntry = i;
                }
            }

            // Accept or cancel the menu? We pass in our ControllingPlayer, which may
            // either be null (to accept input from any player) or a specific index.
            // If we pass a null controlling player, the InputState helper returns to
            // us which player actually provided the input. We pass that through to
            // OnSelectEntry and OnCancel, so they can tell which player triggered them.
            PlayerIndex playerIndex;

            //if (input.IsMenuSelect(ControllingPlayer, out playerIndex)
            if (isMouseOver && (input.IsMenuSelect(ControllingPlayer, out playerIndex) || input.IsMouseLeftButtonClick()))
            {
                OnSelectEntry(selectedEntry, PlayerIndex.One);
            }
            else if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
            {
                OnCancel(playerIndex);
            }
        }


        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
        {
            menuEntries[selectedEntry].OnSelectEntry(playerIndex);
        }


        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel(PlayerIndex playerIndex)
        {
            ExitScreen();
        }


        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            camera.Update(gameTime);

            // Update each nested MenuEntry object.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry) && isMouseOver;

                menuEntries[i].Update(isSelected, gameTime);
            }
        }


        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.Transform);

            // Draw the menu title.
            Color titleColor = Color.White * TransitionAlpha;
            float titleScale = 1.25f;

            if (titleTexture == null)
                spriteBatch.DrawString(ScreenManager.GameContent.symbolFont, titleString, titlePosition,
                    titleColor, 0, Vector2.Zero, titleScale, SpriteEffects.None, 0);
            else
                spriteBatch.Draw(titleTexture, titlePosition, Color.White * TransitionAlpha);

            // Draw each menu entry in turn.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];

                bool isSelected = IsActive && (i == selectedEntry) && isMouseOver;

                menuEntry.Draw(isSelected, gameTime);
            }

            spriteBatch.End();
        }


        #endregion
    }
}
