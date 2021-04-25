using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleLight : MonoBehaviour
{
	private Light myLight;
	private bool lightEnabled;

    // Start is called before the first frame update
    void Start()
    {
		myLight = GetComponent<Light>();
		if (myLight)
		{
			lightEnabled = myLight.isActiveAndEnabled;
		}
    }

    // Update is called once per frame
    void Update()
    {
        if (myLight && Input.GetMouseButtonDown(0))
		{
			lightEnabled = !lightEnabled;
			myLight.enabled = lightEnabled;
		}
    }
}
