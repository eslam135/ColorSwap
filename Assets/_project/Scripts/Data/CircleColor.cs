using UnityEngine;

public enum CircleColor
{
    Red,
    Green,
    Purple
}

public static class CircleColorExtensions
{
    public static Color ToUnityColor(this CircleColor circleColor)
    {
        switch (circleColor)
        {
            case CircleColor.Red:
                return new Color(0.95f, 0.15f, 0.15f);

            case CircleColor.Green:
                return new Color(0.35f, 0.9f, 0.15f);

            case CircleColor.Purple:
                return new Color(0.65f, 0.3f, 0.95f);

            default:
                return Color.white;
        }
    }
}