using UnityEngine;
using System.Collections;

public class DestroyParentObject : MonoBehaviour {

	void DestroyParent(){
		Destroy(this.gameObject.transform.parent.gameObject);
	}
}
