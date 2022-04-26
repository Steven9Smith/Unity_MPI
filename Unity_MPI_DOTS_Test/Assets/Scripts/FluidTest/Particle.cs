using UnityEngine;
using System.Collections;

public class Particle : MonoBehaviour {
	public float lifeTime = 60;
	public float speed = 1;
	public float inertia = 0.0f;
	public float velX, velY;

	float[] vFieldX, vFieldY;
	int width, height;

	public void Init(float[] velFieldX, float[] velFieldY, int w, int h) {
		vFieldX = velFieldX;
		vFieldY = velFieldY;
		width = w;
		height = h;
		Destroy(gameObject, lifeTime);
	}

	void Update() {
		Vector3 pos = transform.position;
		float posX = pos.x;
		float posY = pos.z;
		// destroy if not on grid
		if (posX < 2 || posX > (width - 2) || posY < 2 || posY > (height - 2)) {
			Destroy(gameObject);
			return;
		}
		// move
		velX += Bilin(vFieldX, posX, posY);
		velY += Bilin(vFieldY, posX, posY);
		transform.Translate(velX*speed, velY*speed, 0);
		velX *= inertia;
		velY *= inertia;
	}

	float Bilin(float[] field, float xPos, float yPos) {
		xPos -= 0.5f;
		yPos -= 0.5f;
		// casting to int is like floor
		int xInt = (int)xPos;
		int yInt = (int)yPos;
		// so diff is always positive
		float xDiff = xPos - xInt;
		float yDiff = yPos - yInt;
		// bilinear interpolation
		float cRow = (1 - xDiff) * field[xInt+yInt*width] + xDiff * field[xInt+yInt*width+1];
		float nRow = (1 - xDiff) * field[xInt+(yInt+1)*width] + xDiff * field[xInt+(yInt+1)*width+1];
		return (1 - yDiff) * cRow + yDiff * nRow;
	}
}
