using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameDataLibrary
{
    public class Box
    {
        public float Width, Height, RotationInDeg;
        public Vector2 Position;
    }

    public class Circle
    {
        public float Diameter;
        public Vector2 Position;
    }

    public class ClampData
    {
        public bool ClampEnabled;
        public float RightClampPositionX;
        public float RotationInDeg;
    }

    public class EquipmentData
    {
        public Vector2 TopLeftVertex, Origin, RotationButtonPosition;

        public ClampData ClampData;

        public List<Vector2> ContinuousEdges;

        public List<Box> Boxes;

        public List<Circle> Circles;
    }
}
