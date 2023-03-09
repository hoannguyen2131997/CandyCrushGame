using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField]private int CountPoolCandy;
    public float TimeToRenderEachCandy = 0.05f;
    public float TimeDelayEatingCandy = 1f;
    public float TimeDelayToMove = 1f;
    
    private static int score = 0;
    private float timer;   // timer variable be used to control for the duration that show for each candy object 
    private int idToShow;
    private int CountCandyInBoard;

    // assign cells list and objects candy list from gameBoard and CandyList variables
    public Board gameBoard;
    public List<GameObject> CandyList;
    public List<GameObject> CandyListInPool;

    public UIScore uiScore;
    private List<int[]> listCandysValid = new(); // element is list id-candy(category candy) example: list{{3 3 3}, {2 2 2 2}} = 7 point 
    private List<int> listIDToDestroy = new();  // element is id, Candy is valid for scoring, they are removed from list candy example: {5 6 7 11 12 13 14}}
    private List<int> listIDCandiesLeft = new();  // listIDCandiesToMove is the list of ids left after eating candy
    private List<int[]> tempClicked = new();
    
    private Coroutine RenderCandies;
    private Coroutine DestroyCandies;
    private Coroutine CheckScoreCandies;

    private void Awake()
    {
        try
        {
            CountCandyInBoard = Board.BoardWidth*Board.BoardHeight;
        }
        catch (Exception e)
        {
            Debug.Log($"Error: {e}");
        }
    }

    private void Start()
    {
        GenerateCandies(CountCandyInBoard);
        
        gameBoard.GenerateBroad(Board.BoardWidth, Board.BoardHeight);
        Board.GeneratePositions();
        Board.GetCandyListToScore();
        
        RenderCandies = StartCoroutine(RenderCandyList());
        //StartCoroutine(CheckToScore(Board.BoardWidth));
        StartCoroutine(Handle());
    }

    
    IEnumerator CheckStuck()
    {
        // int[] temp = new int[Board.BoardWidth];
        
        // for (int i = 0; i < Board.BoardWidth; i++)
        // {
        //     
        //     List<List<int>> value = new List<List<int>>();
        //     if (Board.ListRow.TryGetValue(i, out value))
        //     {
        //         foreach (var item in value)
        //         {
        //             int x = item[0];
        //             int y = item[1];
        //             int incell = GetIDFormXY(x, y);
        //             int idCandy = CandyList[incell].GetComponent<Candy>().CandyID;
        //
        //             // if (i > 0)
        //             // {
        //             //     List<List<int>> valuePre = new List<List<int>>();
        //             //     if (Board.ListRow.TryGetValue(i - 1, out valuePre))
        //             //     {
        //             //         
        //             //     }
        //             // }
        //             //
        //             // if (i < 4)
        //             // {
        //             //     
        //             // }
        //             Debug.Log($"Row {i}: x: {x}, y: {y}, incell: {incell}, idcandy: {idCandy}");
        //         }
        //     }
        // }
        
        // int[,] listIdCandy = new int[Board.BoardWidth,Board.BoardWidth];
        
        List<int[]> listIdCandy = new List<int[]>();
        
        for (int i = 0; i < Board.BoardWidth; i++)
        {
            List<(int, int)> value = new List<(int, int)>();
            int[] temp = new int[Board.BoardWidth];
            if (Board.ListColumn.TryGetValue(i, out value))
            {
                int count = 0;
                foreach (var item in value)
                {
                    int x = item.Item1;
                    int y = item.Item2;
                    int incell = GetIDFormXY(x, y);
                    temp[count] = CandyList[incell].GetComponent<Candy>().CandyID;
                    count++;
                }
            }
            listIdCandy.Add(temp);
        }

        for (int i = 0; i < listIdCandy.Count; i++)
        {
            for (int j = 0; j < Board.BoardWidth; j++)
            {
                Debug.Log($"idCandy [{i}] : [{j}]: {listIdCandy[i][j]}");
            }
        }
        yield return null;
    }
    
    private int GetIDFormXY(int x, int y)
    {
        int result = -1;

       
        for (int i = 0; i < Board.positions.Count; i++)
        {
            if (x == Board.positions[i].x && y == Board.positions[i].y)
            {
                result = CandyList.Count - i - 1;
            }
        }

        if (result == -1)
        {
            Debug.Log("Don't find id(incell) of candy");    
        }
        
        return result;
    }
    
    
    // in-cell not change, only sprite, idcandy change
    // when candy is valid then in-cell of candy will change
    private Coroutine swapCoroutine;
    IEnumerator Swap(int inCell, Vector3 nextCell, int nextInCell, Vector3 nextCellPre)
    {
        // int tempInCell = 0;
        // int tempInCellPre = 0;
        // for (int i = 0; i < CountCandyInBoard; i++)
        // {
        //     if (CandyList[i].GetComponent<Candy>().InCell == inCell)
        //     {
        //         CandyManager.MoveSwap(CandyList[i], nextCell);
        //         tempInCell = i;
        //     }
        // }
        // for (int i = 0; i < CountCandyInBoard; i++)
        // {
        //     if (CandyList[i].GetComponent<Candy>().InCell == nextInCell)
        //     {
        //         CandyManager.MoveSwap(CandyList[i], nextCellPre);
        //         tempInCellPre = i;
        //     }
        // }
        //
        // CandyList[tempInCell].GetComponent<Candy>().InCell = nextInCell;
        // CandyList[tempInCellPre].GetComponent<Candy>().InCell = inCell;
        CandyManager.MoveSwap(CandyList[inCell], nextCell);
        CandyManager.MoveSwap(CandyList[nextInCell], nextCellPre);
        var itemp = CandyList[inCell];
        CandyList[inCell] = CandyList[nextInCell];
        CandyList[nextInCell] = itemp;
        StartCoroutine(RestListCandies());
        yield return null;
    }
    

    [SerializeField] private Camera mainCamera;

    private Coroutine AddClick;
    void AddClickPosition(int x, int y, int inCell)
    {
        int xPre;
        int yPre;
        int inCellPre;
        if (tempClicked.Count == 0)
        {
            Board.HighLightCell(x, y);
            int[] tempPos = new int[3];
            tempPos[0] = x;
            tempPos[1] = y;
            tempPos[2] = inCell;
            tempClicked.Add(tempPos);
        }
        else if (tempClicked.Count == 1)
        {
            //var contains = tempClicked.Any(arr => arr.SequenceEqual(MyArray));
            xPre = tempClicked[0][0];
            yPre = tempClicked[0][1];
            inCellPre = tempClicked[0][2];
            int[] tempPos = new int[3];
            if (x == xPre && y == yPre + 1) // top
            {
                Board.HighLightCell(x, y);
                tempPos[0] = x;
                tempPos[1] = y;
                tempPos[2] = inCell;
                tempClicked.Add(tempPos);
                Vector3 nextCell = new Vector3(xPre, yPre, 0);
                Vector3 nextCellPre = new Vector3(x, y, 0);
                StartCoroutine(Swap(inCell, nextCell, inCellPre, nextCellPre));
                //StartCoroutine(Handle());
            } else if (x == xPre && y == yPre - 1) // bottom
            {
                Board.HighLightCell(x, y);
                tempPos[0] = x;
                tempPos[1] = y;
                tempPos[2] = inCell;
                tempClicked.Add(tempPos);
                Vector3 nextCell = new Vector3(xPre, yPre, 0);
                Vector3 nextCellPre = new Vector3(x, y, 0);
                StartCoroutine(Swap(inCell, nextCell, inCellPre, nextCellPre));
                //StartCoroutine(Handle());
            } else if (y == yPre && x == xPre + 1)
            {
                Board.HighLightCell(x, y);
                tempPos[0] = x;
                tempPos[1] = y;
                tempPos[2] = inCell;
                tempClicked.Add(tempPos);
                Vector3 nextCell = new Vector3(xPre, yPre, 0);
                Vector3 nextCellPre = new Vector3(x, y, 0);
                StartCoroutine(Swap(inCell, nextCell, inCellPre, nextCellPre));
                //StartCoroutine(Handle());
            } else if (y == yPre && x == xPre - 1)
            {
                Board.HighLightCell(x, y);
                tempPos[0] = x;
                tempPos[1] = y;
                tempPos[2] = inCell;
                tempClicked.Add(tempPos);
                Vector3 nextCell = new Vector3(xPre, yPre, 0);
                Vector3 nextCellPre = new Vector3(x, y, 0);
                StartCoroutine(Swap(inCell, nextCell, inCellPre, nextCellPre));
                //StartCoroutine(Handle());
            }
            else
            {
                Board.UnHighLightCell(xPre, yPre);
                tempClicked.Clear();
                tempPos[0] = x;
                tempPos[1] = y;
                tempPos[2] = inCell;
                tempClicked.Add(tempPos);
                Board.HighLightCell(x, y);
            }
        }
        else
        {
            Board.UnHighLightCell(x, y);
            tempClicked.Clear();
            Debug.Log("done!");
        }
       
    }

    void CheckPosition()
    {
       
        Debug.Log("count: " + tempClicked.Count);
        // for (int i = 0; i < tempClicked.Count; i++)
        // {
        //     Debug.Log(" x: " + tempClicked[0][0] + " y: " + tempClicked[0][1]);
        // }
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                if(hit.collider.gameObject.tag == "cell")
                {
                    int x = (int)hit.transform.position.x;
                    int y = (int)hit.transform.position.y;
                    int myKey = Board.DictPositionToMove.FirstOrDefault(item => item.Value[0] == x && item.Value[1] == y).Key;
                    int inCell = CandyList.Count - myKey - 1;
                   
                    AddClickPosition(x, y, inCell);
                    CheckPosition();
                }
            }
        }
    }
    private void GenerateCandy(int idCandy, int inCell, List<GameObject> CandyListToAdd)
    {
        var candyKey = CandyManager.Instance.GenerateCandy(idCandy, inCell);
        candyKey.transform.parent = gameBoard.parentCandyTranForm;
        candyKey.SetActive(false);
        CandyListToAdd.Add(candyKey);
    }
    
    private void GenerateCandyInPool(int idCandy, int inCell, List<GameObject> CandyListToAdd)
    {
        var candyKey = CandyManager.Instance.GenerateCandy(idCandy, inCell);
        candyKey.transform.parent = gameBoard.ParentCandyPoolTranSform;
        candyKey.SetActive(false);
        CandyListToAdd.Add(candyKey);
    }
    
    private void GenerateCandies(int totalCandy)
    {
        for (var i = 0; i < totalCandy; i++)
        {
            if (i == totalCandy-1)
            {
                GenerateCandy(0,i, CandyList);
            }
            else
            {
                var idCandy = Random.Range(1, CandyManager.SpritesCount);
                GenerateCandy(idCandy,i, CandyList);
            }
        }
        
        for (int i = 0; i < CountPoolCandy; i++)
        {
            var idCandy = Random.Range(1, CandyManager.SpritesCount);
            GenerateCandyInPool(idCandy,i, CandyListInPool);
        }
    }

    // int ScoreCandyListValid(List<int[]> listCandysValid)
    // {
    //     foreach (var item in listCandysValid)
    //     {
    //         for (int i = 0; i < item.Length; i++)
    //         {
    //             score++;
    //         }
    //     }
    //     listCandysValid.Clear();
    //     return score;
    // }

    bool CheckID(int x, int y, int orderId)
    {
        bool result = false;
        var valueOrder = new int[2];
        if (Board.DictPositionToMove.TryGetValue(orderId, out valueOrder))
        {
            if (valueOrder[0] == x && valueOrder[1] == y)
            {
                result = true;
            }
        }
        return result;
    }

    Dictionary<int,int> GetIDCandy(int x, int y)
    {
        int candyId = 0;
        Dictionary<int,int> candyListIdRow = new Dictionary<int,int>();
        int k = 0;
        
        for (int i = 0; i < Board.DictPositionToMove.Count; i++)
        {
            int idToCheck = CountCandyInBoard - 1 - i;
            if (CheckID(x, y, i))
            {
                candyId = CandyList[idToCheck].GetComponent<Candy>().CandyID; // get id candy;
                candyListIdRow.Add(idToCheck, candyId);
                k++;
            }
        }
        return candyListIdRow;
    }

    void ScoreEachRowOrColumn(int idCandy, List<int> tempCandyValid, int idOrderList, List<int> tempCandyValidToDestroy)
    {
        int countCandyValid = tempCandyValid.Count;
        if (countCandyValid == 0)
        {
            tempCandyValid.Add(idCandy);
            
            tempCandyValidToDestroy.Add(idOrderList); // Destroy
        }
        else if (idCandy != tempCandyValid[countCandyValid - 1] && countCandyValid < 3)
        {
            tempCandyValid.Clear();
            tempCandyValid.Add(idCandy);
            
            tempCandyValidToDestroy.Clear(); // Destroy
            tempCandyValidToDestroy.Add(idOrderList);
        }
        else if (idCandy != tempCandyValid[countCandyValid - 1] && countCandyValid >= 3)
        {
            listCandysValid.Add(tempCandyValid.ToArray());
            tempCandyValid.Clear();
            tempCandyValid.Add(idCandy);
            
            listIDToDestroy.AddRange(tempCandyValidToDestroy); // destroy
            tempCandyValidToDestroy.Clear();
            tempCandyValidToDestroy.Add(idOrderList);
        }
        else 
        {
            tempCandyValid.Add(idCandy);
            
            tempCandyValidToDestroy.Add(idOrderList); // Destroy
        }
    }
    private void ListRowsOrColumns(int check, bool row)
    {
        List<(int, int)> value = new List<(int, int)>(); // value is list row or column candy
        if (row == true)
        {
            //Debug.Log("ListRow: " + Board.ListRow.Count);
            if (Board.ListRow.TryGetValue(check, out value)) // Get value of ListRow 
            {
                //Debug.Log("Is Row" + check);
                List<int> tempCandyValid = new List<int>();
                List<int> tempCandyValidToDestroy = new List<int>();
                
                foreach (var item in value) // each item is a row of cells
                {
                    Dictionary<int, int> candyIdListRow = GetIDCandy(item.Item1, item.Item2);
                    for (int i = 0; i < Board.DictPositionToMove.Count; i++) // for loop be order to get id candy each member of row (check if x,y of candy == x,y of cell => id in list Order then retun id candy (candy categorize))
                    {
                        int idCandy;
                        if (candyIdListRow.TryGetValue(i, out idCandy))
                        {
                            ScoreEachRowOrColumn(idCandy, tempCandyValid, i, tempCandyValidToDestroy);
                        }
                    }
                }
                if (tempCandyValid.Count >= 3)
                {
                    listCandysValid.Add(tempCandyValid.ToArray());
                    listIDToDestroy.AddRange(tempCandyValidToDestroy);
                  
                    // foreach (var VARIABLE in listCandysValid)
                    // {
                    //     Debug.Log("tempCandyValidToDestroy" + VARIABLE);
                    // }
                }
            }
        }
        else
        {
            if (Board.ListColumn.TryGetValue(check, out value))
            {
                //Debug.Log("Is Column: " + check);
                List<int> tempCandyValid = new List<int>();
                List<int> tempCandyValidToDestroy = new List<int>();

                foreach (var item in value)
                {
                    Dictionary<int, int> candyIdListColumn = GetIDCandy(item.Item1, item.Item2);
                    for (int i = 0; i < Board.DictPositionToMove.Count; i++)
                    {
                        int values;
                        if (candyIdListColumn.TryGetValue(i, out values))
                        {
                            int idCandy;
                            if (candyIdListColumn.TryGetValue(i, out idCandy))
                            {
                                ScoreEachRowOrColumn(idCandy, tempCandyValid, i, tempCandyValidToDestroy);
                            }
                        }
                    }
                }
                if (tempCandyValid.Count >= 3)
                {
                    listCandysValid.Add(tempCandyValid.ToArray());
                    listIDToDestroy.AddRange(tempCandyValidToDestroy);
                   
                    // foreach (var VARIABLE in listCandysValid)
                    // {
                    //     for (int i = 0; i < VARIABLE.Length; i++)
                    //     {
                    //         Debug.Log("tempCandyValidToDestroy " + i + " :" + VARIABLE[i]);
                    //     }
                    // }
                }
            }
        }
    }

    private int sumScore = 0;
    IEnumerator CheckToScore(int BoardWidth)
    {
        for (int i = 0; i < BoardWidth; i++)
        {
            ListRowsOrColumns(i,true);
            ListRowsOrColumns(i,false);
        }
        
        // Board.ListRow.Clear();
        // Board.ListColumn.Clear();
        yield return null;
    }

    IEnumerator RenderCandyList()
    {
        while (idToShow < CountCandyInBoard)
        {
            CandyList[idToShow].SetActive(true);
            int posTarget = CountCandyInBoard - idToShow - 1;
            CandyManager.MovingInPath(CandyList[idToShow], posTarget);
            yield return new WaitForSeconds(TimeToRenderEachCandy);
            idToShow++;
        }
    }

    // IEnumerator RenderCandyListFromInCell()
    // {
    //     while (idToShow < CountCandyInBoard)
    //     {
    //         int idFromInCell = CandyList[idToShow].GetComponent<Candy>().InCell;
    //         // if (idFromInCell ==)
    //         // {
    //         //     
    //         // }
    //         CandyList[idToShow].SetActive(true);
    //         int posTarget = CountCandyInBoard - idToShow - 1;
    //         CandyManager.MovingInPath(CandyList[idToShow], posTarget);
    //         yield return new WaitForSeconds(TimeToRenderEachCandy);
    //         idToShow++;
    //     }
    // }
    
    IEnumerator RenderCandiesFormID(int idToStart)
    {
        //listIDCandiesToMove
        for (int i = idToStart; i < CountCandyInBoard; i++)
        {
            if (listIDCandiesLeft.Contains(CandyList[i].GetComponent<Candy>().InCell))
            {
                int posTarget = CountCandyInBoard - CandyList[i].GetComponent<Candy>().InCell - 1;
                CandyManager.MovingInPathForEachItem(CandyList[i], posTarget, CountCandyInBoard - i);
                yield return new WaitForSeconds(TimeToRenderEachCandy);
            }
        }
        
        //StartCoroutine(RenderCandiesFromPool(idToStart));
    }

    void DeletedDulicatedId(List<int> id)
    {
        int index = listIDToDestroy.Count - 1;
        while (index > 0)
        {
            if (listIDToDestroy[index] == listIDToDestroy[index - 1])
            {
                if (index < listIDToDestroy.Count - 1)
                    (listIDToDestroy[index], listIDToDestroy[listIDToDestroy.Count - 1]) = (listIDToDestroy[listIDToDestroy.Count - 1], listIDToDestroy[index]);
                listIDToDestroy.RemoveAt(listIDToDestroy.Count - 1);
                index--;
            }
            else
                index--;
        }
    }

    void GetIdCandiesLeft(int idToStart)
    {
        int countCandy = 0;
        for (int i = 0; i < CountCandyInBoard; i++)
        {
            bool isExist = listIDToDestroy.Contains(i);
            
            if (!isExist) {
                listIDCandiesLeft.Add(i + idToStart - countCandy);
                if (listIDCandiesLeft.Count == CountCandyInBoard - listIDToDestroy.Count - idToStart)
                {
                    break;
                }
            }
            else
            {
                countCandy++;
            }
        }
    }
    
    void ChangeId(int idToStart, List<int> listIDToChange)
    {
        int count = 0;

        for (int i = idToStart; i < CountCandyInBoard; i++)
        {
            if (!listIDToDestroy.Contains(i))
            {
                CandyList[i].GetComponent<Candy>().InCell = listIDToChange[count];
                count++;
            }
        }
        listIDToDestroy.Clear();
    }

    IEnumerator DestroyCandy(GameObject objCandy)
    {
        Vector3 startPos = new Vector3(0, 6, 0);
        objCandy.GetComponent<Candy>().transform.position = startPos;
        objCandy.SetActive(false);
        objCandy.transform.parent = gameBoard.ParentCandyPoolTranSform;
        yield return null;
    }
    
    IEnumerator DestroyListCandy()
    {
        yield return new WaitForSeconds(TimeDelayToMove);
        listIDToDestroy.Sort();
        foreach (var item in listIDToDestroy)
        {
            for (int i = CountCandyInBoard - 1; i >= 0; i--)
            {
                if (item == i)
                {
                    StartCoroutine(DestroyCandy(CandyList[i]));
                    yield return RenderCandies;
                }
            }
        }
    }
    
    void GetCandyInPool()
    {
        int countCandyList = CountCandyInBoard;
        int count = countCandyList - listIDToDestroy.Count;
        int idToGetCandyInPoll = 0; // get first candy item in pool

        for (int i = 0; i < countCandyList; i++)
        {
            foreach (var item in listIDToDestroy)
            {
                if (item == i)
                {
                    var tempItemCandyListPool = CandyListInPool[idToGetCandyInPoll];
                    CandyList[i] = tempItemCandyListPool;
                    RandomCanndyInPool(CandyList[i], count);
                    
                    count++;
                    idToGetCandyInPoll++;
                }
            }
        }
    }

    private void RandomCanndyInPool(GameObject candy, int count)
    {
        int id = Random.Range(1, CandyManager.Instance.CandyKeys.Count);
        
        candy.GetComponent<Candy>().CandyID = id;
        candy.GetComponent<SpriteRenderer>().sprite = CandyManager.Instance.CandySprite[id];
        candy.GetComponent<Candy>().InCell = count;
        candy.transform.parent = gameBoard.parentCandyTranForm;
        candy.SetActive(true);
        
        // List<Candy> tempCandies
    }
    
    IEnumerator RenderCandiesFromPool(int idToStart)
    {
        for (int i = idToStart; i < CountCandyInBoard; i++)
        {
            if (!listIDCandiesLeft.Contains(CandyList[i].GetComponent<Candy>().InCell))
            {
                int posTarget = CountCandyInBoard - CandyList[i].GetComponent<Candy>().InCell - 1;
                CandyManager.MovingInPath(CandyList[i], posTarget);
                yield return new WaitForSeconds(TimeToRenderEachCandy);
            }
        }
        listIDCandiesLeft.Clear();
    }

    private Coroutine handle1;
    private Coroutine handle2;
    
    IEnumerator Handle()
    {
        StartCoroutine(CheckToScore(Board.BoardWidth));
        
        StartCoroutine(CheckStuck());
        
        // After change id, sprite(id = 0, sprite = null) of candy. We will delay 3 seconds to look result that we find before
        DestroyCandies = StartCoroutine(DestroyListCandy());
        yield return DestroyCandies;
        yield return new WaitForSeconds(TimeDelayEatingCandy);
        
        // listIDToDestroy is list(int) of candy list, they are valid candies for scoring and will be removed (will be sent to list of ids in pool) from the list afterwards 
        DeletedDulicatedId(listIDToDestroy);
        
        int idToStart;
        if (listIDToDestroy.Count() == 0)
        {
            idToStart = 0;
        }
        else
        {
            idToStart = listIDToDestroy[0];
            // Get list of candies to replace (they will be moved to the end of the list)
            GetCandyInPool();
        }
      
        // Reverse list 
        CandyListInPool.Reverse();

        // Push candies after removed candies valid for scoring
        GetIdCandiesLeft(idToStart);
      
        // Change ID for candies  
        ChangeId(idToStart, listIDCandiesLeft);
        
        // We will have candy list: 1 2 3 4 5 [ 6 7 8 ] 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 
        // => GetIdCandiesLeft(5) => ChangeId(idToStart) => 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 
        // => GetCandyInPool => 22 23 24 

        handle1 = StartCoroutine(RenderCandiesFormID(idToStart));
        yield return handle1;
        StartCoroutine(RenderCandiesFromPool(idToStart));

        handle2 = StartCoroutine(RestListCandies());
        yield return handle2;

        // CandyList = CandyList.OrderBy(go => go.GetComponent<Candy>().InCell).ToList();
        //
        // for (int i = 0; i < CandyList.Count; i++)
        // {
        //     Debug.Log("incell 1: " + i + " " + CandyList[i].GetComponent<Candy>().InCell);
        // }
        //listCandysValid.Clear(); // element is list id-candy(category candy) example: list{{3 3 3}, {2 2 2 2}} = 7 point 
        //listIDToDestroy.Clear();  // element is id, Candy is valid for scoring, they are removed from list candy  example: {5 6 7 11 12 13 14}}
        // listIDCandiesLeft.Clear();  // listIDCandiesToMove is the list of ids left after eating candy
    }
    
    IEnumerator RestListCandies()
    {
        CandyList = CandyList.OrderBy(go => go.GetComponent<Candy>().InCell).ToList();
        // for (int i = 0; i < CandyList.Count; i++)
        // {
        //     Debug.Log("incell: " + i + " " + CandyList[i].GetComponent<Candy>().InCell);
        // }
        yield return null;
    }
}