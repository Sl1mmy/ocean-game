using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFallofMap (int size, float falloffStart, float falloffEnd)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                
                if (value < falloffStart)
                {
                    map[i, j] = 0;
                } else if(value > falloffEnd)
                {
                    map[i, j] = 1;
                } else
                {
                    map[i, j] = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(falloffStart, falloffEnd, value));
                }
            }
        }
        return map;
    }
}
