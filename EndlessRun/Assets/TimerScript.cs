using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
	public static Image timerBar;
	public static float timeLeft;
	public static float maxTime;
	public static bool countingDown;
    public static string turningAllowed;
    public static string passingAllowed;
    public static bool didTurnLeft;
    public static bool canTurnLeft;
    public static bool didTurnRight;
    public static bool canTurnRight;
    public TutorialPedestrian tutPed;

    // Start is called before the first frame update
    void Start()
    {
        timerBar = GetComponent<Image> ();
        timerBar.fillAmount = 0;
        countingDown = false;
        maxTime = 0;
        turningAllowed = "none";
        didTurnLeft = false;
        didTurnRight = false;
    }

    // Update is called once per frame
    void Update()
    {	
    	if (countingDown){
    		if (timeLeft > 0) {
        	timeLeft -= Time.deltaTime;
        	timerBar.fillAmount = timeLeft/maxTime;
	        } else {
	        	stopTimer();
	        }
    	}
    }

    public static void startTimer(float time, int direction){
        timerBar.fillAmount = 1f;
        countingDown = true;
        maxTime = time;
        timeLeft = maxTime;
        if (direction == 0){
            turningAllowed = "left";
        }else if (direction == 1){
            turningAllowed = "right";
        }else{
            turningAllowed = "none";
        } 

        if (direction == 2){
            passingAllowed = "on your left";
        }else if (direction == 3){
            passingAllowed = "on your right";
        }else{
            passingAllowed = "none";
        }
    }

    public static void stopTimer(){
    	timerBar.fillAmount = 0f;
    	countingDown = false;
        turningAllowed = "none";
    }
}
