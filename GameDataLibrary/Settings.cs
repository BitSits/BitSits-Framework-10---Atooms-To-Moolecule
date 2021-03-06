﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameDataLibrary
{
    public class Settings
    {
        public bool IsFullScreen;
        public bool SoundEnabled;
        public bool MusicEnabled;

        string fileName = "settings.xml";

        public Settings Load()
        {
            Settings s = Storage.LoadXml<Settings>(fileName);

            if (s == null)
            {
                s = new Settings();
                s.LoadDefault();
            }

            return s;
        }

        void LoadDefault()
        {
#if DEBUG
            IsFullScreen = false;
#else
            IsFullScreen = true;
#endif

            SoundEnabled = true;
            MusicEnabled = true;
        }

        public void Save() { Storage.SaveXml<Settings>(fileName, this); }
    }
}
