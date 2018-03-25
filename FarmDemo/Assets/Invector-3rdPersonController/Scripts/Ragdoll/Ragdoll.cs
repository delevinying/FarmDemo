using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ragdoll : MonoBehaviour 
{	
	public AudioSource collisionSource;
	public AudioClip collisionClip;

	ThirdPersonController tpController;
	Animator animator;
    private Transform characterChest, characterHips;

	// get up based on the chest velocity magnitude
	float ragdollStabilizedAt = 0.1f;
	bool inStabilize, isActive;

	void Start ()
	{
		// store the Animator component
		animator = GetComponent<Animator>();
		tpController = GetComponent<ThirdPersonController> ();
        // find character chest and hips
        characterChest = animator.GetBoneTransform(HumanBodyBones.Chest);
        characterHips = animator.GetBoneTransform(HumanBodyBones.Hips);

        // set all RigidBodies to kinematic so that they can be controlled with Mecanim
        // and there will be no glitches when transitioning to a ragdoll
        setKinematic(true);
		setCollider (true);
		
		// find all the transforms in the character, assuming that this script is attached to the root
		Component[] components = GetComponentsInChildren(typeof(Transform));
		
		// for each of the transforms, create a BodyPart instance and store the transform 
		foreach (Component c in components)
		{
			BodyPart bodyPart=new BodyPart();
			bodyPart.transform=c as Transform;
			bodyParts.Add(bodyPart);
		}
	}

	void FixedUpdate()
	{
		if(inStabilize)	StartCoroutine(RagdollStabilizer(2f));
	}

	//**********************************************************************************//
	// ACTIVATE RAGDOLL    																//
	// call this method to activate the ragdoll											//   
	//**********************************************************************************//
	public void ActivateRagdoll()
	{	
		if (isActive)
			return;

		isActive = true;
		//Debug.Log("Ragdoll ON");

		animator.SetBool("GetUpFromBack",false);
		animator.SetBool("GetUpFromBelly",false);

		tpController.ragdolled = isActive;
		tpController.capsuleCollider.enabled = false;
		tpController._rigidbody.useGravity = false;
		tpController._rigidbody.isKinematic = true;
		tpController.lockPlayer = true;

		// turn ragdoll on
		ragdolled = true;

		// start to check if the ragdoll is stable
		inStabilize = true;
	}

	//**********************************************************************************//
	// RAGDOLL STABILIZER  																//
	// wait until the ragdoll became stable based on the chest velocity.magnitude		//   
	//**********************************************************************************//
	IEnumerator RagdollStabilizer(float delay) 
	{ 	
		inStabilize = false;
		float rdStabilize = Mathf.Infinity;
		yield return new WaitForSeconds(delay);

		while (rdStabilize > ragdollStabilizedAt) 
		{
			if (animator != null) 
			{								
				rdStabilize = characterChest.GetComponent<Rigidbody> ().velocity.magnitude;
			} else
				break;
			yield return new WaitForEndOfFrame();
		}

		//Debug.Log("Ragdoll Stable");
		ragdolled = false;

		// reset original setup on tpController
		StartCoroutine(ResetPlayer(2f));
	}

	//**********************************************************************************//
	// RESET PLAYER  	  																//
	// restore control to the character													//   
	//**********************************************************************************//
	IEnumerator ResetPlayer(float waitTime) 
	{
		yield return new WaitForSeconds(waitTime);

		//Debug.Log("Ragdoll OFF");
		tpController.lookAtWeight = 0f;
		tpController.lockPlayer = false;
		tpController.verticalVelocity = 0f;

		isActive = false;
		tpController.ragdolled = isActive;
	}

	//**********************************************************************************//
	// RAGDOLL COLLISION SOUND															//
	//**********************************************************************************//
	public void OnRagdollCollisionEnter(RagdollCollision ragdolCollision)
	{
		if (ragdolCollision.ImpactForce > 1)
		{
			collisionSource.clip = collisionClip;
			collisionSource.volume = ragdolCollision.ImpactForce * 0.05f;
			if (!collisionSource.isPlaying)
				collisionSource.Play();
		}
	}

	//**********************************************************************************//
	// RAGDOLL BLEND																	//
	// code based on the script by Perttu Hämäläinen									//
	// with modifications to work with this Controller									//
	//**********************************************************************************//
	bool ragdolled
	{
		get
		{
			return state != RagdollState.animated;
		}
		set
		{
			if (value == true)
			{
				if (state==RagdollState.animated) 
				{
					//Transition from animated to ragdolled
					setKinematic(false); //allow the ragdoll RigidBodies to react to the environment
					setCollider(false); 
					animator.enabled = false; //disable animation
					state=RagdollState.ragdolled;
				} 
			}
			else 
			{
				if (state==RagdollState.ragdolled) 
				{
					//Transition from ragdolled to animated through the blendToAnim state
					setKinematic(true); //disable gravity etc.
					setCollider(true); 
					ragdollingEndTime=Time.time; //store the state change time
					animator.enabled = true; //enable animation
					state=RagdollState.blendToAnim;  
					
					//Store the ragdolled position for blending
					foreach (BodyPart b in bodyParts)
					{
						b.storedRotation=b.transform.rotation;
						b.storedPosition=b.transform.position;
					}
					
					//Remember some key positions
					ragdolledFeetPosition=0.5f*(animator.GetBoneTransform(HumanBodyBones.LeftToes).position + animator.GetBoneTransform(HumanBodyBones.RightToes).position);
					ragdolledHeadPosition=animator.GetBoneTransform(HumanBodyBones.Head).position;
					ragdolledHipPosition=animator.GetBoneTransform(HumanBodyBones.Hips).position;
					
					//Initiate the get up animation
					//hip hips forward vector pointing upwards, initiate the get up from back animation
					if (animator.GetBoneTransform(HumanBodyBones.Hips).forward.y > 0)					
						animator.SetBool("GetUpFromBack", true);
					else					
						animator.SetBool("GetUpFromBelly", true);
				} //if (state==RagdollState.ragdolled)
			}	//if value==false	
		} //set
	} 
	
	//Possible states of the ragdoll
	enum RagdollState
	{
		animated,	 //Mecanim is fully in control
		ragdolled,   //Mecanim turned off, physics controls the ragdoll
		blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
	}
	
	//The current state
	RagdollState state = RagdollState.animated;
	
	//How long do we blend when transitioning from ragdolled to animated
	float ragdollToMecanimBlendTime = 0.2f;
	float mecanimToGetUpTransitionTime = 0.05f;
	
	//A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
	float ragdollingEndTime = -100;
	
	//Declare a class that will hold useful information for each body part
	public class BodyPart
	{
		public Transform transform;
		public Vector3 storedPosition;
		public Quaternion storedRotation;
	}
	//Additional vectores for storing the pose the ragdoll ended up in.
	Vector3 ragdolledHipPosition, ragdolledHeadPosition, ragdolledFeetPosition;
	
	//Declare a list of body parts, initialized in Start()
	List<BodyPart> bodyParts = new List<BodyPart>();

	//A helper function to set the isKinematc property of all RigidBodies in the children of the 
	//game object that this script is attached to
	void setKinematic(bool newValue)
	{
        var _hips = characterHips.GetComponent<Rigidbody>();
        _hips.isKinematic = newValue;
		Component[] components = _hips.transform.GetComponentsInChildren(typeof(Rigidbody));
		
		foreach (Component c in components)		
			(c as Rigidbody).isKinematic = newValue;
	}

	void setCollider(bool newValue)
	{
        var _hips = characterHips.GetComponent<Collider>();
		_hips.isTrigger = newValue;
		Component[] components = _hips.transform.GetComponentsInChildren(typeof(Collider));
		
		foreach (Component c in components)		
			if(!c.transform.Equals(transform))(c as Collider).isTrigger = newValue;
	}

	void LateUpdate()
	{
		if (animator.GetCurrentAnimatorStateInfo (2).IsName ("StandUp@FromBack")) 
		{	
			if (animator.GetCurrentAnimatorStateInfo (2).normalizedTime > 0.1f)	
			{
				tpController._rigidbody.useGravity = true;
				tpController._rigidbody.isKinematic = false;
				tpController.capsuleCollider.enabled = true;
			}
			if (animator.GetCurrentAnimatorStateInfo (2).normalizedTime > 0.9f)	
				animator.SetBool("GetUpFromBack",false);
		}
		if (animator.GetCurrentAnimatorStateInfo (2).IsName ("StandUp@FromBelly")) 
		{	
			if (animator.GetCurrentAnimatorStateInfo (2).normalizedTime > 0.1f)	
			{
				tpController._rigidbody.useGravity = true;
				tpController._rigidbody.isKinematic = false;
				tpController.capsuleCollider.enabled = true;
			}
			if (animator.GetCurrentAnimatorStateInfo (2).normalizedTime > 0.9f)	
				animator.SetBool("GetUpFromBelly",false);
		}

		//Blending from ragdoll back to animated
		if (state == RagdollState.blendToAnim)
		{
			if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime)
			{
				//If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim
				//character to the best match with the ragdoll
				Vector3 animatedToRagdolled = ragdolledHipPosition - animator.GetBoneTransform(HumanBodyBones.Hips).position;
				Vector3 newRootPosition = transform.position + animatedToRagdolled;
				
				//Now cast a ray from the computed position downwards and find the highest hit that does not belong to the character 
				RaycastHit[] hits = Physics.RaycastAll(new Ray(newRootPosition + Vector3.up,Vector3.down)); 
				newRootPosition.y = 0;
				foreach(RaycastHit hit in hits)
				{
					if (!hit.transform.IsChildOf(transform))
					{
						newRootPosition.y = Mathf.Max(newRootPosition.y, hit.point.y);
					}
				}
				transform.position = newRootPosition;
				
				//Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
				Vector3 ragdolledDirection = ragdolledHeadPosition - ragdolledFeetPosition;
				ragdolledDirection.y = 0;
				
				Vector3 meanFeetPosition = 0.5f * (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
				Vector3 animatedDirection = animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
				animatedDirection.y = 0;
				
				//Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
				//hence setting the y components of the vectors to zero. 
				transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized,ragdolledDirection.normalized);
			}
			//compute the ragdoll blend amount in the range 0...1
			float ragdollBlendAmount = 1.0f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime)/ragdollToMecanimBlendTime;
			ragdollBlendAmount = Mathf.Clamp01(ragdollBlendAmount);
			
			//In LateUpdate(), Mecanim has already updated the body pose according to the animations. 
			//To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips 
			//and slerp all the rotations towards the ones stored when ending the ragdolling
			foreach (BodyPart b in bodyParts)
			{
				if (b.transform!=transform)
				{ //this if is to prevent us from modifying the root of the character, only the actual body parts
					//position is only interpolated for the hips
					if (b.transform == animator.GetBoneTransform(HumanBodyBones.Hips))
						b.transform.position = Vector3.Lerp(b.transform.position, b.storedPosition, ragdollBlendAmount);
					//rotation is interpolated for all body parts
					b.transform.rotation = Quaternion.Slerp(b.transform.rotation, b.storedRotation, ragdollBlendAmount);
				}
			}

			//if the ragdoll blend amount has decreased to zero, move to animated state
			if (ragdollBlendAmount == 0)
			{
				state=RagdollState.animated;
				return;
			}
		}
	}

}
