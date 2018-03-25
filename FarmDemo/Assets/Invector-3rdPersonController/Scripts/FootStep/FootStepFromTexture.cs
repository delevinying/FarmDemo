﻿using UnityEngine;
using System.Collections;

public class FootStepFromTexture : FootPlantingPlayer 
{
	public float colliderRadius = 0.1f;
	public bool debugTextureName;

	private int surfaceIndex = 0;	
	private Terrain terrain;
	private TerrainData terrainData;
	private Vector3 terrainPos;

	public SphereCollider leftFootTrigger;
	public SphereCollider rightFootTrigger;

	void Start () 
	{		
		if (terrain == null) 
		{
			terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                terrainData = terrain.terrainData;
                terrainPos = terrain.transform.position;
            }
		}
	}
	
	private float[] GetTextureMix(Vector3 WorldPos)
	{
		// returns an array containing the relative mix of textures
		// on the main terrain at this world position.
		
		// The number of values in the array will equal the number
		// of textures added to the terrain.
		
		// calculate which splat map cell the worldPos falls within (ignoring y)
		int mapX = (int)(((WorldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((WorldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
		
		// get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
		float[,,] splatmapData = terrainData.GetAlphamaps( mapX, mapZ, 1, 1 );
		
		// extract the 3D array data to a 1D array:
		float[] cellMix = new float[ splatmapData.GetUpperBound(2) + 1 ];
		
		for(int n=0; n<cellMix.Length; n++){
			cellMix[n] = splatmapData[ 0, 0, n ];
		}
		return cellMix;
	}
	
	private int GetMainTexture(Vector3 WorldPos)
	{
		// returns the zero-based index of the most dominant texture
		// on the main terrain at this world position.
		float[] mix = GetTextureMix(WorldPos);
		
		float maxMix = 0;
		int maxIndex = 0;
		
		// loop through each mix value and find the maximum
		for(int n=0; n<mix.Length; n++)
		{
			if ( mix[n] > maxMix )
			{
				maxIndex = n;
				maxMix = mix[n];
			}
		}
		return maxIndex;
	}

	public void StepOnTerrain()
	{
        if(terrainData)
		surfaceIndex = GetMainTexture(transform.position);

        var name = terrainData != null? terrainData.splatPrototypes[surfaceIndex].texture.name:"";
		PlayFootFallSound (name);

		if (debugTextureName)
			Debug.Log (name);
	}

	public void StepOnMesh(string tag)
	{
		PlayFootFallSound (tag);
		if (debugTextureName)
			Debug.Log (tag);
	}
}
