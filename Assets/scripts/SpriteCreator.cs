using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpriteCreator : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private float[] noisesMultipliers;
    [SerializeField] private int WSize;
    [SerializeField] private int HSize;
    [SerializeField] private int NoisesCount;
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
        Debug.Log(1);
        int width = (int) Mathf.Pow(2, WSize);
        int height = (int) Mathf.Pow(2, HSize);
        map = new Map(NoisesCount, width, noisesMultipliers);
        
        Texture2D texture = new Texture2D(width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color color = gradient.Evaluate(map.map[i, j]); //new Color(map.map[i, j], map.map[i, j], map.map[i, j]);
                texture.SetPixel(i,j,color);
            }
        }
        
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0,0,width,height), new Vector2());

        image.sprite = sprite;
    }

    private Color RandomColor()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }
}

public class Map
{
    public Noise[] Noises;
    public float[,] map;
    public int NoisesCount;
    
    public Map(int noisesCount, int mapSize, float[] noisesMultipliers)
    {
        NoisesCount = noisesCount;
        Noises = new Noise[NoisesCount];

        //make all noises
        for (int i = 0; i < NoisesCount; i++)
        {
            int minNoiseLevel = 1;
            //Noises[i] = new Noise((int) Mathf.Pow(2, i + minNoiseLevel), mapSize);
            Noises[i] = new Noise((int) i + minNoiseLevel, mapSize);
        }
        
        //combine noises
        map = new float[mapSize, mapSize];
        float divider = noisesMultipliers.Sum();
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                float result = 0;
                for (int k = 0; k < noisesCount; k++)
                {
                    result += Noises[k].noiseMap[i, j] * noisesMultipliers[k];
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
                noise[i, j] = new Vector2Int(Random.Range(0, clusterSize) + i*clusterSize, Random.Range(0, clusterSize) + j*clusterSize);
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
        
    }

    private float GetNearest(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y); 
        Vector2Int cluster = new Vector2Int(x / clusterSize, y/clusterSize);
        float nearestValue = mapSize;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <=1; j++)
            {
                float distance =
                    Vector2Int.Distance(
                        noise[(cluster.x + i + pow) % pow, (cluster.y + j + pow) % pow],
                        pos);
                if (distance < nearestValue)
                {
                    nearestValue = distance;
                }
            }
        }

        float nonSmooth = nearestValue / clusterSize; //0-1
        return Mathf.Pow(1.1f,-nearestValue);
    }
}