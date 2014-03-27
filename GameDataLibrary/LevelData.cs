using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameDataLibrary
{
    public enum EquipmentName // in layer order
    {
        electrodes, thermometer, pHscale, funnel, testTube, conicalFlask, measuringCylinder, beaker, clipboard,
    }

    public enum BonusType
    {
        None, Ring, Hydrogen
    }

    public class EquipmentDetails
    {
        public EquipmentName EquipmentName;
        public Vector2 Position;
        public float RotationInDeg;
        public bool IsClamped;
    }

    public class LevelData
    {
        public int MaxAtoms;

        public int[] AtomProbability;

        public Vector2 Entry;

        public BonusType BonusType;

        public List<Vector2> ContinuousBoundry;

        public List<EquipmentDetails> EquipmentDetails;
    }
}
