using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ThirdPersonMotor : Character
{

    #region Character Variables    
    [Header("--- Locomotion Setup ---")]

    [Tooltip("The character Head will follow where you look at")]
    public bool headTracking = true;

    [Tooltip("Press the button to crouch or just hit the button once")]
	public bool pressToCrouch = true;

	[Tooltip("Speed of the rotation on free directional movement")]
	public float rotationSpeed = 8f;
    	
	[Tooltip("Adjust the GREEN Raycast until you see it above the head")]
    // press play and adjust slightly above the head, for auto crouch
    public float headDetect = 0.9f;
        
    [Tooltip("Put your Random Idle animations at the AnimatorController and select value to randomize, 0 is disable.")]
    public float randomIdleTime = 0f;

    [Header("--- Grounded Setup ---")]

	[Tooltip("Distance to became not grounded")]
	public float groundCheckDistance = 0.5f;
	[HideInInspector] public float groundDistance;
	
	[Tooltip("Offset height limit for sters - GREY Raycast in front of the legs")]
	public float stepOffsetLimit = 0.35f;

    [Tooltip("Max angle to walk")]
    public float slopeLimit = 45f;
	
	[Tooltip("Apply extra gravity when the character is not grounded")]
	public float extraGravity = 4f;
	
	[Tooltip("Select a VerticalVelocity to turn on Land High animation")]
	public float landHighVel = -5f;
	
	[Tooltip("Select a VerticalVelocity to turn on the Ragdoll")]
	public float ragdollVel = -8f;
	
	[Tooltip("Prevent the character of walking in place when hit a wall")]
	// if you change your Layers order, remember to update here!
	public float stopMoveDistance = 0.5f;

	[Tooltip("Choose the layers the your character will stop moving when hit with the BLUE Raycast")]
	public LayerMask stopMoveLayer = 1 << 0 | 1 << 9;
	public LayerMask groundLayer = 1 << 0;
	public RaycastHit groundHit;
    
    [Header("--- Debug Info ---")]
	public bool debugWindow;
	[Range(0f, 1f)] public float timeScale = 1f;
	
	// general bools of movement
	[HideInInspector] public bool 
		onGround, 		stopMove, 		autoCrouch, 
		ragdolled, 		quickStop,		quickTurn180,
		canSprint,		crouch, 		aiming, 
		landHigh;
	
	// actions bools, used to turn on/off actions animations *check ThirdPersonAnimator*	
	[HideInInspector] public bool
		jumpOver,		stepUp,			climbUp,
		rolling;

	// one bool to rule then all
	[HideInInspector] public bool actions;

    #endregion

    //**********************************************************************************//
    // UPDATE MOTOR 			 													    //
    // call this method on ThirdPersonController FixedUpdate                            //
    //**********************************************************************************//    
    public void UpdateMotor()
	{    
		GroundCheck();
		ControlHeight();
		RagdollControl();
	}
    
	//**********************************************************************************//
	// ACTIVATE RAGDOLL 			 													//
	// check your verticalVelocity and assign a value on the variable RagdollVel		//
	//**********************************************************************************//
	void RagdollControl()
	{
		if(verticalVelocity <= ragdollVel && groundDistance <= 0.1f)
			transform.root.SendMessage ("ActivateRagdoll", SendMessageOptions.DontRequireReceiver);		
	}

	//**********************************************************************************//
	// CAPSULE COLLIDER HEIGHT CONTROL 													//
	// controls height, position and radius of CapsuleCollider							//
	//**********************************************************************************//	
	void ControlHeight()
	{
		if (crouch && !jumpOver)
		{
			capsuleCollider.center = new Vector3(0f, 0.5f, 0f);
			capsuleCollider.radius = 0.3f;
			capsuleCollider.height = 1f;
		}
		else if (jumpOver)
		{
			capsuleCollider.center = new Vector3(0f, 1.3f, 0f);
			capsuleCollider.radius = 0.3f;
			capsuleCollider.height = 1f;
		}
		else if (rolling)
		{
			capsuleCollider.center = new Vector3(0f, 0.5f, 0f);
			capsuleCollider.radius = 0.3f;
			capsuleCollider.height = 1f;
		}
		else
		{
			// back to the original values
			capsuleCollider.center = colliderCenter;
			capsuleCollider.radius = colliderRadius;
			capsuleCollider.height = colliderHeight;
		}
	}

	//**********************************************************************************//
	// GROUND CHECKER																	//
	// define the conditions for the FootIK work                                       	//	
	// check if the character is grounded or not		 								//
	//**********************************************************************************//	
	void GroundCheck()
	{
		CheckGroundDistance();
		
		// change the physics material to very slip when not grounded
		capsuleCollider.material = (onGround) ? frictionPhysics : defaultPhysics;
				
		// we don't want to stick the character grounded if one of these bools is true
		bool groundStickConditions = !jumpOver && !stepUp && !climbUp;

		if (groundStickConditions)
		{
			var onStep = StepOffset();
			if (groundDistance <= 0.1f)
			{
				onGround = true;

				// keeps the character grounded and prevents bounceness on ramps
				if (!onStep) _rigidbody.velocity = Vector3.ProjectOnPlane(_rigidbody.velocity, groundHit.normal);
			}
			else
			{
				if (groundDistance >= groundCheckDistance)
				{
					onGround = false;
					// check vertical velocity
					verticalVelocity = _rigidbody.velocity.y;
					// apply extra gravity when falling
					if (!onStep && !rolling) transform.position -= Vector3.up * (extraGravity * Time.deltaTime);
				}
				else if (!onStep && !rolling && !onGround)
					transform.position -= Vector3.up * (extraGravity * Time.deltaTime);
			}
		}
	}

	//**********************************************************************************//
	// GROUND DISTANCE		      														//
	// get the distance between the middle of the character to the ground				//
	//**********************************************************************************//
	void CheckGroundDistance()
	{
		Ray ray = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.down);
		var dist = Mathf.Infinity;
		if (Physics.Raycast(ray, out groundHit, Mathf.Infinity, groundLayer))
			dist = transform.position.y - groundHit.point.y;
		
		groundDistance = dist;
	}
	
	//**********************************************************************************//
	// STEP OFFSET LIMIT    															//
	// check the height of the object ahead, control by stepOffSet						//
	//**********************************************************************************//
	bool StepOffset()
	{
		if(input.sqrMagnitude < 0.1) return false;
		
		var hit = new RaycastHit();
		Ray ray = new Ray((transform.position + new Vector3(0, stepOffsetLimit + 0.2f, 0) + transform.forward * ((capsuleCollider).radius + 0.03f)), Vector3.down);
		#if UNITY_EDITOR
		Debug.DrawRay(ray.origin, ray.direction, Color.grey);
		#endif
		if (Physics.Raycast(ray, out hit, stepOffsetLimit + 0.5f, groundLayer))
			if (!stopMove && hit.point.y >= (transform.position.y) && hit.point.y <= (transform.position.y + stepOffsetLimit))
		{
			var heightPoint = new Vector3(transform.position.x, hit.point.y + 0.1f, transform.position.z);
			transform.position = Vector3.Slerp(transform.position, heightPoint, (speed * 10) * Time.deltaTime);
			return true;
		}
		return false;
	}

	//**********************************************************************************//
	// STOP MOVE		    															//
	// stop the character if hits a wall and apply slope limit to ramps					//
	//**********************************************************************************//
	public void StopMove()
	{
		if(input.sqrMagnitude < 0.1) return;

		RaycastHit hitinfo;
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		Ray ray = new Ray(transform.position + new Vector3(0, colliderHeight/3, 0), fwd);

		#if UNITY_EDITOR
		Debug.DrawRay(ray.origin, ray.direction * 1f, Color.blue);
		#endif
		
		if (Physics.Raycast(ray, out hitinfo, 1f, stopMoveLayer))
		{
			var hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);
                       
            if(hitinfo.distance <= stopMoveDistance && hitAngle > 85)
                stopMove = true;            
            else if (hitAngle >= slopeLimit + 1f && hitAngle <= 85)
                stopMove = true;                   
		}
		else
			stopMove = false;
	}

    //**********************************************************************************//
    // ROTATE WITH CAMERA	      														//
    // align the character spine rotation with the camera								//    
    // always run this method on LateUpdate for better and smooth rotation              //
    //**********************************************************************************//
    public void RotateWithCamera()
	{
		if (aiming && !rolling)
		{
			// change update animator to sync better the bone rotations
			animator.updateMode = AnimatorUpdateMode.Normal;
			// align the spine rotation with the camera Y rotation
			AlignBonesWhileAiming();
			
			// smooth align character with aim position
			Quaternion newPos = Quaternion.Euler(transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, transform.eulerAngles.z);
			transform.rotation = Quaternion.Slerp(transform.rotation, newPos, 20f * Time.smoothDeltaTime);
		}
		else
			animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
	}

    //**********************************************************************************//
    // ALIGN SPINE WHILE AIMING    														//
    // bones are automatically setup by CreateBonesToCurve() method on Character.cs	    //
    //**********************************************************************************//
    float isAiming;	
	void AlignBonesWhileAiming()
	{
		float aim_height = 1f;
		float diferenceAngle = 0.5f;
		float V_LookAngle = (tpCamera.mouseY/1.5f);
		V_LookAngle = Helper.ClampAngle(V_LookAngle, tpCamera.Y_MinLimit, tpCamera.Y_MaxLimit);

		isAiming = Mathf.MoveTowards(isAiming, (aiming) ? aim_height : 0.0f, Time.deltaTime * 5.0f);
		
		foreach (Transform segment in BonesToCurve)
			segment.RotateAround(segment.position, transform.right,
			                     (V_LookAngle * (isAiming + diferenceAngle)) / BonesToCurve.Length);
	}	

	//**********************************************************************************//
	// DEBUG MODE			      														//
	// display information about the controller in real time 							//
	//**********************************************************************************//	
	public void DebugMode()
	{
		Time.timeScale = timeScale;
		
		if (hud == null)
			return;
		
		if (hud.debugPanel != null)
		{
			if (debugWindow)
			{
				hud.debugPanel.SetActive(true);
				float delta = Time.smoothDeltaTime;
				float fps = 1 / delta;
				
				hud.debugPanel.GetComponentInChildren<Text>().text =
					"FPS " + fps.ToString("#,##0 fps") + "\n" +
					"CameraState = " + tpCamera.currentStateName.ToString() + "\n" +
					"Input = " + Mathf.Clamp(input.sqrMagnitude, 0, 1f).ToString("0.0") + "\n" +
					"Speed = " + speed.ToString("0.0") + "\n" +
					"Direction = " + direction.ToString("0.0") + "\n" +
					"VerticalVelocity = " + verticalVelocity.ToString("0.00") + "\n" +
					"GroundDistance = " + groundDistance.ToString("0.00") + "\n" +
					"\n" + "--- Movement Bools ---" + "\n" +
					"LockPlayer = " + lockPlayer.ToString() + "\n" +
					"StopMove = " + stopMove.ToString() + "\n" +
                    "Ragdoll = " + ragdolled.ToString() + "\n" +
                    "onGround = " + onGround.ToString() + "\n" +
					"canSprint = " + canSprint.ToString() + "\n" +
					"crouch = " + crouch.ToString() + "\n" +
					"aiming = " + aiming.ToString() + "\n" +
					"canTurn = " + quickTurn180.ToString() + "\n" +
					"quickStop = " + quickStop.ToString() + "\n" +
					"landHigh = " + landHigh.ToString() + "\n" +
					"\n" + "--- Actions Bools ---" + "\n" +
					"rollFwd = " + rolling.ToString() + "\n" +					
					"stepUp = " + stepUp.ToString() + "\n" +
					"jumpOver = " + jumpOver.ToString() + "\n" +
					"climbUp = " + climbUp.ToString() + "\n";
			}
			else
				hud.debugPanel.SetActive(false);
		}
	}
}
