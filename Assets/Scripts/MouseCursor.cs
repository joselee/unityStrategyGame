using UnityEngine;
using System.Collections;

public class MouseCursor : MonoBehaviour {
	
	public GameObject Target;
	
	public static ArrayList CurrentlySelectedUnits = new ArrayList();
	public static ArrayList UnitsOnScreen = new ArrayList();
	public static ArrayList UnitsInDrag = new ArrayList();
	
	public GUIStyle MouseDragSkin;
	public static bool MouseDrag;
	public static float TimeLimitBeforeDeclareDrag = 1f;
	public static float TimeLeftBeforeDeclareDrag;
	
	private RaycastHit hit;
	private int raycastRange = 500;
	
	private float clickDragZone = 1.3f;
	private Vector3 mouseDownPoint;
	private Vector2 mouseDragStart;
	private Vector3 currentMousePoint; // world mouse point	
	private bool mouseDragFinishedThisFrame;
	
	private float boxLeft;
	private float boxTop;
	private float boxWidth;
	private float boxHeight;
	private static Vector2 boxStart;
	private static Vector2 boxFinish;
	
	void Awake(){
		mouseDownPoint = Vector3.zero;
	}
	
	void Update () {
		Ray ray  = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if(Physics.Raycast (ray, out hit, raycastRange)){
			currentMousePoint = hit.point;
			
			
			// Save the location of where the left mouse button was clicked
			// mouse just went down.
			if(Input.GetMouseButtonDown(0)){
				//Debug.Log("Mouse down");
				mouseDownPoint = hit.point;
				TimeLeftBeforeDeclareDrag = TimeLimitBeforeDeclareDrag;
				mouseDragStart = Input.mousePosition;
			}
			else if(Input.GetMouseButton(0)){
				// if user is not dragging mouse, do tests to check for drag
				if(!MouseDrag){
					TimeLeftBeforeDeclareDrag -= Time.deltaTime;
					if(TimeLeftBeforeDeclareDrag <= 0f || MouseDragByPosition(mouseDragStart, Input.mousePosition)){
						MouseDrag = true;
					}
				}
				
				if(MouseDrag){
					//Debug.Log("Dragging mouse");
				}
			}
			else if(Input.GetMouseButtonUp(0)){
				if(MouseDrag){
					//Debug.Log("Mouse is up");
					mouseDragFinishedThisFrame = true;
				}
				
				TimeLeftBeforeDeclareDrag = 0f;
				MouseDrag = false;
			}
			
			// mouse click
			if(!MouseDrag){
			
				// Hitting a terrain
				if(hit.collider.name == "Terrain"){
					if(Input.GetMouseButtonDown(1)){ 
						// Right click
						GameObject indicator = Instantiate(Target, hit.point, Quaternion.identity) as GameObject;
						indicator.name = "Target instantiated";	
					}
					else if(Input.GetMouseButtonUp(0) && LeftMouseClicked(mouseDownPoint)){
						// Left click
						DeselectAllUnits();
					}
				}
				else { // Hitting other objects -- not terrain
					
					if(Input.GetMouseButtonUp(0) && LeftMouseClicked(mouseDownPoint)){
						
						// Did we click on a selectable unit?
						if(hit.collider.transform.FindChild("Selected")){
							GameObject clickedUnit = hit.collider.gameObject;
							
							if(IsUnitSelected(clickedUnit)){ // We clicked a unit that is already selected
								if(HolingShift()){
									DeselectUnit(clickedUnit);
								}
								else{
									DeselectAllUnits();
									SelectUnit(clickedUnit);
								}
							}
							else { // we clicked a unit that is not yet selected
								if(!HolingShift()){
									DeselectAllUnits();
								}
								SelectUnit (clickedUnit);
							}
						}
						else { // the object we clicked is not a selectable unit.
							DeselectAllUnits();
						}
					}
				}
			}
		}
		else { // Clicked outside the map
			if(Input.GetMouseButtonUp(0) && LeftMouseClicked(mouseDownPoint)){
				DeselectAllUnits();
			}
		}
		
		// Do the calculations for the GUI mouse drag box
		MouseDragBoxCalculations();
		
		Debug.DrawRay (ray.origin, ray.direction * raycastRange, Color.yellow);
	}
	
	void OnGUI(){
		
		if(MouseDrag){
			Rect rectangle = new Rect(boxLeft, boxTop, boxWidth, boxHeight);
			GUI.Box (rectangle, "", MouseDragSkin);
		}
		
	}
	
	void LateUpdate(){
		UnitsInDrag.Clear();
		
		// If player is dragging or completed dragging this frame.
		//(For performance reasons, also check if there is at least one thing to select on the screen)
		if((MouseDrag || mouseDragFinishedThisFrame) && (UnitsOnScreen.Count > 0)){
			
			// Iterate through all visible units on the screen
			foreach(GameObject currentUnit in UnitsOnScreen){
				
				GameObject projector = currentUnit.transform.FindChild("Selected").gameObject; // reference to projector
				
				
				if(!IsUnitInDraggedList(currentUnit)){
					
					Vector2 unitScreenPosition = currentUnit.GetComponent<Unit>().ScreenPosition;
					
					if(IsUnitInsideDragBox(unitScreenPosition)){
						//Debug.Log ("yes it is in the drag box");
						projector.SetActive(true);
						UnitsInDrag.Add(currentUnit);
					}
					else{
						//Debug.Log ("no it is NOT in the drag box");
						if(!IsUnitSelected(currentUnit)){
							projector.SetActive(false);
						}
					}
				}
			}	
		}
		
		if(mouseDragFinishedThisFrame){
			mouseDragFinishedThisFrame = false;
			SelectDraggedUnits();
		}
	}
		
	
	
	
	
	/*BELOW ARE HELPERS AND METHODS*/
	
	public bool MouseDragByPosition(Vector2 dragStartPoint, Vector2 newPoint){
		if( (newPoint.x > dragStartPoint.x + clickDragZone) ||
			(newPoint.y > dragStartPoint.y + clickDragZone) ||
			(newPoint.x < dragStartPoint.x - clickDragZone) ||
			(newPoint.y < dragStartPoint.y - clickDragZone) ){
			return true;
		}
		else{
			return false;
		}
	}
	
	public void MouseDragBoxCalculations(){
		
		if(MouseDrag){
			// Mouse drag box variables
			boxWidth  = Camera.main.WorldToScreenPoint(mouseDownPoint).x - Camera.main.WorldToScreenPoint(currentMousePoint).x;
			boxHeight = Camera.main.WorldToScreenPoint(mouseDownPoint).y - Camera.main.WorldToScreenPoint(currentMousePoint).y;
			boxLeft   = Input.mousePosition.x;
			boxTop    = (Screen.height - Input.mousePosition.y) - boxHeight;
		}
		
		if(boxWidth > 0f && boxHeight < 0f){
			Debug.Log("cursor is at top left");
			boxStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
		else if (boxWidth > 0f && boxHeight > 0f){
			Debug.Log ("cursor is at bottom left");
			boxStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y + boxHeight);
		}
		else if (boxWidth < 0f && boxHeight < 0f){
			Debug.Log ("cursor is at top right");
			boxStart = new Vector2(Input.mousePosition.x + boxWidth, Input.mousePosition.y);
		}
		else if (boxWidth < 0f && boxHeight > 0f){
			Debug.Log ("cursor is at bottom right");
			boxStart = new Vector2(Input.mousePosition.x + boxHeight, Input.mousePosition.y + boxHeight);
		}
		
		boxFinish = new Vector2(boxStart.x + Mathf.Abs(boxWidth), boxStart.y - Mathf.Abs(boxHeight));
	}
	
	public bool LeftMouseClicked(Vector3 hitpoint){
		if( (mouseDownPoint.x < hitpoint.x + clickDragZone && mouseDownPoint.x > hitpoint.x - clickDragZone) &&
			(mouseDownPoint.y < hitpoint.y + clickDragZone && mouseDownPoint.y > hitpoint.y - clickDragZone) &&
			(mouseDownPoint.z < hitpoint.z + clickDragZone && mouseDownPoint.z > hitpoint.z - clickDragZone) ){
			return true;
		}
		else {
			return false;
		}
	}
	
	// Check if unit is in the array of selected units
	public static bool IsUnitSelected(GameObject unit){
		bool foundUnit = false;
		
		if(CurrentlySelectedUnits.Count > 0){
			foreach(GameObject selectedUnit in CurrentlySelectedUnits){
				if (selectedUnit == unit){
					foundUnit = true;
					break;
				}
			}
		}
		
		return foundUnit;
	}
	
	// Selecting unit
	public void SelectUnit(GameObject unit){
		// activate the selector on the new unit
		hit.collider.transform.FindChild("Selected").gameObject.SetActive(true);
		CurrentlySelectedUnits.Add(unit);
	}
	
	// remove a unit form the selected arraylist
	public void DeselectUnit(GameObject unit){
		foreach(GameObject selectedUnit in CurrentlySelectedUnits){
			if(selectedUnit == unit){
				selectedUnit.transform.FindChild("Selected").gameObject.SetActive(false);
				CurrentlySelectedUnits.Remove(selectedUnit);
				break;
			}
		}
	}
	
	//Deselect all units
	public static void DeselectAllUnits(){
		foreach(GameObject unit in CurrentlySelectedUnits){
			unit.transform.FindChild("Selected").gameObject.SetActive(false);
			unit.GetComponent<Unit>().Selected = false;
		}
		CurrentlySelectedUnits.Clear();
	}
	
	// Player is holding the shift key down
	public static bool HolingShift(){
		bool shiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		return shiftKeyDown;
	}
	
	// Check if the position of the unit is visble on the screen
	public static bool VisibleOnScreen(Vector2 unitScreenPosition){
		bool horizontallyOnScreen = (unitScreenPosition.x > 0f) && (unitScreenPosition.x < Screen.width);
		bool verticallyOnScreen = (unitScreenPosition.y > 0f) && (unitScreenPosition.y < Screen.height);
		
		bool visibleOnScreen = horizontallyOnScreen && verticallyOnScreen;
		return visibleOnScreen;
	}
	
	//Check if unit is inside the drag box
	public static bool IsUnitInsideDragBox(Vector2 unitScreenPosition){
		bool horizontallyInBox = (unitScreenPosition.x > boxStart.x) && (unitScreenPosition.x < boxFinish.x);
		bool verticallyInBox   = (unitScreenPosition.y < boxStart.y) && (unitScreenPosition.y > boxFinish.y);
		
		bool unitInsideDragBox = horizontallyInBox && verticallyInBox;
		return unitInsideDragBox;
	}
	
	
	//Check if unit is in UnitsInDrag array list
	public static bool IsUnitInDraggedList(GameObject unit){
		bool foundUnit = false;
		
		if(UnitsInDrag.Count > 0){
			foreach(GameObject selectedUnit in UnitsInDrag){
				if (selectedUnit == unit){
					foundUnit = true;
					break;
				}
			}
		}
		
		return foundUnit;
	}
	
	//Remove a unit from screen units
	public static void RemoveFromOnScreenUnits(GameObject unit){
		foreach(GameObject unitOnScreen in UnitsOnScreen){
			if(unitOnScreen == unit){
				UnitsOnScreen.Remove(unitOnScreen);
				unitOnScreen.GetComponent<Unit>().OnScreen = false;
				break;
			}
		}
	}
	
	// Put all units from UnitsInDrag list to Selected list
	public static void SelectDraggedUnits(){
		if(!HolingShift()){
			DeselectAllUnits();
		}
		
		if(UnitsInDrag.Count > 0){
			foreach(GameObject unit in UnitsInDrag){
				if(!IsUnitSelected(unit)){
					CurrentlySelectedUnits.Add(unit);
					unit.GetComponent<Unit>().Selected = true;
				}
			}
			
			UnitsInDrag.Clear();
		}
	}
}
