using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text;


public class TutorialPedestrian : MonoBehaviour
{
    const int ZONE_CLOSE_OFFSET = -2;
    const int ZONE_FAR_OFFSET = 1;
    public int walkSpeed;
    const float INACTIVE_Y = -10;
    const float ACTIVE_Y = 1;
    public bool bikeEntered;
    public int bikeExited;

    bool passedLeft = false;
    bool passedRight = false;

    Tutorial.Direction dir;

    Rigidbody person;
    Rigidbody bike;
    Tutorial moveBike;
    Vector3 personPosition;
    Vector3 bikePosition;
    // Start is called before the first frame update
    void Start()
    {
        bike = GameObject.Find("Sphere").GetComponent<Rigidbody>();
        moveBike = bike.GetComponent<Tutorial>();
        person = GetComponent<Rigidbody>();
        personPosition = person.position;
        bikePosition = bike.position;
        bikeEntered = false;
        bikeExited = 0;
    }

    // Update is called once per frame
    void Update()
    {
        personPosition = person.position;
        bikePosition = bike.position;
        // if personPosition y coordinate is < 0, it is inactive
        //if (personPosition.y )
        //{
        //Debug.Log(Vector3.Distance(personPosition, bikePosition));
        if (Vector3.Distance(personPosition, bikePosition) < 1.2)
        {
            moveBike.pedestrianCrash = true;
        }
        if (inZone())
        {
        	Debug.Log("Entered Zone: P(" + personPosition.x + ", " + personPosition.z + "), B(" + bikePosition.x + ", " + bikePosition.z + ")");
            checkLeftRight();
            bikeEntered = true;
        }
        // if bike entered this pedestrian's area and then left the area
        if (bikeEntered && !inZone())
        {
            //Debug.Log("Exiting...");
            if(passedLeft)
            {
                bikeExited = -1;
                //Debug.Log("Left");
            }
            else if(passedRight)
            {
                bikeExited = 1;
                //Debug.Log("Right");
            }
        }
        //}
    }

    public void makeActive(float x, float z)
    {
        person.position = new Vector3(x, ACTIVE_Y, z);
        //Debug.Log("Pedestrian at " + x + "  " + z);
        //active = true;
    }
    public void setDirection(Tutorial.Direction d)
    {
        dir = d;
    }
    private void checkLeftRight()
    {

        if(dir == Tutorial.Direction.East && Tutorial.playerDir == Tutorial.Direction.East)
        {
            if(personPosition.z < bikePosition.z)
            {
                passedLeft = true;
                passedRight = false;
                Debug.Log("East left: " + personPosition.z + ", " + bikePosition.z);
            }
            else if (personPosition.z > bikePosition.z)
            {
                passedRight = true;
                passedLeft = false;
                Debug.Log("East right: " + personPosition.z + ", " + bikePosition.z);
            }
        }
        else if(dir == Tutorial.Direction.West && Tutorial.playerDir == Tutorial.Direction.West)
        {
            if (personPosition.z < bikePosition.z)
            {
                passedRight = true;
                passedLeft = false;
                Debug.Log("west right: " + personPosition.z + ", " + bikePosition.z);
            }
            else if (personPosition.z > bikePosition.z)
            {
                passedLeft = true;
                passedRight = false;
                Debug.Log("west left: " + personPosition.z + ", " + bikePosition.z);
            }
        }
        else if (dir == Tutorial.Direction.North && Tutorial.playerDir == Tutorial.Direction.North)
        {
            if (personPosition.x < bikePosition.x)
            {
                passedRight = true;
                passedLeft = false;
                Debug.Log("north right: " + personPosition.x + ", " + bikePosition.x);
            }
            else if (personPosition.x > bikePosition.x)
            {
                passedLeft = true;
                passedRight = false;
                Debug.Log("north left: " + personPosition.x + ", " + bikePosition.x);
            }
        }
        else if (dir == Tutorial.Direction.South && Tutorial.playerDir == Tutorial.Direction.South)
        {
            if (personPosition.x < bikePosition.x)
            {
                passedLeft = true;
                passedRight = false;
                Debug.Log("south left: " + personPosition.x + ", " + bikePosition.x);
            }
            else if (personPosition.x > bikePosition.x)
            {
                passedRight = true;
                passedLeft = false;
                Debug.Log("south right: " + personPosition.x + ", " + bikePosition.x);
            }
        }


    }
    private void makeInactive()
    {
        person.position = new Vector3(0, INACTIVE_Y, 0);
        //active = false;
    }
    private bool inZone()
    {
        if(dir == Tutorial.Direction.North)
        {
            return (bikePosition.z > personPosition.z + ZONE_CLOSE_OFFSET) && (bikePosition.z < personPosition.z + ZONE_FAR_OFFSET);
        }
        else if (dir == Tutorial.Direction.South)
        {
            return (bikePosition.z < personPosition.z - ZONE_CLOSE_OFFSET) && (bikePosition.z > personPosition.z - ZONE_FAR_OFFSET);
        }
        else if (dir == Tutorial.Direction.East)
        {
            return (bikePosition.x > personPosition.x + ZONE_CLOSE_OFFSET) && (bikePosition.x < personPosition.x + ZONE_FAR_OFFSET);
        }
        else
        {
            return (bikePosition.x < personPosition.x - ZONE_CLOSE_OFFSET) && (bikePosition.x > personPosition.x - ZONE_FAR_OFFSET);
        }
    }
}
