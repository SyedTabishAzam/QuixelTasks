using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCamera : MonoBehaviour {

    public float speedOfMovement = 0.5f;
    public float speedOfPan = 4f;
    bool pressed = false;
    Vector3 origin;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
        if(Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * speedOfMovement;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * speedOfMovement;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * speedOfMovement;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * speedOfMovement;
        }

        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            pressed = true;
            origin = Input.mousePosition;
        }

        
        if(pressed)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - origin);

            transform.RotateAround(transform.position, transform.right, -pos.y * speedOfPan );
            transform.RotateAround(transform.position, Vector3.up, pos.x * speedOfPan);
        }

        if (Input.GetKeyUp(KeyCode.Mouse2))
        {
            pressed = false;
        }
    }


}
