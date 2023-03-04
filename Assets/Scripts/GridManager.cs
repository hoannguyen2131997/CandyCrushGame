//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class GridManager : MonoBehaviour
//{
//    // is where we are going to place the images that can appear in the grid
//    public List<Sprite> Sprites = new List<Sprite>();

//    // is a reference to a prefab a component needed to display the sprites on the screen
//    public GameObject TilePrefab;

//    //  store the size of the grid (5x5)
//    public int GridDimension = 5;

//    // store the distance of the cells when we will draw them
//    public float Distance = 1.0f;

//    // the grid will use the Row and Column indexes which will represent the rows and columns of map, the index 0 will be at the bottom left
//    private GameObject[,] Grid;

//    void InitGrid()
//    {
//        Vector3 positionOffset = transform.position - new Vector3(GridDimension * Distance / 2.0f, GridDimension * Distance / 2.0f, 0); //  tinh toan do lech, no se duoc cong cho moi cell sau do

//        for (int row = 0; row < GridDimension; row++)
//            for (int column = 0; column < GridDimension; column++) 
//            {


//                GameObject newTile = Instantiate(TilePrefab); // khoi tao mot cell
//                SpriteRenderer renderer = newTile.GetComponent<SpriteRenderer>(); // tham chieu toi SpriteRenderer (hien thi hinh anh (duoi dang sprite))

//                renderer.sprite = Sprites[Random.Range(0, Sprites.Count)]; // chi dinh cac sprites bang cach chon random 0, 1, ... Sprites.Count - 1
//                newTile.transform.parent = transform; // chuyen cac sprite thanh con cua grid (neu grid di chuyen thi cac sprite cung se di chuyen theo)
//                newTile.transform.position = new Vector3(column * Distance, row * Distance, 0) + positionOffset; // chi dinh vi tri cua o va cong them do du dda duoc tinh truoc do

//                Grid[column, row] = newTile; // luu lai
//                List<Sprite> possibleSprites = new List<Sprite>();

//                Sprite left1 = GetSpriteAt(column - 1, row);
//                Sprite left2 = GetSpriteAt(column - 2, row);
//                if (left2 != null && left1 == left2)
//                {
//                    possibleSprites.Remove(left1);
//                }

//                Sprite down1 = GetSpriteAt(column, row - 1); // 5
//                Sprite down2 = GetSpriteAt(column, row - 2);
//                if (down2 != null && down1 == down2)
//                {
//                    possibleSprites.Remove(down1);
//                }
//            }
//    }

//    //thuat toan xu ly 3 o lien tiep giong nhau, tao Grid se khong chap nhan 3 o thang hang
//   Sprite GetSpriteAt(int column, int row)
//    {
//        if (column < 0 || column >= GridDimension
//            || row < 0 || row >= GridDimension)
//            return null;
//        GameObject tile = Grid[column, row];
//        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
//        return renderer.sprite;
//    }
//    // Start is called before the first frame update
//    void Start()
//    {
//        Grid = new GameObject[GridDimension, GridDimension];
//        InitGrid();
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}

