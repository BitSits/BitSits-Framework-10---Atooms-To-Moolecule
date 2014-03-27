using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace BitSits_Framework
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen() : base() { }


        public override void LoadContent()
        {
            titleTexture = ScreenManager.GameContent.mainMenuTitle;

            // Create our menu entries.
            MenuEntry playGameMenuEntry = new MenuEntry(this, "Continue", new Vector2(470, 240));
            MenuEntry newGameMenuEntry = new MenuEntry(this, "New Game", new Vector2(440, 310));
            MenuEntry labSetupMenuEntry = new MenuEntry(this, "LAB setup", new Vector2(480, 380));
            MenuEntry creditsMenuEntry = new MenuEntry(this, "Credits", new Vector2(500, 450));
            MenuEntry optionsMenuEntry = new MenuEntry(this, "Options", new Vector2(400, 520));
            MenuEntry exitMenuEntry = new MenuEntry(this, "Exit", new Vector2(650, 520));

            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            newGameMenuEntry.Selected += NewGameMenuEntrySelected;
            labSetupMenuEntry.Selected += LabSetupMenuEntrySelected;
            creditsMenuEntry.Selected += CreditsMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            exitMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            if (BitSitsGames.ScoreData.CurrentLevel > 0)
                MenuEntries.Add(playGameMenuEntry);

            MenuEntries.Add(newGameMenuEntry);
            MenuEntries.Add(labSetupMenuEntry);
            MenuEntries.Add(creditsMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            //if (MediaPlayer.State == MediaState.Stopped) MediaPlayer.Play(ScreenManager.GameContent.MenuMusic);
        }

        #endregion

        #region Update and Handle Input


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            GameContent gameContent = ScreenManager.GameContent;

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            //if (MediaPlayer.State == MediaState.Stopped) MediaPlayer.Play(gameContent.MenuMusic);
        }


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new LevelMenuScreen(), e.PlayerIndex);
        }


        void NewGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            BitSitsGames.ScoreData.LoadDefault(GameContent.MaxLevelIndex);

            PlayGameMenuEntrySelected(sender, e);
        }


        void LabSetupMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, e.PlayerIndex, new LabScreen());
        }


        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }


        void CreditsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new MessageBoxScreen(ScreenManager.GameContent.credits, false), e.PlayerIndex);
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
#if WINDOWS_PHONE
            ScreenManager.Game.Exit();
#endif
        }


        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        #endregion
    }
}
