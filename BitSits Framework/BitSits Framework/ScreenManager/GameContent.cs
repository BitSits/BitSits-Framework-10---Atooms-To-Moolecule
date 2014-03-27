using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Box2D.XNA;
using GameDataLibrary;

#if WINDOWS_PHONE
using System.Windows;
#endif


namespace BitSits_Framework
{
    /// <summary>
    /// All the Contents of the Game is loaded and stored here
    /// so that all other screen can copy from here
    /// </summary>
    public class GameContent
    {
        public ContentManager content;
        public Vector2 PlayArea = new Vector2(800, 600);

        // Level Data
        public const int MaxLevelIndex = 8;
        public int levelIndex = 7;

        public int b2Scale = 30;

        public Random random = new Random();

        // Textures
        public Texture2D blank, gradient, blackhole;

        public Texture2D logo;
        public Texture2D credits, levelUp, retry, menuBackground, mainMenuTitle, levelMenuTitle, pauseTitle, optionsTitle;

        public Texture2D[] levelBackground = new Texture2D[MaxLevelIndex];
        public Texture2D labTable;
        public Texture2D labNextButton, labPrevButton, labClampButton, labAtoomButton,
            labEquipButton, labClearButton;

        public Texture2D[] tutorial = new Texture2D[6];

        public Texture2D[] levelButton = new Texture2D[8];
        public Texture2D[] labEquipButtons = new Texture2D[9];

        public int symbolCount = 6;
        public float atomRadius = 25;
        public Vector2 atomOrigin;
        public Texture2D[] atom;
        public Texture2D shine;

        public Texture2D[,] equipment = new Texture2D[9, 3];
        public Texture2D clamp, clampJoint, clampRod, clampStand;
        public Vector2 clampOrigin, clampJointOrigin, clampRodOrigin, clampStandOrigin;

        public int clampDistance;

        public Texture2D thermometerThread, pointer, tick, arrow;
        public Vector2 thermoThreadOrigin, pointerOrigin, tickOrigin, arrowOrigin;

        public Texture2D equipMoveButton, equipRotationButton;
        public Vector2 equipMoveButtOrigin, equipRotButtOrigin;

        public Texture2D[,] eyes = new Texture2D[2, 6];

        public Texture2D[] bond = new Texture2D[7];
        public Vector2[] bondOrigin = new Vector2[7];

        public Texture2D electroShock, radSmoke;
        public Vector2 radSmokeOrigin;

        public Texture2D cross;

        // Fonts
        public SpriteFont debugFont;
        public SpriteFont symbolFont, thermometerFont;
        public int symbolFontSize;

        // Audio objects
        //public Song MenuMusic;
        //public int musicIndex = 0;
        //public Song[] GameMusic = new Song[2];

        public SoundEffect[] radio = new SoundEffect[3], shock = new SoundEffect[3], detach = new SoundEffect[2],
            broop = new SoundEffect[5], bop = new SoundEffect[2];


        /// <summary>
        /// Load GameContents
        /// </summary>
        public GameContent(GameComponent screenManager)
        {
            content = screenManager.Game.Content;

            blank = content.Load<Texture2D>("Graphics/blank");
            blackhole = content.Load<Texture2D>("Graphics/blackhole");
            gradient = content.Load<Texture2D>("Graphics/gradient");

            menuBackground = content.Load<Texture2D>("Graphics/menuBackground");
            credits = content.Load<Texture2D>("Graphics/credits");
            mainMenuTitle = content.Load<Texture2D>("Graphics/mainMenuTitle");
            levelMenuTitle = content.Load<Texture2D>("Graphics/levelMenuTitle");
            pauseTitle = content.Load<Texture2D>("Graphics/pauseTitle");
            optionsTitle = content.Load<Texture2D>("Graphics/optionsTitle");

            levelUp = content.Load<Texture2D>("Graphics/levelUp");
            retry = content.Load<Texture2D>("Graphics/retry");

            for (int i = 0; i < levelButton.Length; i++)
                levelButton[i] = content.Load<Texture2D>("Graphics/levelButton" + i);

            for (int i = 0; i < labEquipButtons.Length; i++)
                labEquipButtons[i] = content.Load<Texture2D>("Graphics/labButton" + i);

            logo = content.Load<Texture2D>("Graphics/BitSitsGamesLogo");

            for (int i = 0; i < levelBackground.Length; i++)
                levelBackground[i] = content.Load<Texture2D>("Graphics/levelBackground" + i);

            for (int i = 0; i < tutorial.Length; i++)
                tutorial[i] = content.Load<Texture2D>("Graphics/tutorial" + i);

            labTable = content.Load<Texture2D>("Graphics/labTable");
            labNextButton = content.Load<Texture2D>("Graphics/labNextButton");
            labPrevButton = content.Load<Texture2D>("Graphics/labPrevButton");
            labClampButton = content.Load<Texture2D>("Graphics/labClampButton");
            labAtoomButton = content.Load<Texture2D>("Graphics/labAtoomButton");
            labEquipButton = content.Load<Texture2D>("Graphics/labEquipButton");
            labClearButton = content.Load<Texture2D>("Graphics/labClearButton");

            atom = new Texture2D[symbolCount];
            for (int i = 0; i < atom.Length; i++)
                atom[i] = content.Load<Texture2D>("Graphics/atom" + i);

            atomOrigin = new Vector2(atom[0].Width, atom[0].Height) / 2;
            shine = content.Load<Texture2D>("Graphics/atomShine");

            for (int i = 0; i < eyes.GetLength(0); i++)
                for (int j = 0; j < eyes.GetLength(1); j++)
                    eyes[i, j] = content.Load<Texture2D>("Graphics/eye" + i + j);

            for (int i = 0; i < bond.Length; i++)
            {
                bond[i] = content.Load<Texture2D>("Graphics/bond" + i);
                bondOrigin[i] = new Vector2(bond[i].Width, bond[i].Height) / 2;
            }

            bondOrigin[0].X = 0;

            electroShock = content.Load<Texture2D>("Graphics/shock");
            radSmoke = content.Load<Texture2D>("Graphics/radiationSmoke");
            radSmokeOrigin = new Vector2(radSmoke.Width, radSmoke.Height) / 2;

            for (int i = 0; i < equipment.GetLength(0); i++) for (int j = 0; j < equipment.GetLength(1); j++)
                    equipment[i, j] = content.Load<Texture2D>("Graphics/" + (EquipmentName)i + j);

            clamp = content.Load<Texture2D>("Graphics/clamp");
            clampOrigin = new Vector2(clamp.Width, clamp.Height) / 2;

            clampJoint = content.Load<Texture2D>("Graphics/clampJoint");
            clampJointOrigin = new Vector2(clampJoint.Width, clampJoint.Height) / 2;

            clampRod = content.Load<Texture2D>("Graphics/clampRod");
            clampRodOrigin = new Vector2(clampRod.Width, clampRod.Height) / 2;

            clampStand = content.Load<Texture2D>("Graphics/clampStand");
            clampStandOrigin = new Vector2(clampStand.Width / 2, clampStand.Height);

            equipMoveButton = content.Load<Texture2D>("Graphics/equipMoveButton");
            equipMoveButtOrigin = new Vector2(equipMoveButton.Width, equipMoveButton.Height) / 2;
            equipRotationButton = content.Load<Texture2D>("Graphics/equipRotationButton");
            equipRotButtOrigin = new Vector2(equipRotationButton.Width, equipRotationButton.Height) / 2;

            thermometerThread = content.Load<Texture2D>("Graphics/thermometerThread");
            thermoThreadOrigin = new Vector2(thermometerThread.Width, 0) / 2;
            pointer = content.Load<Texture2D>("Graphics/pointer");
            pointerOrigin = new Vector2(0, pointer.Height) / 2;

            tick = content.Load<Texture2D>("Graphics/tick");
            tickOrigin = new Vector2(tick.Width, tick.Height) / 2;
            arrow = content.Load<Texture2D>("Graphics/arrow");
            arrowOrigin = new Vector2(arrow.Width, arrow.Height) / 2;

            cross = content.Load<Texture2D>("Graphics/cross");

            debugFont = content.Load<SpriteFont>("Fonts/debugFont");
            symbolFontSize = 72;
            symbolFont = content.Load<SpriteFont>("Fonts/AklatanicTSO" + symbolFontSize);
            symbolFont.Spacing = 2;

            thermometerFont = content.Load<SpriteFont>("Fonts/thermometerFont");

            // Initialize audio objects.
            //MenuMusic = content.Load<Song>("Audio/Democracy - Alexander Blu");
            //GameMusic[0] = content.Load<Song>("Audio/May - Alexander Blu");
            //GameMusic[1] = content.Load<Song>("Audio/Nostalgia - Alexander Blu");

            for (int i = 0; i < shock.Length; i++) shock[i] = content.Load<SoundEffect>("Audio/shock" + i);
            for (int i = 0; i < radio.Length; i++) radio[i] = content.Load<SoundEffect>("Audio/radio" + i);
            for (int i = 0; i < detach.Length; i++) detach[i] = content.Load<SoundEffect>("Audio/detach" + i);
            for (int i = 0; i < broop.Length; i++) broop[i] = content.Load<SoundEffect>("Audio/broop" + i);
            for (int i = 0; i < bop.Length; i++) bop[i] = content.Load<SoundEffect>("Audio/bop" + i);

#if DEBUG
            MediaPlayer.Volume = .4f; SoundEffect.MasterVolume = .4f;
#else
            if (BitSitsGames.Settings.MusicEnabled) PlayMusic();

            if (BitSitsGames.Settings.SoundEnabled) SoundEffect.MasterVolume = 1;
            else SoundEffect.MasterVolume = 0;
#endif

            //Thread.Sleep(2000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            screenManager.Game.ResetElapsedTime();
        }


        public void PlayMusic()
        {
            if (MediaPlayer.GameHasControl)
            {
                if (MediaPlayer.State == MediaState.Paused)
                {
                    MediaPlayer.Resume();

                    return;
                }
            }
            else if (MediaPlayer.State == MediaState.Playing)
            {
#if WINDOWS_PHONE
                MessageBoxResult Choice;

                Choice = MessageBox.Show("Media is currently playing, do you want to stop it?",
                    "Stop Player", MessageBoxButton.OKCancel);

                if (Choice == MessageBoxResult.OK) MediaPlayer.Pause();
                else
                {
                    BitSitsGames.Settings.MusicEnabled = false;
                    return;
                }
#endif
            }

            MediaPlayer.Play(content.Load<Song>("Audio/May - Alexander Blu"));
            MediaPlayer.IsRepeating = true;
        }


        /// <summary>
        /// Unload GameContents
        /// </summary>
        public void UnloadContent() { content.Unload(); }
    }
}
