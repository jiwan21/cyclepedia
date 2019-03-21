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
    
    public int walkSpeed;
    const float INACTIVE_Y = -10;
    const float ACTIVE_Y = 1;
    public bool bikeEntered;
    public bool bikeExited;
    Rigidbody person;
    Rigidbody bike;
    MoveBike moveBike;
    Vector3 personPosition;
    Vector3 bikePosition;
    // Start is called before the first frame update
    void Start()
    {
        bike = GameObject.Find("Sphere").GetComponent<Rigidbody>();
        moveBike = bike.GetComponent<MoveBike>();
        person = GetComponent<Rigidbody>();
        personPosition = person.position;
        bikePosition = bike.position;
        bikeEntered = false;
    }

    // Update is called once per frame
    void Update()
    {
        personPosition = person.position;
        bikePosition = bike.position;
        // if personPosition y coordinate is < 0, it is inactive
        //if (personPosition.y )
        //{
        if (Vector3.Distance(personPosition, bikePosition) < 1.5)
        {
            moveBike.pedestrianCrash = true;
        }
        if (Vector3.Distance(personPosition, bikePosition) < 5)
        {
            bikeEntered = true;
        }
        // if bike entered this pedestrian's area and then left the area
        if (bikeEntered && Vector3.Distance(personPosition, bikePosition) > 5)
        {
            bikeExited = true;
        }
        //}
    }

    public void makeActive(float x, float z)
    {
        person.position = new Vector3(x, ACTIVE_Y, z);
        Debug.Log("Pedestrian at " + x + "  " + z);
        //active = true;
    }

    private void makeInactive()
    {
        person.position = new Vector3(0, INACTIVE_Y, 0);
        //active = false;
    }
}
