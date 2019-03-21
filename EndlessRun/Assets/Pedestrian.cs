using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text;


public class Pedestrian : MonoBehaviour
{
    const int ZONE_CLOSE_OFFSET = -3;
    const int ZONE_FAR_OFFSET = 1;
    public int walkSpeed;
    const float INACTIVE_Y = -10;
    const float ACTIVE_Y = 1;
    public bool bikeEntered;
    public int bikeExited;

    bool passedLeft = false;
    bool passedRight = false;
    Rigidbody person;
    Rigidbody bike;
    MoveBike moveBike;
    public Vector3 personPosition;
    Vector3 bikePosition;
    World.Direction dir;
    // Start is called before the first frame update
    void Start()
    {
        bike = GameObject.Find("Sphere").GetComponent<Rigidbody>();
        moveBike = bike.GetComponent<MoveBike>();
        person = GetComponent<Rigidbody>();
        personPosition = person.position;
        bikePosition = bike.position;
        bikeEntered = false;
        bikeExited = 0;
        /*
        if (dir == World.Direction.North)
        {
            person.velocity = new Vector3(0, 0, 1);
        }
        else if (dir == World.Direction.South)
        {
            person.velocity = new Vector3(0, 0, -1);
        }
        else if (dir == World.Direction.East)
        {
            person.velocity = new Vector3(1, 0, 0);
        }
        else
        {
            person.velocity = new Vector3(-1, 0, 0);
        }
        */
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
    public void setDirection(World.Direction d)
    {
        dir = d;
    }
    private void checkLeftRight()
    {
        if(dir == World.Direction.East)
        {
            if(personPosition.z < bikePosition.z)
            {
                passedLeft = true;
            }
            else if (personPosition.z > bikePosition.z)
            {
                passedRight = true;
            }
        }
        else if(dir == World.Direction.West)
        {
            if (personPosition.z < bikePosition.z)
            {
                passedRight = true;
            }
            else if (personPosition.z > bikePosition.z)
            {
                passedLeft = true;
            }
        }
        else if (dir == World.Direction.North)
        {
            if (personPosition.x < bikePosition.x)
            {
                passedRight = true;
            }
            else if (personPosition.x > bikePosition.x)
            {
                passedLeft = true;
            }
        }
        else if (dir == World.Direction.South)
        {
            if (personPosition.x < bikePosition.x)
            {
                passedLeft = true;
            }
            else if (personPosition.x > bikePosition.x)
            {
                passedRight = true;
            }
        }
    }
    private bool inZone()
    {
        if(dir == World.Direction.North)
        {
            return (bikePosition.z > personPosition.z + ZONE_CLOSE_OFFSET) && (bikePosition.z < personPosition.z + ZONE_FAR_OFFSET);
        }
        else if (dir == World.Direction.South)
        {
            return (bikePosition.z < personPosition.z - ZONE_CLOSE_OFFSET) && (bikePosition.z > personPosition.z - ZONE_FAR_OFFSET);
        }
        else if (dir == World.Direction.East)
        {
            return (bikePosition.x > personPosition.x + ZONE_CLOSE_OFFSET) && (bikePosition.x < personPosition.x + ZONE_FAR_OFFSET);
        }
        else
        {
            return (bikePosition.x < personPosition.x - ZONE_CLOSE_OFFSET) && (bikePosition.x > personPosition.x - ZONE_FAR_OFFSET);
        }
    }
}
