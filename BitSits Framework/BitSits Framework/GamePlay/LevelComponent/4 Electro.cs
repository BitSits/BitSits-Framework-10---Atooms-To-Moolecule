using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;

namespace BitSits_Framework
{
    class Electro : LevelComponent
    {
        int a = 6, max = 16;

        public Electro(GameContent gameContent, World world)
            : base(gameContent, world) { }

        public override bool UpdateNewFormula(Formula formula)
        {
            int total = 0;
            for (int i = 0; i < formula.atomCount.Length; i++)
            {
                total += formula.atomCount[i];
            }

            if (total >= a)
            {
                if (a >= max) { IsLevelUp = true; return true; }

                a += 2; return true;
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);

            spriteBatch.DrawString(gameContent.symbolFont, a.ToString(), new Vector2(330, 315), Color.Gainsboro,
                -(float)Math.PI / 20, Vector2.Zero, 50f / gameContent.symbolFontSize, SpriteEffects.None, 1);
        }
    }
}
