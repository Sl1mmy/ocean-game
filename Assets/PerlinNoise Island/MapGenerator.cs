using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap};
    public DrawMode drawMode;


    const int mapChunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;

    [Range(0f, 1f)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool useFalloff;

    [Range (0f, 1f)]
    public float falloffStart = 0f;
    [Range(0f, 1f)]
    public float falloffEnd = 1f;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    float[,] falloffMap;

    private void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFallofMap(mapChunkSize, falloffStart, falloffEnd);
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for(int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if(useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap)
        {
            display.drawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.drawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.drawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        } else if (drawMode == DrawMode.FalloffMap)
        {
            display.drawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFallofMap(mapChunkSize, falloffStart, falloffEnd)));
        }
        

    }

    private void OnValidate()
    {
        if(lacunarity < 1)
        {
            lacunarity = 1;
        }

        if(octaves < 0) 
        {
            octaves = 0;
        }

        falloffMap = FalloffGenerator.GenerateFallofMap(mapChunkSize, falloffStart, falloffEnd);
    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
}
