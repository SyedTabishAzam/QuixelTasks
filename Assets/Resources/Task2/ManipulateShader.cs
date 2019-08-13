using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManipulateShader : MonoBehaviour {
    public InputField iField;
    Renderer rend;
    // Use this for initialization
    void Start () {

        rend = GetComponent<Renderer>();
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RadiusChange(int highlow)
    {
        //This function allows changing the radius at runtime

        //It gets the radius value from shader and increase or decrease it accordingly
        float value = rend.material.GetFloat("_Radius");
        if(highlow==0)
        {
            value += 5;
        }
        if(highlow==1)
        {
            value -= 5;
        }
        rend.material.SetFloat("_Radius", value);
    }

    public void WidthChange(bool enter)
    {
        //Function to change the width of circle at runtime
        if(enter)
        {
            //Get value entered in input field and apply to shader
            float value = float.Parse(iField.text);
            rend.material.SetFloat("_LineWidth", value);
        }
    }
}
