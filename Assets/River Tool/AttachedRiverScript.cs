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

public class AttachedRiverScript : MonoBehaviour 
{
	public GameObject riverObject;
	MeshCollider riverCollider; 

	public RiverNodeObject[] nodeObjects;
	public Vector3[] nodeObjectVerts;
	public int smoothingLevel;
	public int riverbedTexture;
	public int riverWidth;
	public float riverDepth;
	public int riverSmooth;
	
	public bool finalized;
	// Used to force the river downhill
	public float lowestHeight; 
	public bool showHandles;
	public bool riverNodeMode;
	public GameObject parentTerrain;
	
	// Used to reference terrain cells
	public TerrainCell[] terrainCells;
	public ArrayList riverCells;
	
	public GameObject terrainObject;
	public WaterToolScript waterScript;
	public Terrain terComponent;
	public TerrainData terData;
	public float[,] terrainHeights;
	
	public void Start()
	{
		terrainObject = parentTerrain;
		waterScript = (WaterToolScript)terrainObject.GetComponent("WaterToolScript");
		terComponent = (Terrain) terrainObject.GetComponent(typeof(Terrain));
		
		if(terComponent == null)
			Debug.LogError("This script must be attached to a terrain object - Null reference will be thrown");
			
		terData = terComponent.terrainData;
		terrainHeights = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);
		
		riverSmooth = 15;
		riverNodeMode = false;
		finalized = false;
		showHandles = true;
	}

	public void NewRiver()
	{
		nodeObjects = new RiverNodeObject[0];
		riverCollider = (MeshCollider)riverObject.AddComponent(typeof(MeshCollider));
	}
	
	public void CreateRiverNode(TerrainCell nodeCell)
	{
		Vector3 riverPosition = new Vector3();
		
		riverPosition = new Vector3((nodeCell.position.x/terData.heightmapResolution) * terData.size.x, nodeCell.heightAtCell * terData.size.y , (nodeCell.position.y/terData.heightmapResolution) * terData.size.z);
		
		AddNode(riverPosition, riverWidth);
		CreateMesh(riverSmooth, true);
	}

	public void AddNode(Vector3 position, float width)
	{
		RiverNodeObject newRiverNodeObject = new RiverNodeObject();
		int nNodes;
		
		if(nodeObjects == null)
		{
			nodeObjects = new RiverNodeObject[0];
			nNodes = 1;
			newRiverNodeObject.position = position;
		}
		
		else 
		{
			nNodes = nodeObjects.Length + 1;
			newRiverNodeObject.position = position;
		}

		RiverNodeObject[] newNodeObjects = new RiverNodeObject[nNodes];
		newRiverNodeObject.width = width;
		
		int n = newNodeObjects.Length;
		
		for (int i = 0; i < n; i++) 
		{
			if (i != n - 1) 
			{
				newNodeObjects[i] = nodeObjects[i];
			} 
			
			else 
			{
				newNodeObjects[i] = newRiverNodeObject;
			}
		}
		
		nodeObjects = newNodeObjects;
	}
	
	
	public void CreateMesh(int smoothLevel, bool finalize) 
	{
		lowestHeight = 9999999f;
		MeshFilter meshFilter = (MeshFilter)riverObject.GetComponent(typeof(MeshFilter));
		Mesh newMesh = meshFilter.sharedMesh;
	 
		if (newMesh == null) 
		{
			newMesh = new Mesh();
			newMesh.name = "Generated River Mesh";
			meshFilter.sharedMesh = newMesh;
		} 
	  
		else 
			newMesh.Clear();

		
		if (nodeObjects == null || nodeObjects.Length < 2) 
		{
			return;
		}
		
		int n = nodeObjects.Length;

		int verticesPerNode = 2 * (smoothLevel + 1) * 2;
		int trianglesPerNode = 6 * (smoothLevel + 1);
		int[] newTriangles = new int[(trianglesPerNode * (n - 1))];
		Vector3[] newVertices = new Vector3[(verticesPerNode * (n - 1))];
		Vector2[] uvs = new Vector2[(verticesPerNode * (n - 1))];
		nodeObjectVerts = new Vector3[(verticesPerNode * (n - 1))];
		
		int nextVertex  = 0;
		int nextTriangle = 0;
		int nextUV = 0;

		float[] cubicX = new float[n];
		float[] cubicZ = new float[n];
		
		Vector3 handle1Tween = new Vector3();
		
		Vector3[] g1 = new Vector3[smoothLevel+1];
		Vector3[] g2 = new Vector3[smoothLevel+1];
		Vector3[] g3 = new Vector3[smoothLevel+1];
		Vector3 oldG2 = new Vector3();
		Vector3 extrudedPointL = new Vector3();
		Vector3 extrudedPointR = new Vector3();
		
		for(int i = 0; i < n; i++)
		{
			cubicX[i] = nodeObjects[i].position.x;
			cubicZ[i] = nodeObjects[i].position.z;
		}
		
		for (int i = 0; i < n; i++) 
		{
			g1 = new Vector3[smoothLevel+1];
			g2 = new Vector3[smoothLevel+1];
			g3 = new Vector3[smoothLevel+1];
			
			extrudedPointL = new Vector3();
			extrudedPointR = new Vector3();
			
			if (i == 0)
			{
				newVertices[nextVertex] = nodeObjects[0].position;
				nextVertex++;
				uvs[0] = new Vector2(0f, 1f);
				nextUV++;
				newVertices[nextVertex] = nodeObjects[0].position;
				nextVertex++;
				uvs[1] = new Vector2(1f, 1f);
				nextUV++;
				
				continue;
			}
			
			// Interpolate the points on the path using natural cubic splines
			for (int j = 0; j < smoothLevel + 1; j++) 
			{
				// clone the vertex for uvs
				if(i == 1)
				{
					if(j != 0)
					{
						newVertices[nextVertex] = newVertices[nextVertex-2];
						nextVertex++;
						
						newVertices[nextVertex] = newVertices[nextVertex-2];
						nextVertex++;
						
						uvs[nextUV] = new Vector2(0f, 1f);
						nextUV++;
						uvs[nextUV] = new Vector2(1f, 1f);
						nextUV++;
					}
					
					else
						oldG2 = nodeObjects[0].position;
				}
				
				else
				{
					newVertices[nextVertex] = newVertices[nextVertex-2];
					nextVertex++;
					
					newVertices[nextVertex] =newVertices[nextVertex-2];
					nextVertex++;
					
					uvs[nextUV] = new Vector2(0f, 1f);
					nextUV++;
					uvs[nextUV] = new Vector2(1f, 1f);
					nextUV++;
				}
				
				float u = (float)(j+1)/(float)(smoothLevel+1f);
	
				Cubic[] X = calcNaturalCubic(n-1, cubicX);
				Cubic[] Z = calcNaturalCubic(n-1, cubicZ);
				
				Vector3 tweenPoint = new Vector3(X[i-1].eval(u), 0f, Z[i-1].eval(u));
				
				float _widthAtNode = riverWidth;
				
				g2[j] = tweenPoint;
				g1[j] = oldG2;
				g3[j] = g2[j] - g1[j];
				oldG2 = g2[j];
				
				extrudedPointL = new Vector3(-g3[j].z, 0, g3[j].x);
				extrudedPointR = new Vector3(g3[j].z, 0, -g3[j].x);
				
				extrudedPointL.Normalize();
				extrudedPointR.Normalize();
				extrudedPointL *= _widthAtNode;
				extrudedPointR *= _widthAtNode;    
				
				tweenPoint.y = terrainHeights[(int)(((float)(tweenPoint.z-parentTerrain.transform.position.z)/terData.size.z) * terData.heightmapResolution), (int)(((float)(tweenPoint.x-parentTerrain.transform.position.x)/terData.size.x) * terData.heightmapResolution)] * terData.size.y + parentTerrain.transform.position.y;

				// used to update the handles
				if(i == 1)
				{
					if(j == 0)
					{
						handle1Tween = tweenPoint;
					}
				}
				
				newVertices[nextVertex] = tweenPoint + extrudedPointR;
				newVertices[nextVertex].y = tweenPoint.y;
				nextVertex++;
				
				newVertices[nextVertex] = tweenPoint + extrudedPointL;
				newVertices[nextVertex].y = tweenPoint.y;
				nextVertex++;

				uvs[nextUV] = new Vector2(0f, 0f);
				nextUV++;
				uvs[nextUV] = new Vector2(1f, 0f);
				nextUV++;

				// flatten and force downhill
				if(newVertices[nextVertex-1].y > (newVertices[nextVertex-2].y-0.2f))
				{
					newVertices[nextVertex-1].y = newVertices[nextVertex-2].y;
					
					if(newVertices[nextVertex-1].y > lowestHeight)
					{
						newVertices[nextVertex-1].y = lowestHeight;
						newVertices[nextVertex-2].y = lowestHeight;
					}
					
					else
						lowestHeight = newVertices[nextVertex-1].y;
				}
			
				else
				{
					newVertices[nextVertex-2].y = newVertices[nextVertex-1].y;
				
					if(newVertices[nextVertex-1].y > lowestHeight)
					{
						newVertices[nextVertex-1].y = lowestHeight;
						newVertices[nextVertex-2].y = lowestHeight;
					}
					
					else
						lowestHeight = newVertices[nextVertex-1].y;
				}		
				
				// Create triangles...
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j); // 0
				nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) + 1; // 1
				nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) + 2; // 2
				nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) + 1; // 1
				nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) + 3; // 3
				nextTriangle++;
				newTriangles[nextTriangle] = (verticesPerNode * (i - 1)) + (4 * j) + 2; // 2
				nextTriangle++;
			}
		}

		// update handles
		g2[0] = handle1Tween;
		g1[0] = nodeObjects[0].position;
		g3[0] = g2[0] - g1[0];
	
		extrudedPointL = new Vector3(-g3[0].z, 0, g3[0].x);
		extrudedPointR = new Vector3(g3[0].z, 0, -g3[0].x);
		
		extrudedPointL.Normalize();
		extrudedPointR.Normalize();
		extrudedPointL *= nodeObjects[0].width;
		extrudedPointR *= nodeObjects[0].width;
		
		newVertices[0] = nodeObjects[0].position + extrudedPointR;
		newVertices[0].y = newVertices[2].y; 
		newVertices[1] = nodeObjects[0].position + extrudedPointL;
		newVertices[1].y = newVertices[3].y;
		
		for(int i = 0; i <newVertices.Length; i++)
		{
			nodeObjectVerts[i] = newVertices[i];
		}
		
		newMesh.vertices = newVertices;
		newMesh.triangles = newTriangles;
		
		newMesh.uv =  uvs;
		
		Vector3[] myNormals = new Vector3[newMesh.vertexCount];
		for(int p = 0; p < newMesh.vertexCount; p++)
		{
			myNormals[p] = Vector3.up;
		}
		
		newMesh.normals = myNormals;
		
		TangentSolver(newMesh);

//		newMesh.RecalculateNormals();
		newMesh.Optimize();
		riverCollider.sharedMesh = newMesh;
//		  riverCollider.convex = true;
	}
	
	public void FinalizeRiver()
	{
		Vector3 returnCollision = new Vector3();
		riverCells = new ArrayList();
		
		float[,] tempLRheightmap = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);
		float[,,] LRsplatmap = terData.GetAlphamaps(0, 0, terData.alphamapResolution,  terData.alphamapResolution);
		
		riverWidth += 1;
	
		CreateMesh(riverSmooth, true);
		
		transform.Translate(new Vector3(0f, riverDepth, 0f));
		transform.localScale = new Vector3(1,1,1);
		
		bool minimumTerrainBreach = false;
		
		foreach(TerrainCell tC in terrainCells)
		{
		  RaycastHit raycastHit = new RaycastHit();
		  Ray riverRay = new Ray(new Vector3((float)((tC.position.x * terData.size.x)/terData.heightmapResolution) + parentTerrain.transform.position.x, (float)tC.heightAtCell * terData.size.y + parentTerrain.transform.position.y, (float)((tC.position.y* terData.size.z)/terData.heightmapResolution) + parentTerrain.transform.position.z), -Vector3.up);

		  if(riverCollider.Raycast(riverRay, out raycastHit, Mathf.Infinity))
		  {	  
			returnCollision = raycastHit.point;
			
			if(returnCollision.y < parentTerrain.transform.position.y)
				minimumTerrainBreach = true;
				
			returnCollision.y = (returnCollision.y - parentTerrain.transform.position.y)/terData.size.y;
			
			tempLRheightmap[(int)(tC.position.y), (int)(tC.position.x)] = returnCollision.y;

			riverCells.Add(tC);
		  }
		}
		
		if(minimumTerrainBreach)
			Debug.LogError("River depth is below minimum terrain height of 0. Resulting river may be below terrain and need to be manually moved up");
		
		/* This has been disabled in Unity 3.0 :((
		
		// Remove trees and details from path
		int detailRadius = (int)((float)terData.detailResolution/(terData.heightmapResolution-1));
		
		foreach(TerrainCell tC in riverCells)
		{
			for(int i = 0; i < terData.detailPrototypes.Length; i++)
			{
				int[,] detailLayer = terData.GetDetailLayer(0, 0, terData.detailResolution, terData.detailResolution, i);

				for(int j = -detailRadius+1; j < detailRadius; j++)
				{
					for(int k = -detailRadius+1; k < detailRadius; k++)
					{
						detailLayer[(int)(((float)tC.position.y/terData.heightmapResolution) * terData.detailResolution) + j, (int)(((float)tC.position.x/terData.heightmapResolution) * terData.detailResolution) + k] = 0;
					}
				}
				terData.SetDetailLayer(0, 0, i, detailLayer);
			}
			
			for(int i = 0; i < terData.treePrototypes.Length; i++)
			{
				terComponent.RemoveTrees(new Vector3(((float)tC.position.x/terData.heightmapResolution), ((float)tC.position.y/terData.heightmapResolution)), ((float)10f/terData.size.x), i);
			}
		}

		terData.RecalculateTreePositions();
		
		*/
		
		for(int i = 0; i < nodeObjects.Length; i++)
		{
		  riverWidth += 2;
		}
		
		transform.Translate(new Vector3(0f, 1.5f, 0f));
		
		if(terData.alphamapLayers > riverbedTexture)
		{
			foreach(TerrainCell tC in riverCells)
			{
				for(int i = 0; i < terData.alphamapLayers; i++)
				{
					LRsplatmap[(int)((float)(tC.position.y/terData.heightmapResolution) * terData.alphamapResolution), (int)((float)(tC.position.x/terData.heightmapResolution) * terData.alphamapResolution), i] = 0.0f;
				}
				
				LRsplatmap[(int)((float)(tC.position.y/terData.heightmapResolution) * terData.alphamapResolution), (int)((float)(tC.position.x/terData.heightmapResolution) * terData.alphamapResolution), riverbedTexture] = 1.0f;
			}
			terData.SetAlphamaps(0, 0, LRsplatmap);
		}
		
		else
			Debug.Log("Riverbed prototype out of range. Not applying texture");
		
		terData.SetHeights(0, 0, tempLRheightmap);
		
		CreateMesh(riverSmooth, true);
		
		
		finalized = true;
		
		Debug.Log("Now that you have finalized your road, it is advised that you delete the 'Attached River Script' component of this game object to avoid corrupting it in the future");
	}
	
	public void CloseRiver()
	{
		MeshFilter meshFilter = (MeshFilter)riverObject.GetComponent(typeof(MeshFilter));
		Mesh newMesh = meshFilter.sharedMesh;
		Vector3[] newVertices = newMesh.vertices;
		TerrainCollider terCollider = (TerrainCollider)terrainObject.GetComponent(typeof(TerrainCollider));
		
		for(int i = 0; i < newVertices.Length; i++)
		{
			Ray riverRay = new Ray(new Vector3(newVertices[i].x, newVertices[i].y + transform.position.y, newVertices[i].z), -Vector3.up);
			RaycastHit raycastHit = new RaycastHit();
			
			if(terCollider.Raycast(riverRay, out raycastHit, Mathf.Infinity))
			{
				newVertices[i].y = (raycastHit.point.y - 1.0f) - transform.position.y;
			}
		}
		
		newMesh.vertices = newVertices;
	}

	public void AreaSmooth(ArrayList terrainList, float blendAmount)
	{
		TerrainCell tc;
		TerrainCell lh;
		
		float[,] blendLRheightmap = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);
		
		foreach(TerrainCell tC in terrainList)
		{		
			terrainCells[Convert.ToInt32((tC.position.y) + ((tC.position.x) * (terData.heightmapResolution)))].isAdded = true;	
		}
		
		for(int i = 0; i < terrainList.Count; i++)
		{
			tc = (TerrainCell)terrainList[i];
			ArrayList locals = new ArrayList();
			
			for(int x = 7; x > -8; x--)
			{
				for(int y =  7; y > -8; y--)
				{				
					if(terrainCells[Convert.ToInt32((tc.position.y + y) + ((tc.position.x+x) * (terData.heightmapResolution)))].isAdded == false)
					{
						locals.Add(terrainCells[Convert.ToInt32((tc.position.y + y) + ((tc.position.x+x) * (terData.heightmapResolution)))]);
						terrainCells[Convert.ToInt32((tc.position.y + y) + ((tc.position.x+x) * (terData.heightmapResolution)))].isAdded = true;
					}
				}
			}
				
			for(int p = 0; p < locals.Count; p++)
			{
				lh = (TerrainCell)locals[p];
				ArrayList localHeights = new ArrayList();
				float cumulativeHeights = 0f;
				
				// Get all immediate neighbors 
				for(int x = 1; x > -2; x--)
				{
					for(int y =  1; y > -2; y--)
					{
						localHeights.Add(terrainCells[Convert.ToInt32((lh.position.y + y) + ((lh.position.x+x) * (terData.heightmapResolution)))]);
					}
				}
				
				foreach(TerrainCell lH in localHeights)
				{
					cumulativeHeights += blendLRheightmap[(int)lH.position.y, (int)lH.position.x];
				}

				blendLRheightmap[(int)(lh.position.y), (int)(lh.position.x)] = (terrainHeights[(int)lh.position.y, (int)lh.position.x] * (1f-blendAmount)) + ( ((float)cumulativeHeights/((Mathf.Pow(((float)1f*2f + 1f), 2f)) - 0f)) * blendAmount);
			}
		}
		
		terData.SetHeights(0, 0, blendLRheightmap);
	}

	
	public void OnDrawGizmos()
	{
		if(showHandles)
		{
			if(nodeObjectVerts != null)
				if (nodeObjectVerts.Length > 0) 
				{
					int n = nodeObjectVerts.Length;
					for (int i = 0; i < n; i++) 
					{
						// Handles...
						Gizmos.color = Color.white;
						
						Gizmos.DrawLine(transform.TransformPoint(nodeObjectVerts[i] + new Vector3(-0.5f, 0, 0)), transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0.5f, 0, 0)));
						Gizmos.DrawLine(transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0, -0.5f, 0)), transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0, 0.5f, 0)));
						Gizmos.DrawLine(transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0, 0, -0.5f)), transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0, 0, 0.5f)));
					}
				}
		}
	}
	
	/*
	Derived from
	Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. Terathon Software 3D Graphics Library, 2001.
	http://www.terathon.com/code/tangent.html
	*/

	public void TangentSolver(Mesh theMesh)
    {
        int vertexCount = theMesh.vertexCount;
        Vector3[] vertices = theMesh.vertices;
        Vector3[] normals = theMesh.normals;
        Vector2[] texcoords = theMesh.uv;
        int[] triangles = theMesh.triangles;
        int triangleCount = triangles.Length/3;
        Vector4[] tangents = new Vector4[vertexCount];
        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];
        int tri = 0;
		
		int i1, i2, i3;
		Vector3 v1, v2, v3, w1, w2, w3, sdir, tdir;
		float x1, x2, y1, y2, z1, z2, s1, s2, t1, t2, r;
        for (int i = 0; i < (triangleCount); i++)
        {
            i1 = triangles[tri];
            i2 = triangles[tri+1];
            i3 = triangles[tri+2];

            v1 = vertices[i1];
            v2 = vertices[i2];
            v3 = vertices[i3];

            w1 = texcoords[i1];
            w2 = texcoords[i2];
            w3 = texcoords[i3];

            x1 = v2.x - v1.x;
            x2 = v3.x - v1.x;
            y1 = v2.y - v1.y;
            y2 = v3.y - v1.y;
            z1 = v2.z - v1.z;
            z2 = v3.z - v1.z;

            s1 = w2.x - w1.x;
            s2 = w3.x - w1.x;
            t1 = w2.y - w1.y;
            t2 = w3.y - w1.y;

            r = 1.0f / (s1 * t2 - s2 * t1);
            sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;

            tri += 3;
        }
		
        for (int i = 0; i < (vertexCount); i++)
        {
            Vector3 n = normals[i];
            Vector3 t = tan1[i];

            Vector3.OrthoNormalize(ref n, ref t);

            tangents[i].x  = t.x;
            tangents[i].y  = t.y;
            tangents[i].z  = t.z;

            tangents[i].w = ( Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f ) ? -1.0f : 1.0f;
        }       
		
        theMesh.tangents = tangents;
    }
	
	/* Derived from original Java source 
	Tim Lambert                                       
	School of Computer Science and Engineering         
	The University of New South Wales                
	*/
	
	public class Cubic
	{
	  float a,b,c,d;        

	  public Cubic(float a, float b, float c, float d){
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
	  }
	  
	  public float eval(float u) 
	  {
		return (((d*u) + c)*u + b)*u + a;
	  }
	}
	
	public Cubic[] calcNaturalCubic(int n, float[] x) 
	{
		float[] gamma = new float[n+1];
		float[] delta = new float[n+1];
		float[] D = new float[n+1];
		int i;
    
		gamma[0] = 1.0f/2.0f;
		
		for ( i = 1; i < n; i++) 
		{
		  gamma[i] = 1/(4-gamma[i-1]);
		}
		
		gamma[n] = 1/(2-gamma[n-1]);
		
		delta[0] = 3*(x[1]-x[0])*gamma[0];
		
		for ( i = 1; i < n; i++) 
		{
		  delta[i] = (3*(x[i+1]-x[i-1])-delta[i-1])*gamma[i];
		}
		
		delta[n] = (3*(x[n]-x[n-1])-delta[n-1])*gamma[n];
		
		D[n] = delta[n];
		
		for ( i = n-1; i >= 0; i--) 
		{
		  D[i] = delta[i] - gamma[i]*D[i+1];
		}

		Cubic[] C = new Cubic[n+1];
		for ( i = 0; i < n; i++) {
		  C[i] = new Cubic((float)x[i], D[i], 3*(x[i+1] - x[i]) - 2*D[i] - D[i+1],
				   2*(x[i] - x[i+1]) + D[i] + D[i+1]);
		}
		
		return C;
	}
}