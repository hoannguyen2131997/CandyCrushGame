using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    [SerializeField] private GameObject PrefabObjCell;
    public static List<GameObject> Cells;
    
    public int CellWidth = 1;
    public int CellHeight = 1;
    public int size;
    private int startPosX = (int)startPos.x;
    private int startPosY = (int)startPos.y;
    public static int BoardWidth;
    public static int BoardHeight;
    public Vector3 CheckCell;
    
    public Transform parentBoardTranForm;
    public Transform parentCandyTranForm;
    public Transform ParentCandyPoolTranSform;
    
    private static Vector2 startPos = new Vector2(0, 4);

    // Order is list to contain position (x,y -> List<int>) and id(int) of them
    public static Dictionary<int, int[]> Order = new();
    public static Dictionary<int, List<List<int>>> ListRow = new();
    public static Dictionary<int, List<List<int>>> ListColumn = new();
    private void Awake()
    {
        try
        {
            BoardWidth = size;
            BoardHeight = size;
            startPosY = size - 1;
            GenerateListOfCells(startPosX, startPosY, size);
        }
        catch (Exception e)
        {
            Debug.Log($"Error: {e}");
        }

        if (CheckCell.x >= BoardWidth || CheckCell.x < 0 || CheckCell.y >= BoardHeight || CheckCell.y < 0)
        {
            CheckCell.x = 0;
            CheckCell.y = 0;
            Debug.Log("This cell is being without game board");
        }
    }

    private void Start()
    {
        GenerateBroad(BoardWidth, BoardHeight);
        HighLightCell((int)CheckCell.x, (int)CheckCell.y);
        // GetCandyListToScore();
    }

    public static (int, int) getXYFromID(int id, int width)
    {
        return (id % width, (int)Mathf.Floor(id / width));  // Mathf.Floor returns the largest integer smaller than or equal to id / width
    }

    public static int getIdFromXY(int x, int y, int width)
    {
        return y * width + x;
    }
    
    private void GenerateBroad(int width, int height) // Create board with size 5x5
    {
        // Calc total cell and create list cells
        var totalCell = width * height;
        Cells = new List<GameObject>(totalCell);

        // Loop to create total cell
        for (var i = 0; i < totalCell; i++)
        {
            // Create game object with a square shape 
            var cellObj = Instantiate(PrefabObjCell);
            cellObj.transform.parent = parentBoardTranForm;
            var (x, y) = getXYFromID(i, BoardWidth);
            
            cellObj.name = "Cell " + i + $"[{x}.{y}]";
            cellObj.transform.position = new Vector3(x * CellWidth, y * CellHeight, 0);
            cellObj.tag = "cell";
            cellObj.AddComponent<BoxCollider>();
            
            var m_Collider = cellObj.GetComponent<BoxCollider>();
            m_Collider.size = new Vector3(1, 1, 1);
            Cells.Add(cellObj);
        }
    }

    public static void HighLightCell(int cellX, int cellY)
    {
        var id = getIdFromXY(cellX, cellY, BoardWidth);
        Cells[id].GetComponent<SpriteRenderer>().color = Color.yellow;
    }
    
    public static void UnHighLightCell(int cellX, int cellY)
    {
        var id = getIdFromXY(cellX, cellY, BoardWidth);
        Cells[id].GetComponent<SpriteRenderer>().color = Color.white;
    }
    
    public static void GenerateListOfCells(int x, int y, int m_size)
    {
        var size = m_size;
        var count = 0;
        while (size > 0)
        {
            for (var i = y; i >= y - size + 1; i--)
            {
                var item = new int[2];
                item[0] = x;
                item[1] = i;
                Order.Add(count, item);
                count++;
            }
            for (var j = x + 1; j <= x + size - 1; j++)
            {
                var item = new int[2];
                item[0] = j;
                item[1] = y - size + 1;
                Order.Add(count, item);
                count++;
            }
            for (var i = y - size + 2; i <= y; i++)
            {
                var item = new int[2];
                item[0] = x + size - 1;
                item[1] = i;
                Order.Add(count, item);
                count++;
            }
            for (var i = x + size - 2; i >= x + 1; i--)
            {
                var item = new int[2];
                item[0] = i;
                item[1] = y;
                Order.Add(count, item);
                count++;
            }
            x++;
            y--;
            size -= 2;
        }
    }

    // Add each row and column to calculate score
    public static void GetCandyListToScore()
    {
        ListRow.Clear();
        ListColumn.Clear();
        ListRow = new();
        ListColumn = new();
        int count = 0;
        
        while (count < 5)
        {
            List<List<int>> tempRow = new List<List<int>>();
            List<List<int>> tempColumn = new List<List<int>>();
            
            for (int i = 0; i < Cells.Count; i++)
            {
                int y = (int)Cells[i].transform.position.y;
                int x = (int)Cells[i].transform.position.x;

                if (y == count)
                {
                    List<int> temp = new List<int>();
                    temp.Add(x);
                    temp.Add(y);
                    tempRow.Add(temp);
                }

                if (x == count)
                {
                    List<int> temp = new List<int>();
                    temp.Add(x);
                    temp.Add(y);
                    tempColumn.Add(temp);
                }
            }
            ListRow.Add(count, tempRow);
            ListColumn.Add(count, tempColumn);
            count++;
        }
    }

    // public List<int[]> CandyRow = new List<int[]>();
    // public List<int[]> CandyColumn = new List<int[]>();
    //
    // public void GetCandyListToScoreWithID()
    // {
    //     int count = 0;
    //     
    //     while (count < 5)
    //     {
    //         int[] tempRow = new int[BoardWidth];
    //         int[] tempColumn = new int[BoardWidth];
    //         
    //         for (int i = 0; i < Cells.Count; i++)
    //         {
    //             int y = (int)Cells[i].transform.position.y;
    //             int x = (int)Cells[i].transform.position.x;
    //
    //             if (y == count)
    //             {
    //                 tempRow[0] = x;
    //                 tempRow[1] = y;
    //             }
    //
    //             if (x == count)
    //             {
    //                 tempColumn[0] = x;
    //                 tempColumn[1] = y;
    //             }
    //         }
    //         
    //         CandyRow.Add(tempRow);
    //         CandyColumn.Add(tempColumn);
    //         count++;
    //     }
    // }
}