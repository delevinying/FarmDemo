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
        RotateView();
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