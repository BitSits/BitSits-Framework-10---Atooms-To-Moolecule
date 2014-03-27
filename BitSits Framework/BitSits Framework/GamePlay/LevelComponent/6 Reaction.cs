using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;
using GameDataLibrary;

namespace BitSits_Framework
{
    class Reaction : LevelComponent
    {
        int index = 0;
        List<string> reactStr = new List<string>();
        List<Formula> reactFor = new List<Formula>();
        List<bool> ticked = new List<bool>();
        List<Vector2> tickPos = new List<Vector2>();

        public Reaction(GameContent gameContent, World world)
            : base(gameContent, world)
        {
            reactStr = gameContent.content.Load<List<string>>("Levels/reactions");
            GetNewReactionFormulas();
        }

        void GetNewReactionFormulas()
        {
            if (index == reactStr.Count) { IsLevelUp = true; return; }

            reactFor.Clear(); tickPos.Clear(); ticked.Clear();
            for (int i = 0; i < 4; i++)
            {
                if (reactStr[index] != "")
                {
                    reactFor.Add(new Formula(reactStr[index], new Vector2(200 + 150 * i, 500), gameContent));
                    ticked.Add(false);
                }

                index += 1;
            }
        }

        public override bool UpdateNewFormula(Formula formula)
        {
            for (int i = 0; i < reactFor.Count; i++)
            {
                if (reactFor[i].strFormula == formula.strFormula && ticked[i] == false)
                {
                    tickPos.Add(new Vector2(200 + 150 * i, 500));
                    ticked[i] = true;

                    if (tickPos.Count == reactFor.Count) GetNewReactionFormulas();
                    return true;
                }
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);

            spriteBatch.Draw(gameContent.menuBackground, Vector2.Zero, Color.White);

            Vector2 v = new Vector2(358, 300) - new Vector2(445, 300);
            spriteBatch.Draw(gameContent.equipment[(int)EquipmentName.clipboard, 0], v, Color.White);
            //spriteBatch.Draw(gameContent.equipment[(int)EquipmentName.clipboard, 1], v, Color.White);

            foreach (Formula f in reactFor) f.Draw(spriteBatch, gameTime);

            for (int i = 0; i < reactFor.Count - 2; i++)
                spriteBatch.DrawString(gameContent.symbolFont, "+", new Vector2(180 + 150 / 2 + 150 * 2 * i, 475),
                    Color.Gainsboro, 0, Vector2.Zero, 30f / gameContent.symbolFontSize, SpriteEffects.None, 1);

            spriteBatch.Draw(gameContent.arrow, new Vector2(350 + 150 / 2, 500) - gameContent.arrowOrigin,
                Color.Gainsboro);

            foreach (Vector2 u in tickPos)
                spriteBatch.Draw(gameContent.tick, u - gameContent.tickOrigin, Color.Gainsboro* .5f);

            spriteBatch.DrawString(gameContent.symbolFont,
                "\"Where are my Moolecules!!\n   Make them fast..\"",
                new Vector2(120, 130), Color.Gainsboro, (float)MathHelper.Pi / 20,
                Vector2.Zero, 35f / gameContent.symbolFontSize, SpriteEffects.None, 1);
        }
    }
}
