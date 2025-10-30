using UnityEngine;

public class CarMath
{
    public const float MPSToKPH = 3.6f;
    public const float KPHToMPS = 1f / MPSToKPH;

    public const float RPSToRPM = 30f / Mathf.PI;
    public const float RPMToRPS = 1f / RPSToRPM;

    public const float NmToFTLB = 0.737562f;

    public const float WToPS = 0.00135962f;

    public static Vector3 GetClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point, out float t)
    {
        t = 0f;

        var vec = lineEnd - lineStart;

        var lenSq = vec.sqrMagnitude;
        if (lenSq == 0f)
        {
            return lineStart;
        }

        t = Mathf.Clamp01(Vector3.Dot(point - lineStart, vec) / lenSq);

        return lineStart + vec * t;
    }

    public static int ModuloInt(int x, int y)
    {
        return (x % y + y) % y;
    }

    public static float ModuloFloat(float x, float y)
    {
        return (x % y + y) % y;
    }

    public static float CalcSignedDistance(float x, float y, float length)
    {
        var d = x - y;
        d -= Mathf.Floor(d / length) * length;
        if (d > length / 2f)
        {
            d -= length;
        }
        return d;
    }

    public static float CalcDistance(float x, float y, float length)
    {
        return Mathf.Abs(CalcSignedDistance(x, y, length));
    }

    public static bool RandomBool()
    {
        return Random.Range(0, 2) == 0;
    }

    public static float EngineRPMToSpeedKPH(float engineRPM, float gearRatio, float finalGearRatio, float wheelRadius)
    {
        return (engineRPM * 2f * Mathf.PI * wheelRadius * 60f) / (gearRatio * finalGearRatio * 1000f);
    }

    public static float SpeedKPHToEngineRPM(float speedKPH, float gearRatio, float finalGearRatio, float wheelRadius)
    {
        return (speedKPH * gearRatio * finalGearRatio * 1000f) / (2f * Mathf.PI * wheelRadius * 60f);
    }
}
