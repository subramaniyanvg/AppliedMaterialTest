using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

//Class responsible for handle input from vr devices(currently supports htc vive)
public class InputManager : MonoBehaviour
{
	public bool 					activatLaserOnStart;
	public float 					handMovVelMag = 0.3f;
	public float 					handMovDelay = 0.1f;
	private bool 					canComPuteRightHandMov = true;
	private bool 					canComPuteLeftHandMov = true;
    public static Hand				rightHandObj;
	public static Hand				leftHandObj;




	public delegate	void            	   	onRightHandMovedUpDel(SteamVR_Action_Pose pose);
	public static onRightHandMovedUpDel    	onRightHandMovedUpEvt;

	public delegate	void            	   	onRightHandMovedDownDel(SteamVR_Action_Pose pose);
	public static onRightHandMovedDownDel  	onRightHandMovedDownEvt;

	public delegate	void            		onRightHandMovedLeftDel(SteamVR_Action_Pose pose);
	public static onRightHandMovedLeftDel 	onRightHandMovedLeftEvt;

	public delegate	void            	   	onRightHandMovedRightDel(SteamVR_Action_Pose pose);
	public static onRightHandMovedRightDel 	onRightHandMovedRightEvt;


	public delegate	void            		 onRightHandMovedForwardDel(SteamVR_Action_Pose pose);
	public static onRightHandMovedForwardDel onRightHandMovedForwardEvt;

	public delegate	void            	   	  onRightHandMovedBackwardDel(SteamVR_Action_Pose pose);
	public static onRightHandMovedBackwardDel onRightHandMovedBackwarEvt;


	public delegate	void            	  	  onLeftHandMovedUpDel(SteamVR_Action_Pose pose);
	public static onLeftHandMovedUpDel    	  onLeftHandMovedUpEvt;

	public delegate	void            	  	  onLeftHandMovedDownDel(SteamVR_Action_Pose pose);
	public static onLeftHandMovedDownDel  	  onLeftHandMovedDownEvt;

	public delegate	void            	  	  onLeftHandMovedLeftDel(SteamVR_Action_Pose pose);
	public static onLeftHandMovedLeftDel  	  onLeftHandMovedLeftEvt;

	public delegate	void            	  	  onLeftHandMovedRightDel(SteamVR_Action_Pose pose);
	public static onLeftHandMovedRightDel 	  onLeftHandMovedRightEvt;

	public delegate	void            		 onLeftHandMovedForwardDel(SteamVR_Action_Pose pose);
	public static onLeftHandMovedForwardDel  onLeftHandMovedForwardEvt;

	public delegate	void            	   	  onLeftHandMovedBackwardDel(SteamVR_Action_Pose pose);
	public static onLeftHandMovedBackwardDel  onLeftHandMovedBackwarEvt;



	public delegate	void            	   	  	 onLeftHandGripButtonPressedDel();
	public static onLeftHandGripButtonPressedDel onLeftHandGripButtonPressedEvt;

	public delegate	void            	   	  	  onLeftHandGripButtonReleasedDel();
	public static onLeftHandGripButtonReleasedDel onLeftHandGripButtonReleasedEvt;

	public delegate	void            	   	  	  onLeftHandGripButtonDownDel();
	public static onLeftHandGripButtonDownDel 	  onLeftHandGripButtonDownEvt;

	public delegate	void            	   	  	 onRightHandGripButtonPressedDel();
	public static onRightHandGripButtonPressedDel onRightHandGripButtonPressedEvt;

	public delegate	void            	   	  	   onRightHandGripButtonReleasedDel();
	public static onRightHandGripButtonReleasedDel onRightHandGripButtonReleasedEvt;

	public delegate	void            	   	  	  onRightHandGripButtonDownDel();
	public static onRightHandGripButtonDownDel 	  onRightHandGripButtonDownEvt;


	public delegate	void            	   	  	 onLeftHandTrigButtonPressedDel();
	public static onLeftHandTrigButtonPressedDel onLeftHandTrigButtonPressedEvt;

	public delegate	void            	   	  	  onLeftHandTrigButtonReleasedDel();
	public static onLeftHandTrigButtonReleasedDel onLeftHandTrigButtonReleasedEvt;

	public delegate	void            	   	  	  onLeftHandTrigButtonDownDel();
	public static onLeftHandTrigButtonDownDel 	  onLeftHandTrigButtonDownEvt;

	public delegate	void            	   	  	 onRightHandTrigButtonPressedDel();
	public static onRightHandTrigButtonPressedDel onRightHandTrigButtonPressedEvt;

	public delegate	void            	   	  	  onRightHandTrigButtonReleasedDel();
	public static onRightHandTrigButtonReleasedDel onRightHandTrigButtonReleasedEvt;

	public delegate	void            	   	  	  onRightHandTrigButtonDownDel();
	public static onRightHandTrigButtonDownDel 	  onRightHandTrigButtonDownEvt;



	public delegate	void            	   	  	   onLeftHandTelprtButtonPressedDel();
	public static onLeftHandTelprtButtonPressedDel onLeftHandTelprtButtonPressedEvt;

	public delegate	void            	   	  	 	onLeftHandTelprtButtonReleasedDel();
	public static onLeftHandTelprtButtonReleasedDel onLeftHandTelprtButtonReleasedEvt;

	public delegate	void            	   	  	   onLeftHandTelprtButtonDownDel();
	public static onLeftHandTelprtButtonDownDel    onLeftHandTelprtButtonDownEvt;

	public delegate	void            	   	  	    onRightHandTelprtButtonPressedDel();
	public static onRightHandTelprtButtonPressedDel onRightHandTelprtButtonPressedEvt;

	public delegate	void            	   	  	     onRightHandTelprtButtonReleasedDel();
	public static onRightHandTelprtButtonReleasedDel onRightHandTelprtButtonReleasedEvt;

	public delegate	void            	   	  	  	  onRightHandTelprtButtonDownDel();
	public static onRightHandTelprtButtonDownDel 	  onRightHandTelprtButtonDownEvt;


	

	void Start()
	{
		GameObject playerObj = GameObject.Find("Player");
		Hand[] hands =  playerObj.GetComponentsInChildren<Hand> ();
		for (int i = 0; i < hands.Length; i++) 
		{
			if (hands [i].handType == SteamVR_Input_Sources.RightHand) 
			{
				rightHandObj = hands [i];
			} 
			else if (hands [i].handType == SteamVR_Input_Sources.LeftHand)
			{
				leftHandObj = hands [i];
			}
		}
	}

	// Update is called once per frame
	void Update () 
	{
		//Left Grip Pressed/Released/Down
		if (SteamVR_Input._default.inActions.GrabGrip.GetStateDown (SteamVR_Input_Sources.LeftHand)) 
		{
			if (onLeftHandGripButtonPressedEvt != null)
				onLeftHandGripButtonPressedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.GrabGrip.GetStateUp (SteamVR_Input_Sources.LeftHand)) {
			if (onLeftHandGripButtonReleasedEvt != null)
				onLeftHandGripButtonReleasedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.GrabGrip.GetState (SteamVR_Input_Sources.LeftHand)) 
		{
			if (onLeftHandGripButtonDownEvt != null)
				onLeftHandGripButtonDownEvt.Invoke ();
		}

		//Right Grip Pressed/Released/Down
		if (SteamVR_Input._default.inActions.GrabGrip.GetStateDown (SteamVR_Input_Sources.RightHand)) 
		{
			if (onRightHandGripButtonPressedEvt != null)
				onRightHandGripButtonPressedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.GrabGrip.GetStateUp (SteamVR_Input_Sources.RightHand))
		{
			if (onRightHandGripButtonReleasedEvt != null)
				onRightHandGripButtonReleasedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.GrabGrip.GetState (SteamVR_Input_Sources.RightHand)) 
		{
			if (onRightHandGripButtonDownEvt != null)
				onRightHandGripButtonDownEvt.Invoke ();
		}


		//Left Trigger Pressed/Released/Down
		if (SteamVR_Input._default.inActions.GrabPinch.GetStateDown (SteamVR_Input_Sources.LeftHand))
		{
			if (onLeftHandTrigButtonPressedEvt != null)
				onLeftHandTrigButtonPressedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.GrabPinch.GetStateUp (SteamVR_Input_Sources.LeftHand))
		{
			if (onLeftHandTrigButtonReleasedEvt != null)
				onLeftHandTrigButtonReleasedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.GrabPinch.GetState (SteamVR_Input_Sources.LeftHand)) 
		{
			if (onLeftHandTrigButtonDownEvt != null)
				onLeftHandTrigButtonDownEvt.Invoke ();
		}

		//Right Trigger Pressed/Released/Down
		if (SteamVR_Input._default.inActions.GrabPinch.GetStateDown (SteamVR_Input_Sources.RightHand))
		{
			if (onRightHandTrigButtonPressedEvt != null)
				onRightHandTrigButtonPressedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.GrabPinch.GetStateUp (SteamVR_Input_Sources.RightHand)) 
		{
			if (onRightHandTrigButtonReleasedEvt != null)
				onRightHandTrigButtonReleasedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.GrabPinch.GetState (SteamVR_Input_Sources.RightHand))
		{
			if (onRightHandTrigButtonDownEvt != null)
				onRightHandTrigButtonDownEvt.Invoke ();
		}

		//TouchPad Left hand Pressed/Released/Down
		if (SteamVR_Input._default.inActions.Teleport.GetStateDown (SteamVR_Input_Sources.LeftHand))
		{
			if (onLeftHandTelprtButtonPressedEvt != null)
				onLeftHandTelprtButtonPressedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.Teleport.GetStateUp (SteamVR_Input_Sources.LeftHand))
		{
			if (onLeftHandTelprtButtonReleasedEvt != null)
				onLeftHandTelprtButtonReleasedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.Teleport.GetState (SteamVR_Input_Sources.LeftHand))
		{
			if (onLeftHandTelprtButtonDownEvt != null)
				onLeftHandTelprtButtonDownEvt.Invoke ();
		}

		//TouchPad Right hand Pressed/Released/Down
		if (SteamVR_Input._default.inActions.Teleport.GetStateDown (SteamVR_Input_Sources.RightHand))
		{
			if (onRightHandTelprtButtonPressedEvt != null)
				onRightHandTelprtButtonPressedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.Teleport.GetStateUp (SteamVR_Input_Sources.RightHand))
		{
			if (onRightHandTelprtButtonReleasedEvt != null)
				onRightHandTelprtButtonReleasedEvt.Invoke ();
		}
		if (SteamVR_Input._default.inActions.Teleport.GetState (SteamVR_Input_Sources.RightHand))
		{
			if (onRightHandTelprtButtonDownEvt != null)
				onRightHandTelprtButtonDownEvt.Invoke ();
		}
    }

	private void ComputeRightHandMov()
	{
		canComPuteRightHandMov = true; 
	}

	private void ComputeLeftHandMov()
	{
		canComPuteLeftHandMov = true; 
	}

	public void ComputeRightHandMov(SteamVR_Action_Pose pose)
	{
		if (pose.GetLastVelocity (SteamVR_Input_Sources.RightHand).magnitude >= handMovVelMag && canComPuteRightHandMov) 
		{
			Vector3 result = pose.GetLocalPosition(SteamVR_Input_Sources.RightHand) - pose.GetLastLocalPosition (SteamVR_Input_Sources.RightHand);
			float xMag = Mathf.Abs (result.x);
			float yMag = Mathf.Abs (result.y);
			float zMag = Mathf.Abs (result.z);
			if (xMag > yMag && xMag > zMag) 
			{
				canComPuteRightHandMov = false;
				Invoke ("ComputeRightHandMov", handMovDelay);
				if (result.x < 0) 
				{
					if (onRightHandMovedLeftEvt != null)
						onRightHandMovedLeftEvt.Invoke (pose);
				} 
				else 
				{
					if (onRightHandMovedRightEvt != null)
						onRightHandMovedRightEvt.Invoke (pose);
				}
			} 
			else if (yMag > xMag && yMag > zMag) 
			{
				canComPuteRightHandMov = false;
				Invoke ("ComputeRightHandMov", handMovDelay);
				if (result.y < 0) 
				{
					if (onRightHandMovedDownEvt != null)
						onRightHandMovedDownEvt.Invoke (pose);
				} 
				else 
				{
					if (onRightHandMovedUpEvt != null)
						onRightHandMovedUpEvt.Invoke (pose);
				}
			}
			else if (zMag > xMag && zMag > yMag)
			{
				canComPuteRightHandMov = false;
				Invoke ("ComputeRightHandMov", handMovDelay);
				if (result.z < 0) 
				{
					if (onRightHandMovedBackwarEvt != null)
						onRightHandMovedBackwarEvt.Invoke (pose);
				} 
				else 
				{
					if (onRightHandMovedForwardEvt != null)
						onRightHandMovedForwardEvt.Invoke (pose);
				}
			}
		}
	}

	public void ComputeLeftHandMov(SteamVR_Action_Pose pose)
	{
		if (pose.GetLastVelocity (SteamVR_Input_Sources.RightHand).magnitude >= handMovVelMag && canComPuteLeftHandMov) 
		{
			Vector3 result = pose.GetLocalPosition(SteamVR_Input_Sources.LeftHand) - pose.GetLastLocalPosition (SteamVR_Input_Sources.LeftHand);
			float xMag = Mathf.Abs (result.x);
			float yMag = Mathf.Abs (result.y);
			float zMag = Mathf.Abs (result.z);

			if (xMag > yMag && xMag > zMag) 
			{
				canComPuteLeftHandMov = false;
				Invoke ("ComputeLeftHandMov", handMovDelay);
				if (result.x < 0) 
				{
					if (onLeftHandMovedLeftEvt != null)
						onLeftHandMovedLeftEvt.Invoke (pose);
				} 
				else 
				{
					if (onLeftHandMovedRightEvt != null)
						onLeftHandMovedRightEvt.Invoke (pose);
				}
			} 
			else if (yMag > xMag && yMag > zMag) 
			{
				canComPuteLeftHandMov = false;
				Invoke ("ComputeLeftHandMov", handMovDelay);
				if (result.y < 0) 
				{
					if (onLeftHandMovedDownEvt != null)
						onLeftHandMovedDownEvt.Invoke (pose);
				} 
				else 
				{
					if (onLeftHandMovedUpEvt != null)
						onLeftHandMovedUpEvt.Invoke (pose);
				}
			}
			else if (zMag > xMag && zMag > yMag)
			{
				canComPuteLeftHandMov = false;
				Invoke ("ComputeLeftHandMov", handMovDelay);
				if (result.z < 0) 
				{
					if (onLeftHandMovedBackwarEvt != null)
						onLeftHandMovedBackwarEvt.Invoke (pose);
				} 
				else 
				{
					if (onLeftHandMovedForwardEvt != null)
						onLeftHandMovedForwardEvt.Invoke (pose);
				}
			}
		 }
	}
}