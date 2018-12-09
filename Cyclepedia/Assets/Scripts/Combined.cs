using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Combined : MonoBehaviour
{

    public float speed = 10f;
    public Text speedText;
    public Text signalText;

    private Rigidbody2D rb;
    private Vector2 moveVelocity;
    private string signal;

    Direction currentDir;
    Vector2 input;
    bool isMoving = false;
    Vector3 startPos;
    Vector3 endPos;
    float t;
    char dir;

    public Sprite northSprite;
    public Sprite southSprite;
    public Sprite eastSprite;
    public Sprite westSprite;

    static UdpClient udp;
    Thread thread;

    // Use this for initialization

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetSpeedText();
        SetSignalText();
        udp = new UdpClient(12345);
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();
    }

    private void ThreadMethod()
    {
        while (true)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            dir = returnData[0];
            //Debug.Log(returnData);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2 moveInput = new Vector2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"));
        //moveVelocity = moveInput.normalized * speed;
        SetSpeedText();

        if (!isMoving)
        {
            input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if(dir == 'F')
            {
                input.y = 0.5f;
                input.x = 0.0f;
            }
            else if(dir == 'B')
            {
                input.y = -0.5f;
                input.x = 0.0f;
            }
            else if (dir == 'L')
            {
                input.y = 0.0f;
                input.x = -0.5f;
            }
            else if (dir == 'R')
            {
                input.y = 0.0f;
                input.x = 0.5f;
            }
            else
            {
                input.y = 0.0f;
                input.x = 0.0f;
            }
             
            //Debug.Log("x: " + input.x);
            //Debug.Log("y: " + input.y);

            if (input != Vector2.zero)
            {
                if (input.x < 0)
                {
                    currentDir = Direction.West;
                }
                if (input.x > 0)
                {
                    currentDir = Direction.East;
                }
                if (input.y < 0)
                {
                    currentDir = Direction.South;
                }
                if (input.y > 0)
                {
                    currentDir = Direction.North;
                }

                switch (currentDir)
                {
                    case Direction.North:
                        gameObject.GetComponent<SpriteRenderer>().sprite = northSprite;
                        break;
                    case Direction.East:
                        gameObject.GetComponent<SpriteRenderer>().sprite = eastSprite;
                        break;
                    case Direction.South:
                        gameObject.GetComponent<SpriteRenderer>().sprite = southSprite;
                        break;
                    case Direction.West:
                        gameObject.GetComponent<SpriteRenderer>().sprite = westSprite;
                        break;
                }

                StartCoroutine(Move(transform));
            }
        }
    }

    public IEnumerator Move(Transform entity)
    {
        isMoving = true;
        startPos = entity.position;
        t = 0;

        endPos = new Vector3(startPos.x + (float)0.16f * System.Math.Sign(input.x), startPos.y + (float)0.16f * System.Math.Sign(input.y), startPos.z);

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            entity.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        isMoving = false;
        yield return 0;
    }

    void SetSpeedText()
    {
        speedText.text = "Speed: " + speed.ToString();
    }
    void SetSignalText()
    {
        signalText.text = "Signal: " + signal;
    }
}

enum Direction
{
    North,
    East,
    South,
    West
}