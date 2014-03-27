using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;

namespace BitSits_Framework
{
    class RadioActive : LevelComponent
    {
        int a = 2, max = 10;
        bool loadRadioActive;

        public RadioActive(GameContent gameContent, World world)
            : base(gameContent, world) { }

        public override void Update(GameTime gameTime)
        {
            if (!loadRadioActive)
            {
                loadRadioActive = true;
                for (int i = 0; i < 4; i++)
                    atoms.Add(new Atom(Symbol.Ra, new Vector2(300), gameContent, world));
            }

            base.Update(gameTime);
        }

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

            spriteBatch.DrawString(gameContent.symbolFont, a.ToString(), new Vector2(260, 230), Color.Gainsboro,
                -(float)Math.PI / 20, Vector2.Zero, 50f / gameContent.symbolFontSize, SpriteEffects.None, 1);
        }
    }
}
