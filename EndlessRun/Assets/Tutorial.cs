using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Tutorial : MonoBehaviour
{
    public Image staticPlayer;
    public Image rightPlayer;
    public Image leftPlayer;
    public Image armPose;
    public Image bloodSplatter;

    public TutorialPedestrian tutPed;

    public static int NUM_ROADS_IN_GAME = 15;
    const int NUM_PEDS_IN_GAME = 15;
    const int PLATFORM_LEN = 10;
    const int NUM_CITY_SCENES = 7;
    const int NUM_PARK_SCENES = 2;
    public int NUM_OF_STRAIGHTS = 7;

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
    public KeyCode keyZ;
    public KeyCode keyC;

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
    public TextMeshProUGUI dataText;
    public TextMeshProUGUI errorText;
    public GameObject pedestrian;
    public bool pedestrianCrash;

    public Vector3 bikePosition;

    static UdpClient udp;
    Thread thread;

    // for tutorial levels
    float timeCheck;
    int turns;
    int armPoseNum;
    bool armPoseStruck;
    int armPoseTurnAsk;
    int pedCreateIndex = 0;
    int nextPed = 0;

    // for leaning
    bool angleReset = false;

    // PLAYING MODE
    bool keyBoardMode = true;
    bool openCVOn = false;
    int level = 0;
    // level 0 -> pedaling
    // level 1 -> arm pose
    // level 2 -> arm pose + turning
    // level 3 -> lane changing
    // level 4/5 -> voice recognition

    System.Random rnd = new System.Random();
    int roadIndex;
    public static int bikeSpeed; // units: miles per hour
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
    int lastTurnNumberRight = -1;
    int lastTurnNumberLeft = -1;
    int pedCounter = 0;




    GameObject[] roads = new GameObject[NUM_ROADS_IN_GAME];
    GameObject[,] scenes = new GameObject[NUM_ROADS_IN_GAME, 2];
    GameObject[] peds = new GameObject[NUM_PEDS_IN_GAME];
    int pedIndex = 0;

    // streetPieceCounter is used to keep track of what index player is at in positions array below
    int streetPieceCounter;
    Vector3[] positions = new Vector3[NUM_ROADS_IN_GAME];
    Direction[] directions = new Direction[NUM_ROADS_IN_GAME];

    public enum Direction { North = 0, East = 1, South = 2, West = 3 };
    enum Landscape { City = 0, Park = 1 };

    public static Direction playerDir;
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
        timeCheck = 0;
        turns = 0;
        armPoseNum = 3; // user side 
        armPoseStruck = false;
        armPoseTurnAsk = rnd.Next(0, 2);

        pedestrianCrash = false;


        for (int i = 0; i < NUM_ROADS_IN_GAME; i++)
        {
            createScene();
        }
        udp = new UdpClient(12345);
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();

        resetPlayer();
        armPose.enabled = false;
        bloodSplatter.enabled = false;
        
        keyBoardMode = true;
    }

    void tutorialLevel(){

        helpOn = false;

        // level 0 -> pedaling
        // level 1 -> arm pose
        // level 2 -> arm pose + turning
        // level 3 -> lane changing
        // level 4 -> voice recognition



        if (level == 0 && timeCheck == 0){
            infoText.text = "Start pedaling to move the bike forward.";
            timeCheck += Time.deltaTime;
        }else if (timeCheck > 5 && bikeSpeed > 0 && level == 0){ // jump from level 0 to level 1
            level = 1;
            infoText.text = "Ok! Good job, let's move on.";
            timeCheck = 0;
        }else if(level == 0 & timeCheck < 5){
            timeCheck += Time.deltaTime;
        }

        if (level == 1 && timeCheck <= 2) {
            timeCheck += Time.deltaTime;
            dataText.text = "";
            armPoseNum = 3;
        }else if (level == 1 && timeCheck <= 4){
            infoText.text = "Lets disable speed for a second to learn a new skill.";
            timeCheck += Time.deltaTime;
        }else if (level == 1 && timeCheck <= 8){
            infoText.text = "Before turning, you need to indicate to others the intention to turn.";
            timeCheck += Time.deltaTime;
        }else if (level == 1 && timeCheck <= 12){
            infoText.text = "To do this, use your left arm and copy the images.";
            timeCheck += Time.deltaTime;
        }else if (level == 1 && timeCheck <= 15){
            armPose.enabled = true;
            timeCheck += Time.deltaTime;
        }else if(level == 1 && turns == 1 && timeCheck <= 10004){
            infoText.text = "The decreasing red timer indicates how long you have to turn after an arm pose.";
            timeCheck += Time.deltaTime;
            dataText.text = "";
        }else if(level == 1 && turns == 1 && timeCheck <= 10009){
            infoText.text = "Turning is disabled unless the red timer is live.";
            timeCheck += Time.deltaTime;
            dataText.text = "";
        }else if (level == 1 && turns <= 3){
            if (armPoseStruck){
                if (armPoseNum == 2){
                    armPoseTurnAsk = rnd.Next(0, 2); // (0: left)(1: right)
                    armPoseStruck = false;

                    if (turns == 0){
                        timeCheck=10000;
                    }

                    turns++;
                    dataText.text = (turns-1) + "/3 arm poses completed";
                }else{
                    infoText.text = "Good, now put your arm down.";
                    dataText.text = "";
                    
                }
            }
            
            if(armPoseTurnAsk == 0 && !armPoseStruck){
                infoText.text = "Put up the 'left signal' arm pose.";
            }else if (!armPoseStruck){
                infoText.text = "Put up the 'right signal' arm pose.";
            }

            if(!armPoseStruck && armPoseTurnAsk == armPoseNum){
                armPoseStruck = true;
            }

        }else if(level == 1 && turns > 3){
            level = 2;
            infoText.text = "Ok! Good job, let's move on.";
            dataText.text = (turns-1) + "/3 arm poses completed";
            timeCheck = 0;
            turns = 0;
            armPose.enabled = false;
        }


        if( level == 2 && timeCheck <= 2) {
            timeCheck += Time.deltaTime;
            turns = 0;
        }else if (level == 2 && timeCheck < 4){
            dataText.text = "";
            infoText.text = "Let's re-enable speed now. Start pedaling";
            bikeSpeed = 0;
            timeCheck += Time.deltaTime;
        }else if (level == 2 && timeCheck < 8){
            infoText.text = "Always remember to use an arm pose before turning.";
            timeCheck += Time.deltaTime;
        }else if (level == 2 && timeCheck < 12){
            infoText.text = "Lets incorporate arm poses with turning.";
            timeCheck += Time.deltaTime;
        }else if (level == 2 && turns <= 2){
            infoText.text = "At the end of the road, use the correct arm pose then lean left or right to turn. Remember to time your pose with your turning.";
            dataText.text = turns + "/3 turns";
        }else if (level == 2 && turns > 2){
            infoText.text = "Ok! Good job, let's move on.";
            dataText.text = turns + "/3 turns";
            timeCheck = 0;
            turns = 0;
            level = 3;
        }

        if( level == 3 && timeCheck <= 2) {
            timeCheck += Time.deltaTime;
            turns = 0;
            errorText.text = "";
        }else if (level == 3 && timeCheck < 4){
            dataText.text = "";
            infoText.text = "Let's introduce the concept of moving side to side.";
            timeCheck += Time.deltaTime;
        }else if (level == 3 && timeCheck < 8){
            dataText.text = "";
            infoText.text = "Change lanes by leaning left or right. There are three lanes on the road: left, middle, and right.";
            timeCheck += Time.deltaTime;
        }else if (level == 3 && timeCheck < 12){
            dataText.text = "";
            infoText.text = "One lean will shift you one lane over.";
            timeCheck += Time.deltaTime;
        }else if (level == 3 && turns <= 3){
            infoText.text = "Ok, Try it a few of times!";
            dataText.text = "One lean will shift you one lane over. Don't shift out of bounds.";
            timeCheck += Time.deltaTime;

        }else if (level == 3 && turns > 3){
            infoText.text = "Ok! Good job, let's move on.";
            dataText.text = "";
            timeCheck = 0;
            turns = 0;
            level = 4;
        }

        if( level == 4 && timeCheck <= 2) {
            timeCheck += Time.deltaTime;
            turns = 0;
        }else if (level == 4 && timeCheck < 4){
            dataText.text = "";
            bloodSplatter.enabled = false;
            infoText.text = "The road is a shared space. Let's introduce pedestrians onto the road.";
            timeCheck += Time.deltaTime;
        }else if (level == 4 && timeCheck < 8){
            dataText.text = "";
            infoText.text = "When you pass by a pedestrian, make sure to say 'on your left' or 'on your right'.";
            timeCheck += Time.deltaTime;
        }else if (level == 4 && timeCheck < 12){
            dataText.text = "After, a timer will display the amount of time you have to pass.";
            infoText.text = "When you pass by a pedestrian, make sure to say 'on your left' or 'on your right'.";
            timeCheck += Time.deltaTime;
        }else if (level == 4 && timeCheck < 15){
            dataText.text = "After, a timer will display the amount of time you have to pass";
            timeCheck += Time.deltaTime;
        }else if (level == 4 &&  turns < 1){
            infoText.text = "Say 'on your right' or 'on your left'";
            timeCheck += Time.deltaTime;
        }else if (level == 4 && turns >= 1){
            infoText.text = "Ok! Good job, let's move on.";
            
            timeCheck = 0;
            turns = 0;
            level = 5;
        }

        if(level == 5 && timeCheck <= 2){
            timeCheck += Time.deltaTime;
            turns = 0;
        }else if(level == 5 && turns < 1){
            bloodSplatter.enabled = false;
            infoText.text = "When you pass by a pedestrian, make sure to say 'on your left' or 'on your right'.";
        }else if (level == 5 && turns >= 1){
            infoText.text = "Ok! Good job, let's move on.";
            timeCheck = 0;
            turns = 0;
            level = 6;
        }

        if (level == 6){
            infoText.text = "Congratulations! You have finished the tutorial.";
            dataText.text = "Say 'main menu' and then 'game' to play the full game!";
            errorText.text = "";
            Time.timeScale = 0;
        }
    }


    // Update is called once per frame
    void Update()
    {
        mBody.velocity = new Vector3(xVel, 0, zVel);
        bikePosition = mBody.position;

        tutorialLevel();
        checkPedestrian();
        updateLean();

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

    
        if(SocketClient.signalString == "right" && level>= 1){
            rightPlayer.enabled = true;
            staticPlayer.enabled = false;
            leftPlayer.enabled = false;
            armPoseNum = 1;
            TimerScript.startTimer(3f, 1);
        }else if(SocketClient.signalString == "left" && level>= 1){
            rightPlayer.enabled = false;
            staticPlayer.enabled = false;
            leftPlayer.enabled = true;
            armPoseNum = 0;
            TimerScript.startTimer(3f, 0);
        }else if(SocketClient.signalString == "none" && level>= 1){
            rightPlayer.enabled = false;
            staticPlayer.enabled = true;
            leftPlayer.enabled = false;
            armPoseNum = 2;
            
        }else if (Input.GetKeyDown(keyT)){ // right
                rightPlayer.enabled = true;
                staticPlayer.enabled = false;
                leftPlayer.enabled = false;
                armPoseNum = 1;
                TimerScript.startTimer(3f, 1);
        }else if (Input.GetKeyDown(keyR)){ // middle
            rightPlayer.enabled = false;
            staticPlayer.enabled = true;
            leftPlayer.enabled = false;
            armPoseNum = 2;
        }else if (Input.GetKeyDown(keyE)){ // left
            rightPlayer.enabled = false;
            staticPlayer.enabled = false;
            leftPlayer.enabled = true;
            armPoseNum = 0;
            TimerScript.startTimer(3f, 0);
        }

        

        if((level == 4 && timeCheck > 15) || level == 5){
            if (SocketClientVoice.turn == "L" && lastTurnNumberLeft < SocketClientVoice.turnNumber){
                TimerScript.startTimer(6f, 2);
                Debug.Log("registered: on your left");
                lastTurnNumberLeft = SocketClientVoice.turnNumber;
                if(level==4){
                    turns++;
                }
            }else if(SocketClientVoice.turn == "R" && lastTurnNumberRight < SocketClientVoice.turnNumber){
                TimerScript.startTimer(6f, 3);
                Debug.Log("registered: on your right");
                lastTurnNumberRight = SocketClientVoice.turnNumber;
                if(level==4){
                    turns++;
                }
            }

            if (Input.GetKeyDown(keyZ)){ // on your left
                TimerScript.startTimer(2f, 2);
                Debug.Log("on your left");
                if(level==4){
                    turns++;
                }
            }else if (Input.GetKeyDown(keyC)){ // on your right
                TimerScript.startTimer(2f, 3);
                Debug.Log("on your right");
                if(level==4){
                    turns++;
                }
            }
        }

        if (keyBoardMode == true && level == 3){
            if (Input.GetKeyDown(arrowL) && (controlLocked == "n") && (laneNum > 1) && (level > 2)){ // go left one lane
                horizVel = -5;
                StartCoroutine (stopSlide());
                laneNum -= 1;
                controlLocked = "y";
                resetPlayer();
                TimerScript.stopTimer();

                if( level == 3){
                    turns++;
                }
            }else if (Input.GetKeyDown(arrowL) && (controlLocked == "n") && (laneNum == 1)){
                endGame(4);
            }
            if (Input.GetKeyDown(arrowR) && (controlLocked == "n") && (laneNum < 3) && (level > 2)){ // go right one lane
                horizVel = 5;
                StartCoroutine (stopSlide());
                laneNum += 1;
                controlLocked = "y";
                resetPlayer();
                TimerScript.stopTimer();

                if( level == 3){
                    turns++;
                }
            }else if(Input.GetKeyDown(arrowR) && (controlLocked == "n") && (laneNum == 3)){
                endGame(4);
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
        if (Input.GetKeyDown(keyD)) // turn right
        {
            if(TimerScript.turningAllowed == "right"){
                Rotate(90, bikePosition.x, bikePosition.z);
                StartCoroutine(RotateCam(Vector3.up, 90, 0.3f));
                TimerScript.stopTimer();
                laneNum = 2;
                resetPlayer();
                TimerScript.stopTimer();
                if(level == 2){
                    turns++;
                }

                errorText.text = "";
            }else{
                endGame(2);
            }
            
        }
        if (Input.GetKeyDown(keyA)) // turn left
        {
            if(TimerScript.turningAllowed == "left"){
                Rotate(-90, bikePosition.x, bikePosition.z);
                StartCoroutine(RotateCam(Vector3.up, -90, 0.3f));
                TimerScript.stopTimer();
                laneNum = 2;
                resetPlayer();
                TimerScript.stopTimer();
                if(level == 2){
                    turns++;
                }
                errorText.text = "";
            }else{
                endGame(2);
            }
        }

        

        if (!inBounds()){ // if not within boundaries
            endGame(3);
        }



        if (destroyRoad(bikePosition.x, bikePosition.z))
        {
            createScene();
        }

        if(pedestrianCrash)
        {
            endGame(5);
        }
    }



    void endGame(int val){

        if (val == 1){
            infoText.text = "OOPS, you used an arm pose without turning. Please use the arm pose before turning again.";
        }else if (val == 2){
            infoText.text = "OOPS, you turned without using an arm pose";
        }else if(val == 3){
            infoText.text = "OOPS, bikes can't go there! Please strike an arm pose and turn.";
            //bloodSplatter.enabled = true;
        }else if(val == 4){
            infoText.text = "OOPS, you can't change lanes off of the road! Let's try again.";
        }else if(val == 5){
            Debug.Log("Ran over a kid. wtf");
            infoText.text = "OOPS, bikes can't run over innocent kids!";
            speedText.text = "";
            bloodSplatter.enabled = true;
            //resetGame();
        }else if(val == 6){
            infoText.text = "OOPS, you didn't say 'on your left'";
        }else if(val == 7){
            infoText.text = "OOPS, you didn't say 'on your right'";
        }

        speedText.text ="";
        

        timeCheck = 0;
        //Application.Quit();
        //Time.timeScale = 0;
        bikeSpeed = 0;
        //udp.Close();
        //thread.Abort();
        //Debug.Break();
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

    void updateLean()
    {

        if(level >= 2){
                    // turning
            if(rpiUDP.bikeAngle > -0.4 && rpiUDP.bikeAngle < 0.4){
                angleReset = true;
            }else if(rpiUDP.bikeAngle <= -0.5 && angleReset && TimerScript.turningAllowed == "right"){
                // lean right
                Rotate(90, bikePosition.x, bikePosition.z);
                StartCoroutine(RotateCam(Vector3.up, 90, 0.3f));
                TimerScript.stopTimer();
                laneNum = 2;
                resetPlayer();
                TimerScript.stopTimer();
                angleReset = false;
                errorText.text = "";

                if(level == 2){
                    turns++;
                }
                
            }else if(rpiUDP.bikeAngle >= 0.5 && angleReset && TimerScript.turningAllowed == "left"){
                // lean left
                Rotate(-90, bikePosition.x, bikePosition.z);
                StartCoroutine(RotateCam(Vector3.up, -90, 0.3f));
                TimerScript.stopTimer();
                laneNum = 2;
                resetPlayer();
                errorText.text = "";
                TimerScript.stopTimer();
                angleReset = false;
                if(level == 2){
                    turns++;
                }
                
            }

            if(level >= 3){
                // changing lanes
                if(rpiUDP.bikeAngle > -0.4 && rpiUDP.bikeAngle < 0.4){
                    angleReset = true;
                }else if(rpiUDP.bikeAngle <= -0.5 && angleReset && laneNum < 3){
                    // lean right
                    horizVel = 5;
                    StartCoroutine(stopSlide());
                    laneNum += 1;
                    angleReset = false;
                    if(level ==3){
                        turns++;
                    }
          
                }else if(rpiUDP.bikeAngle <= -0.5 && angleReset && laneNum == 3){
                    endGame(4);
                }else if(rpiUDP.bikeAngle >= 0.5 && angleReset && laneNum > 1){
                    // lean left
                    horizVel = -5;
                    StartCoroutine(stopSlide());
                    laneNum -= 1;
                    angleReset = false;
                    if(level ==3){
                        turns++;
                    }
                }else if(rpiUDP.bikeAngle >= 0.5 && angleReset && laneNum == 1){
                    endGame(4);
                }
            }
        }
    }

    void ThreadMethod()
    {
        while (true)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            pedalSpeed = returnData[0];
            //Debug.Log(returnData);
        }
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
        peds[pedCreateIndex].GetComponent<TutorialPedestrian>().setDirection(d);
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
                Debug.Log("Next Ped: " + peds[nextPed].GetComponent<Pedestrian>().personPosition);
                if(TimerScript.passingAllowed != "on your left"){
                    endGame(6);
                }
                TimerScript.stopTimer();
                turns++;
                
            }
            else if (passed == 1)
            {
                Debug.Log("Passed Ped #" + nextPed + " on the right");
                nextPed = (nextPed + 1) % NUM_PEDS_IN_GAME;
                Debug.Log("Next Ped: " + peds[nextPed].GetComponent<Pedestrian>().personPosition);
                if(TimerScript.passingAllowed != "on your right"){
                    endGame(7);
                }
                TimerScript.stopTimer();
                turns++;
            }
        }
    }

    void createScene()
    {
        if (counter % NUM_OF_STRAIGHTS == (NUM_OF_STRAIGHTS - 1))
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
            if(level >= 3 && (pedCounter%3 == 0) ) addPedestrian(endX + 5, endZ, Direction.East);
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
            if(level >= 3 && (pedCounter%3 == 0)) addPedestrian(endX - 5, endZ, Direction.West);
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
            if (level >= 3 && (pedCounter%3 == 0)) addPedestrian(endX, endZ - 5, Direction.South);
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
            if (level >= 3 && (pedCounter%3 == 0)) addPedestrian(endX, endZ + 5, Direction.North);
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
        pedCounter++;
    }

    bool inBounds(){
        Vector3 curPosition = mBody.position;

        Vector3 endPosition = positions[streetPieceCounter % NUM_ROADS_IN_GAME];
        Vector3 nextEndPosition = positions[(streetPieceCounter+1) % NUM_ROADS_IN_GAME];


        // North is z+, South is z-, East is x+, West is x-
        Direction nextDir = directions[streetPieceCounter%NUM_ROADS_IN_GAME];

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
        if(land == Landscape.City && level > 5)
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
        if(rpiUDP.bikeSpeed > 0)
        {
            bikeSpeed = rpiUDP.bikeSpeed;
        }

        if(bikeSpeed > 0 && level == 0){
            bikeSpeed = 15;
        }else if (bikeSpeed > 0 && level == 0){
            bikeSpeed = 15;
        }else if (bikeSpeed > 0 && level == 1){
            bikeSpeed = 0;
        }else if (bikeSpeed > 0 && level > 1){
            bikeSpeed = (int)(bikeSpeed * 3 * 0.1);
        }

        // else if(bikeSpeed > 0 && level == 4){
        //     bikeSpeed = 10;
        // }else if (bikeSpeed > 0){
        //     bikeSpeed = 15;
        // }

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

        if(level == 1){
            bikeSpeed = 0;
            speedText.text = "SPEED\r\n" + "disabled";
        }else {
            speedText.text = "SPEED\r\n" + bikeSpeed + " mph";
        }
        

        
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
