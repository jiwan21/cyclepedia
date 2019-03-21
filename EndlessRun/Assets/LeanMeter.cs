using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeanMeter : MonoBehaviour
{
    public static Image leanBar;
    float bikeLean;

    // Start is called before the first frame update
    void Start()
    {
        leanBar = GetComponent<Image>();
        leanBar.transform.position = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        bikeLean = rpiUDP.bikeAngle;
        if (bikeLean < -1)
        {
            bikeLean = -1f;
        }
        else if (bikeLean > 1)
        {
            bikeLean = 1f;
        }
        bikeLean = bikeLean / 2;
        leanBar.transform.position = new Vector3(bikeLean, 0, 0);
    }
}
