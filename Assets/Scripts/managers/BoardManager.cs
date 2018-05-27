using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    //Serializable class that generates random counts for objects in our game board.
    [Serializable]
    public class Count {

        public int minimum;
        public int maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }

    }

    public int rows = 8; //Amount of rows on the board. Default to 8.
    public int cols = 8; //Amount of columns on the board. Default to 8.
    public Count wallCount = new Count(5, 9); //Generate a random number for the wall count
    public Count foodCount = new Count(1, 5); //Generate a random number for the food count

    public GameObject exit; //Game object for the exit tile.There is one exit in every board.

    //Lists to be populated in editor
    public GameObject[] l_floorTiles; //List of floor tiles
    public GameObject[] l_foodTiles; //List of food tiles
    public GameObject[] l_innerWallTiles; //List of inner wall tiles
    public GameObject[] l_outerWallTiles; //List of outer wall tiles
    public GameObject[] l_enemyTiles; //List of enemy tiles

    /*
     * PRIVATE VARS
     */

    private Transform boardHolder; //Transform that holds the entire board. Will allow us to clean up the hierarchy in the editor.
    private List<Vector3> gridPositions = new List<Vector3>(); //List that keeps track of objects' positions on the board.

    //Initialize and populate the grid positions list
    private void Initialize_List()
    {
        gridPositions.Clear(); //Clear the list

        //Populate
        for(int x = 1; x <= cols-1; x++)
        {
            for(int y = 1; y <= rows-1; y++)
            {
                gridPositions.Add(new Vector3(x, y, 0.0f));
            }
        }
    }

    //Setup the actual graphic board.
    private void Board_Setup()
    {
        //Create the board object
        boardHolder = new GameObject("Board").transform;

        //Start at -1 in both the column and rows of the board and go until +1.
        //We do this because there is a frame around the board that the player cannot traverse.
        for(int x = -1; x <= cols+1; x++)
        {
            for(int y = -1; y <= rows+1; y++)
            {
                //Get set to instantiate a floor or wall tile.
                GameObject toInstantiate = null;
                if (x == -1 || x == cols+1 || y == -1 || y == rows+1)
                {
                    //if we are in the outer rim of the board, grab an outer wall tile prefab
                    toInstantiate = l_outerWallTiles[Random.Range(0, l_outerWallTiles.Length)];
                }
                else
                {
                    //if we are in the inner board, grab a floor tile prefab
                    toInstantiate = l_floorTiles[Random.Range(0, l_floorTiles.Length)];
                }

                //Create an instance of the prefab we grabbed
                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0.0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(boardHolder);
            }
        }
    }

    //Secure a random position from the board.
    private Vector3 Random_Position()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);

        Vector3 rand_gridPos = gridPositions[randomIndex]; //Pick a random position
        gridPositions.RemoveAt(randomIndex); //Remove it from the list of available positions

        return rand_gridPos;
    }

    //Spawn a game object on the board at a random position
    private void Layout_Object_At_Random(GameObject[] _tileArray, int minimum, int maximum)
    {
        int objectCount = Random.Range(minimum, maximum + 1); //Find how many of an object to spawn

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = Random_Position();

            GameObject tileChoice = _tileArray[Random.Range(0, _tileArray.Length)];

            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }


    }

    //Will be called by the game manager to set up the board.
    public void Setup_Scene(int level)
    {
        Board_Setup();
        Initialize_List();

        Layout_Object_At_Random(l_innerWallTiles, wallCount.maximum, wallCount.minimum); //Layout all the inner walls
        Layout_Object_At_Random(l_foodTiles, foodCount.minimum, foodCount.minimum); //Layout all the food

        int enemyCount = (int)Mathf.Log(level, 2f); //Set the enemy count as log(level)
        Layout_Object_At_Random(l_enemyTiles, enemyCount, enemyCount); //Place enemies

        //POTENTIAL BUG: Couldn't this algorithm cause the exit to be blocked off by inner walls?
        Instantiate(exit, new Vector3(cols, rows, 0f), Quaternion.identity); //Place the exit at the upper left corner.
        
    }
}
