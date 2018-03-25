using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public static class Extensions
{
	/// <summary>
	/// Normalizeds the angle. between -180 and 180 degrees
	/// </summary>
	/// <param Name="eulerAngle">Euler angle.</param>
	public static Vector3 NormalizeAngle(this Vector3 eulerAngle)
	{
		var delta = eulerAngle;

		if (delta.x > 180)		delta.x -= 360;
		else if (delta.x < -180)delta.x += 360;

		if (delta.y > 180)		delta.y -= 360;
		else if (delta.y < -180)delta.y += 360;

		if (delta.z > 180)		delta.z -= 360;
		else if (delta.z < -180)delta.z += 360;

		return new Vector3((int)delta.x,(int)delta.y,(int)delta.z);//round values to angle;
	}

	public static Vector3 Difference(this Vector3 vector, Vector3 otherVector)
	{
		return otherVector -vector;
	}
	public static void SetActiveChildren(this GameObject gameObjet, bool value)
	{
		foreach (Transform child in gameObjet.transform)
			child.gameObject.SetActive (value);
	}

	public static void SetLayerRecursively(this GameObject obj, int layer)
	{
		obj.layer = layer;
		
		foreach (Transform child in obj.transform)
		{
			child.gameObject.SetLayerRecursively(layer);
		}
	}
}
