using UnityEngine;
using System.Collections;

public class FootStepTrigger : MonoBehaviour 
{
	private bool enable;

	void Start()
	{
		enable = true;
	}

	void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag ("Player") && enable) 
		{
			enable = false;

			if(other.GetComponent<Terrain>() != null)			
				transform.root.SendMessage("StepOnTerrain", SendMessageOptions.DontRequireReceiver);
			else
			{
				var renderer = other.GetComponent<Renderer>();
				
				if(renderer != null && renderer.material.mainTexture != null)				
				{
					var _name = renderer.material.mainTexture.name;
					transform.root.SendMessage("StepOnMesh", _name, SendMessageOptions.DontRequireReceiver);
				}
			}
			Invoke("Enable", 0.025f);
		}
	}

	void Enable()
	{
		enable = true;
	}
}
