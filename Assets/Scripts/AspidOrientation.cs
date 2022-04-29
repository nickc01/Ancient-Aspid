public enum AspidOrientation
{
    Left,
    Center,
    Right
}


namespace UnityEngine
{
    public static class AspidOrientation_Extensions
    {
        public static AspidOrientation Flip(this AspidOrientation orientation)
        {
            switch (orientation)
            {
                case AspidOrientation.Left:
                    return AspidOrientation.Right;
                case AspidOrientation.Right:
                    return AspidOrientation.Left;
                default:
                    return orientation;
            }
        }
    }
}
