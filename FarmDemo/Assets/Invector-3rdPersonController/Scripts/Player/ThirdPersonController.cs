using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class ThirdPersonController : ThirdPersonAnimator
{
    private static ThirdPersonController _instance;
    public static ThirdPersonController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<ThirdPersonController>();
                //Tell unity not to destroy this object when loading a new scene
                //DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
	   
    void Awake()
    {
        StartCoroutine("UpdateRaycast");	// limit raycasts calls for better performance
    }

    void Start()
    {
        InitialSetup();						// setup the basic information, created on Character.cs	
    }

    void FixedUpdate()
    {
		UpdateMotor();					// call ThirdPersonMotor methods
		UpdateAnimator();				// update animations on the Animator and their methods
		UpdateHUD();                    // update HUD elements like health bar, texts, etc
        ControlCameraState();			// change CameraStates
    }

    void LateUpdate()
    {
        HandleInput();					// handle input from controller, keyboard&mouse or touch
        RotateWithCamera();				// rotate the character with the camera    
		DebugMode();					// display information about the character on PlayMode
    }

    //**********************************************************************************//
    // INPUT  		      																//
    // here you can config everything that recieve input								//   
    //**********************************************************************************//
    void HandleInput()
    {
        CloseApp();
        CameraInput();
        if (onGround && !lockPlayer)
        {            
            ControllerInput();
            RunningInput();
            RollingInput();
            CrouchInput();
            AimInput();
        }
        else
        {
            input = Vector2.zero;
            speed = 0f;
			canSprint = false;
        }
    }

	//**********************************************************************************//
	// UPDATE RAYCASTS																	//
	// handles a separate update for better performance									//
	//**********************************************************************************//
	public IEnumerator UpdateRaycast()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			
			CheckAutoCrouch();
			CheckForwardAction();
			StopMove();
		}
	}

    //**********************************************************************************//
    // CAMERA STATE    		      														//
    // you can change de CameraState here, the bool means if you want lerp of not		//
    // make sure to use the same CameraState String that you named on TPCameraListData  //
    //**********************************************************************************//
    void ControlCameraState()
    {
        if (crouch)
        {
            if (aiming)
                tpCamera.ChangeState("CrouchAim", true);
            else
                tpCamera.ChangeState("Crouch", true);
        }
        else if (aiming)
            tpCamera.ChangeState("Aim", true);
        else
            tpCamera.ChangeState("Default", true);
    }

	//**********************************************************************************//
	// MOVE CONTROLLER 		      														//
	// gets input from keyboard, controller or mobile touch								//
	//**********************************************************************************//
    void ControllerInput()
    {
		#if MOBILE_INPUT
		input = new Vector2(CrossPlatformInputManager.GetAxis("Horizontal"), CrossPlatformInputManager.GetAxis("Vertical"));	
		#else
        if (inputType == InputType.MouseKeyboard)
            input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        else if (inputType == InputType.Controler)
        {
            float deadzone = 0.25f;
            input = new Vector2(Input.GetAxis("LeftAnalogHorizontal"), Input.GetAxis("LeftAnalogVertical"));
            if (input.magnitude < deadzone)
                input = Vector2.zero;
            else
                input = input.normalized * ((input.magnitude - deadzone) / (1 - deadzone));
        }
		#endif
    }

	//**********************************************************************************//
	// MOVE CAMERA	 		      														//
	//**********************************************************************************//
    void CameraInput()
    {
        if (!tpCamera.lockCamera)
        {
			#if MOBILE_INPUT
			tpCamera.mouseX += CrossPlatformInputManager.GetAxis ("Mouse X") * tpCamera.X_MouseSensitivity;
			tpCamera.mouseY -= CrossPlatformInputManager.GetAxis ("Mouse Y") * tpCamera.Y_MouseSensitivity;
			#else
            if (inputType == InputType.MouseKeyboard)
            {
                tpCamera.mouseX += Input.GetAxis("Mouse X") * tpCamera.X_MouseSensitivity;
                tpCamera.mouseY -= Input.GetAxis("Mouse Y") * tpCamera.Y_MouseSensitivity;
            }
            else if (inputType == InputType.Controler)
            {
                tpCamera.mouseX += Input.GetAxis("RightAnalogHorizontal") * tpCamera.X_MouseSensitivity;
                tpCamera.mouseY -= Input.GetAxis("RightAnalogVertical") * tpCamera.Y_MouseSensitivity;
            }
			#endif

            //Limit the Mouse Y using Helper Script
            tpCamera.mouseY = Helper.ClampAngle(tpCamera.mouseY, tpCamera.Y_MinLimit, tpCamera.Y_MaxLimit);
            tpCamera.mouseX = Helper.ClampAngle(tpCamera.mouseX, tpCamera.X_MinLimit, tpCamera.X_MaxLimit);
        }
        else
        {
            // keeps the camera behind the player if tpCamera.lockCamera = true
            tpCamera.mouseY = tpCamera.Player.localEulerAngles.x;
            tpCamera.mouseX = tpCamera.Player.localEulerAngles.y;
        }
    }

	//**********************************************************************************//
	// RUNNING INPUT		      														//
	//**********************************************************************************//
    void RunningInput()
    {
		#if MOBILE_INPUT
		if(CrossPlatformInputManager.GetButtonDown("RB") && currentStamina > 0 && input.sqrMagnitude > 0.1f)
			canSprint = true;
		if(CrossPlatformInputManager.GetButtonUp("RB") || currentStamina <= 0)
			canSprint = false;
		#else
		if (Input.GetButtonDown("RB") && currentStamina > 0 && input.sqrMagnitude > 0.1f)
		{
			if (speed >= 0.5f && onGround && !aiming && !crouch)
				canSprint = true;
		}
		else if (Input.GetButtonUp("RB") || currentStamina <= 0)
			canSprint = false;
		#endif


		if (canSprint)
		{
			currentStamina -= 0.5f;
			if (currentStamina < 0)
				currentStamina = 0;
		}
		else
		{
			currentStamina += 1f;
			if (currentStamina >= startingStamina)
				currentStamina = startingStamina;
		}
    }

	//**********************************************************************************//
	// CROUCH INPUT		      															//
	//**********************************************************************************//
    void CrouchInput()
    {
        if (autoCrouch)
            crouch = true;
        else if (pressToCrouch)
        {
			#if MOBILE_INPUT
			crouch = (CrossPlatformInputManager.GetButton ("Y") && onGround);
			#else
            crouch = Input.GetButton("Y") && onGround && !actions;
			#endif
        }
        else
        {
			#if MOBILE_INPUT
			if(CrossPlatformInputManager.GetButtonDown ("Y") && onGround)
				crouch = !crouch;				
			#else
			if (Input.GetButtonDown("Y") && onGround && !actions)
                crouch = !crouch;
			#endif
        }
    }

	//**********************************************************************************//
	// AUTO CROUCH			      														//
	// keep it crouched while you have something above					                //
	//**********************************************************************************//
	void CheckAutoCrouch()
	{
		RaycastHit hitinfo;
		#if UNITY_EDITOR
		Debug.DrawRay(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.up * headDetect, Color.green);
		#endif
		if (Physics.Raycast(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.up, out hitinfo, headDetect, stopMoveLayer))
		{
			autoCrouch = true;
			crouch = true;
		}			
		else
			autoCrouch = false;
	}

	//**********************************************************************************//
	// AIMING INPUT		      															//
	//**********************************************************************************//
    void AimInput()
    {
        if (!quickTurn180 && !actions)
        {
            if (crouch) aiming = false;

			#if MOBILE_INPUT
			if( CrossPlatformInputManager.GetButtonDown("LB") && !crouch)
				aiming = !aiming;
			#else
            if (Input.GetButtonDown("LB") && !crouch)
                aiming = !aiming;
			#endif
        }
    }

	//**********************************************************************************//
	// ROLLING INPUT		      														//
	//**********************************************************************************//
    void RollingInput()
    {
		#if MOBILE_INPUT
		if( CrossPlatformInputManager.GetButtonDown("B"))
			Rolling();					
		#else
        if (Input.GetButtonDown("B"))
            Rolling();
		#endif
    }

    //**********************************************************************************//
    // ACTIONS 		      																//
    // raycast to check if there is anything interactable ahead							//
    //**********************************************************************************//
    void CheckForwardAction()
    {
		bool checkConditions = onGround && !aiming && !landHigh && !actions;

        if (checkConditions)
        {
            Vector3 yOffSet = new Vector3(0f, -0.5f, 0f);
            Vector3 fwd = transform.TransformDirection(Vector3.forward);
            RaycastHit hitinfo;
			#if UNITY_EDITOR
            Debug.DrawRay(transform.position - yOffSet, fwd * 0.45f, Color.white);
			#endif

            if (Physics.Raycast(transform.position - yOffSet, fwd, out hitinfo, 0.45f))
            {
                //Debug.Log(hitinfo.collider.name);

                if (hitinfo.collider.gameObject.CompareTag("ClimbUp"))
                    DoAction(hitinfo, ref climbUp);

                else if (hitinfo.collider.gameObject.CompareTag("StepUp"))
                    DoAction(hitinfo, ref stepUp);

                else if (hitinfo.collider.gameObject.CompareTag("JumpOver"))
                    DoAction(hitinfo, ref jumpOver);
            }
            else if (hud != null)
            {
                if (hud.showInteractiveText) hud.DisableSprite();
            }
        }
    }

    void DoAction(RaycastHit hit, ref bool action)
    {
        var findTarget = hit.transform.GetComponent<FindTarget>();
        if (hud != null) hud.EnableSprite(findTarget.message);

		#if MOBILE_INPUT
		if (CrossPlatformInputManager.GetButtonDown("A"))
		{
			if(hud != null) hud.DisableSprite();
			matchTarget = findTarget.target;
			var rot = hit.transform.rotation;
			transform.rotation = rot;
			action = true;
		}
		#else
        if (Input.GetButton("A"))
        {
            // turn the action bool true and call the animation
            action = true;
            // disable the text and sprite 
            if (hud != null) hud.DisableSprite();
            // find the target height to match with the character animation
            matchTarget = findTarget.target;
            // align the character rotation with the object rotation
            var rot = hit.transform.rotation;
            transform.rotation = rot;
        }
		#endif
    }

	// close the app/exe
	void CloseApp()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}
    
}
