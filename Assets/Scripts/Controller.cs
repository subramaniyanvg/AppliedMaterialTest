#region namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using DentedPixel;
using cakeslice;
using UnityEngine.XR;
#endregion

public class Controller : MonoBehaviour
{
    #region PublicFields
        public enum PlayerState
        {
            Performing_Operation,
            Free_Roam
        }
        public PlayerState  playerState = PlayerState.Free_Roam;//Specifies whether the player is in FreeRoam state or in operation state
        public delegate void OnFadeScreenCompl();
        public static event OnFadeScreenCompl OnFadeScreenCompEvt = null;//This event will be invoked when camera fadeinout completes
        public GameObject   player;//Reference to player obj
        [Range(1,10)]
        public int          maxRotCount = 1;//1 rotation equals 360 degree this tells how many rotations is needed for tidal locking the bolt
        public float        swipeMagForWrenchRot = 0;//This is the magnitude that is used for checking wrench rot with bolt
        public float        swipeAngForWrenchRot = 0;//This is the angle that is used for checking wrench rot with bolt
        public GameObject   interactableContainer;//This container has referecne to wrench and bolt.
        public Transform    interactableContainerEndTrans;//This ref transform is used along with bolt and wrench for tidal locking on full tight it will reach these point
        public GameObject   wrench;//Ref to wrench
        public GameObject   bolt;//Ref to bolt
        public LayerMask    tableMask;//We perform operations on table so this layermask will be used to check for interaction
        public LayerMask    wrenchHandleMask;//User has to hold the wrench handle and has to do tidal lock this layer mask will check whether user is holding wrench handle
        [Range(0.1f, 1f)]
        public float        camFadespeed;//How fast camera has to fadeinout
        public float        playerMovSpeed = 1;//Self explanatory
        public float        horizontalLookSpeed = 1.0f;//HorizontalLookSpeed of player when looking around 
        public float        verticalLookSpeed = 1.0f;//VerticalLookSpeed of player when looking around
        public Transform    playerTransOnEntOprArea;//This is the transform player will be when entered higlighted area
        public Transform    playerTransOnExtOprArea;//This is the transform player will be when exit operation area
        public Collider     operationArea;//This is the area that allows player to do interact with operation 
        public UnityEvent   onAreaEnteredEvt;//Th//This event will be triggered when player enters the area we can do any other like enabling,disabling etc. is event will be triggered when player enters the area we can do any other like enabling,disabling etc. 
        public UnityEvent   onAreaExtEvt;//This event will be triggered when player exits the area we can do any other like enabling,disabling etc. 
        public UnityEvent   onStartEvt;//This event will be triggered when player starts this scene we can do any other like enabling,disabling etc. 
        public UnityEvent   onWrenchBoltLockedEvt;//This event is triggered when full lock is performed do necessary actions in this event
        public UnityEvent   onWrenchBoltUnlockedEvt;//This event is triggered when full unlock is performed do necessary actions in this event
        public UnityEvent   onWrenchBoltPerformOprtnEvt;//This event is triggered when operation is getting performed do necessary actions in this event

    #endregion

    #region PrivateFields
    private bool                canPlayerMove = true;//Whether player can move
        private CharacterController controller;//Ref to character controller
        private Texture2D           blk;//Ref to fade texture
        private bool                canPerformOprtn = false;//Whether player can perform operation
        private bool                canInteract = false;//Player can only interact with the operation if he is holding wrench handle this checks for that
        private Vector3             interactionContainerStartPnt = Vector3.positiveInfinity;
        private Vector3             interactionContainerEndPnt = Vector3.positiveInfinity;
        private Vector3             lastPos = Vector3.positiveInfinity;//The last pos of table hit point
        private float               camFarPlane;// Camera far plane
        private Camera              cam;//Ref to cam
        private float               currRotAmt = 0;//How much amount we have rotated till now
        private float               maxRotAmt = 360;//Max rotation amount player needs to do for tidal locking
        private bool                fadeInOut;//Can do fadeinfadeout
        private bool                fadeIn;//canfadein if above is true
        private float               alpha = 1;//Default alpha used for fade in.
    #endregion

    #region PublicPropeties
    public bool DofadeInFadeOut
    {
        set
        {
            if (value)
            {
                fadeIn = true;
                fadeInOut = true;
                alpha = 0;
            }
            else
                fadeInOut = value;
        }
    }
    #endregion

    #region PrivateMethods
    private void Awake()
    {
        XRSettings.enabled = false;
        controller = transform.GetComponent<CharacterController>();
        interactionContainerStartPnt = interactableContainer.transform.position;
        interactionContainerEndPnt = interactableContainerEndTrans.transform.position;
        maxRotAmt = 360 * maxRotCount;
        cam = Camera.main;
        camFarPlane = Camera.main.farClipPlane;

        blk = new Texture2D(1, 1);
        blk.SetPixel(0, 0, new Color(0, 0, 0, 1));
        blk.Apply();
    }

    private void Start()
    {
        onStartEvt.Invoke();
    }

    private Vector3 GetRayDirFromCamera()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = camFarPlane;
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        Vector3 rayDir = (worldPos - transform.position).normalized;
        return rayDir;    
    }

    private void PerformOperation()
    {
        Vector3 swipeDir = Vector3.zero;
        float swipeMag = 0;
        Vector3 currHitPos = Vector3.positiveInfinity;
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(transform.position, GetRayDirFromCamera(), out hitInfo, Mathf.Infinity, tableMask))
        {
            currHitPos = hitInfo.point;
            if (!Vector3.Equals(lastPos, Vector3.positiveInfinity))
            {
                swipeDir = (currHitPos - lastPos).normalized;
                swipeMag = (currHitPos - lastPos).magnitude;
            }
            lastPos = currHitPos;
            if (swipeMag >= swipeMagForWrenchRot && Vector3.Angle(swipeDir, wrench.transform.forward) <= swipeAngForWrenchRot)
            {
                Plane p = new Plane(wrench.transform.right, Vector3.zero);
                float rotAmt = 0;
                if (p.GetSide(swipeDir))
                    rotAmt = -1;
                else
                    rotAmt = 1;
                currRotAmt += rotAmt;
                currRotAmt = Mathf.Clamp(currRotAmt, 0, maxRotAmt);
                if (currRotAmt == 0)
                {
                    onWrenchBoltLockedEvt.Invoke();
                    Debug.Log("Cant rotate reverse");
                    return;
                }
                else if (currRotAmt == maxRotAmt)
                {
                    onWrenchBoltUnlockedEvt.Invoke();
                    Debug.Log("maxRotDone");
                    return;
                }
                onWrenchBoltPerformOprtnEvt.Invoke();
                interactableContainer.transform.position = Vector3.Lerp(interactionContainerStartPnt, interactionContainerEndPnt, currRotAmt / maxRotAmt);
                wrench.transform.RotateAround(bolt.transform.position, Vector3.up, rotAmt);
                bolt.transform.RotateAround(bolt.transform.position, Vector3.up, rotAmt);
            }
        }
    }

    private void CheckForPerformOperation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(transform.position, GetRayDirFromCamera(), out hitInfo, Mathf.Infinity, wrenchHandleMask))
                canInteract = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            lastPos = Vector3.positiveInfinity;
            canInteract = false;
        }
        if (canInteract)
            PerformOperation();
    }

    private void OnPlayerEnteredOperationArea()
    {
        canPerformOprtn = true;
        OnFadeScreenCompEvt -= OnPlayerEnteredOperationArea;
        playerState = PlayerState.Performing_Operation;
        player.transform.position = playerTransOnEntOprArea.transform.position;
        player.transform.rotation = playerTransOnEntOprArea.transform.rotation;
        onAreaEnteredEvt.Invoke();
    }

    private void OnPlayerExitedOperationArea()
    {
        canPlayerMove = true;
        canInteract = false;
        OnFadeScreenCompEvt -= OnPlayerExitedOperationArea;
        playerState = PlayerState.Free_Roam;
        player.transform.position = playerTransOnExtOprArea.transform.position;
        player.transform.rotation = playerTransOnExtOprArea.transform.rotation;
        onAreaExtEvt.Invoke();    
    }

    private void ProcessPlayerMov()
    {

        float rotSPeed = 0;
        rotSPeed = Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * horizontalLookSpeed;
        transform.RotateAround(transform.position, Vector3.up, rotSPeed * Time.deltaTime);
        rotSPeed = Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * verticalLookSpeed;
        transform.RotateAround(transform.position, transform.right, -rotSPeed * Time.deltaTime);
        Plane p = new Plane(transform.position + transform.right, transform.position + Vector3.up, transform.position + (-transform.right));
        if (Vector3.Dot(p.normal, transform.forward) < 0)
            transform.RotateAround(transform.position, transform.right, rotSPeed * Time.deltaTime);
        Vector3 dirToMov = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            dirToMov = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            controller.Move(dirToMov * (playerMovSpeed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.S))
        {
            dirToMov = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            controller.Move(-dirToMov * (playerMovSpeed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.A))
        {
            controller.Move(-transform.right * (playerMovSpeed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            controller.Move(transform.right * (playerMovSpeed * Time.deltaTime));
        }

    }
    private void ProcessInteraction()
    {
        if(playerState == PlayerState.Free_Roam)
        {
            canPlayerMove = false;
            OnFadeScreenCompEvt += OnPlayerEnteredOperationArea;
            DofadeInFadeOut = true;
        }
        else if(playerState == PlayerState.Performing_Operation)
        {
            canPerformOprtn = false;
            OnFadeScreenCompEvt += OnPlayerExitedOperationArea;
            DofadeInFadeOut = true;
        }
    }

    private void Update()
    {
        if (playerState == PlayerState.Free_Roam)
        {
            if (operationArea.bounds.Contains(player.transform.position))
            {
                if (Input.GetKeyUp(KeyCode.E))
                    ProcessInteraction();
            }
            if (canPlayerMove)
                ProcessPlayerMov();
        }
        else if (playerState == PlayerState.Performing_Operation)
        {
            if (Input.GetKeyUp(KeyCode.E))
                    ProcessInteraction();
            if (canPerformOprtn)
                CheckForPerformOperation();
        }
    }

    private void OnGUI()
    {
        if (fadeInOut)
        {
            if (fadeIn)
            {
                alpha += 1 * camFadespeed * Time.deltaTime;
                alpha = Mathf.Clamp01(alpha);

                Color newColor = GUI.color;
                newColor.a = alpha;

                GUI.color = newColor;
                GUI.depth = -1000;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blk);
                if (alpha >= 1)
                {
                    alpha = 1;
                    fadeIn = false;
                }
            }
            else
            {
                alpha += -1 * camFadespeed * Time.deltaTime;
                alpha = Mathf.Clamp01(alpha);

                Color newColor = GUI.color;
                newColor.a = alpha;

                GUI.color = newColor;
                GUI.depth = -1000;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blk);
                if (alpha <= 0)
                {
                    DofadeInFadeOut = false;
                    if (OnFadeScreenCompEvt != null)
                        OnFadeScreenCompEvt.Invoke();
                }
            }
        }
    }
    #endregion
}