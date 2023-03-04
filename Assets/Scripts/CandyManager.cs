using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
public class CandyManager : MonoBehaviour
{
    public GameObject CandyPrefab;
    
    public static CandyManager Instance;
    
    public static int SpritesCount;
    public float TimeToMovePath;
    public float TimeToMoveEachPosition;
    
    public List<int> CandyKeys;
    public List<Sprite> CandySprite;

    public static Dictionary<int, Sprite> candyMaps = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);
        for (var i = 0; i < CandyKeys.Count; i++)
            candyMaps[CandyKeys[i]] = CandySprite[i];
        SpritesCount = CandySprite.Count;
    }

    public GameObject GenerateCandy(int candyId, int inCell)
    {
        var candy = Instantiate(CandyPrefab);
        
        // called SetCandySprite with input is a sprite, output is render a sprite in game mode
        candy.GetComponent<Candy>().SetCandySprite(candyMaps[CandyKeys[candyId]]);
        candy.GetComponent<Candy>().CandyID = candyId;
        candy.GetComponent<Candy>().InCell = inCell;
        return candy;
    }
    
    public static void MovingInPath(GameObject a, int endPosition)
    {
        Instance.StartCoroutine(Instance.MovingPath(a, endPosition));
    }
    
    public static void MovingInPathForEachItem(GameObject a, int endPosition, int startPosition)
    {
        Instance.StartCoroutine(Instance.MovingPathForEachItem(a, endPosition, startPosition));
    }
    
    IEnumerator MovingPathForEachItem(GameObject a,int endPosition, int start)
    {
        while (start <= endPosition)
        {
            Instance.StartCoroutine(Moving(a, GameManager.positions[start]));
            start++;
            yield return new WaitForSeconds(TimeToMovePath);
        }
    }
    
    IEnumerator MovingPath(GameObject a,int endPosition)
    {
        int currentPosition = 0;
        while (currentPosition <= endPosition)
        {
            Instance.StartCoroutine(Moving(a, GameManager.positions[currentPosition]));
            currentPosition++;
            yield return new WaitForSeconds(TimeToMovePath);
        }
    }
    
    public static void MoveSwap(GameObject a, Vector3 endPosition)
    {
        Instance.StartCoroutine(Instance.Moving(a, endPosition));
    }
    
    IEnumerator Moving(GameObject a, Vector3 currentPosition)
    {
        var startPos = a.transform.position;
        float time = 0;
        
        while (time <= TimeToMoveEachPosition)
        {
            a.transform.position = Vector3.Lerp(startPos, currentPosition, time / TimeToMoveEachPosition);
            time += Time.deltaTime;
            yield return null;
        }
    }
}