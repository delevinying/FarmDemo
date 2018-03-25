using UnityEngine;
using System.Collections;

public class ObjectDamage : MonoBehaviour 
{
	public int damage;

	void OnCollisionEnter(Collision hit)
	{
		if(hit.collider.CompareTag("Player"))
		{
			// apply damage to PlayerHealth
			hit.transform.root.SendMessage ("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
			// activate the Ragdoll 
			hit.transform.root.SendMessage ("ActivateRagdoll", SendMessageOptions.DontRequireReceiver);
		}
	}
}