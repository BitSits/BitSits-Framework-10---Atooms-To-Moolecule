using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Box2D.XNA;
using GameDataLibrary;

namespace BitSits_Framework
{
    class Level : IDisposable
    {
        #region Fields


        public int Score { get; private set; }

        public bool IsLevelUp { get; private set; }
        public bool ReloadLevel { get; private set; }

        GameContent gameContent;

        Vector2 atomGravity = new Vector2(0, 1), equipGravity = new Vector2(0, 5);
        World world;

        bool isLAB = false, editMode;
        public bool isMenuEntrySelected = false;

        LevelComponent levelComponent;
        LabComponent labComponent;

        Camera2D camera;
        float targetScale, scale;

        Vector2 mousePos;
        Body mouseGroundBody;
        Atom mouseAtom = null;
        MouseJoint mouseJoint;

        // Atom data
        List<Atom> atoms = new List<Atom>();
        List<Formula> formulas = new List<Formula>();

        // Eqipment data
        List<Equipment> equipments = new List<Equipment>();

        Equipment mouseEq, selectedEq;
        Joint pin;
        bool equipmentAdded;


        #endregion

        #region Initialization


        public Level(GameContent gameContent)
        {
            world = new World(atomGravity, false);
            this.gameContent = gameContent;

            if (gameContent.levelIndex == -1) this.isLAB = true;

            camera = new Camera2D();

            if (!isLAB) LoadLevelComponent();
            else
            {
                labComponent = new LabComponent(gameContent, world);
                editMode = true;
                world.Gravity = equipGravity;
                camera.Scale = targetScale = scale = labComponent.EqScale;
                camera.Position = new Vector2(labComponent.Width, labComponent.Height) / 2 * Camera2D.PhoneScale;
                camera.Speed = 500;
                camera.ScrollBar = new Vector2(50, 50);
                camera.ScrollArea = new Vector2(labComponent.Width, labComponent.Height) * Camera2D.PhoneScale;
            }

            mouseGroundBody = world.CreateBody(new BodyDef());
        }

        void LoadLevelComponent()
        {
            switch (gameContent.levelIndex)
            {
                case 0: levelComponent = new Tutorial(gameContent, world); break;

                case 1: levelComponent = new Ring(gameContent, world); break;

                case 2: levelComponent = new AbsoluteZero(gameContent, world); break;

                case 3: levelComponent = new ClearAll(gameContent, world); break;

                case 4: levelComponent = new Electro(gameContent, world); break;

                case 5: levelComponent = new pH(gameContent, world); break;

                case 6: levelComponent = new Reaction(gameContent, world); break;

                case 7: levelComponent = new RadioActive(gameContent, world); break;

                default: levelComponent = null; break;
            }

            if (levelComponent != null)
            {
                equipments = levelComponent.equipments; atoms = levelComponent.atoms;
            }
        }

        public void Dispose() { }


        #endregion

        #region Update and HandleInput


        public void Update(GameTime gameTime)
        {
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds, 10, 10);

            camera.Update(gameTime);
            if (isLAB)
            {
                if (targetScale != camera.Scale)
                {
                    float f = 0.5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (scale > targetScale) scale = Math.Max(scale - f, targetScale);
                    else if (scale < targetScale) scale = Math.Min(scale + f, targetScale);

                    camera.Scale = scale;
                }
            }

            for (int i = atoms.Count - 1; i >= 0; i--)
            {
                atoms[i].Update(gameTime); atoms[i].IsMolecularityChecked = false;
                if (atoms[i].eye == EyeState.Remove) atoms.RemoveAt(i);
            }

            if (!isLAB) CheckMolecularity(gameTime);

            for (int i = formulas.Count - 1; i >= 0; i--) if (!formulas[i].IsActive) formulas.RemoveAt(i);

            if (levelComponent != null)
            {
                levelComponent.Update(gameTime);
                IsLevelUp = levelComponent.IsLevelUp; ReloadLevel = levelComponent.ReloadLevel;
            }

            if (isLAB) CheckForRemoval();
        }

        void CheckMolecularity(GameTime gameTime)
        {
            foreach (Atom a in atoms)
            {
                if (a.IsMolecularityChecked) continue;

                bool canBeMolecule = true;
                int twiceNumberOfBonds = 0, numberOfRings = 0;
                int[] atomCount = new int[gameContent.symbolCount];
                Vector2 averagePosition = new Vector2();

                List<Atom> molecule = new List<Atom>();
                molecule.Add(a);

                for (int i = 0; i < molecule.Count; i++)
                {
                    molecule[i].IsMolecularityChecked = true;

                    if (molecule[i].eye != EyeState.Sleep) canBeMolecule = false;

                    twiceNumberOfBonds += molecule[i].bondedAtoms.Count;
                    atomCount[(int)molecule[i].symbol] += 1;
                    averagePosition += molecule[i].body.Position;

                    foreach (Atom em in molecule[i].bondedAtoms)
                    {
                        if (molecule.IndexOf(em) > i) numberOfRings += 1;
                        if (!em.IsMolecularityChecked && !molecule.Contains(em))
                            molecule.Add(em);
                    }
                }

                // Form a molecule
                if (canBeMolecule)
                {
                    gameContent.bop[gameContent.random.Next(gameContent.bop.Length)].Play();

                    averagePosition *= (float)gameContent.b2Scale / molecule.Count;

                    Formula f = new Formula(atomCount, twiceNumberOfBonds, numberOfRings,
                        levelComponent != null? levelComponent.bonusType : BonusType.None,
                        averagePosition, gameContent);

                    formulas.Add(f);

                    // Destroy
                    foreach (Atom am in molecule) am.Remove();

                    if (levelComponent != null)
                    {
                        if (levelComponent.UpdateNewFormula(f)) Score += f.score;
                        else f.strScore = "";
                    }
                }
            }
        }

        void CheckForRemoval()
        {
            for (int i = equipments.Count - 1; i >= 0; i--)
                if (equipments[i].body.Position.Y * gameContent.b2Scale > labComponent.Height * 1.5f
                    && equipments[i] != mouseEq)
                {
                    equipments[i].Remove(); equipments.Remove(equipments[i]);
                }

            for (int i = atoms.Count - 1; i >= 0; i--)
                if (atoms[i].body.Position.Y * gameContent.b2Scale > labComponent.Height * 1.5f
                    && atoms[i] != mouseAtom)
                {
                    atoms[i].Remove(); atoms.Remove(atoms[i]);
                }
        }

        public void ToggleEditMode()
        {
            editMode = !editMode;

            if (editMode)
            {
                world.Gravity = equipGravity;
                targetScale = labComponent.EqScale;
            }
            else
            {
                world.Gravity = atomGravity;
                targetScale = labComponent.AtomScale;
            }

            if (selectedEq != null)
            {
                selectedEq.isSelected = false; selectedEq = null;
            }

            foreach (Equipment e in equipments) e.SetMode(editMode, false);

            foreach (Atom a in atoms) a.SetMode(editMode, false);
        }

        public void HandleInput(InputState input, int playerIndex)
        {
            mousePos = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y) / Camera2D.PhoneScale;

            camera.HandleInput(input, (PlayerIndex)playerIndex);

            mousePos = ((camera.Position / Camera2D.PhoneScale - gameContent.PlayArea / 2 / camera.Scale)
                + mousePos / camera.Scale) ;

            mousePos /= gameContent.b2Scale;

            if (editMode) HandleEquipment(input);

            if (!(isLAB && editMode)) HandleAtom(input);

            if (mouseJoint != null) mouseJoint.SetTarget(mousePos);
        }

        void HandleEquipment(InputState input)
        {
            Vector2 m = mousePos * gameContent.b2Scale;
            Point mp = new Point((int)m.X, (int)m.Y);

            Equipment mouseOverEq = null;
            if (mouseEq == null)
                foreach (Equipment e in equipments)
                {
                    e.isMouseOver = false;

                    if (mouseOverEq == null && !isMenuEntrySelected
                        && (e.MoveButtonBound.Contains(mp) || e.RotationButtonBound.Contains(mp)))
                    {
                        mouseOverEq = e; e.isMouseOver = true;
                    }
                }

            if (input.IsMouseLeftButtonClick() && mouseOverEq != null)
            {
                if (mouseOverEq.MoveButtonBound.Contains(mp) || mouseOverEq.RotationButtonBound.Contains(mp))
                {
                    MouseJointDef mjd = new MouseJointDef();
                    mjd.maxForce = 500;
                    //mjd.frequencyHz = 10f; mjd.dampingRatio = .1f;
                    mjd.target = mousePos; mjd.bodyA = mouseGroundBody; mjd.bodyB = mouseOverEq.body;

                    mouseJoint = (MouseJoint)world.CreateJoint(mjd);
                    mouseEq = mouseOverEq;
                    mouseEq.body.SetAngularDamping(20000);

                    if (mouseOverEq.RotationButtonBound.Contains(mp))
                    {
                        RevoluteJointDef rjd = new RevoluteJointDef();
                        rjd.bodyA = mouseGroundBody; rjd.bodyB = mouseOverEq.body;
                        rjd.localAnchorA = mouseOverEq.body.Position; rjd.localAnchorB = Vector2.Zero;
                        pin = world.CreateJoint(rjd);

                        mouseEq.body.SetAngularDamping(20);
                    }

                    if (selectedEq != mouseOverEq)
                    {
                        if (selectedEq != null)
                        {
                            selectedEq.isSelected = false; selectedEq.SetMode(editMode, false); selectedEq = null;
                        }

                        selectedEq = mouseOverEq; selectedEq.isClamped = false; selectedEq.isSelected = true;
                    }

                    mouseEq.SetMode(editMode, true);
                }
            }
            else if (mouseOverEq != null && input.IsMouseRightButtonClick()
                || input.CurrentMouseState.LeftButton == ButtonState.Released)
            {
                if (mouseJoint != null && mouseEq != null)
                {
                    world.DestroyJoint(mouseJoint);
                    mouseJoint = null;

                    if (pin != null) { world.DestroyJoint(pin); pin = null; }

                    mouseEq.SetMode(editMode, false);
                    mouseEq = null;
                }

                if (mouseOverEq != null && input.IsMouseRightButtonClick())
                {
                    selectedEq = null;

                    mouseOverEq.Remove(); equipments.Remove(mouseOverEq);
                }
            }

            // Remove
            if (!equipmentAdded && (input.IsMouseLeftButtonClick() || input.IsMouseRightButtonClick()) && mouseOverEq == null
                && mouseEq == null && selectedEq != null)
            {
                selectedEq.isSelected = false; selectedEq.SetMode(editMode, false); selectedEq = null;
            }
            equipmentAdded = false;
        }

        void HandleAtom(InputState input)
        {
            Atom mouseOverAtom = null;
            foreach (Atom a in atoms)
            {
                if (mouseAtom == null && !isMenuEntrySelected && a.eye < EyeState.Disappear
                    && a.fixture.TestPoint(mousePos))
                    mouseOverAtom = a;
            }

            // click to add atoms
            if (mouseOverAtom == null && input.IsMouseLeftButtonClick() && isLAB && !editMode && !isMenuEntrySelected)
                atoms.Add(new Atom((Symbol)(gameContent.random.Next(gameContent.symbolCount - 1) + 1),
                        mousePos * gameContent.b2Scale, gameContent, world));

            if (input.IsMouseLeftButtonClick())
            {
                if (mouseOverAtom != null)
                {
                    MouseJointDef mjd = new MouseJointDef();
                    mjd.maxForce = 500;
                    //mjd.frequencyHz = 10;
                    //mjd.dampingRatio = .1f;
                    mjd.target = mouseOverAtom.body.Position;
                    mjd.bodyA = mouseGroundBody;
                    mjd.bodyB = mouseOverAtom.body;

                    mouseJoint = (MouseJoint)world.CreateJoint(mjd);
                    mouseAtom = mouseOverAtom;
                    mouseAtom.SetMode(editMode, true);
                }
            }
            else if (mouseJoint != null && mouseAtom != null && (mouseAtom.eye == EyeState.Disappear
                || input.CurrentMouseState.LeftButton == ButtonState.Released))
            {
                if (mouseAtom.eye != EyeState.Disappear) // Disappear has priority over LeftButton Released
                {
                    world.DestroyJoint(mouseJoint);
                    mouseAtom.SetMode(editMode, false);
                    mouseAtom.CreateBond();
                }

                mouseJoint = null; mouseAtom = null;
            }

            if (input.IsMouseRightButtonClick())
            {
                if (mouseOverAtom != null)
                {
                    if (mouseOverAtom.bondedAtoms.Count > 0)
                    {
                        gameContent.detach[gameContent.random.Next(gameContent.detach.Length)].Play();
                        mouseOverAtom.DestroyBonds();
                    }
                    else if (isLAB && !editMode && mouseOverAtom.bondedAtoms.Count == 0)
                    {
                        mouseOverAtom.Remove(); atoms.Remove(mouseOverAtom);
                    }
                }
            }
        }

        public void AddEqipment(object sender, PlayerIndexEventArgs ea)
        {
            EquipmentName en = EquipmentName.beaker;
            if (((MenuEntry)sender).UserData is EquipmentName)
                en = ((EquipmentName)((MenuEntry)sender).UserData);
            else return;
            
            Equipment e = new Equipment(en, gameContent, world);
            e.body.Position = camera.Position / Camera2D.PhoneScale / gameContent.b2Scale;

            if (selectedEq != null)
            {
                selectedEq.isSelected = false; selectedEq.SetMode(editMode, false); selectedEq = null;
            }
            selectedEq = e; selectedEq.isSelected = true; selectedEq.SetMode(editMode, false);
            equipmentAdded = true;

            if (equipments.Count == 0) equipments.Add(e);
            else
            {
                for (int i = 0; i < equipments.Count; i++)
                    if (e.equipName <= equipments[i].equipName)
                    {
                        equipments.Insert(i, e); return;
                    }

                equipments.Add(e);
            }
        }

        public void ClampEquipment(object sender, PlayerIndexEventArgs e)
        {
            if (selectedEq != null)
            {
                selectedEq.isSelected = false;
                selectedEq.isClamped = true; selectedEq.SetMode(editMode, false);
                selectedEq = null;
            }
        }

        public void ClearLAB(object sender, PlayerIndexEventArgs e)
        {
            if (isLAB)
            {
                if (editMode)
                {
                    selectedEq = null;

                    for (int i = equipments.Count - 1; i >= 0; i--)
                    {
                        equipments[i].Remove(); equipments.Remove(equipments[i]);
                    }
                }
                else
                {
                    for (int i = atoms.Count - 1; i >= 0; i--)
                    {
                        atoms[i].Remove(); atoms.Remove(atoms[i]);
                    }
                }
            }
        }


        #endregion

        #region Draw


        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.Transform);
            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
            //    SaveStateMode.SaveState, camera.Transform);

            if (labComponent != null) labComponent.Draw(spriteBatch);

            if (levelComponent != null) levelComponent.Draw(spriteBatch, gameTime);

            for (int i = equipments.Count - 1; i >= 0; i--)
            {
                if (equipments[i] == selectedEq) continue; equipments[i].DrawBottomLayer(spriteBatch);
            }

            if (mouseAtom != null) mouseAtom.DrawPreBond(spriteBatch);

            if (!DebugComponent.ShowBonds) DrawBonds(spriteBatch);

            foreach (Atom a in atoms) a.Draw(spriteBatch, gameTime);

            foreach (Formula f in formulas) f.Draw(spriteBatch, gameTime);

            foreach (Equipment e in equipments)
            {
                if (e == selectedEq) continue; e.DrawTopLayer(spriteBatch);
            }

            if (selectedEq != null)
            {
                selectedEq.DrawBottomLayer(spriteBatch); selectedEq.DrawTopLayer(spriteBatch);
            }

            if (DebugComponent.ShowBonds) DrawJoints(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.Transform);
        }

        void DrawJoints(SpriteBatch spriteBatch)
        {
            for (Joint j = world.GetJointList(); j != null; j = j.GetNext())
            {
                Vector2 a = j.GetAnchorA() * gameContent.b2Scale;
                Vector2 b = j.GetAnchorB() * gameContent.b2Scale;

                float scale = Vector2.Distance(a, b);
                float rotation = (float)Math.Atan((b.Y - a.Y) / (b.X - a.X));

                spriteBatch.Draw(gameContent.blank, (a + b) / 2, null, Color.WhiteSmoke, rotation,
                    new Vector2(.5f), new Vector2(scale, 1), SpriteEffects.None, 1);
            }
        }

        void DrawBonds(SpriteBatch spriteBatch)
        {
            for (Joint j = world.GetJointList(); j != null; j = j.GetNext())
            {
                object userData = j.GetUserData();
                if (userData is Bond) ((Bond)userData).Draw(spriteBatch);
            }
        }
        

        #endregion
    }
}
