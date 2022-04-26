using UnityEngine;
using System.Collections;

public class VelocityField : MonoBehaviour {
	public float visc = 0.01f;
	public int iterations = 10;
	public float particleSpeed = 100;
	/*public Texture2D border;
	public Texture2D flow;*/
	
	Texture2D tex;
	int width, height;
	float[,] u, v, u_prev, v_prev;
	int numParticles = 4048;
	Particle[] particles;
	/*float[,] bndX, bndY;
	float[,] velX, velY;*/

	void Start() {
		// duplicate the original texture and assign to the material
		tex = Instantiate(GetComponent<Renderer>().material.mainTexture) as Texture2D;
		GetComponent<Renderer>().material.mainTexture = tex;
		// get grid dimensions from texture
		width = tex.width;
		height = tex.height;
		// initialize velocity arrays
		u = new float[width, height];
		v = new float[width, height];
		u_prev = new float[width, height];
		v_prev = new float[width, height];
		// random position for particles
		particles = new Particle[numParticles];
		for (int i = 0; i < numParticles; i++) {
			particles[i].x = Random.Range(1, width-2);
			particles[i].y = Random.Range(1, height-2);
		}
		/*// read border and velocity data from textures
		bndX = new float[width, height];
		bndY = new float[width, height];
		velX = new float[width, height];
		velY = new float[width, height];
		for (int j = 0; j < height-1; j++) {
			for (int i = 0; i < width-1; i++) {
				bndX[i, j] = border.GetPixel(i, j).grayscale - border.GetPixel(i+1, j).grayscale;
				bndY[i, j] = border.GetPixel(i, j).grayscale - border.GetPixel(i, j+1).grayscale;
				velX[i, j] = (flow.GetPixel(i, j).grayscale - flow.GetPixel(i+1, j).grayscale);
				velY[i, j] = (flow.GetPixel(i, j).grayscale - flow.GetPixel(i, j+1).grayscale);
			}
		}*/
	}
	
	void Update() {
		/*for (int j = 1; j < height-1; j++) {
			for (int i = 1; i < width-1; i++) {
				u[i, j] = velX[i, j];
				v[i, j] = velY[i, j];
			}
		}*/
		UserInput();
		VelStep();
		MoveParticles();
		Draw();
	}

	void SetBounds(int b, float[,] x) {
		/*// b/w texture as obstacles
		for (int j = 1; j < height; j++) {
			for (int i = 1; i < width; i++) {
				if (bndX[i, j] < 0) {
					x[i, j] = (b == 1) ? -x[i+1, j] : x[i+1, j];
				}
				if (bndX[i, j] > 0) {
					x[i, j] = (b == 1) ? -x[i-1, j] : x[i-1, j];
				}
				if (bndY[i, j] < 0) {
					x[i, j] = (b == 2) ? -x[i, j+1] : x[i, j+1];
				}
				if (bndY[i, j] > 0) {
					x[i, j] = (b == 2) ? -x[i, j-1] : x[i, j-1];
				}
			}
		}*/
		// rect borders
		float sign;
		// left/right: reflect if b is 1, else keep value before edge
		sign = (b == 1) ? -1 : 1;
		for (int i = 1; i < height-1; i++) {
			x[0, i] = sign * x[1, i];
			x[width-1, i] = sign * x[width-2, i];
		}
		// bottom/top: reflect if b is 2, else keep value before edge
		sign = (b == 2) ? -1 : 1;
		for (int i = 1; i < width-1; i++) {
			x[i, 0] = sign * x[i, 1];
			x[i, height-1] = sign * x[i, height-2];
		}
		// vertices
		x[0, 0]				 = 0.5f * (x[1, 0] + x[0, 1]);
		x[width-1, 0]		 = 0.5f * (x[width-2, 0] + x[width-1, 1]);
		x[0, height-1]		 = 0.5f * (x[1, height-1] + x[0, height-2]);
		x[width-1, height-1] = 0.5f * (x[width-2, height-1] + x[width-1, height-2]);
	}

	void Diffuse(int b, float[,] x, float[,] x0, float a, float c) {
		for (int k = 0; k < iterations; k++) {
			for (int j = 1; j < height-1; j++) {
				for (int i = 1; i < width-1; i++) {
					x[i, j] = (x0[i, j] + a * (x[i-1, j] + x[i+1, j] + x[i, j-1] + x[i, j+1])) / c;
				}
			}
			SetBounds(b, x);
		}
	}
	
	void Project(float[,] u, float[,] v, float[,] p, float[,] div) {
		float h = 1 / Mathf.Sqrt(width * height);
		for (int j = 1; j < height-1; j++ ) {
			for (int i = 1; i < width-1; i++) {
				div[i, j] = -0.5f * h * (u[i+1, j] - u[i-1, j] + v[i, j+1] - v[i, j-1]);
				p[i, j] = 0;
			}
		}
		SetBounds(0, div);
		SetBounds(0, p);
		
		Diffuse(0, p, div, 1, 4);

		for (int j = 1; j < height-1; j++) {
			for (int i = 1; i < width-1; i++) {
				u[i, j] -= 0.5f * (p[i+1, j] - p[i-1, j]) / h;
				v[i, j] -= 0.5f * (p[i, j+1] - p[i, j-1]) / h;
			}
		}
		SetBounds(1, u);
		SetBounds(2, v);
	}

	void VelStep() {
		float a = Time.deltaTime * visc * width * height;
		Diffuse(1, u, u, a, 1 + 4 * a);
		Diffuse(2, v, v, a, 1 + 4 * a);
		Project(u, v, u_prev, v_prev);
	}
	
	void UserInput() {
		if (Input.GetMouseButton(0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100)) {
				// determine indices where the user clicked
				int x = (int)(hit.point.x * width);
				int y = (int)(hit.point.z * width);
				if (x < 1 || x >= width || y < 1 || y >= height) return;
				// add velocity
				u[x, y] += Input.GetAxis("Mouse X");
				v[x, y] += Input.GetAxis("Mouse Y");
			}
		}
	}
	
	void Draw() {
		// visualize water
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				tex.SetPixel(x, y, new Color(u[x, y]*20 + 0.5f, v[x, y]*20 + 0.5f, 1));
			}
		}
		// visualize particles
		foreach (Particle p in particles) {
			tex.SetPixel((int)p.x, (int)p.y, new Color(1, 1, 1));
		}
		tex.Apply(false);
	}

	void MoveParticles() {
		for (int i = 0; i < numParticles; i++) {
			float pX = particles[i].x;
			float pY = particles[i].y;
			particles[i].x = pX += Bilin(u, pX, pY) * particleSpeed;
			particles[i].y = pY += Bilin(v, pX, pY) * particleSpeed;
			if (pX < 1 || pX > width-2 || pY < 1 || pY > height-2) {
				particles[i].x = Random.Range(1, width-2);
				particles[i].y = Random.Range(1, height-2);
			}
		}
	}

	float Bilin(float[,] field, float xPos, float yPos) {
		xPos -= 0.5f;
		yPos -= 0.5f;
		// casting to int is like floor
		int xInt = Mathf.Clamp((int)xPos, 1, width-2);
		int yInt = Mathf.Clamp((int)yPos, 1, height-2);
		// so diff is always positive
		float xDiff = xPos - xInt;
		float yDiff = yPos - yInt;
		// bilinear interpolation
		float cRow = (1 - xDiff) * field[xInt, yInt] + xDiff * field[xInt+1, yInt];
		float nRow = (1 - xDiff) * field[xInt, yInt+1] + xDiff * field[xInt+1, yInt+1];
		return (1 - yDiff) * cRow + yDiff * nRow;
	}

	struct Particle {
		public float x;
		public float y;
	}
}
