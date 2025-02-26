using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using Unity.Mathematics;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap};
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int editorPreviewLOD;
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

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFallofMap(mapChunkSize, falloffStart, falloffEnd); //TODO: 3 map sizes
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero, false);

        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.drawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.drawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.drawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.drawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFallofMap(mapChunkSize, falloffStart, falloffEnd)));
        }
    }

    public void RequestMapData(Vector2 centre, bool isOcean, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, isOcean, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, bool isOcean, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre, isOcean);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }


    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0;i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 centre, bool isOcean)
    {
        float newPersistance;
        if (isOcean) { newPersistance = 1; } else { newPersistance = persistance; }

        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, newPersistance, lacunarity, centre + offset, normalizeMode);
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
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    } else 
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
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

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo (Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
