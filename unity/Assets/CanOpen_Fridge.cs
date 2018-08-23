﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanOpen_Fridge : MonoBehaviour 
{
	[Header("Doors for this Fridge")]
	[SerializeField]
	GameObject[] Doors;
    
	[Header("Animation Parameters")]
	[SerializeField]
    protected Vector3[] openPositions;

    [SerializeField]
    protected Vector3[] closedPositions;

    [SerializeField]
    protected float animationTime = 1.0f;

    [SerializeField]
    protected float openPercentage = 1.0f; //0.0 to 1.0 - percent of openPosition the object opens. 

	[Header("Objects To Ignore Collision With - For Cabinets/Drawers with hinges too close together")]
    //these are objects to ignore collision with. For example, two cabinets right next to each other
    //might clip into themselves, so ignore the "reset" event in that case by putting the object to ignore in the below array
    [SerializeField]
    public GameObject[] IgnoreTheseObjects;

	[Header("State information bools")]
	[SerializeField]
    public bool isOpen = false;

    //private Hashtable iTweenArgs;

	[SerializeField]
    public bool canReset = true;
    
	// Use this for initialization
	void Start () 
	{
		//init Itween in all doors to prep for animation
		if(Doors != null)
		{
			foreach (GameObject go in Doors)
            {
                iTween.Init(go);
            }
		}

	}
	
	// Update is called once per frame
	void Update () 
	{
		//test if it can open without Agent Command - Debug Purposes
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Interact();
        }
        #endif
	}

	public void SetOpenPercent(float val)
    {
        if (val >= 0.0 && val <= 1.0)
            openPercentage = val;

        else
            Debug.Log("Please give an open percentage between 0.0f and 1.0f");
    }
    
    public void Interact()
    {
        //it's open? close it
        if (isOpen)
        {
			for (int i = 0; i < Doors.Length; i++)
			{
				iTween.RotateTo(Doors[i],iTween.Hash("rotation", closedPositions[i], "islocal", true));
			}
        }

        //open it here
        else
        {
			for (int i = 0; i < Doors.Length; i++)
            {
				iTween.RotateTo(Doors[i], iTween.Hash("rotation", openPositions[i] * openPercentage, "islocal", true));            
			}
        }
       

        isOpen = !isOpen;
        //canReset = true;
    }

    public float GetOpenPercent()
    {
        //if open, return the percent it is open
        if (isOpen)
        {
            return openPercentage;
        }

        //we are closed, so I guess it's 0% open?
        else
            return 0.0f;
    }

    public bool GetisOpen()
    {
        return isOpen;
    }

    //for use in OnTriggerEnter ignore check
    public bool IsInIgnoreArray(Collider other, GameObject[] arrayOfCol)
    {
        for (int i = 0; i < arrayOfCol.Length; i++)
        {
            if (other.GetComponentInParent<CanOpen>().transform ==
                arrayOfCol[i].GetComponentInParent<CanOpen>().transform)
                return true;
        }
        return false;
    }

    public int GetiTweenCount()
    {
		//the number of iTween instances running on all doors managed by this fridge
		int count = 0;

		foreach (GameObject go in Doors)
        {
			count += iTween.Count(go);
        }
		return count;//iTween.Count(this.transform.gameObject);
    }


    //need to move OnTriggerEnter function to the doors of this fridge....hmm

    //trigger enter/exit functions reset the animation if the Agent is hit by the object opening
    public void OnTriggerEnter(Collider other)
    {

        //note: Normally rigidbodies set to Kinematic will never call the OnTriggerX events
        //when colliding with another rigidbody that is kinematic. For some reason, if the other object
        //has a trigger collider even though THIS object only has a kinematic rigidbody, this
        //function is still called so we'll use that here:

        //The Agent has a trigger Capsule collider, and other cabinets/drawers have
        //a trigger collider, so this is used to reset the position if the agent or another
        //cabinet or drawer is in the way of this object opening/closing

        //if hitting the Agent, reset position and report failed action
        if (other.name == "FPSController" && canReset == true)
        {
            Debug.Log(gameObject.name + " hit " + other.name + " Resetting position");
            canReset = false;
            Reset();
        }

        //if hitting another cabinet/drawer, do some checks 
        if (other.GetComponentInParent<CanOpen>() && canReset == true)
        {
            if (IsInIgnoreArray(other, IgnoreTheseObjects))
            {
                //don't reset, it's cool to ignore these since some cabinets literally clip into each other if they are double doors
                return;
            }

            //oh it was something else RESET! DO IT!
            else
            {
                //check the collider hit's parent for itween instances
                //if 0, then it is not actively animating so check against it. This is needed so CanOpen objects don't reset unless they are the active
                //object moving. Otherwise, an open cabinet hit by a drawer would cause the Drawer AND the cabinet to try and reset.
                //this should be fine since only one cabinet/drawer will be moving at a time given the Agent's action only opening on object at a time
                if (other.transform.GetComponentInParent<CanOpen>().GetiTweenCount() == 0)//iTween.Count(other.transform.GetComponentInParent<CanOpen>().transform.gameObject) == 0)
                {
                    //print(other.GetComponentInParent<CanOpen>().transform.name);
                    Debug.Log(gameObject.name + " hit " + other.name + " Resetting position");
                    canReset = false;
                    Reset();
                }

            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.name == "FPSController" || other.GetComponentInParent<CanOpen>())
        {
            //print("HAAAAA");
            canReset = true;
        }
    }

    public void Reset()
    {
        if (!canReset)
            Interact();
    }
}
