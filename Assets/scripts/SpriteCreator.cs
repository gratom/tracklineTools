using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SpriteCreator : MonoBehaviour
{
    public static float SmoothMultipolier;

    [Range(0.001f, 0.1f)] [SerializeField] private float smoothMultipolier;

    [SerializeField] private Image image;
    [SerializeField] private NoiseSetting[] noises;
    [SerializeField] private int WSize;
    [SerializeField] private int HSize;
    [SerializeField] private Gradient gradient;


    private Map map;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            MakeSprite();
        }
    }

    private void MakeSprite()
    {
        SmoothMultipolier = smoothMultipolier;
        Debug.Log(1);
        int width = (int)Mathf.Pow(2, WSize);
        int height = (int)Mathf.Pow(2, HSize);
        map = new Map(noises, width);

        Texture2D texture = new Texture2D(width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color color = gradient.Evaluate(map.map[i, j]); //new Color(map.map[i, j], map.map[i, j], map.map[i, j]);
                texture.SetPixel(i, j, color);
            }
        }

        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2());

        image.sprite = sprite;
    }

    private Color RandomColor()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }
}

[Serializable]
public class NoiseSetting
{
    public float multiplier;
    public int noiseSize;
}

public class Map
{
    public Noise[] Noises;
    public float[,] map;
    public int NoisesCount;

    public Map(NoiseSetting[] noises, int mapSize)
    {
        NoisesCount = noises.Length;
        Noises = new Noise[NoisesCount];

        //make all noises
        for (int i = 0; i < NoisesCount; i++)
        {
            Noises[i] = new Noise(noises[i].noiseSize, mapSize);
            //Noises[i] = new Noise((int)i + minNoiseLevel, mapSize);
        }

        //combine noises
        map = new float[mapSize, mapSize];
        float divider = noises.Sum(x => x.multiplier);
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                float result = 0;
                for (int k = 0; k < noises.Length; k++)
                {
                    result += Noises[k].noiseMap[i, j] * noises[k].multiplier;
                }

                map[i, j] = result / divider;
            }
        }
    }
}

public class Noise
{
    public Vector2Int[,] noise;
    public float[,] noiseMap;
    public float[,] smoothMap;
    public int mapSize;
    public int clusterSize;
    public int pow;

    public Noise(int powSize, int fullMapSize)
    {
        pow = powSize;
        mapSize = fullMapSize;
        noise = new Vector2Int[pow, pow];
        clusterSize = mapSize / pow;
        for (int i = 0; i < pow; i++)
        {
            for (int j = 0; j < pow; j++)
            {
                noise[i, j] = new Vector2Int(Random.Range(0, clusterSize) + i * clusterSize, Random.Range(0, clusterSize) + j * clusterSize);
            }
        }

        noiseMap = new float[mapSize, mapSize];

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                noiseMap[i, j] = GetNearest(i, j);
            }
        }

        //smooth
        smoothMap = new float[mapSize, mapSize];
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                smoothMap[i, j] = GetSmooth(i, j);
            }
        }

        noiseMap = smoothMap;
    }

    private float GetNearest(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        Vector2Int cluster = new Vector2Int(x / clusterSize, y / clusterSize);
        float nearestValue = mapSize;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector2Int posDot = noise[(cluster.x + i + pow) % pow, (cluster.y + j + pow) % pow];

                if (cluster.x + i < 0)
                {
                    posDot.x -= mapSize;
                }
                else
                {
                    if (cluster.x + i >= pow)
                    {
                        posDot.x += mapSize;
                    }
                }

                if (cluster.y + j < 0)
                {
                    posDot.y -= mapSize;
                }
                else
                {
                    if (cluster.y + j >= pow)
                    {
                        posDot.y += mapSize;
                    }
                }

                float distance =
                    Vector2Int.Distance(
                        posDot,
                        pos);
                if (distance < nearestValue)
                {
                    nearestValue = distance;
                }
            }
        }

        float nonSmooth = 1 - nearestValue / clusterSize; //0-1
        return nonSmooth; // Mathf.Pow(1.1f,-nearestValue);
    }

    private float GetSmooth(int x, int y)
    {
        float mapperMultiplier = SpriteCreator.SmoothMultipolier;
        int step = (int)(clusterSize * mapperMultiplier);
        int mapperX = ((int)(x + (-1.5f * step)) + mapSize * 2) % mapSize;
        int mapperY = ((int)(y + (-1.5f * step)) + mapSize * 2) % mapSize;

        float[,] points = new float[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                points[i, j] = noiseMap[mapperX, mapperY];
                mapperX += step;
                mapperX = (mapperX + mapSize * 2) % mapSize;
            }

            mapperY += step;
            mapperY = (mapperY + mapSize * 2) % mapSize;
        }

        float f1 = Interpolate4(points[0, 0], points[1, 0], points[0, 1], points[1, 1]);
        float f2 = Interpolate4(points[2, 0], points[3, 0], points[2, 1], points[1, 1]);
        float f3 = Interpolate4(points[0, 2], points[1, 2], points[0, 3], points[1, 3]);
        float f4 = Interpolate4(points[2, 2], points[3, 2], points[2, 3], points[3, 3]);

        return Interpolate4(f1, f2, f3, f4);
    }

    private float Interpolate4(float f1, float f2, float f3, float f4)
    {
        return (f1 + f2 + f3 + f4) / 4;
    }
}
