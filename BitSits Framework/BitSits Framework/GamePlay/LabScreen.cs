using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using GameDataLibrary;

namespace BitSits_Framework
{
    class LabScreen : MenuScreen
    {
        GameContent gameContent;
        Level level;

        bool editMode = true;

        int numberOfEntries = 2, maxEntries, startEntryIndex = 0;
        List<MenuEntry> eqMenuEntry = new List<MenuEntry>();
        List<string> eqipFooters = new List<string>();

        public LabScreen()
            : base()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            gameContent = ScreenManager.GameContent;

            maxEntries = gameContent.labEquipButtons.Length;

            gameContent.levelIndex = -1;
            level = new Level(gameContent);

            eqipFooters = gameContent.content.Load<List<string>>("Graphics/labEquipFooters");

            AddEntries();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive) level.Update(gameTime);

            //if (MediaPlayer.State == MediaState.Stopped)
            //    MediaPlayer.Play(gameContent.GameMusic[gameContent.random.Next(gameContent.GameMusic.Length)]);
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            if (input == null) throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
                ScreenManager.AddScreen(new PauseMenuScreen(this), ControllingPlayer);

            level.isMenuEntrySelected = isMouseOver; //Dont handle if menu entries are selected
            level.HandleInput(input, playerIndex);
        }

        void ToggleEditMode(object sender, PlayerIndexEventArgs e)
        {
            editMode = !editMode;

            AddEntries();

            level.ToggleEditMode();
        }

        void AddEntries()
        {
            MenuEntries.Clear();
            MenuEntry menuEntry;

            if (editMode)
            {
                menuEntry = new MenuEntry(this, gameContent.labAtoomButton, new Vector2(650, 50));
                menuEntry.Selected += ToggleEditMode;
                menuEntry.footers = "Atoomic View: left click to add and\n         right click to remove atooms";
                MenuEntries.Add(menuEntry);

                menuEntry = new MenuEntry(this, gameContent.labClampButton, new Vector2(450, 50));
                menuEntry.Selected += level.ClampEquipment;
                menuEntry.footers = "Clamp It: clamp some of the lab equipments";
                MenuEntries.Add(menuEntry);

                menuEntry = new MenuEntry(this, gameContent.labPrevButton, new Vector2(125, 60));
                menuEntry.Selected += GetPrev;
                MenuEntries.Add(menuEntry);

                menuEntry = new MenuEntry(this, gameContent.labNextButton, new Vector2(370, 60));
                menuEntry.Selected += GetNext;
                MenuEntries.Add(menuEntry);

                GetEquipMenuEntries();
            }
            else
            {
                menuEntry = new MenuEntry(this, gameContent.labEquipButton, new Vector2(650, 50));
                menuEntry.Selected += ToggleEditMode;
                menuEntry.footers = "Equipment View: add, move and rotate lab equipments";
                MenuEntries.Add(menuEntry);
            }

            menuEntry = new MenuEntry(this, gameContent.labClearButton, new Vector2(650, 150));
            menuEntry.Selected += level.ClearLAB;
            if (editMode) menuEntry.footers = "Clear all the equipments form Lab";
            else menuEntry.footers = "Clear all Atooms from Lab";
            MenuEntries.Add(menuEntry);

            foreach (MenuEntry m in MenuEntries) m.footerPosition = new Vector2(70, 545);

#if WINDOWS_PHONE
            foreach (MenuEntry m in MenuEntries) m.footerPosition = new Vector2(-1000); // hide
#endif
        }

        void GetPrev(object sender, PlayerIndexEventArgs e)
        {
            if (startEntryIndex - numberOfEntries >= 0) startEntryIndex -= numberOfEntries;
            GetEquipMenuEntries();
        }

        void GetNext(object sender, PlayerIndexEventArgs e)
        {
            if (startEntryIndex + numberOfEntries < maxEntries) startEntryIndex += numberOfEntries;
            GetEquipMenuEntries();
        }

        void GetEquipMenuEntries()
        {
            //Remove previous ones
            for (int i = eqMenuEntry.Count - 1; i >= 0; i--) MenuEntries.Remove(eqMenuEntry[i]);

            for (int i = 0; i < numberOfEntries; i++)
            {
                int equipIndex = startEntryIndex + i;
                if (equipIndex == maxEntries) break;

                MenuEntry menuEntry = new MenuEntry(this, gameContent.labEquipButtons[equipIndex],
                    new Vector2(170 + i * 100, 50));
                menuEntry.UserData = (EquipmentName)(equipIndex);
                menuEntry.footers = eqipFooters[equipIndex];
                menuEntry.footerPosition = new Vector2(70, 545);

#if WINDOWS_PHONE
                menuEntry.footerPosition = new Vector2(-1000); // hide
#endif

                menuEntry.Selected += level.AddEqipment;

                MenuEntries.Add(menuEntry); eqMenuEntry.Add(menuEntry);
            }
        }

        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            // Do nothing
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.Transform);

            level.Draw(gameTime, ScreenManager.SpriteBatch);

            //Hud back
            //if (editMode)
            //    spriteBatch.Draw(gameContent.blank, new Rectangle(150, 50, 530, 80), new Color(Color.Black, 0.2f));

            spriteBatch.End();

#if WINDOWS_PHONE
            Camera2D camera2 = new Camera2D();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera2.Transform);

            Vector2 ori = new Vector2(gameContent.labNextButton.Width, gameContent.labNextButton.Height) / 2;
            spriteBatch.Draw(gameContent.labNextButton, new Vector2(775, 300) - ori, Color.White * TransitionAlpha);
            spriteBatch.Draw(gameContent.labPrevButton, new Vector2(25, 300) - ori, Color.White * TransitionAlpha);

            if (!editMode)
            {
                spriteBatch.Draw(gameContent.labNextButton, new Vector2(400, 25), null,
                    Color.White * TransitionAlpha, -90f / 180 * (float)Math.PI, ori, 1, SpriteEffects.None, 1);

                spriteBatch.Draw(gameContent.labNextButton, new Vector2(400, 585), null,
                    Color.White * TransitionAlpha, 90f / 180 * (float)Math.PI, ori, 1, SpriteEffects.None, 1);
            }

            spriteBatch.End();
#endif

            base.Draw(gameTime);
        }
    }
}
