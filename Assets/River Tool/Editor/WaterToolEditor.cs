/* Written for "Dawn of the Tyrant" by SixTimesNothing 
/* Please visit www.sixtimesnothing.com to learn more
/*
/* Note: This code is being released under the Artistic License 2.0
/* Refer to the readme.txt or visit http://www.perlfoundation.org/artistic_license_2_0
/* Basically, you can use this for anything you want but if you plan to change
/* it or redistribute it, you should read the license
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor(typeof(WaterToolScript))]

public class WaterToolEditor : Editor 
{
	public GameObject terrainObject;
	public WaterToolScript waterScript;
	public Terrain terComponent;
	public TerrainData terData;
	public float[,] terrainHeights;
	
	public void Awake()
	{
		waterScript = (WaterToolScript)target as WaterToolScript;
		terComponent = (Terrain) waterScript.GetComponent(typeof(Terrain));
		if(terComponent == null)
			Debug.LogError("This script must be attached to a terrain object - Null reference will be thrown");
		terData = terComponent.terrainData;
		terrainHeights = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);
	}
	
	public void OnSceneGUI()
	{
		
	}
	
	public override void OnInspectorGUI() 
	{
		EditorGUIUtility.LookLikeControls();
		
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		Rect createRiverButton = EditorGUILayout.BeginHorizontal();
		createRiverButton.x = createRiverButton.width / 2 - 100;
		createRiverButton.width = 200;
		createRiverButton.height = 18;
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
					
		if (GUI.Button(createRiverButton, "New River")) 
		{		
			waterScript.CreateRiver();
		}
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		
		if (GUI.changed) 
		{
			EditorUtility.SetDirty(waterScript);
		}
	}
}