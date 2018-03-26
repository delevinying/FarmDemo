using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



public class ClickEventManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        //Debug.Log("Input.GetMouseButton(0)   :" +Input.GetMouseButton(0));
        if (Input.GetMouseButton(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//camare2D.ScreenPointToRay (Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {

                Debug.Log("hit:" + hit.collider.gameObject.name);
                if(hit.collider.gameObject.name.Equals("Tree_demo_click")){
                    Debug.Log("开始偷取果实");
                   // OnGUI();
                }
                checkFun();
            }

        }
	}

    void checkFun(){
        int _lastTime = PlayerPrefs.GetInt("time");
        int _time = getTimeStamp();
        Debug.Log("相差  "+(_time - _lastTime));
        PlayerPrefs.SetInt("time", _time);

    }

    int getTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        int ret;
        ret = (int)Convert.ToInt64(ts.TotalSeconds);
        Debug.Log("ret   "+ret);
        return ret;
    }

    public Rect windowRect = new Rect(20, 20, 120, 50);

    void OnGUI()
    {

        windowRect = GUI.Window(0, windowRect, DoMyWindow, "My Window");
        //GUI.

    }

    void DoMyWindow(int windowID)
    {

        if (GUI.Button(new Rect(10, 20, 100, 20), "Hello World"))

            print("Got a click");



    }
}
