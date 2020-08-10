using UnityEngine;

namespace shaco
{
    public enum Direction
    {
		None = -2,
        Automatic = -1,
        Right,
        Left,
        Down,
        Up,
    }

    public enum Anchor
    {
        UpperLeft = 0,
        UpperCenter = 1,
        UpperRight = 2,
        MiddleLeft = 3,
        MiddleCenter = 4,
        MiddleRight = 5,
        LowerLeft = 6,
        LowerCenter = 7,
        LowerRight = 8
    }

    public class Pivot
    {
        static public Vector3 UpperLeft = new Vector3(0, 1);
        static public Vector3 UpperCenter = new Vector3(0.5f, 1);
        static public Vector3 UpperRight = new Vector3(1, 1);
        static public Vector3 MiddleLeft = new Vector3(0, 0.5f);
        static public Vector3 MiddleCenter = new Vector3(0.5f, 0.5f);
        static public Vector3 MiddleRight = new Vector3(1, 0.5f);
        static public Vector3 LowerLeft = new Vector3(0, 0);
        static public Vector3 LowerCenter = new Vector3(0.5f, 0);
        static public Vector3 LowerRight = new Vector3(1, 0);
    }
}