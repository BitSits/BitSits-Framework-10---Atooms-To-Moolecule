using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;
using GameDataLibrary;

namespace BitSits_Framework
{
    class AbsoluteZero : LevelComponent
    {
        float temp, currentTemp;

        public AbsoluteZero(GameContent gameContent, World world)
            : base(gameContent, world)
        {
            temp = 1; currentTemp = 475;
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds * 30f;

            if (temp > currentTemp) temp = Math.Max(temp - dt, currentTemp);

            else if (temp < currentTemp) temp = Math.Min(temp + dt, currentTemp);

            else currentTemp = Math.Min(currentTemp + (float)gameTime.ElapsedGameTime.TotalSeconds * 1.2f, 
                thermometer.MaxTemperature);

            if (thermometer != null) thermometer.Temperature = temp;

            if (temp == 0) IsLevelUp = true;
            else if (temp == thermometer.MaxTemperature) ReloadLevel = true;

            base.Update(gameTime);
        }

        public override bool UpdateNewFormula(Formula formula)
        {
            currentTemp = Math.Max(currentTemp - formula.score * 1, 0);

            return true;
        }
    }
}
