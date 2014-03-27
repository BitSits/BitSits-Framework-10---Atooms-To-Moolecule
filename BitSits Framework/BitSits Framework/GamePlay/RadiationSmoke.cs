using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BitSits_Framework
{
    class RadiationSmoke
    {
        GameContent gameContent;
        Atom atom;
        Vector2 atomPrevPosition;

        const int MaxParticle = 8, VariedAngle = 30;
        const float MaxParticlePos = 60;

        float[] particlePos = new float[MaxParticle], particleRotation = new float[MaxParticle];
        
        public RadiationSmoke(GameContent gameContent, Atom atom)
        {
            this.gameContent = gameContent;
            this.atom = atom;

            for (int i = 0; i < MaxParticle; i++)
                particlePos[i] = MaxParticlePos / (MaxParticle - 1) * i;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Vector2 atomPosition = atom.body.Position * gameContent.b2Scale;
            float theta = (float)Math.Atan2(atomPrevPosition.Y - atomPosition.Y, atomPrevPosition.X - atomPosition.X)
                + (float)Math.PI;

            spriteBatch.Draw(gameContent.radSmoke, atomPosition, null, Color.White, 0, gameContent.radSmokeOrigin,
                    1, SpriteEffects.None, 1);

            for (int i = 0; i < MaxParticle; i++)
            {
                particlePos[i] += 130 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (particlePos[i] > MaxParticlePos)
                {
                    particlePos[i] = 0;
                    particleRotation[i] = theta
                        + (float)(gameContent.random.Next(2 * VariedAngle) - VariedAngle) / 180 * (float)Math.PI;
                }

                Vector2 localRadPos = atomPosition - new Vector2((float)Math.Cos(particleRotation[i]),
                    (float)Math.Sin(particleRotation[i])) * particlePos[i];
                spriteBatch.Draw(gameContent.radSmoke, localRadPos, null,
                    Color.White * (1f - particlePos[i] / MaxParticlePos), 0, gameContent.radSmokeOrigin,
                    MathHelper.Clamp(1f - particlePos[i] / MaxParticlePos, 0.2f, 0.9f), SpriteEffects.None, 1);
            }

            atomPrevPosition = atomPosition;
        }
    }
}
