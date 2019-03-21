using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class World : MonoBehaviour
{
    const int NUM_ROADS_IN_GAME = 15;
    const int PLATFORM_LEN = 10;
    const int NUM_CITY_SCENES = 7;
    const int NUM_PARK_SCENES = 2;
    const int NUM_OF_STRAIGHTS = 7;

    public enum Direction { North = 0, East = 1, South = 2, West = 3 };
    public enum Landscape { City = 0, Park = 1 };
    public GameObject cityRoadS; //straight
    public GameObject cityRoadT; //turn
    public GameObject parkRoadS; //straight
    public GameObject parkRoadT; //turn
    public GameObject[] cityScenes = new GameObject[NUM_CITY_SCENES];
    public GameObject[] parkScenes = new GameObject[NUM_PARK_SCENES];
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI speedText;
    System.Random rnd = new System.Random();
    int roadIndex;
    float endZ;
    float endX;
    int counter;
    bool noLeft;
    bool noRight;
    bool justTurned;
    bool helpOn;
    GameObject[] roads = new GameObject[NUM_ROADS_IN_GAME];
    GameObject[,] scenes = new GameObject[NUM_ROADS_IN_GAME, 2];

    // streetPieceCounter is used to keep track of what index player is at in positions array below
    int streetPieceCounter;
    Vector3[] positions = new Vector3[NUM_ROADS_IN_GAME];
    Direction[] directions = new Direction[NUM_ROADS_IN_GAME];

    Direction playerDir;
    Direction nextDir;
    Landscape land;
    // Start is called before the first frame update
    void Start()
    {
        resetWorld();
    }

    // Update is called once per frame
    void Update()
    {
        speedText.text = "SPEED\r\n" + MoveBike.bikeSpeed.ToString() + " mph";
    }

    public void createScene()
    {
        if (counter % NUM_OF_STRAIGHTS == 6)
        {
            createTurnScene();
            land = (Landscape)~(int)land;
        }
        else
            createStraightScene();
        counter++;
    }

    void createStraightScene()
    {
        GameObject sceneOne;
        GameObject sceneTwo;
        GameObject road;
        if (land == Landscape.City)
        {
            sceneOne = cityScenes[rnd.Next(0, NUM_CITY_SCENES)];
            sceneTwo = cityScenes[rnd.Next(0, NUM_CITY_SCENES)];
            road = cityRoadS;
        }
        else
        {
            sceneOne = parkScenes[rnd.Next(0, NUM_PARK_SCENES)];
            sceneTwo = parkScenes[rnd.Next(0, NUM_PARK_SCENES)];
            road = parkRoadS;
        }

        if (nextDir == Direction.East)
        {
            //Debug.Log("Creating East: " + endX + ", " + endZ);
            roads[roadIndex] = Instantiate(road, new Vector3(endX + 5, 0, endZ), Quaternion.Euler(0, 90, 0));
            // South Scene
            if (!noRight) scenes[roadIndex, 0] = GameObject.Instantiate(sceneOne, new Vector3(endX + 5, 0.5f, endZ - 9.5f), Quaternion.Euler(0, 270, 0));
            // North Scene
            if (!noLeft) scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX + 5, 0.5f, endZ + 9.5f), Quaternion.Euler(0, 90, 0));
            endX = endX + 10;
        }
        else if (nextDir == Direction.West)
        {
            //Debug.Log("Creating West: " + endX + ", " + endZ);
            roads[roadIndex] = Instantiate(road, new Vector3(endX - 5, 0, endZ), Quaternion.Euler(0, 90, 0));
            // North Scene
            if (!noRight) scenes[roadIndex, 0] = GameObject.Instantiate(sceneTwo, new Vector3(endX - 5, 0.5f, endZ + 9.5f), Quaternion.Euler(0, 90, 0));
            // South Scene
            if (!noLeft) scenes[roadIndex, 1] = GameObject.Instantiate(sceneOne, new Vector3(endX - 5, 0.5f, endZ - 9.5f), Quaternion.Euler(0, 270, 0));
            endX = endX - 10;
        }
        else if (nextDir == Direction.South)
        {
            //Debug.Log("Creating South: " + endX + ", " + endZ);
            roads[roadIndex] = Instantiate(road, new Vector3(endX, 0, endZ - 5), Quaternion.Euler(0, 180, 0));
            // West Scene
            if (!noRight) scenes[roadIndex, 0] = GameObject.Instantiate(sceneOne, new Vector3(endX - 9.5f, 0.5f, endZ - 5), Quaternion.Euler(0, 0, 0));
            // East Scene
            if (!noLeft) scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX + 9.5f, 0.5f, endZ - 5), Quaternion.Euler(0, 180, 0));
            endZ = endZ - 10;
        }
        else if (nextDir == Direction.North)
        {
            //Debug.Log("Creating North: " + endX + ", " + endZ);
            roads[roadIndex] = Instantiate(road, new Vector3(endX, 0, endZ + 5), Quaternion.Euler(0, 180, 0));
            // West Scene
            if (!noLeft) scenes[roadIndex, 0] = GameObject.Instantiate(sceneOne, new Vector3(endX - 9.5f, 0.5f, endZ + 5), Quaternion.Euler(0, 180, 0));
            // East Scene
            if (!noRight) scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX + 9.5f, 0.5f, endZ + 5), Quaternion.Euler(0, 180, 0));
            endZ = endZ + 10;
        }
        Vector3 pos;
        pos.x = endX;
        pos.z = endZ;
        pos.y = 0;
        positions[roadIndex] = pos;
        directions[roadIndex] = nextDir;
        roadIndex = (roadIndex + 1) % NUM_ROADS_IN_GAME;

        noLeft = false;
        noRight = false;
    }

    void createTurnScene()
    {
        int turnNum = rnd.Next(0, 2); // (0: turn left)(1: turn right)
        GameObject sceneOne;
        GameObject sceneTwo;
        GameObject turn;
        if (land == Landscape.City)
        {
            sceneOne = cityScenes[rnd.Next(0, NUM_CITY_SCENES)];
            sceneTwo = cityScenes[rnd.Next(0, NUM_CITY_SCENES)];
            turn = parkRoadT;
        }
        else
        {
            sceneOne = parkScenes[rnd.Next(0, NUM_PARK_SCENES)];
            sceneTwo = parkScenes[rnd.Next(0, NUM_PARK_SCENES)];
            turn = parkRoadT;
        }

        if (turnNum == 0) // turn left
        {
            noLeft = true;
            noRight = false;
        }
        else if (turnNum == 1) // turn right
        {
            noLeft = false;
            noRight = true;
        }
        if (nextDir == Direction.North)
        {
            endZ = endZ + 4.5f;
            scenes[roadIndex, 0] = GameObject.Instantiate(sceneOne, new Vector3(endX, 0.5f, endZ + 9.5f), Quaternion.Euler(0, 90, 0));
            if (turnNum == 0) // West
            {
                roads[roadIndex] = Instantiate(turn, new Vector3(endX, 0, endZ), Quaternion.Euler(0, 270, 0));
                scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX + 9.5f, 0.5f, endZ), Quaternion.Euler(0, 180, 0));
                endX = endX - 4.5f;
                nextDir = Direction.West;
            }
            else if (turnNum == 1) // East
            {
                roads[roadIndex] = Instantiate(turn, new Vector3(endX, 0, endZ), Quaternion.Euler(0, 180, 0));
                scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX - 9.5f, 0.5f, endZ), Quaternion.Euler(0, 0, 0));
                endX = endX + 4.5f;
                nextDir = Direction.East;
            }
        }
        else if (nextDir == Direction.South)
        {
            endZ = endZ - 4.5f;
            scenes[roadIndex, 0] = GameObject.Instantiate(sceneOne, new Vector3(endX, 0.5f, endZ - 9.5f), Quaternion.Euler(0, 270, 0));
            if (turnNum == 0) // East
            {
                roads[roadIndex] = Instantiate(turn, new Vector3(endX, 0, endZ), Quaternion.Euler(0, 90, 0));
                scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX - 9.5f, 0.5f, endZ), Quaternion.Euler(0, 0, 0));
                endX = endX + 4.5f;
                nextDir = Direction.East;
            }
            else if (turnNum == 1) // West
            {
                roads[roadIndex] = Instantiate(turn, new Vector3(endX, 0, endZ), Quaternion.Euler(0, 0, 0));
                scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX + 9.5f, 0.5f, endZ), Quaternion.Euler(0, 180, 0));
                endX = endX - 4.5f;
                nextDir = Direction.West;
            }
        }
        else if (nextDir == Direction.West)
        {
            endX = endX - 4.5f;
            scenes[roadIndex, 0] = GameObject.Instantiate(sceneOne, new Vector3(endX - 9.5f, 0.5f, endZ), Quaternion.Euler(0, 90, 0));
            if (turnNum == 1) // North
            {
                roads[roadIndex] = Instantiate(turn, new Vector3(endX, 0, endZ), Quaternion.Euler(0, 90, 0));
                scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX, 0.5f, endZ - 9.5f), Quaternion.Euler(0, 90, 0));
                endZ = endZ + 4.5f;
                nextDir = Direction.North;
            }
            else if (turnNum == 0) // South
            {
                roads[roadIndex] = Instantiate(turn, new Vector3(endX, 0, endZ), Quaternion.Euler(0, 180, 0));
                scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX, 0.5f, endZ + 9.5f), Quaternion.Euler(0, 270, 0));
                endZ = endZ - 4.5f;
                nextDir = Direction.South;
            }

        }
        else if (nextDir == Direction.East)
        {
            endX = endX + 4.5f;
            scenes[roadIndex, 0] = GameObject.Instantiate(sceneOne, new Vector3(endX + 9.5f, 0.5f, endZ), Quaternion.Euler(0, 180, 0));
            if (turnNum == 0) // North
            {
                roads[roadIndex] = Instantiate(turn, new Vector3(endX, 0, endZ), Quaternion.Euler(0, 0, 0));
                scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX, 0.5f, endZ - 9.5f), Quaternion.Euler(0, 270, 0));
                endZ = endZ + 4.5f;
                nextDir = Direction.North;
            }
            else if (turnNum == 1) // South
            {
                roads[roadIndex] = Instantiate(turn, new Vector3(endX, 0, endZ), Quaternion.Euler(0, 270, 0));
                scenes[roadIndex, 1] = GameObject.Instantiate(sceneTwo, new Vector3(endX, 0.5f, endZ + 9.5f), Quaternion.Euler(0, 90, 0));
                endZ = endZ - 4.5f;
                nextDir = Direction.South;
            }
        }
        Vector3 pos;
        pos.x = endX;
        pos.z = endZ;
        pos.y = 0;
        positions[roadIndex] = pos;
        directions[roadIndex] = nextDir;
        roadIndex = (roadIndex + 1) % NUM_ROADS_IN_GAME;

    }

    public bool destroyScene(float posX, float posZ)
    {
        float x = roads[(roadIndex + 1) % NUM_ROADS_IN_GAME].transform.position.x;
        float z = roads[(roadIndex + 1) % NUM_ROADS_IN_GAME].transform.position.z;
        if (posX > x + 20 || posX < x - 20 || posZ > z + 20 || posZ < z - 20)
        {
            Destroy(roads[(roadIndex + 1) % NUM_ROADS_IN_GAME]);
            Destroy(scenes[(roadIndex + 1) % NUM_ROADS_IN_GAME, 0]);
            Destroy(scenes[(roadIndex + 1) % NUM_ROADS_IN_GAME, 1]);
            return true;
        }
        return false;
    }

    public bool inBounds()
    {
        Vector3 curPosition = MoveBike.bikePosition;
        Vector3 endPosition = positions[streetPieceCounter % NUM_ROADS_IN_GAME];
        Vector3 nextEndPosition = positions[(streetPieceCounter + 1) % NUM_ROADS_IN_GAME];

        // North is z+, South is z-, East is x+, West is x-
        Direction nextDir = directions[streetPieceCounter % NUM_ROADS_IN_GAME];

        // Debug.Log("nextDir: " + nextDir);
        // Debug.Log("currentDir: " + playerDir);
        // Debug.Log("curPosition: " + curPosition);
        // Debug.Log("endPosition: " + endPosition);
        // Debug.Log("nextEndPosition: " + nextEndPosition);
        // Debug.Log("ROAD INDEX: " +roadIndex);
        // Debug.Log("streetPieceCounter " + streetPieceCounter);
        if (helpOn)
        {
            Direction nextNextDir = directions[(streetPieceCounter + 1) % NUM_ROADS_IN_GAME];
            if (nextNextDir != playerDir)
            { // if next next piece is a turn
                if (playerDir == Direction.North)
                {
                    if ((curPosition.z - (nextEndPosition.z + 2.5)) > -18)
                    {
                        TimerScript.startTimer(2f);
                    }
                }
                else if (playerDir == Direction.South)
                {
                    if ((curPosition.z - (nextEndPosition.z - 2.5)) < 18)
                    {
                        TimerScript.startTimer(2f);
                    }
                }
                else if (playerDir == Direction.East)
                {
                    if ((curPosition.x - (nextEndPosition.x + 2.5)) > -18)
                    {
                        TimerScript.startTimer(2f);
                    }
                }
                else if (playerDir == Direction.West)
                {
                    if ((curPosition.x - (endPosition.x - 2.5)) < 18)
                    {
                        TimerScript.startTimer(2f);
                    }
                }
            }
        }


        if (nextDir != playerDir) // if the next piece is a turn
        {
            if (playerDir == Direction.North)
            {
                if ((curPosition.z - (endPosition.z + 2.5)) > 0)
                { // past the point
                    return false;
                }
            }
            else if (playerDir == Direction.South)
            {
                if ((curPosition.z - (endPosition.z - 2.5)) < 0)
                {
                    return false;
                }
            }
            else if (playerDir == Direction.East)
            {
                if ((curPosition.x - (endPosition.x + 2.5)) > 0)
                {
                    return false;
                }

            }
            else if (playerDir == Direction.West)
            {
                if ((curPosition.x - (endPosition.x - 2.5)) < 0)
                {
                    return false;
                }
            }
        }
        else
        // if we are going straight, need to be able to increment
        // streetPieceCounter once we pass a piece
        {
            if (playerDir == Direction.North)
            {
                if ((curPosition.z - endPosition.z) > 0) // past the point
                {
                    streetPieceCounter++;
                }

            }
            else if (playerDir == Direction.South)
            {
                if ((curPosition.z - endPosition.z) < 0)
                {
                    streetPieceCounter++;
                }
            }
            else if (playerDir == Direction.East)
            {
                if ((curPosition.x - endPosition.x) > 0)
                {
                    streetPieceCounter++;
                }
            }
            else if (playerDir == Direction.West)
            {
                if ((curPosition.x - endPosition.x) < 0)
                {
                    streetPieceCounter++;
                }
            }
        }

        // check side boundaries
        // pieces are 5 units wide


        return true;
    }

    public void resetWorld()
    {
        StartCoroutine(Pause(3));
        for (int i = 0; i < NUM_ROADS_IN_GAME; i++)
        {
            Destroy(roads[i]);
            Destroy(scenes[i, 0]);
            Destroy(scenes[i, 1]);
        }
        land = Landscape.City;
        roadIndex = 0;
        endZ = -5;
        endX = 0;
        counter = 0;
        noLeft = false;
        noRight = false;
        justTurned = false;
        helpOn = true;
        positions = new Vector3[NUM_ROADS_IN_GAME];
        directions = new World.Direction[NUM_ROADS_IN_GAME];
        for (int i = 0; i < NUM_ROADS_IN_GAME; i++)
        {
            createScene();
        }
    }
    // pause for p seconds
    IEnumerator Pause(int p)
    {
        Time.timeScale = 0.000001f;
        yield return new WaitForSeconds(p * Time.timeScale);
        Time.timeScale = 1;
    }
}
