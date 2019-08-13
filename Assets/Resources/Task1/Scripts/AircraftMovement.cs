using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AircraftMovement : MonoBehaviour {

    public float speed = 4;
    public float rotationSpeed = 0.5f;
    private Quaternion currentRotation,targetRotation;
    private float factor;
    // Use this for initialization
    void Start () {
  
        factor = -1;
        targetRotation = Quaternion.Euler(new Vector3(0f, 0f, transform.rotation.eulerAngles.x + 90));
        InvokeRepeating("GenerateTarget",0f,rotationSpeed);
  
    }
	
	// Update is called once per frame
	void Update () {
        transform.position += Vector3.forward * speed * Time.deltaTime ;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);
    }

    void GenerateTarget()
    {
        targetRotation = Quaternion.Euler(new Vector3(0f, 0f, transform.rotation.eulerAngles.x + (factor *Random.Range(0,90))));
        factor *= -1;
    }


}
