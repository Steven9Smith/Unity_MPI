using UnityEngine;
using System.Collections;

public class TestBilin : MonoBehaviour {
	public Transform picker;

	Texture2D tex;
	int width, height;
	Color[,] colors;

	void Start() {
		// duplicate the original texture and assign to the material
		tex = Instantiate(GetComponent<Renderer>().material.mainTexture) as Texture2D;
		GetComponent<Renderer>().material.mainTexture = tex;
		width = tex.width;
		height = tex.height;
		// scale platform
		transform.position = new Vector3(0.5f * width, 0, 0.5f * height);
		transform.localScale = new Vector3((float)width, (float)height, 1);
		// zoom camera on platform
		Camera cam = Camera.main;
		cam.orthographicSize = width * 0.5f * Screen.height / Screen.width;
		cam.transform.position = new Vector3(0.5f * width, 10, 0.5f * height);
		// initialize color array
		colors = new Color[width, height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				colors[x, y] = tex.GetPixel(x, y);
			}
		}
	}

	void Update() {
		float fX = picker.position.x;
		float fY = picker.position.z;
		if (fX < 1 || fY < 1 || fX > width-1 || fY > height-1) {
			picker.position = new Vector3(1, 1, 1);
			return;
		}
		float speed = Time.deltaTime;
		picker.Translate(Input.GetAxis("Horizontal") * speed, Input.GetAxis("Vertical") * speed, 0);
		picker.GetComponent<Renderer>().material.color = Bilin(colors, fX, fY);
	}

	Color Bilin(Color[,] field, float xPos, float yPos) {
		// casting to int is like floor
		int xInt = (int)(xPos - 0.5f);
		int yInt = (int)(yPos - 0.5f);
		// so diff is always positive
		float xDiff = (xPos - 0.5f) - xInt;
		float yDiff = (yPos - 0.5f) - yInt;
		// bilinear interpolation
		Color cRow = (1 - xDiff) * field[xInt, yInt] + xDiff * field[xInt + 1, yInt];
		Color nRow = (1 - xDiff) * field[xInt, yInt + 1] + xDiff * field[xInt + 1, yInt + 1];
		return (1 - yDiff) * cRow + yDiff * nRow;
	}
}
