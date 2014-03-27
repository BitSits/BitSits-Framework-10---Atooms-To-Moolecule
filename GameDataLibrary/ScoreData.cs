using System;
using System.Collections.Generic;

namespace GameDataLibrary
{
    public class ScoreData
    {
        public int CurrentLevel;

        public List<int> HighScores;

        string fileName = "score.bin";

        public ScoreData Load(int maxLevel)
        {
            ScoreData s = Storage.LoadXml<ScoreData>(fileName);

            if (s == null)
            {
                s = new ScoreData();
                s.LoadDefault(maxLevel);
            }

            return s;
        }

        public void LoadDefault(int maxLevel)
        {
            CurrentLevel = 0;

            HighScores = new List<int>();
            for (int i = 0; i < maxLevel; i++) HighScores.Add(0);
        }

        public void Save() { Storage.SaveXml<ScoreData>(fileName, this); }
    }
}
