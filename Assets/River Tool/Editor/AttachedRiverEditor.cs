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

[CustomEditor(typeof(AttachedRiverScript))]

public class AttachedRiverEditor : Editor 
{
	public AttachedRiverScript riverScript;
	
	void Awake ()
	{
		 riverScript = (AttachedRiverScript) target as AttachedRiverScript;
	}
	
	void OnSceneGUI() 
	{
		Event currentEvent = Event.current;
		
		if (riverScript.nodeObjects != null && riverScript.nodeObjects.Length != 0 && !riverScript.finalized) 
		{
			int n = riverScript.nodeObjects.Length;
			for (int i = 0; i < n; i++) 
			{
				RiverNodeObject node = riverScript.nodeObjects[i];
				node.position = Handles.PositionHandle(node.position, Quaternion.identity);
			}
		}
	
		if(riverScript.riverNodeMode == true)
		{
			if(currentEvent.isKey && currentEvent.character == 'r')
			{
				Vector3 riverNode = GetTerrainCollisionInEditor(currentEvent, KeyCode.R);
				
				TerrainCell riverNodeCell = new TerrainCell();
				riverNodeCell.position.x = riverNode.x;
				riverNodeCell.position.y = riverNode.z;
				riverNodeCell.heightAtCell = riverNode.y;
				
				riverScript.CreateRiverNode(riverNodeCell);
				riverScript.riverNodeMode = false;
			}
		}
		
		 if (GUI.changed) 
		{
			if(!riverScript.finalized)
			{
				EditorUtility.SetDirty(riverScript);
				riverScript.CreateMesh(riverScript.riverSmooth, false);
			}
		}

	}
	
	public override void OnInspectorGUI() 
	{
		if(riverScript.terrainObject != null)
		{
			Terrain terComponent = (Terrain) riverScript.terrainObject.GetComponent(typeof(Terrain));
			TerrainData terData = terComponent.terrainData;
			
			if(!riverScript.finalized)
			{
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.PrefixLabel("Show handles");
				riverScript.showHandles = EditorGUILayout.Toggle(riverScript.showHandles);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				riverScript.riverbedTexture = (int) EditorGUILayout.IntSlider("Riverbed Texture Prototype", riverScript.riverbedTexture, 0, 30);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Separator();
				
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				riverScript.riverWidth = (int) EditorGUILayout.IntSlider("River Width", riverScript.riverWidth, 2, 30);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				riverScript.riverDepth = EditorGUILayout.Slider("River Depth", riverScript.riverDepth, -10f, -30f);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				riverScript.riverSmooth = (int) EditorGUILayout.IntSlider("Mesh Smooth", riverScript.riverSmooth, 5, 30);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				Rect createRiverNodeButton = EditorGUILayout.BeginHorizontal();
				createRiverNodeButton.x = createRiverNodeButton.width / 2 - 100;
				createRiverNodeButton.width = 200;
				createRiverNodeButton.height = 18;
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
							
				if (GUI.Button(createRiverNodeButton, "Add River Node")) 
				{	
					if(!riverScript.finalized)
					{
						// define terrain cells
						riverScript.terrainCells = new TerrainCell[terData.heightmapResolution * terData.heightmapResolution];

						for(int x = 0; x < terData.heightmapResolution; x++)
						{
							for(int y = 0; y < terData.heightmapResolution; y++)
							{
									riverScript.terrainCells[ (y) + (x* terData.heightmapResolution)].position.y = y;
									riverScript.terrainCells[ (y) + (x* terData.heightmapResolution)].position.x = x;
									riverScript.terrainCells[ (y) + (x* terData.heightmapResolution)].heightAtCell = riverScript.terrainHeights[y, x]; 
									riverScript.terrainCells[ (y) + (x* terData.heightmapResolution)].isAdded = false;
							}
						}
						
						riverScript.riverNodeMode = true;
					}
					
					else
						Debug.Log("River has been finalized");
				}
				
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				
				Rect finalizeRiverButton = EditorGUILayout.BeginHorizontal();
				finalizeRiverButton.x = finalizeRiverButton.width / 2 - 100;
				finalizeRiverButton.width = 200;
				finalizeRiverButton.height = 18;
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
							
				if (GUI.Button(finalizeRiverButton, "Finalize River")) 
				{	
					if(!riverScript.finalized)
					{
						if(riverScript.nodeObjects.Length > 1)
						{
							Undo.RegisterUndo (terData, "Undo Finalize");
							riverScript.FinalizeRiver();
						}
								
						
						else
						{
							Debug.Log("Not enough node objects");
						}
					}
					
					else
						Debug.Log("River has been finalized");
				}
				
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
			}
			
			if(riverScript.finalized)
			{
				Rect smoothRiverButton = EditorGUILayout.BeginHorizontal();
				smoothRiverButton.x = smoothRiverButton.width / 2 - 100;
				smoothRiverButton.width = 200;
				smoothRiverButton.height = 18;
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
							
				if (GUI.Button(smoothRiverButton, "Smooth River")) 
				{	
					if(riverScript.finalized)
					{
						Undo.RegisterUndo (terData, "Undo River Smooth");
						
						riverScript.AreaSmooth(riverScript.riverCells, 1.0f);
						
						foreach(TerrainCell tc in riverScript.terrainCells)
							riverScript.terrainCells[Convert.ToInt32((tc.position.y) + ((tc.position.x) * (terData.heightmapResolution)))].isAdded = false;
					}
					
					else
						Debug.Log("River has not been finalized");
				}
				
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				
				Rect closeRiverButton = EditorGUILayout.BeginHorizontal();
				closeRiverButton.x = closeRiverButton.width / 2 - 100;
				closeRiverButton.width = 200;
				closeRiverButton.height = 18;
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
							
				if (GUI.Button(closeRiverButton, "Close River")) 
				{	
					if(riverScript.finalized)
					{
						MeshFilter meshFilter = (MeshFilter)riverScript.riverObject.GetComponent(typeof(MeshFilter));
						Undo.RegisterUndo (meshFilter.sharedMesh, "Undo River Close");
						
						riverScript.CloseRiver();
					}
					
					else
						Debug.Log("River has not been finalized");
				}
				
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
				EditorGUILayout.Separator();
			}
		}
		
		 if (GUI.changed) 
		{
			EditorUtility.SetDirty(riverScript);
			riverScript.CreateMesh(riverScript.riverSmooth, false);
		}
	}
	
	// This is a method that returns a point on the terrain that has been selected with the mouse when pressing a certain key
	public Vector3 GetTerrainCollisionInEditor(Event currentEvent, KeyCode keysCode)
	{
		Vector3 returnCollision = new Vector3();
		
		AttachedRiverScript riverScript = (AttachedRiverScript) target as AttachedRiverScript;
		
		Camera SceneCameraReceptor = new Camera();
		
		GameObject terrain = riverScript.parentTerrain;
		Terrain terComponent = (Terrain) terrain.GetComponent(typeof(Terrain));
		TerrainCollider terCollider = (TerrainCollider)terrain.GetComponent(typeof(TerrainCollider));
		TerrainData terData = terComponent.terrainData;
		
		Ray terrainRay = new Ray();
		
		if(Camera.current != null)
		{
			SceneCameraReceptor = Camera.current;
		
			RaycastHit raycastHit = new RaycastHit();
			
			Vector2 newMousePosition = new Vector2(currentEvent.mousePosition.x, Screen.height - (currentEvent.mousePosition.y + 25));
			
			terrainRay = SceneCameraReceptor.ScreenPointToRay(newMousePosition);
			
			if(terCollider.Raycast(terrainRay, out raycastHit, Mathf.Infinity))
			{
				returnCollision = raycastHit.point;
				returnCollision.x = Mathf.RoundToInt((returnCollision.x/terData.size.x) * terData.heightmapResolution);
				returnCollision.y = returnCollision.y/terData.size.y;
				returnCollision.z = Mathf.RoundToInt((returnCollision.z/terData.size.z) * terData.heightmapResolution);
			}
			
			else
				Debug.LogError("Error: No collision with terrain to create node");
		}
		
		return returnCollision;
	}
	
}