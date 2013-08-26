using UnityEngine;
using System.Collections;

/*
 * This script should be attached to ALL units in the game
 * For example: buildings, troops, resources. Whether they can walk or not.
 * */

public class Unit : MonoBehaviour {
	// for MouseCursor.cs
	public Vector2 ScreenPosition;
	public bool OnScreen;
	public bool Selected = false;
	
	void Update(){
		if(!Selected){
			// Get this unit's 2D screen position
			ScreenPosition = Camera.main.WorldToScreenPoint(this.transform.position);
			
			// if within the screen space
			if(MouseCursor.VisibleOnScreen(ScreenPosition)){
				
				// not already added to UnitsOnScreen list
				if(!OnScreen){
					MouseCursor.UnitsOnScreen.Add (this.gameObject);
					OnScreen = true;
				}
			}
			else {
				// unit is not visible on screen
				// remove object if it was previously on screen
				if(OnScreen){
					MouseCursor.RemoveFromOnScreenUnits(this.gameObject);
				}
			}
		}
	}	
}
