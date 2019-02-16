using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveball : MonoBehaviour {
    public const int NUM_ROADS_IN_GAME = 10;
    public const int PLATFORM_LEN = 10;
    public KeyCode moveL;
    public KeyCode moveR;
    public KeyCode arrowL;
    public KeyCode arrowR;
    public GameObject roadS; //straight
    public GameObject roadT; //turn
    public int roadIndex = 8;
    public int destroyIndex = 0;
    public float zVel = 0;
    public float xVel = 0;
    public float endZ = 90;
    public float endX = 0;
    public int counter = 0;
    public float distanceSinceTurn;
    public float nextUpdateDistance;
    public float lastPosition;
    public bool turned;
    GameObject[] roads = new GameObject[NUM_ROADS_IN_GAME];

    public enum Direction { North=0, East=1, South=2, West=3 };
    public Direction playerDir;
    public Direction nextDir;
    public Rigidbody mBody;
    // Use this for initialization
    void Start () {
        roads[0] = (GameObject)GameObject.Instantiate(roadS, new Vector3(0, 0, 10), Quaternion.identity);
        roads[1] = (GameObject)GameObject.Instantiate(roadS, new Vector3(0, 0, 20), Quaternion.identity);
        roads[2] = (GameObject)GameObject.Instantiate(roadS, new Vector3(0, 0, 30), Quaternion.identity);
        roads[3] = (GameObject)GameObject.Instantiate(roadS, new Vector3(0, 0, 40), Quaternion.identity);
        roads[4] = (GameObject)GameObject.Instantiate(roadS, new Vector3(0, 0, 50), Quaternion.identity);
        roads[5] = (GameObject)GameObject.Instantiate(roadS, new Vector3(0, 0, 60), Quaternion.identity);
        roads[6] = (GameObject)GameObject.Instantiate(roadS, new Vector3(0, 0, 70), Quaternion.identity);
        roads[7] = (GameObject)GameObject.Instantiate(roadS, new Vector3(0, 0, 80), Quaternion.identity);
        playerDir = Direction.North;
        nextDir = Direction.North;
        mBody = GetComponent<Rigidbody>();
        lastPosition = mBody.position.z;
        distanceSinceTurn = 0;
        nextUpdateDistance = distanceSinceTurn + 20;
        roadIndex = 8;
        destroyIndex = 0;
        zVel = 10;
        xVel = 0;
        endZ = 85;
        endX = 0;
        counter = 7;
        turned = false;
}
	
	// Update is called once per frame
	void Update () {
        mBody.velocity = new Vector3(xVel, 0, zVel);
        Vector3 pos = mBody.position;
        if (Input.GetKeyDown(moveL))
        {
            xVel = -2;
            StartCoroutine(stopSlide());
        }
        if (Input.GetKeyDown(moveR))
        {
            xVel = 2;
            StartCoroutine(stopSlide());
        }
        if (Input.GetKeyDown(arrowR))
        {
            Rotate(90, pos.x, pos.z);
            StartCoroutine(RotateCam(Vector3.up, 90, 1.0f));
        }
        if (Input.GetKeyDown(arrowL))
        {
            Rotate(-90, pos.x, pos.z);
            StartCoroutine(RotateCam(Vector3.up, -90, 1.0f));
        }
        
        if(destroyRoad(pos.x, pos.z))
        {
            if (counter % 8 == 0)
                turnRoad();
            else
                continueRoad();
            counter++;
        }
        
        
    }
    bool destroyRoad(float posX, float posZ)
    {
        if (playerDir == Direction.East || playerDir == Direction.West)
        {
            distanceSinceTurn = distanceSinceTurn + Math.Abs(posX - lastPosition);
            lastPosition = posX;
        }
        else if (playerDir == Direction.North || playerDir == Direction.South)
        {
            distanceSinceTurn = distanceSinceTurn + Math.Abs(posZ - lastPosition);
            lastPosition = posZ;
        }
        if (distanceSinceTurn > nextUpdateDistance)
        {
            Destroy(roads[(roadIndex + 1) % NUM_ROADS_IN_GAME]);
            //destroyIndex = (roadIndex + 1) % NUM_ROADS_IN_GAME;
            //Debug.Log("Destroy");
            Debug.Log("Destroy Index: " + roadIndex);
            nextUpdateDistance = nextUpdateDistance + 10;
            return true;
        }

        return false;
    }

    void continueRoad()
    {
        if(nextDir == Direction.East)
        {
            roads[roadIndex] = Instantiate(roadS, new Vector3(endX + 5, 0, endZ), Quaternion.Euler(0,90,0));
            endX = endX + 10;
        }
        else if (nextDir == Direction.West)
        {
            roads[roadIndex] = Instantiate(roadS, new Vector3(endX - 5, 0, endZ), Quaternion.Euler(0, 90, 0));
            endX = endX - 10;
        }
        else if (nextDir == Direction.South)
        {
            roads[roadIndex] = Instantiate(roadS, new Vector3(endX, 0, endZ - 5), Quaternion.identity);
            endZ = endZ - 10;
        }
        else
        {
            roads[roadIndex] = Instantiate(roadS, new Vector3(endX, 0, endZ + 5), Quaternion.identity);
            endZ = endZ + 10;
        }
        roadIndex = (roadIndex + 1) % NUM_ROADS_IN_GAME;
        Debug.Log("Create");
    }
    void turnRoad()
    {
        if (playerDir == Direction.North)
        {
            roads[roadIndex] = Instantiate(roadT, new Vector3(endX, 0, endZ + 2.5f), Quaternion.Euler(0,180,0));
            endZ = endZ + 2.5f;
            endX = endX + 2.5f;
            nextDir = Direction.East;
        }
        else if (playerDir == Direction.South)
        {
            roads[roadIndex] = Instantiate(roadT, new Vector3(0, 0, endZ - 2.5f), Quaternion.Euler(0, 90, 0));
            endZ = endZ - 2.5f;
            endX = endX + 2.5f;
            nextDir = Direction.East;
        }
        else if (playerDir == Direction.West)
        {
            roads[roadIndex] = Instantiate(roadT, new Vector3(endX - 2.5f, 0, endZ), Quaternion.Euler(0, 90, 0));
            endZ = endZ + 2.5f;
            endX = endX - 2.5f;
            nextDir = Direction.North;
        }
        else if (playerDir == Direction.East)
        {
            roads[roadIndex] = Instantiate(roadT, new Vector3(endX + 2.5f, 0, endZ), Quaternion.Euler(0, 0, 0));
            endZ = endZ + 2.5f;
            endX = endX + 2.5f;
            nextDir = Direction.North;
        }
        roadIndex = (roadIndex + 1) % NUM_ROADS_IN_GAME;
        Debug.Log("Create");
    }
    bool checkPos(int posX, int posZ)
    {
        if (playerDir == Direction.East)
        {
            if(posX > endX)
            {
                return true;
            }
        }
        else if (playerDir == Direction.West)
        {
            if (posX < endX)
            {
                return true;
            }
        }
        else if (playerDir == Direction.South)
        {
            if (posZ < endZ)
            {
                return true;
            }
        }
        else if (playerDir == Direction.North)
        {
            if (posZ > 100)
            {
                return true;
            }
        }

        return false;
    }

    void Rotate(int angle, float posX, float posZ)
    {
        if(angle == 90)
        {
            playerDir = (Direction)(((int)playerDir + 1) % 4);
        }
        else if (angle == -90)
        {
            playerDir = (Direction)(((int)playerDir + 3) % 4);
        }

        if(playerDir == Direction.North)
        {
            xVel = 0;
            zVel = 5;
            lastPosition = posZ;
        }
        else if (playerDir == Direction.East)
        {
            xVel = 5;
            zVel = 0;
            lastPosition = posX;
        }
        else if (playerDir == Direction.South)
        {
            xVel = 0;
            zVel = -5;
            lastPosition = posZ;
        }
        else if (playerDir == Direction.West)
        {
            xVel = -5;
            zVel = 0;
            lastPosition = posX;
        }

        distanceSinceTurn = 0;
        nextUpdateDistance = 10;
    }
    IEnumerator stopSlide()
    {
        yield return new WaitForSeconds(0.1f);
        xVel = 0;
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

    }
}
