using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NurbsUnity;
using System;

public class PointOnSurface : MonoBehaviour {
    public Surface surface;
    public float uCoordinate = 0.5f;
    public float vCoordinate = 0.5f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmos()
    {
        try
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(surface.PointOnSurface(uCoordinate, vCoordinate), Vector3.one);
        }
        catch(Exception e)
        {

        }
    }
}
