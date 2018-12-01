using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : MonoBehaviour {

    public float speed = 5.0f;
    public float jump = 8.0f;

    Rigidbody2D myridg;

	// Use this for initialization
	void Start () {
        myridg = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetButton("Right"))
        {
            myridg.position += new Vector2(speed * Time.deltaTime, 0);
        }
        if (Input.GetButton("Left"))
        {
            myridg.position += new Vector2(-speed * Time.deltaTime, 0);
        }
        if (Input.GetButton("Up"))
        {
            if(myridg.IsTouchingLayers())
                myridg.velocity = new Vector2(0, jump);
        }
    }
}
