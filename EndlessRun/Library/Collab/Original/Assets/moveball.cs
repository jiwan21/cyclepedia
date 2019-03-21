using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class moveball : MonoBehaviour
{
    public Image staticPlayer;
    public Image rightPlayer;
    public Image leftPlayer;

    const int NUM_ROADS_IN_GAME = 15;
    const int PLATFORM_LEN = 10;
    const int NUM_CITY_SCENES = 7;
    const int NUM_PARK_SCENES = 2;
    const int NUM_OF_STRAIGHTS = 7;

    // for horizontal movement (left / right lanes)
    public float horizVel = 0;
    public int laneNum = 2; 
    public string controlLocked = "n"; 

    public KeyCode keyW;
    public KeyCode keyS;
    public KeyCode keyA;
    public KeyCode keyD;
    public KeyCode arrowL;
    public KeyCode arrowR;

    //for testing arm poses
    public KeyCode keyE;
    public KeyCode keyR;
    public KeyCode keyT;

    public GameObject cityRoadS; //straight
    public GameObject cityRoadT; //turn
    public GameObject parkRoadS; //straight
    public GameObject parkRoadT; //turn
    public GameObject[] cityScenes = new GameObject[NUM_CITY_SCENES];
    public GameObject[] parkScenes = new GameObject[NUM_PARK_SCENES];
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI infoText;

    public static Vector3 bikePosition;

    static UdpClient udp;
    Thread thread;

    bool keyBoardMode = true;

    System.Random rnd = new System.Random();
    int roadIndex;
    int bikeSpeed; // units: miles per hour
    char pedalSpeed; // units: rotations per hour
    float zVel; // unity grid units
    float xVel; // unity grid units
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
    // hi
    int streetPieceCounter;
    Vector3[] positions = new Vector3[NUM_ROADS_IN_GAME];
    Direction[] directions = new Direction[NUM_ROADS_IN_GAME];

    enum Direction { North = 0, East = 1, South = 2, West = 3 };
    enum Landscape { City = 0, Park = 1 };

    Direction playerDir;
    Direction nextDir;
    Landscape land;
    Rigidbody mBody;

    // Use this for initialization
    void Start()
    {
        playerDir = Direction.North;
        nextDir = Direction.North;
        land = Landscape.City;
        mBody = GetComponent<Rigidbody>();
        roadIndex = 0;
        bikeSpeed = 0;
        zVel = 0;
        xVel = 0;
        endZ = -5;
        endX = 0;
        counter = 0;
        noLeft = false;
        noRight = false;
        justTurned = false;
        helpOn =  true;
        for (int i = 0; i < NUM_ROADS_IN_GAME; i++)
        {
            createScene();
        }
        resetPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        mBody.velocity = new Vector3(xVel, 0, zVel);
        bikePosition = mBody.position;

        updateVelocity();
        if (playerDir == Direction.North){
            xVel = horizVel;
        }else if (playerDir == Direction.South){
            xVel = -horizVel;
        }else if (playerDir == Direction.East){
            zVel = -horizVel;
        }else if (playerDir == Direction.West){
            zVel = horizVel;
        }

        if (keyBoardMode == true){
            if (Input.GetKeyDown(arrowL) && (controlLocked == "n") && (laneNum > 1)){
                horizVel = -5;
                StartCoroutine (stopSlide());
                laneNum -= 1;
                controlLocked = "y";
                resetPlayer();
            }
            if (Input.GetKeyDown(arrowR) && (controlLocked == "n") && (laneNum < 3)){
                horizVel = 5;
                StartCoroutine (stopSlide());
                laneNum += 1;
                controlLocked = "y";
                resetPlayer();
            }

            // for testing arm poses
            // E is for left and T is for right
            if (Input.GetKeyDown(keyT)){
                rightPlayer.enabled = true;
                staticPlayer.enabled = false;
                leftPlayer.enabled = false;
            }else if (Input.GetKeyDown(keyR)){
                rightPlayer.enabled = false;
                staticPlayer.enabled = true;
                leftPlayer.enabled = false;
            }else if (Input.GetKeyDown(keyE)){
                rightPlayer.enabled = false;
                staticPlayer.enabled = false;
                leftPlayer.enabled = true;
            }
        }

        if (Input.GetKeyDown(keyW))
        {
            bikeSpeed = bikeSpeed + 2;
            updateVelocity();
        }
        if (Input.GetKeyDown(keyS))
        {
            bikeSpeed = bikeSpeed - 2;
            updateVelocity();
        }
        if (Input.GetKeyDown(keyD))
        {
            Rotate(90, bikePosition.x, bikePosition.z);
            StartCoroutine(RotateCam(Vector3.up, 90, 0.3f));
            if (helpOn){
                TimerScript.stopTimer();
            }
            laneNum = 2;
            resetPlayer();
            
        }
        if (Input.GetKeyDown(keyA))
        {
            Rotate(-90, bikePosition.x, bikePosition.z);
            StartCoroutine(RotateCam(Vector3.up, -90, 0.3f));
            if (helpOn){
                TimerScript.stopTimer();
            }
            laneNum = 2;
            resetPlayer();
            
        }

        // each time you go past the next endpoint, increment streetPieceCounter
        if (!inBounds()) // if not within boundaries
        {
            Debug.Log("RAN OUT OF THE STREET, THATS NO BUENO");
            infoText.text = "OOPS, bikes can't go there!";
            speedText.text ="";
            //Application.Quit();
            //Time.timeScale = 0;
            //Debug.Break();
            resetGame();
        }

        if (destroyRoad(bikePosition.x, bikePosition.z))
        {
            createScene();
        }
    }

    IEnumerator stopSlide()
    {
        yield return new WaitForSeconds (0.3f);
        horizVel = 0;
        xVel = 0;
        controlLocked = "n";
    }

    void resetPlayer(){
        rightPlayer.enabled = false;
        staticPlayer.enabled = true;
        leftPlayer.enabled = false;
    }

    void createScene()
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
        if(land == Landscape.City)
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
        roadIndex = (roadIndex+1) % NUM_ROADS_IN_GAME;
        
        noLeft = false;
        noRight = false;
    }

    bool inBounds(){
        Vector3 curPosition = mBody.position;

        Vector3 endPosition = positions[streetPieceCounter % NUM_ROADS_IN_GAME];
        Vector3 nextEndPosition = positions[(streetPieceCounter+1) % NUM_ROADS_IN_GAME];


        // North is z+, South is z-, East is x+, West is x-
        Direction nextDir = directions[streetPieceCounter%NUM_ROADS_IN_GAME];

        // Debug.Log("nextDir: " + nextDir);
        // Debug.Log("currentDir: " + playerDir);
        // Debug.Log("curPosition: " + curPosition);
        // Debug.Log("endPosition: " + endPosition);
        // Debug.Log("nextEndPosition: " + nextEndPosition);
        // Debug.Log("ROAD INDEX: " +roadIndex);
        // Debug.Log("streetPieceCounter " + streetPieceCounter);
        if (helpOn){
            Direction nextNextDir = directions[(streetPieceCounter+1)%NUM_ROADS_IN_GAME];
            if (nextNextDir != playerDir){ // if next next piece is a turn
                if (playerDir == Direction.North)
                {
                    if ((curPosition.z - (nextEndPosition.z+2.5)) > -18){
                        TimerScript.startTimer(2f);
                    }
                }else if (playerDir == Direction.South){
                    if ((curPosition.z - (nextEndPosition.z-2.5)) < 18){ 
                        TimerScript.startTimer(2f);
                    }
                }else if (playerDir == Direction.East){
                    if ((curPosition.x - (nextEndPosition.x+2.5)) > -18){ 
                        TimerScript.startTimer(2f);
                    }
                }else if (playerDir == Direction.West){
                    if ((curPosition.x - (endPosition.x-2.5)) < 18){ 
                        TimerScript.startTimer(2f);
                    }
                }
            }
        }
        

        if(nextDir != playerDir) // if the next piece is a turn
        {
            if (playerDir == Direction.North)
            {
                if ((curPosition.z - (endPosition.z+2.5)) > 0){ // past the point
                    return false;
                }
            }else if(playerDir == Direction.South){
                if ((curPosition.z - (endPosition.z-2.5)) < 0)
                {
                    return false;
                }
            }else if(playerDir == Direction.East)
            {
                if ((curPosition.x - (endPosition.x+2.5)) > 0)
                {
                    return false;
                }
                
            }else if(playerDir == Direction.West)
            {
                if ((curPosition.x - (endPosition.x-2.5)) < 0)
                {
                    return false;
                }
            }
        }else
        // if we are going straight, need to be able to increment
        // streetPieceCounter once we pass a piece
        {
            if (playerDir == Direction.North)
            {
                if ((curPosition.z - endPosition.z) > 0) // past the point
                {
                    streetPieceCounter++;
                }

            }else if(playerDir == Direction.South)
            {
                if ((curPosition.z - endPosition.z) < 0)
                {
                    streetPieceCounter++;
                }
            }else if(playerDir == Direction.East)
            {
                if ((curPosition.x - endPosition.x) > 0)
                {
                    streetPieceCounter++;
                }
            }else if(playerDir == Direction.West)
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

    bool destroyRoad(float posX, float posZ)
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

    void createTurnScene()
    {
        int turnNum = rnd.Next(0, 2); // (0: turn left)(1: turn right)
        GameObject sceneOne;
        GameObject sceneTwo;
        GameObject turn;
        if(land == Landscape.City)
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
            scenes[roadIndex,0] = GameObject.Instantiate(sceneOne, new Vector3(endX, 0.5f, endZ + 9.5f), Quaternion.Euler(0, 90, 0));
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
        roadIndex = (roadIndex+1) % NUM_ROADS_IN_GAME;

    }

    void Rotate(int angle, float posX, float posZ)
    {
        if (angle == 90)
        {
            playerDir = (Direction)(((int)playerDir + 1) % 4);
        }
        else if (angle == -90)
        {
            playerDir = (Direction)(((int)playerDir + 3) % 4);
        }
        updateVelocity();

        
    }

    void updateVelocity()
    {
        if(!keyBoardMode)
        {
            bikeSpeed = rpiUDP.bikeSpeed;
        }

        if (playerDir == Direction.North)
        {
            xVel = 0;
            zVel = 0.2f * bikeSpeed;
        }
        else if (playerDir == Direction.East)
        {
            xVel = 0.2f * bikeSpeed;
            zVel = 0;
        }
        else if (playerDir == Direction.South)
        {
            xVel = 0;
            zVel = -0.2f * bikeSpeed;
        }
        else if (playerDir == Direction.West)
        {
            xVel = -0.2f * bikeSpeed;
            zVel = 0;
        }
        speedText.text = "SPEED\r\n" + bikeSpeed.ToString() + " mph";
    }

    void resetGame()
    {
        StartCoroutine(Pause(3));
        for (int i = 0; i < NUM_ROADS_IN_GAME; i++)
        {
            Destroy(roads[i]);
            Destroy(scenes[i, 0]);
            Destroy(scenes[i, 1]);
        }
        for (int i = 0; i < NUM_ROADS_IN_GAME; i++)
        {
            createScene();
        }

        playerDir = Direction.North;
        nextDir = Direction.North;
        land = Landscape.City;
        mBody.position = new Vector3(0, 0, 0);
        mBody.rotation = Quaternion.Euler(0, 0, 0);
        roadIndex = 0;
        bikeSpeed = 0;
        zVel = 0;
        xVel = 0;
        endZ = -5;
        endX = 0;
        counter = 0;
        noLeft = false;
        noRight = false;
        justTurned = false;
        helpOn = true;
        resetPlayer();
        positions = new Vector3[NUM_ROADS_IN_GAME];
        directions = new Direction[NUM_ROADS_IN_GAME];
    }

    // pause for p seconds
    IEnumerator Pause(int p) 
    {
        Time.timeScale = 0.000001f;
        yield return new WaitForSeconds(p*Time.timeScale);
        Time.timeScale = 1;
    }


    IEnumerator RotateCam(Vector3 axis, float angle, float duration = 1.0f)
    {
        Quaternion from = mBody.rotation;
        Quaternion to = mBody.rotation;
        to *= Quaternion.Euler(axis * angle);

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            mBody.rotation = Quaternion.Slerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mBody.rotation = to;

        // put the ball in the middle of the lane
        Vector3 endPosition = positions[streetPieceCounter % NUM_ROADS_IN_GAME];
        if (playerDir == Direction.North || playerDir == Direction.South){
            transform.position = new Vector3 (endPosition.x, 0, mBody.position.z);
        }else if (playerDir == Direction.East || playerDir == Direction.West){
            transform.position = new Vector3 (mBody.position.x, 0, endPosition.z);
        }



    }
}
