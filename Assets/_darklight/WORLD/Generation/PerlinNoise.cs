using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.World
{
public class PerlinNoise : MonoBehaviour
{

    static int width = 256;
    static int height = 256;
    static float scale = 20f;

    void Update()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material.mainTexture = GenerateTexture();
    }

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Color color = CalculateColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    Color CalculateColor (int x, int y)
    {
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(sample, sample, sample);
    }

    public static int CalculateHeightFromNoise(Vector2Int coord)
    {
        float xCoord = ((float)coord.x / width) * scale;
        float yCoord = ((float)coord.y / height) * scale;

        float sample = Mathf.PerlinNoise(xCoord, yCoord);

        // Scale the sample from 0-1 to 0-10 and convert to an integer
        int heightValue = Mathf.FloorToInt(sample * 20);

        return heightValue;
    }

}
}

