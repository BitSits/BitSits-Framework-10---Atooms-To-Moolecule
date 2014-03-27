using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;

namespace BitSits_Framework
{
    class Ring : LevelComponent
    {
        int numberOfRings = 0, totalRings = 8;

        public Ring(GameContent gameContent, World world)
            : base(gameContent, world) { }

        public override bool UpdateNewFormula(Formula formula)
        {
            if (formula != null && formula.numberOfRings > 0)
            {
                numberOfRings += 1;

                if (numberOfRings == totalRings) IsLevelUp = true;

                return true;
            }            

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);

            spriteBatch.DrawString(gameContent.symbolFont, totalRings.ToString(), new Vector2(100, 244),
                Color.Gainsboro, (float)MathHelper.Pi / 20, Vector2.Zero, 40f / gameContent.symbolFontSize,
                SpriteEffects.None, 1);

            if (numberOfRings > 0)
                spriteBatch.DrawString(gameContent.symbolFont,
                    numberOfRings + " of " + totalRings, new Vector2(100, 440),
                    Color.Gainsboro, -(float)MathHelper.Pi / 20, Vector2.Zero, 40f / gameContent.symbolFontSize,
                    SpriteEffects.None, 1);

            if (numberOfRings == 0)
                spriteBatch.Draw(gameContent.tutorial[5], new Vector2(50, 400), null, Color.Gainsboro,
                    -0.008f, Vector2.Zero, 0.6f, SpriteEffects.None, 1);
        }
    }
}
