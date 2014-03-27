using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using GameDataLibrary;

namespace BitSits_Framework
{
    class LevelMenuScreen : MenuScreen
    {
        #region Initialization


        public LevelMenuScreen() : base() { }


        public override void LoadContent()
        {
            GameContent gameContent = ScreenManager.GameContent;
            camera.ScrollArea.X = 1600 * Camera2D.PhoneScale;
            camera.ScrollBar = new Vector2(100, 0);
            camera.Speed = 250;

            titleTexture = gameContent.levelMenuTitle;

            MenuEntry backMenuEntry = new MenuEntry(this, "Back to Main Menu", new Vector2(400, 50));
            backMenuEntry.Selected += OnCancel;

            List<Vector2> v = gameContent.content.Load<List<Vector2>>("Graphics/levelButton");

            for (int i = 0; i < GameContent.MaxLevelIndex; i++)
            {
                MenuEntry me = new MenuEntry(this, gameContent.levelButton[i], v[i]);
                me.UserData = i;

                if (i <= BitSitsGames.ScoreData.CurrentLevel && BitSitsGames.ScoreData.HighScores[i] > 0)
                {
                    me.footers = "Atoomic \nValue " + BitSitsGames.ScoreData.HighScores[i].ToString();
                    me.footerSize = 25;
                }

                me.Selected += LoadLevelMenuEntrySelected;
                MenuEntries.Add(me);
            }
        }


        #endregion

        #region Update and Handle Input


        void LoadLevelMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            int i = (int)((MenuEntry)sender).UserData;

#if !DEBUG
            if (i > BitSitsGames.ScoreData.CurrentLevel) return;
#endif

            ScreenManager.GameContent.levelIndex = i;
            LoadingScreen.Load(ScreenManager, false, e.PlayerIndex, new GameplayScreen());
        }


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            GameContent gameContent = ScreenManager.GameContent;

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            //if (MediaPlayer.State == MediaState.Stopped)
            //    MediaPlayer.Play(gameContent.GameMusic[gameContent.random.Next(gameContent.GameMusic.Length)]);
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            GameContent gameContent = ScreenManager.GameContent;

            Camera2D camera2 = new Camera2D();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera2.Transform);

            Vector2 ori = new Vector2(gameContent.labNextButton.Width, gameContent.labNextButton.Height) / 2;
            spriteBatch.Draw(gameContent.labNextButton, new Vector2(775, 200) - ori, Color.White * TransitionAlpha);
            spriteBatch.Draw(gameContent.labPrevButton, new Vector2(25, 200) - ori, Color.White * TransitionAlpha);

            spriteBatch.End();
        }

        #endregion
    }
}
