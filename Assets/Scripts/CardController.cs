using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SuitType
{
    Spade,
    Club,
    Diamond,
    Heart,
    Joker
}

public class CardController : MonoBehaviour
{
    public const float Width = 0.06f;
    public const float Height = 0.09f;
    public SuitType Suit;
    public int No;
    public int PlayerNo;
    public int Index;
    public Vector3 HandPosition;
    public Vector2Int IndexPosition;
    public Color SuitColor;
    public bool isFrontUp;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void FlipCard(bool frontup = true)
    {
        float anglez = 0;
        if (!frontup)
        {
            anglez = 180;
        }
        isFrontUp = frontup;
        transform.eulerAngles = new Vector3(0, 0, anglez);
    }
}
