using System;
using System.Collections.Generic;
using UnityEngine;

public class Candy : MonoBehaviour
{
    public int CandyID;
    public Vector3 startPos;
    public Vector3 endPos;
    public int InCell;
    public bool isMoving;
    public float SegmentTime;
    public float LerpTime;

    public static Candy instance;
    
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // if (isMoving)
        //     if (LerpTime < SegmentTime)
        //     {
        //         LerpTime += Time.deltaTime;
        //         LerpStep(LerpTime / SegmentTime);
        //     }
    }

    // Input: time T, Output: position of object at T time
    private void LerpStep(float t)
    {
        var pos = Vector3.Lerp(startPos, endPos, t);
        transform.position = pos;
    }
    
    // Start is called before the first frame update
    public void SetCandySprite(Sprite candySprite)
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = candySprite;
    }
}