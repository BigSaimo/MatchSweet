using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSweet : MonoBehaviour {

	public enum ColorType
    {
        YELLOW,
        BLUE,
        PURPLE,
        RED,
        GREEN,
        PINK,
        ANY,
        COUNT
    };

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    };

    private Dictionary<ColorType, Sprite> colorSpriteDict;

    public ColorSprite[] colorSprites;

    private SpriteRenderer sprite;

    private ColorType color;
    public ColorType Color
    {
        get
        {
            return color;
        }

        set
        {
            SetColor(value);
        }
    }

    public int NumColors
    {
        get
        {
            return colorSprites.Length;
        }
    }

    private void Awake()
    {
        sprite = transform.Find("Sweet").GetComponent<SpriteRenderer>();

        colorSpriteDict = new Dictionary<ColorType, Sprite>();

        for (int i = 0;i < colorSprites.Length;i++)
        {
            if(!colorSpriteDict.ContainsKey(colorSprites[i].color))
            {
                colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
    }

    public void SetColor(ColorType _color)
    {
        color = _color;
        if (colorSpriteDict.ContainsKey(color))
        {
            sprite.sprite = colorSpriteDict[color];
        }
    }
}
