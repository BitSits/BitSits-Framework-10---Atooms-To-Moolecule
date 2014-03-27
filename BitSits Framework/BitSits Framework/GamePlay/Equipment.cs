using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;
using GameDataLibrary;

namespace BitSits_Framework
{
    class Equipment
    {
        GameContent gameContent;
        World world;

        public readonly EquipmentName equipName;
        public Body body;
        int fixtureCount;
        Joint pin;

        Filter mouseFilter, groundFilter, equipFilter;

        Vector2 origin, topLeftVertex;

        EquipmentData equipmentData;

        Vector2 rotationButtonPos;

        bool editMode;
        public bool isMouseOver, isSelected, isClamped;

        //clamp data
        float rightClampPositionX, clampRotation;
        public readonly bool clampEnabled;

        //public data
        public float Temperature, pHvalue;
        public readonly float MaxTemperature = 500, MaxPhValue = 10, MinPhValue = 1;

        public Equipment(EquipmentName equipName, GameContent gameContent, World world)
        {
            this.gameContent = gameContent;
            this.world = world;

            this.equipName = equipName;
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            body = world.CreateBody(bd);

            equipmentData = gameContent.content.Load<EquipmentData>("Graphics/" + equipName.ToString());

            topLeftVertex = equipmentData.TopLeftVertex;
            origin = equipmentData.Origin;
            rotationButtonPos = equipmentData.RotationButtonPosition;

            rightClampPositionX = equipmentData.ClampData.RightClampPositionX;
            clampRotation = equipmentData.ClampData.RotationInDeg / 180f * (float)Math.PI;
            clampEnabled = equipmentData.ClampData.ClampEnabled;

            SetFixtures();

            // Collide only with Ground but not with itself and bonded Filter
            mouseFilter = new Filter();
            mouseFilter.categoryBits = 0x0002; mouseFilter.maskBits = 0x0001; mouseFilter.groupIndex = -2;

            // Collide with every thing
            groundFilter = new Filter();
            groundFilter.categoryBits = 0x0001; groundFilter.maskBits = 65535; groundFilter.groupIndex = 0;

            equipFilter = new Filter();
            equipFilter.categoryBits = 0x0002; equipFilter.maskBits = 0x0001; equipFilter.groupIndex = 2;

            //SetMode(false, false);

            body.SetUserData((EquipmentName)equipName);
        }

        void SetFixtures()
        {
            FixtureDef fd = new FixtureDef();
            fd.density = 1; fd.friction = .3f; fd.restitution = 0f;

            PolygonShape ps = new PolygonShape();

            float area;
            fixtureCount = 0;

            List<Vector2> v = equipmentData.ContinuousEdges;
            for (int i = 0; i < v.Count - 1; i++)
            {
                area = 0;

                if (v[i + 1].X == -1 && v[i + 1].Y == -1) { i++; continue; }

                Vector2 c = (v[i] + v[i + 1] - 2 * origin) / 2 / gameContent.b2Scale;
                float h = 8f / 2 / gameContent.b2Scale;
                float w = Vector2.Distance(v[i], v[i + 1]) / 2 / gameContent.b2Scale;
                float theta = (float)Math.Atan((v[i + 1].Y - v[i].Y) / (v[i + 1].X - v[i].X));

                ps.SetAsBox(w, h, c, theta);
                fd.shape = ps;
                area = h * w * 4;

                fd.userData = area; fixtureCount += 1; body.CreateFixture(fd);
            }

            List<Box> b = equipmentData.Boxes;
            for (int i = 0; i < b.Count; i++)
            {
                float hx = b[i].Width / 2 / gameContent.b2Scale, hy = b[i].Height / 2 / gameContent.b2Scale;
                ps.SetAsBox(hx, hy, (b[i].Position - origin) / gameContent.b2Scale,
                    b[i].RotationInDeg / 180 * (float)Math.PI);

                fd.shape = ps;
                area = hx * hy * 4;

                fd.userData = area; fixtureCount += 1; body.CreateFixture(fd);
            }

            List<Circle> circles = equipmentData.Circles;
            for (int i = 0; i < circles.Count; i++)
            {
                CircleShape cs = new CircleShape();
                cs._radius = circles[i].Diameter / 2 / gameContent.b2Scale;
                cs._p = (circles[i].Position - origin) / gameContent.b2Scale;

                fd.shape = cs;
                area = (float)Math.PI * cs._radius * cs._radius;
                fd.userData = area; fixtureCount += 1; body.CreateFixture(fd);
            }
        }

        public void SetMode(bool editMode, bool isMouse)
        {
            if (!clampEnabled) isClamped = false;

            this.editMode = editMode;
            float reqMass = 10;

            if (isSelected) reqMass = .2f;
            else
            {
                body.SetActive(true); body.SetLinearDamping(0); body.SetAngularDamping(0);
            }

            for (Fixture f = body.GetFixtureList(); f != null; f = f.GetNext())
            {
                object area = f.GetUserData();
                if (area is float) f.SetDensity(reqMass / (float)area / fixtureCount);

                if (isSelected) f.SetFilterData(ref mouseFilter);
                else if (editMode) f.SetFilterData(ref equipFilter);
                else f.SetFilterData(ref groundFilter);
            }

            body.ResetMassData();

            if (!editMode || (isSelected && !isMouse) || isClamped) 
            {
                if (pin == null) // Pin
                {
                    BodyDef bd = new BodyDef(); Body b = world.CreateBody(bd);

                    RevoluteJointDef jd = new RevoluteJointDef();
                    jd.bodyA = b; jd.bodyB = body; jd.localAnchorA = body.Position; jd.localAnchorB = Vector2.Zero;
                    pin = world.CreateJoint(jd);

                    body.SetAngularVelocity(0); body.SetFixedRotation(true);
                }
            }
            else if (pin != null)
            {
                body.SetFixedRotation(false); world.DestroyJoint(pin); pin = null;
            }
        }

        public void Remove()
        {
            isClamped = false; isSelected = false; SetMode(true, false);
            world.DestroyBody(body);
        }


        public Rectangle MoveButtonBound
        {
            get
            {
                if (!editMode) return new Rectangle();

                Vector2 pos = body.Position * gameContent.b2Scale;
                int w = (int)(gameContent.equipMoveButton.Width * 0.6f);
                int h = (int)(gameContent.equipMoveButton.Height * 0.6f);

                return new Rectangle((int)(pos.X - w / 2), (int)(pos.Y - h / 2), w, h);
            }
        }

        public Rectangle RotationButtonBound
        {
            get
            {
                if (!editMode) return new Rectangle();

                Vector2 pos = RotationButtonPos();
                int w = (int)(gameContent.equipRotationButton.Width * 0.6f);
                int h = (int)(gameContent.equipRotationButton.Height * 0.6f);

                return new Rectangle((int)(pos.X - w / 2), (int)(pos.Y - h / 2), w, h);
            }
        }

        Vector2 RotationButtonPos()
        {
            float theta = (float)Math.Atan2(rotationButtonPos.Y - origin.Y, rotationButtonPos.X - origin.X);
            theta += body.Rotation;
            Vector2 pos = body.Position * gameContent.b2Scale + new Vector2((float)Math.Cos(theta),
                (float)Math.Sin(theta)) * (rotationButtonPos - origin).Length();

            return pos;
        }


        public void DrawBottomLayer(SpriteBatch spriteBatch)
        {
            if (equipName == EquipmentName.pHscale && gameContent.levelIndex == -1)
            {
                body.Rotation = MathHelper.Clamp(body.Rotation, 0, (float)Math.PI * 2 / 11 * 9f);
                pHvalue = body.Rotation / ((float)Math.PI * 2 / 11) + 1;
            }

            if (isSelected)
                spriteBatch.Draw(gameContent.equipment[(int)equipName, 2], body.Position * gameContent.b2Scale, null,
                    Color.White, body.Rotation, origin - topLeftVertex, 1, SpriteEffects.None, 1);

            if (isClamped) DrawClamps(spriteBatch);

            spriteBatch.Draw(gameContent.equipment[(int)equipName, 0], body.Position * gameContent.b2Scale, null,
                Color.White, body.Rotation, origin - topLeftVertex, 1, SpriteEffects.None, 1);

            if (equipName == EquipmentName.thermometer) DrawThetmometerThread(spriteBatch);

            if (equipName == EquipmentName.pHscale) DrawpHvalue(spriteBatch);
        }

        void DrawClamps(SpriteBatch spriteBatch)
        {
            if(gameContent.levelIndex == -1)
            {
                Vector2 position = body.Position * gameContent.b2Scale;
                int i = (int)position.X / gameContent.clampDistance;
                Vector2 jointPosition = new Vector2(i * gameContent.clampDistance + gameContent.clampDistance / 2,
                    position.Y);

                spriteBatch.Draw(gameContent.clampRod, (position + jointPosition) / 2 - gameContent.clampRodOrigin,
                    Color.White);

                spriteBatch.Draw(gameContent.clampJoint, jointPosition - gameContent.clampJointOrigin, Color.White);
            }

            Vector2 pos = new Vector2((float)Math.Cos(body.Rotation),
                (float)Math.Sin(body.Rotation)) * (rightClampPositionX - origin.X);

            spriteBatch.Draw(gameContent.clamp, body.Position * gameContent.b2Scale + pos, null, Color.White,
                body.Rotation + clampRotation, gameContent.clampOrigin, 1, SpriteEffects.None, 1);

            spriteBatch.Draw(gameContent.clamp, body.Position * gameContent.b2Scale - pos, null, Color.White,
                body.Rotation + -clampRotation, gameContent.clampOrigin, 1, SpriteEffects.None, 1);
        }

        void DrawThetmometerThread(SpriteBatch spriteBatch)
        {
            float threadTopY = 247, threadBottomY = 485, bulbCenterY = 515;

            Temperature = MathHelper.Clamp(Temperature, 0, MaxTemperature);

            float ht = Temperature / 500 * (threadBottomY - threadTopY);
            Vector2 pos = body.Position * gameContent.b2Scale;
            Vector2 ori = new Vector2(0, origin.Y - (threadBottomY - ht));

            //Thread
            spriteBatch.Draw(gameContent.thermometerThread, pos,
                new Rectangle(0, 0, gameContent.thermometerThread.Width, (int)(bulbCenterY - threadBottomY + ht)),
                Color.White, body.Rotation, gameContent.thermoThreadOrigin + ori,
                1, SpriteEffects.None, 1);

            ori.X -= 25;

            //Pointer
            spriteBatch.Draw(gameContent.pointer, pos, null, Color.White, body.Rotation,
                gameContent.pointerOrigin + ori, 1, SpriteEffects.None, 1);

            ori.X -= 40;
            ori.Y += 20;

            // unit Kelvin
            spriteBatch.DrawString(gameContent.thermometerFont,
                Temperature == 0 ? "Absolute Zero" : Temperature.ToString("0") + " Kelvin", pos,
                Color.WhiteSmoke, body.Rotation, ori, 1, SpriteEffects.None, 1);
        }

        void DrawpHvalue(SpriteBatch spriteBatch)
        {
            pHvalue = MathHelper.Clamp(pHvalue, MinPhValue, MaxPhValue);

            if (gameContent.levelIndex > -1)
            {
                float theta = (float)(Math.Floor(pHvalue) - 1) * (float)Math.PI * 2 / 11;
                theta += (pHvalue - (float)Math.Floor(pHvalue)) / 180f * (float)Math.PI * 25f;
                body.Rotation = theta;
            }

            spriteBatch.Draw(gameContent.pointer,
                body.Position * gameContent.b2Scale + new Vector2(100, 0) - gameContent.pointerOrigin,
                Color.White);

            spriteBatch.DrawString(gameContent.symbolFont, pHvalue.ToString("0.00"),
                body.Position * gameContent.b2Scale + new Vector2(140, -20),
                Color.Gainsboro, 0, Vector2.Zero, 30f / gameContent.symbolFontSize, SpriteEffects.None, 1);
        }

        public void DrawTopLayer(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(gameContent.equipment[(int)equipName, 1], body.Position * gameContent.b2Scale, null,
                Color.White, body.Rotation, origin - topLeftVertex, 1, SpriteEffects.None, 1);

            if (editMode)
            {
                Color c = Color.White * 0.65f;
                if (isMouseOver) c = Color.White * 1f; 

                spriteBatch.Draw(gameContent.equipMoveButton, body.Position * gameContent.b2Scale, null,
                    c, 0, gameContent.equipMoveButtOrigin, isMouseOver ? 1 : 0.8f, SpriteEffects.None, 1);

                spriteBatch.Draw(gameContent.equipRotationButton, RotationButtonPos(), null, c, 0,
                    gameContent.equipRotButtOrigin, isMouseOver ? 1 : 0.8f, SpriteEffects.None, 1);
            }
        }
    }
}
