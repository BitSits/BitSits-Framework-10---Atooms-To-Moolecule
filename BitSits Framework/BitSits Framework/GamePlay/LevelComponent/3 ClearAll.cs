using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;

namespace BitSits_Framework
{
    class ClearAll : LevelComponent
    {
        int count = 0;
        int maxCount = 5;

        public ClearAll(GameContent gameContent, World world)
            : base(gameContent, world) { }

        public override bool UpdateNewFormula(Formula formula)
        {
            int c = 0;
            for (int i = 0; i < formula.atomCount.Length; i++)
                c += formula.atomCount[i];

            if (c == MaxAtoms)
            {
                count += 1;
                if (count == maxCount) IsLevelUp = true;

                return true;
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);

            spriteBatch.DrawString(gameContent.symbolFont, (maxCount - count) + " times", new Vector2(200, 400),
                Color.Gainsboro, (float)MathHelper.Pi / 30, Vector2.Zero, 45f / gameContent.symbolFontSize, 
                SpriteEffects.None, 1);            
        }
    }
}
