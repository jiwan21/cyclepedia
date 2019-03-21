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

public class MoveBike : MonoBehaviour
{
    public World W;

    public Image staticPlayer;
    public Image rightPlayer;
    public Image leftPlayer;
    public Image bloodSplatter;

    // for horizontal movement (left / right lanes)
    public float horizVel = 0;
    public int laneNum = 2; 
    public string controlLocked = "n"; 

    public KeyCode keyW;
    public KeyCode keyS;
    public KeyCode keyA;
    public KeyCode keyD;
    public KeyCode keyQ;
    public KeyCode arrowL;
    public KeyCode arrowR;

    //for testing arm poses
    public KeyCode keyE;
    public KeyCode keyR;
    public KeyCode keyT;
    // for testing voice recognition "on your left" and "on your right"
    public KeyCode keyZ;
    public KeyCode keyC;

    public Vector3 bikePosition;
    public int bikeSpeed; // units: miles per hour
    public bool pedestrianCrash;

    bool keyBoardMode = true;
    bool openCVOn = true;

    System.Random rnd = new System.Random();
    char pedalSpeed; // units: rotations per hour
    float zVel; // unity grid units
    float xVel; // unity grid units
    bool justTurned;
    bool helpOn;
    bool angleReset = false;
    Rigidbody bike;
    public World.Direction bikeDir;
    public World.Direction nextDir;
    World.Landscape land;
    public float velocityScale = 1;

    int lastScore = 0;
    int lastTurnNumberRight = -1;
    int lastTurnNumberLeft = -1;

    // Use this for initialization
    void Start()
    {
        bikeDir = World.Direction.North;
        nextDir = World.Direction.North;
        bike = GetComponent<Rigidbody>();
        bikeSpeed = 0;
        zVel = 0;
        xVel = 0;
        justTurned = false;
        helpOn =  true;
        resetPlayer();
        pedestrianCrash = false;
        nextDir = W.resetWorld();
        bloodSplatter.enabled = false;
    }

    
    // Update is called once per frame
    void Update()
    {
        bike.velocity = new Vector3(xVel, 0, zVel);
        bikePosition = bike.position;

        updateVelocity();
        updateLean();

        if(W.score - lastScore > 150){
            velocityScale = (float) (1 + W.score * 0.005);
            lastScore = W.score;
            W.levelUp();
        }

        
        

        if (bikeDir == World.Direction.North)
        {
            xVel = horizVel;
        }
        else if (bikeDir == World.Direction.South)
        {
            xVel = -horizVel;
        }
        else if (bikeDir == World.Direction.East)
        {
            zVel = -horizVel;
        }
        else if (bikeDir == World.Direction.West)
        {
            zVel = horizVel;
        }

        if(SocketClient.signalString == "right"){
            rightPlayer.enabled = true;
            staticPlayer.enabled = false;
            leftPlayer.enabled = false;
            TimerScript.startTimer(3f, 1);
        }else if(SocketClient.signalString == "left"){
            rightPlayer.enabled = false;
            staticPlayer.enabled = false;
            leftPlayer.enabled = true;
            TimerScript.startTimer(3f, 0);
        }else if(SocketClient.signalString == "none"){
            rightPlayer.enabled = false;
            staticPlayer.enabled = true;
            leftPlayer.enabled = false;
            
        }else if (Input.GetKeyDown(keyT)){ // right
            rightPlayer.enabled = true;
            staticPlayer.enabled = false;
            leftPlayer.enabled = false;
            TimerScript.startTimer(3f, 1);
        }else if (Input.GetKeyDown(keyR)){ // middle
            rightPlayer.enabled = false;
            staticPlayer.enabled = true;
            leftPlayer.enabled = false;
        }else if (Input.GetKeyDown(keyE)){ // left
            rightPlayer.enabled = false;
            staticPlayer.enabled = false;
            leftPlayer.enabled = true;
            TimerScript.startTimer(3f, 0);
        }

        if (keyBoardMode == true){
            if (Input.GetKeyDown(arrowL) && (controlLocked == "n")){
                horizVel = -5;
                StartCoroutine (stopSlide());
                laneNum -= 1;
                controlLocked = "y";
                resetPlayer();
            }else if (Input.GetKeyDown(arrowR) && (controlLocked == "n")){
                horizVel = 5;
                StartCoroutine (stopSlide());
                laneNum += 1;
                controlLocked = "y";
                resetPlayer();
            }

        }
        if (SocketClientVoice.turn == "L" && lastTurnNumberLeft < SocketClientVoice.turnNumber){
            TimerScript.startTimer(2f, 2);
            Debug.Log("registered: on your left");
            lastTurnNumberLeft = SocketClientVoice.turnNumber;
        }else if(SocketClientVoice.turn == "R" && lastTurnNumberRight < SocketClientVoice.turnNumber){
            TimerScript.startTimer(2f, 3);
            Debug.Log("registered: on your right");
            lastTurnNumberRight = SocketClientVoice.turnNumber;
        }

        if (Input.GetKeyDown(keyZ)){ // on your left
            TimerScript.startTimer(4f, 2);
            Debug.Log("on your left");
        }else if (Input.GetKeyDown(keyC)){ // on your right
            TimerScript.startTimer(4f, 3);
            Debug.Log("on your right");
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
            //resetGame();
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
            }else{
                endGame(2);
            }
        }

        if (Input.GetKeyDown(keyQ))
        {
            Time.timeScale = 1;
            resetGame();
        }
        // each time you go past the next endpoint, increment streetPieceCounter
        checkCrash();
        
        
        if (W.destroyScene(bikePosition.x, bikePosition.z))
        {
            nextDir = W.createScene(nextDir);
        }
    }

    void checkCrash()
    {
        if (!W.inBounds(bikeDir)) // if not within boundaries
        {
            endGame(3);
        }
        if(pedestrianCrash)
        {
            endGame(5);
            
            //resetGame();
        }
    }

    public void endGame(int val){

        if (val == 1){
            W.infoText.text = "OOPS, you used an arm pose without turning";
        }else if (val == 2){
            W.infoText.text = "OOPS, you turned without using an arm pose";
        }else if(val == 3){
            W.infoText.text = "OOPS, bikes can't go there!";
            bloodSplatter.enabled = true;
        }else if(val == 4){
            W.infoText.text = "OOPS, you changed lanes off of the road!";
        }else if(val == 5){ // pedestrian crash
            Debug.Log("Ran over a kid. wtf");
            W.infoText.text = "OOPS, bikes can't run over innocent kids!";
            bloodSplatter.enabled = true;
        }else if(val == 6){
            W.errorText.text = "OOPS, you didn't say 'on your left'";
        }else if(val == 7){
            W.errorText.text = "OOPS, you didn't say 'on your right'";
        }
        
        W.speedText.text = "";
        W.pause();
        //Application.Quit();
        //Time.timeScale = 0;
        bikeSpeed = 0;
        //Debug.Break();
    }
    IEnumerator stopSlide()
    {
        yield return new WaitForSeconds (0.3f);
        horizVel = 0;
        xVel = 0;
        controlLocked = "n";

        if (laneNum < 1 || laneNum > 3){
            W.infoText.text = "you can't go there";
            Time.timeScale = 0;
        }
    }

    void resetPlayer(){
        rightPlayer.enabled = false;
        staticPlayer.enabled = true;
        leftPlayer.enabled = false;
    }
  
    void Rotate(int angle, float posX, float posZ)
    {
        if (angle == 90)
        {
            bikeDir = (World.Direction)(((int)bikeDir + 1) % 4);
        }
        else if (angle == -90)
        {
            bikeDir = (World.Direction)(((int)bikeDir + 3) % 4);
        }
        updateVelocity();
    }

    void updateVelocity()
    {
        if(!keyBoardMode)
        {
            bikeSpeed = rpiUDP.bikeSpeed;
        }

        if (bikeDir == World.Direction.North)
        {
            xVel = 0;
            zVel = 0.2f * bikeSpeed * velocityScale;
        }
        else if (bikeDir == World.Direction.East)
        {
            xVel = 0.2f * bikeSpeed * velocityScale;
            zVel = 0;
        }
        else if (bikeDir == World.Direction.South)
        {
            xVel = 0;
            zVel = -0.2f * bikeSpeed * velocityScale;
        }
        else if (bikeDir == World.Direction.West)
        {
            xVel = -0.2f * bikeSpeed * velocityScale;
            zVel = 0;
        }
        
    }
    void updateLean()
    {

        // turning
        if(rpiUDP.bikeAngle > -0.7 && rpiUDP.bikeAngle < 0.7){
            angleReset = true;
        }else if(rpiUDP.bikeAngle <= -0.4 && angleReset && TimerScript.turningAllowed == "right"){
            // lean right
            Rotate(90, bikePosition.x, bikePosition.z);
            StartCoroutine(RotateCam(Vector3.up, 90, 0.3f));
            TimerScript.stopTimer();
            laneNum = 2;
            resetPlayer();
            TimerScript.stopTimer();
            angleReset = false;
        }else if(rpiUDP.bikeAngle >= 0.4 && angleReset && TimerScript.turningAllowed == "left"){
            // lean left
            Rotate(-90, bikePosition.x, bikePosition.z);
            StartCoroutine(RotateCam(Vector3.up, -90, 0.3f));
            TimerScript.stopTimer();
            laneNum = 2;
            resetPlayer();
            TimerScript.stopTimer();
            angleReset = false;
        }else if(rpiUDP.bikeAngle >= 0.4 && angleReset && TimerScript.turningAllowed == "right"){
            endGame(2); // turned the wrong direction
        }else if(rpiUDP.bikeAngle <= -0.4 && angleReset && TimerScript.turningAllowed == "left"){
            endGame(2); // turned the wrong direction
        }

        // changing lanes
        if(rpiUDP.bikeAngle > -0.7 && rpiUDP.bikeAngle < 0.7){
            angleReset = true;
        }else if(rpiUDP.bikeAngle <= -0.4 && angleReset){
            // lean right
            horizVel = 5;
            StartCoroutine(stopSlide());
            laneNum += 1;
            angleReset = false;
        }else if(rpiUDP.bikeAngle >= 0.4 && angleReset){
            // lean left
            horizVel = -5;
            StartCoroutine(stopSlide());
            laneNum -= 1;
            angleReset = false;
        }


    }
    void resetGame()
    {
        bikeDir = World.Direction.North;
        nextDir = World.Direction.North;
        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        bike.position = new Vector3(0, 0, 0);
        bike.rotation = Quaternion.Euler(0, 0, 0);
        bikeSpeed = 0;
        zVel = 0;
        xVel = 0;
        justTurned = false;
        helpOn = true;
        pedestrianCrash = false;
        resetPlayer();
        nextDir = W.resetWorld();
    }

    IEnumerator RotateCam(Vector3 axis, float angle, float duration = 1.0f)
    {
        Quaternion from = bike.rotation;
        Quaternion to = bike.rotation;
        to *= Quaternion.Euler(axis * angle);

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            bike.rotation = Quaternion.Slerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bike.rotation = to;

        // put the ball in the middle of the lane

        Vector3 endPosition = W.getEndPosition();
        if (bikeDir == World.Direction.North || bikeDir == World.Direction.South)
        {
            transform.position = new Vector3 (endPosition.x, 0, bike.position.z);
        }
        else if (bikeDir == World.Direction.East || bikeDir == World.Direction.West)
        {
            transform.position = new Vector3 (bike.position.x, 0, endPosition.z);
        }
        
    }
}
