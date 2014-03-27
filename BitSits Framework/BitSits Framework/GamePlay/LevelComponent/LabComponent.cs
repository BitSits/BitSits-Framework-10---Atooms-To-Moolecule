using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;
using GameDataLibrary;

namespace BitSits_Framework
{
    class LabComponent
    {
        GameContent gameContent;
        public readonly int Height, Width;
        public readonly float EqScale, AtomScale;

        public LabComponent(GameContent gameContent, World world)
        {
            this.gameContent = gameContent;

            EqScale = 0.4f; AtomScale = 0.7f;

            Width = (int)(gameContent.PlayArea.X * 2 / EqScale );
            Height = (int)(gameContent.PlayArea.Y * 1 / EqScale );

            gameContent.clampDistance = (int)(gameContent.PlayArea.X / 2 / EqScale);

            PolygonShape ps = new PolygonShape();
            Body ground = world.CreateBody(new BodyDef());

            Vector2 pos = new Vector2(Width / 2, Height) / gameContent.b2Scale;
            ps.SetAsBox(Width / 2 / gameContent.b2Scale, (float)gameContent.labTable.Height 
                / gameContent.b2Scale, pos, 0);
            ground.CreateFixture(ps, 0);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(gameContent.menuBackground, new Rectangle(0, 0, Width, Height), Color.White);

            for (int i = 0; i <= (Width / 800 + 1); i++)
            {
                Vector2 pos = new Vector2(i * gameContent.clampDistance + gameContent.clampDistance / 2,
                    Height - gameContent.labTable.Height);

                spriteBatch.Draw(gameContent.clampStand, pos - gameContent.clampStandOrigin, Color.White);
            }

            for (int i = 0; i <= (Width / gameContent.labTable.Width + 1); i++)
            {
                spriteBatch.Draw(gameContent.labTable, new Vector2(i * gameContent.labTable.Width,
                    Height - gameContent.labTable.Height), Color.White);
            }
        }
    }
}
