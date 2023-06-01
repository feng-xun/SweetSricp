using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSweet : MonoBehaviour
{

    private SpriteRenderer sprite;
    private ColorType color;
    public ColorSprite[] colorSprites;
    private Dictionary<ColorType, Sprite> colorSpriteDict;

    //用颜色来表示甜品的称呼；
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
    }
    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }
    
    public int NumColors
    {
        get => colorSprites.Length;
    }
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

    private void Awake()
    {
        sprite = transform.Find("Sweet").gameObject.GetComponent<SpriteRenderer>();
        colorSpriteDict = new Dictionary<ColorType, Sprite>();
        for(int i = 0;i < colorSprites.Length; i++)
        {
            if (!colorSpriteDict.ContainsKey(colorSprites[i].color))
            {
                colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
    }

    public void SetColor(ColorType newColor)
    {
        color = newColor;
        if (colorSpriteDict.ContainsKey(newColor)) 
        {
            sprite.sprite = colorSpriteDict[newColor]; 
        }
        
    }

}
