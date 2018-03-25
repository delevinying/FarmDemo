using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class CreateCharacterEditor : EditorWindow
{
    GUISkin skin;
    GameObject charObj;
    Animator charAnimator;   
    RuntimeAnimatorController controller;
    TPCameraListData cameraListData;
    Vector2 rect = new Vector2(500, 100);
    Vector2 scrool;
    Editor humanoidpreview;
    List<CharacterTemplate> templates;
    List<string> templateNames;
    int selectedTemplate;   
    CharacterTemplate currentTemplate;
    Rect buttomRect = new Rect();
    
    /// <summary>
	/// CharacterTemplate Menu 
    /// </summary>
    [MenuItem("3rd Person Controller/Resources/New Character Template")]
    static void NewCameraStateData()
    {
        ScriptableObjectUtility.CreateAsset<CharacterTemplate>();
    }

    /// <summary>
	/// 3rdPersonController Menu 
    /// </summary>    
    [MenuItem("3rd Person Controller/Create New Character")]
    public static void CreateNewCharacter()
    {
        GetWindow<CreateCharacterEditor>();
    }

    void OnEnable()
    {
        templates = new List<CharacterTemplate>();
        LoadAllAssets(Application.dataPath, ref templates);
    }

	bool isHuman,isValidAvatar,charExist;
	void OnGUI()
	{		
		if (!skin) skin = Resources.Load("skin") as GUISkin;
		GUI.skin = skin;
		this.minSize = rect;  
		this.titleContent = new GUIContent("Character", null, "3rd Person Character Creator");  
		if (templateNames == null) templateNames = new List<string>();
		if (templates == null) return;
		foreach (CharacterTemplate template in templates)
		{
			if (!templateNames.Contains(template.name))
				templateNames.Add(template.name);           
		}
		GUILayout.BeginVertical ("box");
		if (templates.Count > 0)
		{
			EditorGUILayout.Space(); 
			GUILayout.BeginHorizontal("box");
			selectedTemplate = EditorGUILayout.Popup("Character Template", selectedTemplate, templateNames.ToArray());
			GUILayout.EndHorizontal();
			currentTemplate = templates[selectedTemplate];			
		}
		else
		{
			EditorGUILayout.HelpBox("Can't find a Character Template\nPlease create Template in\n3rd Person Controller>Resources>New Character Template ", MessageType.Error);			
		}
		var currentEvent = Event.current;
		
		buttomRect = GUILayoutUtility.GetLastRect();
		buttomRect.position = new Vector2(0, buttomRect.position.y);
		buttomRect.width = this.maxSize.x;    
		
		if (buttomRect.Contains(currentEvent.mousePosition) || templates == null)
		{			
			templates = new List<CharacterTemplate>();
			LoadAllAssets(Application.dataPath, ref templates);
		}
		
		if (!isValidTemplate())
		{
			EditorGUILayout.HelpBox("The Character Template is null or incomplete", MessageType.Error);
			GUILayout.EndVertical ();
			GUILayout.FlexibleSpace();
			return;
		}
		if (!charObj)EditorGUILayout.HelpBox("Make sure your FBX model is select as Humanoid!",MessageType.Info);
		else
		if (!charExist) {
			EditorGUILayout.HelpBox ("Missing a Animator Component", MessageType.Error);
			
		} else if (!isHuman)
			EditorGUILayout.HelpBox ("This is not a Humanoid", MessageType.Error);
		else if (!isValidAvatar)
			EditorGUILayout.HelpBox (charObj.name + " is a invalid Humanoid", MessageType.Info);
		
		GUILayout.BeginHorizontal("box");
		
		charObj = EditorGUILayout.ObjectField("FBX Model", charObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
		
		if (GUI.changed && charObj !=null)
			humanoidpreview = Editor.CreateEditor(charObj); 
		
		GUILayout.EndHorizontal();
		EditorGUILayout.Space();
		if(charObj)
			charAnimator = charObj.GetComponent<Animator>();
		charExist = charAnimator != null;
		isHuman = charExist?charAnimator.isHuman:false;
		isValidAvatar = charExist?charAnimator.avatar.isValid:false;
		
		if (CanCreate ()) 
		{
			DrawHumanoidPreview ();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();     
			if (GUILayout.Button ("Create"))
				Create (); 
			GUILayout.FlexibleSpace();  
			GUILayout.EndHorizontal();
		}

		EditorGUILayout.Space();
		GUILayout.EndVertical ();
	}

	bool CanCreate()
	{
		return isValidTemplate () && isValidAvatar && isHuman;
	}

    void LoadAllAssets(string pathTarget,ref List<CharacterTemplate> _templates)
    {       
        if (!Directory.Exists(pathTarget))
            Directory.CreateDirectory(pathTarget);
        DirectoryInfo info = new DirectoryInfo(pathTarget);
        DirectoryInfo[] dirInfos = info.GetDirectories("*",SearchOption.TopDirectoryOnly);
        FileInfo[] fInfos = info.GetFiles("*.asset",SearchOption.TopDirectoryOnly);
      
        foreach (FileInfo fInfo in fInfos)
        {
            var dir = @fInfo.FullName;           
            var path = dir.Remove(0,Application.dataPath.ToString().Length);           
         
            var template = AssetDatabase.LoadAssetAtPath<CharacterTemplate>("Assets"+path);
            if (template != null)
            {
                templates.Add(template);
            }           
        }
        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            LoadAllAssets(dirInfo.FullName, ref _templates);
        }       
    }

    /// <summary>
    /// Check if the Template is valid
    /// </summary>
    /// <returns></returns>
    bool isValidTemplate()
    {
        if (currentTemplate == null ||
            currentTemplate.animatorController==null ||
            currentTemplate.cameraListData==null ||
            currentTemplate.hud ==null) return false;
        return true;
    }

    /// <summary>
    /// Draw the Preview window
    /// </summary>
    void DrawHumanoidPreview()
    {        
        GUILayout.FlexibleSpace();
        
        if (humanoidpreview != null)
        {
            humanoidpreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(100, 400), "window");
        }       
    }

    /// <summary>
    /// Created the Third Person Controller
    /// </summary>
    void Create()
    {
		// base for the character
		var _3rdPerson = GameObject.Instantiate(charObj, Vector3.zero, Quaternion.identity) as GameObject;
        if (!_3rdPerson)
            return;

		_3rdPerson.name = "3rdPersonController";

        _3rdPerson.AddComponent<ThirdPersonController>();
        var footStep = _3rdPerson.AddComponent<FootStepFromTexture>();
		var collider = _3rdPerson.GetComponent<CapsuleCollider>();

        // rigidBody
		var rigidbody = _3rdPerson.GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rigidbody.mass = 80;

        // capsule collider 
        collider.height = ColliderHeight(_3rdPerson.GetComponent<Animator>());
		collider.center = new Vector3(0, (float) System.Math.Round(collider.height * 0.5f, 2), 0);
		collider.radius = (float) System.Math.Round(collider.height * 0.15f, 2);

        // animator
        _3rdPerson.GetComponent<Animator>().avatar = _3rdPerson.GetComponent<Animator>().avatar;
        var leftFT = _3rdPerson.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftFoot).gameObject.AddComponent<SphereCollider>();
        var rightFT = _3rdPerson.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightFoot).gameObject.AddComponent<SphereCollider>();

        controller = currentTemplate.animatorController;
        if (controller)
            _3rdPerson.GetComponent<Animator>().runtimeAnimatorController = controller;

        // footStep
        var _footStepSouce = new GameObject("footStepSource");
        _footStepSouce.transform.SetParent(_3rdPerson.transform);
        var sourceObj = new GameObject("sourceDefault", typeof(AudioSource));
        sourceObj.transform.SetParent(_footStepSouce.transform);
        sourceObj.GetComponent<AudioSource>().playOnAwake = false;
        footStep.defaultSurface = new AudioSurface();
        footStep.defaultSurface.source = sourceObj.GetComponent<AudioSource>();
        footStep.leftFootTrigger = leftFT;
        footStep.rightFootTrigger = rightFT;

        // camera
        GameObject camera = null;
        if (Camera.main == null)
        {
            var cam = new GameObject("3rdPersonCamera");
            cam.AddComponent<Camera>();
            cam.AddComponent<FlareLayer>();
            cam.AddComponent<GUILayer>();
            cam.AddComponent<AudioListener>();
            camera = cam;
            camera.GetComponent<Camera>().tag = "MainCamera";
			camera.GetComponent<Camera>().nearClipPlane = 0.01f;
        }
        else
        {
            camera = Camera.main.gameObject;
            camera.gameObject.name = "3rdPersonCamera";
        }
        var tpcamera = camera.GetComponent<TPCamera>();

        if (tpcamera == null)
            tpcamera = camera.AddComponent<TPCamera>();

		// define the camera target
        //tpcamera.Player = _3rdPerson.transform;
		var hips = _3rdPerson.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
		tpcamera.Player = hips;

        cameraListData = currentTemplate.cameraListData;
        if (cameraListData != null)
        {
            tpcamera.CameraStateList = cameraListData;
        }
        _3rdPerson.GetComponent<ThirdPersonMotor>().tpCamera = tpcamera;
        _3rdPerson.GetComponent<ThirdPersonMotor>().hud = CreateHud();
        _3rdPerson.tag = "Player";
		_3rdPerson.SetLayerRecursively(LayerMask.NameToLayer("Player"));
        this.Close();
    }
    
    /// <summary>
    /// Capsule Collider height based on the Character height
    /// </summary>
    /// <param name="animator">animator humanoid</param>
    /// <returns></returns>
    float ColliderHeight(Animator animator)
    {
        var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
		return (float) System.Math.Round(Vector3.Distance(foot.position, hips.position) * 2f, 2);
    }
    
    /// <summary>
    /// Return Hud Object
    /// </summary>
    /// <returns></returns>
	HUDController CreateHud()
	{
		//check Canvas
		var canvas = FindObjectOfType<Canvas>();
		if (canvas == null)
		{
			var canvasObj = new GameObject("UI") ;
			canvas = canvasObj.AddComponent<Canvas>();
			canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
			canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
		}
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		if (canvas.GetComponent<UnityEngine.UI.CanvasScaler>() != null)
		{
			canvas.GetComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvas.GetComponent<UnityEngine.UI.CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
		}
		//Check HUD
		RectTransform Hud = canvas.transform.Find("HUD") as RectTransform;
		
		if (Hud == null)
		{
			var _Hud = currentTemplate.hud.gameObject;
			_Hud = GameObject.Instantiate(_Hud) as GameObject;
			if (_Hud)
				_Hud.GetComponent<RectTransform>().SetParent(canvas.transform);
			Hud = _Hud.GetComponent<RectTransform>();
			Hud.offsetMax = new Vector2(0, 0);
			
			Hud.name = "HUD";
		}
		if (Hud.GetComponent<HUDController>() == null)
			Hud.gameObject.AddComponent<HUDController>();
		//HUD Components       
		
		return Hud.GetComponent<HUDController>();
	}

   
}
