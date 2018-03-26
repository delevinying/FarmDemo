using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour
{
    private bool isRotating = false;
    public float rotateSpeed = 1;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void FixedUpdate()
    {
        // if the screen has been touched
        if (Input.touchCount > 0)
        {
            Touch[] myTouches = Input.touches; // gets all the touches and stores them in an array

            // loops through all the current touches
            for (int i = 0; i < Input.touchCount; i++)
            {
                // if this touch just started (finger is down for the first time), for this particular touch 
                if (myTouches[i].phase == TouchPhase.Began)
                {
                    // if this touch is on the left-side half of screen
                    if (myTouches[i].position.x > Screen.width / 2)
                    {
                        RotateView();
                    }
                }
            }
        }
    }
    void RotateView()
    {
        //Input .GetAxis ("Mouse X"); 得到鼠标在水平方向的滑动
        //Input .GetAxis ("Mouse Y");得到鼠标在垂直方向的滑动
        if (Input.GetMouseButtonDown(0))
        {//0代表左键1代表右键2代表中键
            isRotating = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }
        if (isRotating)
        {
            transform.RotateAround(transform.position, Vector3.up, rotateSpeed * Input.GetAxis("Mouse X"));
        }
       
    }
}