using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {

	public GameObject[] fluidObjs;

	public void LoadObj(int iObj) {
		// reset camera
		Camera cam = Camera.main;
		cam.orthographicSize = 0.5f * Screen.height / Screen.width;
		cam.transform.position = new Vector3(0.5f, 10, 0.125f);
		// turn on the selected, turn off the others
		for (int i = 0; i < fluidObjs.Length; i++) {
			fluidObjs[i].SetActive((i == iObj) ? true : false);
		}
	}

	public void Reset() {
		Application.LoadLevel(Application.loadedLevel);
	}
}
