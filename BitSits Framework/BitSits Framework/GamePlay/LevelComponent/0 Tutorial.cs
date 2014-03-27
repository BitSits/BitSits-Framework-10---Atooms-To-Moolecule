using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;

namespace BitSits_Framework
{
    class Tutorial : LevelComponent
    {
        List<string> instructions;
        int index = 0;

        string msg, tutFormula;

        public Tutorial(GameContent gameContent, World world)
            : base(gameContent, world)
        {
#if WINDOWS
            instructions = gameContent.content.Load<List<string>>("Levels/tutorial");
#endif
#if WINDOWS_PHONE
            instructions = gameContent.content.Load<List<string>>("Levels/tutorialPhone");
#endif

            NextMsg();
        }

        public override bool UpdateNewFormula(Formula formula)
        {
            if (formula.strFormula == tutFormula)
            {
                if (index == instructions.Count) IsLevelUp = true;
                else NextMsg();

                return true;
            }

            return false;
        }

        public override void Update(GameTime gameTime)
        {
            int maxAtoms = 4 * (index / 2);

            if (maxAtoms <= atoms.Count) return;

            int[] atomsPresent = new int[gameContent.symbolCount];
            foreach (Atom a in atoms) atomsPresent[(int)a.symbol] += 1;

            for (int i = 1; i <= (index / 2); i++)
            {
                if (i >= atomsPresent.Length) break;

                for (int j = 0; j < 4 - atomsPresent[i]; j++)
                {
                    atoms.Add(new Atom((Symbol)i, entryPoint, gameContent, world));
                }
            }
        }

        void NextMsg()
        {
            msg = instructions[index]; tutFormula = instructions[index + 1]; index += 2;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.Draw(spriteBatch, gameTime);

            spriteBatch.DrawString(gameContent.symbolFont, msg, new Vector2(20, 250), Color.Gainsboro,
                -(float)Math.PI / 40, Vector2.Zero, 30f / gameContent.symbolFontSize, SpriteEffects.None, 1);

            spriteBatch.DrawString(gameContent.symbolFont, "Make", new Vector2(50, 390),
                Color.WhiteSmoke, -(float)Math.PI / 20f, Vector2.Zero, 30f / gameContent.symbolFontSize,
                SpriteEffects.None, 1);

            spriteBatch.Draw(gameContent.tutorial[index / 2 - 1], new Vector2(150, 360), null,
                Color.Gainsboro, 0, Vector2.Zero, .65f, SpriteEffects.None, 1);
        }
    }
}
