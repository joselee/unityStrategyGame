using UnityEngine;
using System.Collections;

public class WorldCamera : MonoBehaviour {
	
	
	// Screen boundaries for the mouse
	public struct BoxLimit {
		public float LimitLeft;
		public float LimitRight;
		public float LimitTop;
		public float LimitBottom;
	}
	
	public static BoxLimit cameraLimits = new BoxLimit();
	public static BoxLimit mouseScrollLimits = new BoxLimit();
	public static WorldCamera Instance;
	
	private float cameraMoveSpeed = 10.0f;
	private float mouseBoundary = 20.0f;
	
	void Awake(){
		Instance = this;
	}
	
	void Start () {
		// How far can the camera move?
		cameraLimits.LimitLeft = 10.5f;
		cameraLimits.LimitRight = 15.0f;
		cameraLimits.LimitTop = 18.5f;
		cameraLimits.LimitBottom = 6.0f;
		
		// When does the mouse tell the game to move the camera?
		mouseScrollLimits.LimitLeft = mouseBoundary;
		mouseScrollLimits.LimitRight = mouseBoundary;
		mouseScrollLimits.LimitTop = mouseBoundary;
		mouseScrollLimits.LimitBottom = mouseBoundary;
	}
	
	// Update is called once per frame
	void Update () {
		string inputDirection = getInputDirection();
		
		if(inputDirection != "none"){
			Vector3 scrollDirection = getScrollDirection(inputDirection);
			
			if(!movingOutOfBounds(scrollDirection)){
				this.transform.Translate (scrollDirection);
			}
		}
	}
	
	private string getInputDirection(){
		
		bool mouseUp    = Input.mousePosition.y > (Screen.height - mouseScrollLimits.LimitTop) && Input.mousePosition.y < (Screen.height);
		bool mouseDown  = Input.mousePosition.y < mouseScrollLimits.LimitBottom  && Input.mousePosition.y >= 0;
		bool mouseLeft  = Input.mousePosition.x < mouseScrollLimits.LimitLeft && Input.mousePosition.x > -5;
		bool mouseRight = Input.mousePosition.x > (Screen.width - mouseScrollLimits.LimitRight) && Input.mousePosition.x < (Screen.width + 5);
		
		bool up    = Input.GetKey (KeyCode.UpArrow)    || mouseUp;
		bool down  = Input.GetKey (KeyCode.DownArrow)  || mouseDown;
		bool left  = Input.GetKey (KeyCode.LeftArrow)  || mouseLeft;
		bool right = Input.GetKey (KeyCode.RightArrow) || mouseRight;
		
		if(up){
			return "up";
		}
		else if (down){
			return "down";
		}
		else if (left){
			return "left";
		}
		else if (right){
			return "right";
		}
		else{
			return "none";
		}
	}
	
	private Vector3 getScrollDirection(string inputDirection){
		float moveSpeed = cameraMoveSpeed * Time.deltaTime;
		float desiredX = 0f;
		float desiredZ = 0f;
		
		// Move faster if shift is being pressed
		bool holdingShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		if(holdingShift){
			moveSpeed = moveSpeed * 2.0f;
		}
		
		
		if(inputDirection == "up"){
			desiredZ = moveSpeed;
		}
		if(inputDirection == "down"){
			desiredZ = -moveSpeed;
		}
		if(inputDirection == "left"){
			desiredX = -moveSpeed;
		}
		if(inputDirection == "right"){
			desiredX = moveSpeed;
		}
		
		return new Vector3(desiredX, desiredZ, 0);
	}
	
	// Check if the direction we want to scroll is past the boundary
	private bool movingOutOfBounds(Vector3 scrollDirection){
		float cameraPositionX = this.transform.position.x;
		float cameraPositionZ = this.transform.position.z;
		
		// Z and Y are behaving strangely. They are inversed between code and unity.
		bool outOfBounds = (cameraPositionZ + scrollDirection.y) > cameraLimits.LimitTop    ||
						   (cameraPositionZ + scrollDirection.y) < cameraLimits.LimitBottom ||
					   	   (cameraPositionX + scrollDirection.x) < cameraLimits.LimitLeft   ||
						   (cameraPositionX + scrollDirection.x) > cameraLimits.LimitRight;
		
		return outOfBounds;
	}
}
