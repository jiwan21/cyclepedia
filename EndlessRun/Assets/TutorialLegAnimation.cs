using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialLegAnimation : MonoBehaviour
{
    public Animation anim;
    private static int speed;
    int speedMax;

    void Start()
    {
    	speed = 0;
    	speedMax = 25;
    	anim = GetComponent<Animation>();
        
    }

    void Update()
    {	
    	speed = Tutorial.bikeSpeed;
    	if (speed == 0){
	        foreach (AnimationState state in anim)
	        {
	            state.speed = 0;
	        }
    	}else if (speed < speedMax){
    		foreach (AnimationState state in anim)
	        {
	            state.speed = (float) speed / speedMax;
	        }
    	}else if (speed >= speedMax){
    		foreach (AnimationState state in anim)
	        {
	            state.speed = 1;
	        }
    	}
    }
}