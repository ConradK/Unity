/* Written for "Dawn of the Tyrant" by SixTimesNothing 
/* Please visit www.sixtimesnothing.com to learn more
/*
/* Note: This code is being released under the Artistic License 2.0
/* Refer to the readme.txt or visit http://www.perlfoundation.org/artistic_license_2_0
/* Basically, you can use this for anything you want but if you plan to change
/* it or redistribute it, you should read the license
*/

using UnityEngine;
using System;
using System.Collections;

[ExecuteInEditMode()]

public struct TerrainCell
{
	public float heightAtCell;
	public Vector2 position;
	public bool isAdded;
};

public class RiverNodeObject 
{
	public Vector3 position;
	public float width;
}

public class WaterToolScript : MonoBehaviour 
{
	public TerrainCell[] terrainCells;
	
	public void Start()
	{
		Terrain terComponent = (Terrain) gameObject.GetComponent(typeof(Terrain));
		if(terComponent == null)
			Debug.LogError("This script must be attached to a terrain object - Null reference will be thrown");	
	}
	
	public void CreateRiver()
	{
		GameObject riverObject = new GameObject();
		riverObject.name = "River";
		riverObject.AddComponent(typeof(MeshFilter));
		riverObject.AddComponent(typeof(MeshRenderer));
		riverObject.AddComponent("AttachedRiverScript");
		
		AttachedRiverScript ARS = (AttachedRiverScript)riverObject.GetComponent("AttachedRiverScript");
		ARS.riverObject = riverObject;
		ARS.parentTerrain = gameObject;
		ARS.NewRiver();
	}
}