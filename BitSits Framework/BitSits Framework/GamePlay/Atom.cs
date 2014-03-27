using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Box2D.XNA;
using GameDataLibrary;

namespace BitSits_Framework
{
    enum EyeState { AngrySleep = -1, Angry, Sad, Wake, Shock, Happy, Sleep, Disappear, Remove, }

    class Atom
    {
        int bondsLeft;
        float bondLength = 60;

        public Body body;
        public Fixture fixture;

        Filter mouseFilter, atomFilter;
        bool isMouse;

        Atom nearestAtom;
        int numberOfBonds;

        World world;
        GameContent gameContent;

        Vector2 symbolCenter;
        string symbolStr;
        public Symbol symbol;

        public bool IsMolecularityChecked = false;

        public EyeState eye = EyeState.Sad;
        const float MaxSleepTime = 4.0f, MaxRemoveTime = 3.5f, MaxShockTime = 2.0f, MaxRemoveZoom = 3f;
        float sleepTime = 0, removeZoom = 0, electroShockTime = MaxShockTime, radShockTime = MaxShockTime;

        Vector2 position, shockPosition;

        Animation electroShockAnimation;
        AnimationPlayer animationPlayer = new AnimationPlayer();

        RadiationSmoke radiationSmoke;

        const float MaxJumpTime = 0.5f;
        float jumpTime = 0.0f;

        List<Bond> bonds = new List<Bond>();
        public List<Atom> bondedAtoms = new List<Atom>();

        // Bond Angle
        //    3   6
        //     \ /
        //5 - AtomX - 1     1 = 0; 2 = 120; 3 = 240; 4 = 60; 5 = 180; 6 = 300
        //     / \
        //    2   4
        float[] bondAngle = { 0, (float)Math.PI * 2 / 3, (float)Math.PI * 4 / 3, 
                                (float)Math.PI / 3, (float)Math.PI, (float)Math.PI * 5 / 3 };
        bool[] bondPresent = new bool[6];

        public Atom(Symbol symbol, Vector2 position, GameContent gameContent, World world)
        {
            this.gameContent = gameContent;
            this.world = world;

            if (symbol == Symbol.Ra) eye = EyeState.Angry;

            this.symbol = symbol;
            this.symbolStr = symbol.ToString();
            symbolCenter = gameContent.symbolFont.MeasureString(this.symbolStr);
            symbolCenter.X *= 0.5f;
            symbolCenter.Y *= 0.92f;

            bondsLeft = (int)symbol;

            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            bd.position = this.position = position / gameContent.b2Scale;
            bd.bullet = true;
            body = world.CreateBody(bd);
            body.SetUserData(this);

            CircleShape cs = new CircleShape();
            cs._radius = gameContent.atomRadius / gameContent.b2Scale;

            FixtureDef fd = new FixtureDef();
            fd.shape = cs;
            fd.restitution = 0.2f;
            fd.friction = 0.5f;

            fixture = body.CreateFixture(fd);

            electroShockAnimation = new Animation(gameContent.electroShock, 3, 0.1f, true, new Vector2(0.5f, 0.5f));

            radiationSmoke = new RadiationSmoke(gameContent, this);

            // Collide only with Ground but not with itself and bonded Filter
            mouseFilter = new Filter();
            mouseFilter.categoryBits = 0x0002; mouseFilter.maskBits = 0x0001; mouseFilter.groupIndex = -2;

            // Collide with every thing
            atomFilter = new Filter();
            atomFilter.categoryBits = 0x0001; atomFilter.maskBits = 0x0001; atomFilter.groupIndex = 1;

            fixture.SetFilterData(ref atomFilter);

            SetMode(false, false);
        }

        public void SetMode(bool editMode, bool isMouse)
        {
            if (symbol == Symbol.Ra && isMouse) eye = EyeState.AngrySleep;

            this.isMouse = isMouse;

            float density = 0.5f;

            if (editMode || isMouse)
            {
                density = 0.3f; body.SetAngularDamping(1000);
                body.GetFixtureList().SetFilterData(ref mouseFilter);
            }
            else
            {
                body.SetAngularDamping(0);
                body.GetFixtureList().SetFilterData(ref atomFilter);
            }

            body.GetFixtureList().SetDensity(density);
            body.ResetMassData();

            if (editMode) //fixed
            {
                body.SetAngularVelocity(0); body.SetLinearVelocity(Vector2.Zero); body.SetLinearDamping(10000);
            }
            else if (!editMode)
            {
                body.SetLinearDamping(0);
            }
        }

        public void CreateBond()
        {
            if (nearestAtom == null) return;

            bondsLeft -= numberOfBonds;
            if (symbol == Symbol.X) bondsLeft = 0;

            nearestAtom.bondsLeft -= numberOfBonds;
            if (nearestAtom.symbol == Symbol.X) nearestAtom.bondsLeft = 0;

            DistanceJointDef jd = new DistanceJointDef();
            jd.bodyA = body;
            jd.bodyB = nearestAtom.body;
            jd.dampingRatio = 0.2f;
            //jd.frequencyHz = 20.0f;
            jd.collideConnected = true;

            // Mid
            jd.length = (float)(bondLength) / gameContent.b2Scale;
            jd.localAnchorA = jd.localAnchorB = Vector2.Zero;
            Joint midJoint = world.CreateJoint(jd);

            Vector2 a = body.Position * gameContent.b2Scale;
            Vector2 b = nearestAtom.body.Position * gameContent.b2Scale;
            float slope = (float)Math.Atan2((b.Y - a.Y), (b.X - a.X));

            // Short
            jd.length = (float)(bondLength - gameContent.atomRadius * 2) / gameContent.b2Scale;

            Vector2 e = new Vector2((float)Math.Cos(slope), (float)Math.Sin(slope)) * gameContent.atomRadius;

            jd.localAnchorA = body.GetLocalPoint((a + e) / gameContent.b2Scale);
            jd.localAnchorB = nearestAtom.body.GetLocalPoint((b - e) / gameContent.b2Scale);
            Joint shortJoint = world.CreateJoint(jd);


            bonds.Add(new Bond(this, nearestAtom, shortJoint, midJoint, numberOfBonds, gameContent));
            bondedAtoms.Add(nearestAtom);
            SetBondAngle();

            nearestAtom.bonds.Add(new Bond(nearestAtom, this, shortJoint, midJoint, numberOfBonds, gameContent));
            nearestAtom.bondedAtoms.Add(this);
            nearestAtom.SetBondAngle();

            gameContent.broop[gameContent.random.Next(gameContent.broop.Length)].Play();
        }

        void SetBondAngle()
        {
            bondPresent = new bool[6];

            switch (bonds.Count)
            {
                case 1:
                    bonds[0].finalJointAngle = 0; bondPresent[0] = true; break;

                case 2:
                    for (int i = 0; i < 2; i++)
                    {
                        int j = (i + 1) % 2;
                        if (bonds[i].finalJointAngle < bonds[j].finalJointAngle)
                        {
                            if (bonds[j].finalJointAngle - bonds[i].finalJointAngle < Math.PI)
                            {
                                bonds[j].finalJointAngle = bondAngle[1];
                                bondPresent[1] = true;
                            }
                            else
                            {
                                bonds[j].finalJointAngle = bondAngle[2];
                                bondPresent[2] = true;
                            }

                            bonds[i].finalJointAngle = 0;
                            bondPresent[0] = true; break;
                        }
                    }
                    break;

                case 3:
                    bondPresent[0] = bondPresent[1] = bondPresent[2] = true;

                    for (int i = 0; i < 3; i++)
                    {
                        int j = (i + 1) % 3;
                        int k = (i + 2) % 3;

                        if (bonds[i].finalJointAngle < bonds[j].finalJointAngle
                            && bonds[j].finalJointAngle < bonds[k].finalJointAngle)
                        {
                            bonds[i].finalJointAngle = 0; 
                            bonds[j].finalJointAngle = bondAngle[1];
                            bonds[k].finalJointAngle = bondAngle[2]; break;
                        }
                        else if (bonds[i].finalJointAngle < bonds[k].finalJointAngle
                            && bonds[k].finalJointAngle < bonds[j].finalJointAngle)
                        {
                            bonds[i].finalJointAngle = 0; 
                            bonds[k].finalJointAngle = bondAngle[1];
                            bonds[j].finalJointAngle = bondAngle[2]; break;
                        }
                    } 
                    break;

                case 4:
                    bondPresent[0] = bondPresent[1] = bondPresent[2] = true;

                    if (bonds[3].finalJointAngle < (float)Math.PI * 2 / 3)
                    {
                        bonds[3].finalJointAngle = bondAngle[3];
                        bondPresent[3] = true;
                    }
                    else if (bonds[3].finalJointAngle > (float)Math.PI * 4 / 3)
                    {
                        bonds[3].finalJointAngle = bondAngle[5];
                        bondPresent[5] = true;
                    }
                    else
                    {
                        bonds[3].finalJointAngle = bondAngle[4];
                        bondPresent[4] = true;
                    }
                    break;
            }

            SetEyeState(new GameTime());
        }

        public void DestroyBonds()
        {
            foreach (Bond b in bonds)
            {
                world.DestroyJoint(b.shortJoint);
                world.DestroyJoint(b.midJoint);

                bondsLeft += b.numberOfBonds;

                int index = b.other.bondedAtoms.IndexOf(this);
                b.other.bondedAtoms.RemoveAt(index);
                b.other.bonds.RemoveAt(index);
                b.other.bondsLeft += b.numberOfBonds;

                if (b.other.symbol == Symbol.X) b.other.bondsLeft = (int)b.other.symbol;

                b.other.SetBondAngle();
            }

            if (symbol == Symbol.X) bondsLeft = (int)symbol;
            bondedAtoms.Clear(); bonds.Clear();

            SetBondAngle();
        }

        public void Update(GameTime gameTime)
        {
            foreach (Bond b in bonds) b.Update(gameTime);

            GetNearestBody();

            JumpAndShake(gameTime);

            CheckCollision(gameTime);

            SetEyeState(gameTime);
        }

        void CheckCollision(GameTime gameTime)
        {
            for (ContactEdge ce = body.GetContactList(); ce != null; ce = ce.Next)
            {
                object userData = ce.Other.GetUserData();
                if (ce.Contact.IsTouching() && userData is EquipmentName
                    && (EquipmentName)userData == EquipmentName.electrodes)
                {
                    DestroyBonds();

                    WorldManifold wm;
                    ce.Contact.GetWorldManifold(out wm); shockPosition = wm._points[0];

                    if (electroShockTime >= MaxShockTime)
                    {
                        electroShockTime = 0;

                        //throw
                        body.ApplyForce(new Vector2(250f * 60 * (float)gameTime.ElapsedGameTime.TotalSeconds
                            * (shockPosition.X < body.Position.X ? 1 : -1), 0), body.GetWorldCenter());

                        gameContent.shock[gameContent.random.Next(gameContent.shock.Length)].Play();
                    }
                }

                if (symbol != Symbol.Ra && radShockTime >= MaxShockTime && ce.Contact.IsTouching() 
                    && userData is Atom && ((Atom)userData).symbol == Symbol.Ra)
                {
                    DestroyBonds();
                    gameContent.radio[gameContent.random.Next(gameContent.radio.Length)].Play();
                    radShockTime = 0;
                }

                if (symbol == Symbol.Ra && !isMouse && ce.Contact.IsTouching()) eye = EyeState.Angry;
            }
        }

        void JumpAndShake(GameTime gameTime)
        {
            if (symbol == Symbol.Ra && eye == EyeState.Angry)
            {
                for (ContactEdge ce = body.GetContactList(); ce != null; ce = ce.Next)
                {
                    if (ce.Contact.IsTouching())
                    {
                        WorldManifold wm; ce.Contact.GetWorldManifold(out wm); 
                        Vector2 v = wm._points[0];

                        float theta = (float)Math.Atan2(v.Y - body.Position.Y, v.X - body.Position.X);
                        theta = theta + (float)Math.PI + 2f / 180 * (float)Math.PI;
                        float force = gameContent.random.Next(3) * 60 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        body.ApplyLinearImpulse(new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * force,
                            body.GetWorldCenter());
                    }
                }
                position = body.Position; return;
            }

            // Jump
            jumpTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (bonds.Count == 0)
            {
                if (jumpTime < 0)
                    for (ContactEdge ce = body.GetContactList(); ce != null; ce = ce.Next)
                    {
                        object userData = ce.Other.GetUserData();
                        if (ce.Contact.IsTouching() && userData is Atom &&
                            body.GetWorldCenter().Y < ce.Other.GetWorldCenter().Y)
                        {
                            jumpTime = MaxJumpTime; break;
                        }
                    }
                else
                    body.ApplyLinearImpulse(new Vector2(0, -0.08f) * 60 * (float)gameTime.ElapsedGameTime.TotalSeconds,
                        body.GetWorldCenter());
            }

            // Shake
            if (eye == EyeState.Wake)
            {
                float angle = (float)gameContent.random.Next(360) / 180 * (float)Math.PI;
                position = body.Position +
                    new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 1f / gameContent.b2Scale;
            }
            else position = body.Position;
        }

        void SetEyeState(GameTime gameTime)
        {
            if (eye < EyeState.Disappear && symbol != Symbol.Ra)
            {
                if (bonds.Count == 0)
                {
                    if (electroShockTime < MaxShockTime)
                    {
                        electroShockTime += (float)gameTime.ElapsedGameTime.TotalSeconds; eye = EyeState.Shock;
                    }
                    else if (radShockTime < MaxShockTime)
                    {
                        radShockTime += (float)gameTime.ElapsedGameTime.TotalSeconds; eye = EyeState.Shock;
                    }
                    else eye = EyeState.Sad;
                }
                else electroShockTime = radShockTime = MaxShockTime;
                

                if (bonds.Count > 0) eye = EyeState.Wake;

                if (bondsLeft == 0)
                {
                    eye = EyeState.Happy;
                    sleepTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (sleepTime > MaxSleepTime)
                    {
                        eye = EyeState.Sleep;
                    }
                }
                else sleepTime = 0;
            }
        }

        void GetNearestBody()
        {
            nearestAtom = null; numberOfBonds = 0;

            if (eye >= EyeState.Sleep) return;

            float nearestDist = (bondLength * 1.2f) / gameContent.b2Scale;

            for (Body b = world.GetBodyList(); b != null; b = b.GetNext())
            {
                object userData = b.GetUserData();
                if (userData is Atom && b != body && !bondedAtoms.Contains((Atom)userData)
                    && ((Atom)userData).bondsLeft > 0 && bondsLeft > 0)
                {
                    float d = Vector2.Distance(body.Position, b.Position);
                    if (d < nearestDist)
                    {
                        nearestAtom = (Atom)userData;
                        nearestDist = d;

                        numberOfBonds = Math.Min(bondsLeft, nearestAtom.bondsLeft);
                        numberOfBonds = Math.Min(3, numberOfBonds);
                    }
                }
            }
        }

        public void Remove()
        {
            DestroyBonds(); eye = EyeState.Disappear; world.DestroyBody(body);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (eye == EyeState.Disappear)
            {
                removeZoom += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (removeZoom > MaxRemoveTime) eye = EyeState.Remove;
            }

            if (symbol == Symbol.Ra || radShockTime < MaxShockTime * 0.2f) 
                radiationSmoke.Draw(spriteBatch, gameTime);

            spriteBatch.Draw(gameContent.atom[(int)symbol], position * gameContent.b2Scale, null,
                Color.White * (1 - removeZoom / MaxRemoveTime),
                body.Rotation, gameContent.atomOrigin,
                1 + removeZoom / MaxRemoveTime * MaxRemoveZoom, SpriteEffects.None, 1);

            if (eye > EyeState.Sleep) return;

            if (symbol != Symbol.Ra)
            {
                spriteBatch.DrawString(gameContent.symbolFont, symbolStr, position * gameContent.b2Scale,
                       bondsLeft == 0 ? Color.Gainsboro : Color.White, body.Rotation,
                       symbolCenter, 16f / gameContent.symbolFontSize, SpriteEffects.None, 1);

                spriteBatch.Draw(gameContent.eyes[(int)symbol % 2, (int)eye], position * gameContent.b2Scale,
                    null, Color.White, body.Rotation, gameContent.atomOrigin, 1, SpriteEffects.None, 1);
            }
            else
                spriteBatch.Draw(gameContent.eyes[(int)symbol % 2 - (int)eye, 0], position * gameContent.b2Scale,
                        null, Color.White, body.Rotation, gameContent.atomOrigin, 1, SpriteEffects.None, 1);

            spriteBatch.Draw(gameContent.shine, position * gameContent.b2Scale, null, Color.White,
                body.Rotation, gameContent.atomOrigin, 1, SpriteEffects.None, 1);

            if (eye == EyeState.Shock && electroShockTime < MaxShockTime * 0.15f)
            {
                animationPlayer.PlayAnimation(electroShockAnimation);
                animationPlayer.Draw(gameTime, spriteBatch, shockPosition * gameContent.b2Scale);
            }
        }

        public void DrawPreBond(SpriteBatch spriteBatch)
        {
            if (nearestAtom != null && numberOfBonds > 0)
            {
                nearestAtom.DrawUnusedBonds(spriteBatch, numberOfBonds);

                Vector2 a = body.Position * gameContent.b2Scale;
                Vector2 b = nearestAtom.body.Position * gameContent.b2Scale;

                Bond.DrawBond(spriteBatch, gameContent, a, b, numberOfBonds + 3);
            }

            DrawUnusedBonds(spriteBatch, numberOfBonds);
        }

        void DrawUnusedBonds(SpriteBatch spriteBatch, int numberOfBonds)
        {
            int unusedBonds = bondsLeft - numberOfBonds;
            if (symbol == Symbol.X && numberOfBonds > 0) unusedBonds = 0;

            int count = 0;
            for (int i = 0; i < 6; i++)
            {
                if (!bondPresent[i])
                {
                    if (count == unusedBonds) return;

                    spriteBatch.Draw(gameContent.bond[0], body.Position * gameContent.b2Scale, null, Color.White,
                        body.Rotation + bondAngle[i], gameContent.bondOrigin[0], 1, SpriteEffects.None, 1);

                    count++;
                }
            }
        }
    }
}
