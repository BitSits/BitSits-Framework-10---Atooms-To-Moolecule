using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using GameDataLibrary;

namespace BitSits_Framework
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields


        MenuEntry soundMenuEntry;
        MenuEntry musicMenuEntry;
        MenuEntry isFullScreenMenuEntry;


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen() : base() { }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            soundMenuEntry.Text = "Sound: " + (BitSitsGames.Settings.SoundEnabled ? "on" : "off");
            musicMenuEntry.Text = "Music: " + (BitSitsGames.Settings.MusicEnabled ? "on" : "off");
            isFullScreenMenuEntry.Text = "Full Screen: " + (BitSitsGames.Settings.IsFullScreen ? "on" : "off");
        }


        public override void LoadContent()
        {
            titleTexture = ScreenManager.GameContent.optionsTitle;

            // Create our menu entries.
            isFullScreenMenuEntry = new MenuEntry(this, string.Empty, new Vector2(320, 270));
            soundMenuEntry = new MenuEntry(this, string.Empty, new Vector2(420, 340));
            musicMenuEntry = new MenuEntry(this, string.Empty, new Vector2(430, 410));

            SetMenuEntryText();

            MenuEntry backMenuEntry = new MenuEntry(this, "Back", new Vector2(500, 500));

            // Hook up menu event handlers.
            soundMenuEntry.Selected += SoundEntrySelected;
            musicMenuEntry.Selected += MusicMenuEntrySelected;
            isFullScreenMenuEntry.Selected += IsFullScreenMenuEntrySelected;
            backMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(soundMenuEntry);
            MenuEntries.Add(musicMenuEntry);

#if WINDOWS
            //MenuEntries.Add(resolutionMenuEntry);
            MenuEntries.Add(isFullScreenMenuEntry);
#endif

            MenuEntries.Add(backMenuEntry);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Resolution menu entry is selected.
        /// </summary>
        void SoundEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            BitSitsGames.Settings.SoundEnabled = !BitSitsGames.Settings.SoundEnabled;

            if (BitSitsGames.Settings.SoundEnabled) SoundEffect.MasterVolume = 1;
            else SoundEffect.MasterVolume = 0;

            SetMenuEntryText();
        }


        /// <summary>
        /// Event handler for when the Frobnicate menu entry is selected.
        /// </summary>
        void MusicMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            BitSitsGames.Settings.MusicEnabled = !BitSitsGames.Settings.MusicEnabled;

            if (BitSitsGames.Settings.MusicEnabled) ScreenManager.GameContent.PlayMusic();
            else MediaPlayer.Pause();

            SetMenuEntryText();
        }


        /// <summary>
        /// Event handler for when the Frobnicate menu entry is selected.
        /// </summary>
        void IsFullScreenMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            BitSitsGames.Settings.IsFullScreen = !BitSitsGames.Settings.IsFullScreen;

            ScreenManager.GraphicsDeviceManager.IsFullScreen = BitSitsGames.Settings.IsFullScreen;
            ScreenManager.GraphicsDeviceManager.ApplyChanges();

            SetMenuEntryText();
        }


        #endregion
    }
}
