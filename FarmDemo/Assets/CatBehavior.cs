using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatBehavior : MonoBehaviour {
    public Animator _ani;
    private float _time;
    public Vector3 point1;
    public Vector3 point2;
    private bool fg = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        _time += Time.time;
        Debug.Log("time " + _time);
        if(_time >1000){
            fg = !fg;
        }
        if(!fg){
            
            this.gameObject.transform.Translate(this.gameObject.transform.forward*Time.deltaTime);
        }

       
	}
}
