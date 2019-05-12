using NurbsUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOnCurve : MonoBehaviour {

    public float[] curveParameter;
    public GameObject objectToInstantiate;
    public Curve curve;

	// Use this for initialization
	void Start () {
		for (int i=0; i<curveParameter.Length; i++)
        {
            GameObject obj = Instantiate(objectToInstantiate, transform);
            obj.name = "Object @ " + curveParameter[i];
            Plane plane = new Plane();
            Vector3 position = curve.PointOnCurve(curveParameter[i], out plane);
            obj.transform.position = position;
            obj.transform.LookAt(position + plane.normal);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
