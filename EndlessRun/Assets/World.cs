using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class World : MonoBehaviour
{
    public MoveBike moveBike;
    const int NUM_ROADS_IN_GAME = 15;
    const int NUM_PEDS_IN_GAME = 5;
    const int PLATFORM_LEN = 10;
    const int NUM_CITY_SCENES = 7;
    const int NUM_PARK_SCENES = 2;
    const int NUM_OF_STRAIGHTS = 7;

    public enum Direction { North = 0, East = 1, South = 2, West = 3 };
    public enum Landscape { City = 0, Park = 1 };
    public KeyCode keyQ;
    public GameObject cityRoadS; //straight
    public GameObject cityRoadT; //turn
    public GameObject parkRoadS; //straight
    public GameObject parkRoadT; //turn
    public GameObject[] cityScenes = new GameObject[NUM_CITY_SCENES];
    public GameObject[] parkScenes = new GameObject[NUM_PARK_SCENES];
    public GameObject pedestrian;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI errorText;
    
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
    GameObject[] peds = new GameObject[NUM_PEDS_IN_GAME];
    int pedCreateIndex = 0;
    int nextPed = 0;
    int speed = 0;
    public float timeCheck = 0;

    float distanceTravelled = 0;
    Vector3 lastPosition;
    public int score = 0;
    int sinceTurn = 0;
    // streetPieceCounter is used to keep track of what index player is at in positions array below
    int streetPieceCounter;
    public Vector3[] positions = new Vector3[NUM_ROADS_IN_GAME];
    Direction[] directions = new Direction[NUM_ROADS_IN_GAME];
    bool city = true;
    // Start is called before the first frame update
    void Start()
    {
        //resetWorld();
        lastPosition = moveBike.bikePosition;
    }

    // Update is called once per frame
    void Update()
    {
        speed = (int) (moveBike.bikeSpeed * moveBike.velocityScale);
        speedText.text = "SPEED\r\n" + speed.ToString() + " mph";

        distanceTravelled += Vector3.Distance(moveBike.bikePosition, lastPosition);
        lastPosition = moveBike.bikePosition;
        score = (int)distanceTravelled;

        scoreText.text = "SCORE\r\n" + score.ToString();

        timeCheck += Time.deltaTime;
        if(timeCheck > 4){    
            infoText.text = "";
        }
        checkPedestrian();
    }  
    
    public void levelUp(){
        infoText.text = "Congratulations, your max speed has increased!";
        timeCheck = 0;
    }

    public Direction createScene(Direction nextDir)
    {
        Direction newDir;
        if (counter % NUM_OF_STRAIGHTS == 6)
        {
            newDir = createTurnScene(nextDir);
            city = !city;
            Debug.Log("Turned towards " + (int)newDir);
            sinceTurn = 0;
        }
        else
        {
            // create a straight road. add pedestrian only if it is on park and counter modulus 3 is 0
            if(!city && sinceTurn == 2)
                createStraightScene(nextDir, true);
            else
                createStraightScene(nextDir, false);
            newDir = nextDir;
            Debug.Log("Straight towards " + (int)newDir + " Land: " + city + " Counter: " + counter);
            sinceTurn++;
        }
        counter++;
        return newDir;
    }

    void createStraightScene(Direction nextDir, bool createPedestrian)
    {
        GameObject sceneOne;
        GameObject sceneTwo;
        GameObject road;
        if (createPedestrian) Debug.Log("Creating pedestrian...");
        if (city)
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
            if (createPedestrian) addPedestrian(endX + 5, endZ, Direction.East);
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
            if (createPedestrian) addPedestrian(endX - 5, endZ, Direction.West);
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
            if (createPedestrian) addPedestrian(endX, endZ - 5, Direction.South);
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
            if (createPedestrian) addPedestrian(endX, endZ + 5, Direction.North);
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

    Direction createTurnScene(Direction nextDir)
    {
        int turnNum = rnd.Next(0, 2); // (0: turn left)(1: turn right)
        GameObject sceneOne;
        GameObject sceneTwo;
        GameObject turn;
        if (city)
        {
            sceneOne = cityScenes[rnd.Next(0, NUM_CITY_SCENES)];
            sceneTwo = cityScenes[rnd.Next(0, NUM_CITY_SCENES)];
            turn = cityRoadT;
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
        return nextDir;
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

    public Vector3 getEndPosition()
    {
        return positions[streetPieceCounter % NUM_ROADS_IN_GAME];
    }

    private void addPedestrian(float xMid, float zMid, Direction d)
    {
        //Rigidbody p = ped.GetComponent<Rigidbody>();
        //p.position = new Vector3(0, 1, 0);
        Destroy(peds[(pedCreateIndex + 1) % NUM_PEDS_IN_GAME]);
        int laneShift = rnd.Next(0, 3) - 1;
        
        if (d == Direction.West || d == Direction.East)
        {
            peds[pedCreateIndex] = GameObject.Instantiate(pedestrian, new Vector3(xMid, 1, zMid + laneShift * 1.66f), Quaternion.Euler(0, (int)d * 90, 0));
        }
        else
        {
            peds[pedCreateIndex] = GameObject.Instantiate(pedestrian, new Vector3(xMid + laneShift * 1.66f, 1, zMid), Quaternion.Euler(0, (int)d * 90, 0));
        }
        peds[pedCreateIndex].GetComponent<Pedestrian>().setDirection(d);
        pedCreateIndex = (pedCreateIndex + 1) % NUM_PEDS_IN_GAME;
    }

    private void checkPedestrian()
    {
        if (peds[nextPed] != null)
        {
            int passed = peds[nextPed].GetComponent<Pedestrian>().bikeExited;
            if (passed == -1)
            {
                Debug.Log("Passed Ped #" + nextPed + " on the left");
                nextPed = (nextPed + 1) % NUM_PEDS_IN_GAME;
                if(TimerScript.passingAllowed != "on your left"){
                    moveBike.endGame(6);
                }
                TimerScript.stopTimer();
            }
            else if (passed == 1)
            {
                Debug.Log("Passed Ped #" + nextPed + " on the right");
                nextPed = (nextPed + 1) % NUM_PEDS_IN_GAME;
                if(TimerScript.passingAllowed != "on your right"){
                    moveBike.endGame(7);
                }
                TimerScript.stopTimer();
            }
        }
    }

    public bool inBounds(Direction playerDir)
    {
        Vector3 curPosition = moveBike.bikePosition;
        Vector3 endPosition = positions[streetPieceCounter % NUM_ROADS_IN_GAME];
        Vector3 nextEndPosition = positions[(streetPieceCounter + 1) % NUM_ROADS_IN_GAME];

        // North is z+, South is z-, East is x+, West is x-
        Direction nextDirection = directions[streetPieceCounter % NUM_ROADS_IN_GAME];

        if (nextDirection != playerDir) // if the next piece is a turn
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

    public Direction resetWorld()
    {
        for (int i = 0; i < NUM_ROADS_IN_GAME; i++)
        {
            Destroy(roads[i]);
            Destroy(scenes[i, 0]);
            Destroy(scenes[i, 1]);
        }
        city = true;
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
        Direction dir = Direction.North;
        infoText.text = "";
        for (int i = 0; i < NUM_ROADS_IN_GAME; i++)
        {
            dir = createScene(dir);
        }
        return dir;
    }
    // pause for p seconds
    public void pause()
    {
        Time.timeScale = 0.01f;
    }
}
