using UnityEngine;
using System.Collections;

public class LiquidRipples : MonoBehaviour
{
	struct WaterParticle
	{
		public float height;
		float speed;
		
		public void AddForce(float force)
		{
			speed += force;
		}
		
		public void Update()
		{
			height += speed;
			speed *= 0.8f;
		}
	}

	Texture2D tex;
	int width;
	int height;
	float level = 0.5f;
	const float viscosity = 0.07f;
	WaterParticle[,] particles;
	
	void Start()
	{
		// duplicate the original texture and assign to the material
		tex = Instantiate(GetComponent<Renderer>().material.mainTexture) as Texture2D;
		GetComponent<Renderer>().material.mainTexture = tex;
		width = tex.width;
		height = tex.height;

		// read initial water heights
		particles = new WaterParticle[width, height];
		for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				particles[x, z].height = level; //tex.GetPixel(x, z).grayscale;
			}
		}
	}
	
	void Update()
	{
		UserInput();
		Fluid();
		Draw();
	}
	
	void Fluid() {
		for (int z = 1; z < height - 1; z++) {
			for (int x = 1; x < width - 1; x++) {
				float hDiff = 0;
				float hForce = 0;
				// influences of neighbours
				hDiff = particles[x - 1, z].height - particles[x, z].height;
				hForce += viscosity * hDiff;
				hDiff = particles[x + 1, z].height - particles[x, z].height;
				hForce += viscosity * hDiff;
				hDiff = particles[x, z - 1].height - particles[x, z].height;
				hForce += viscosity * hDiff;
				hDiff = particles[x, z + 1].height - particles[x, z].height;
				hForce += viscosity * hDiff;
				// influence of normal waterlevel
				hDiff = level - particles[x, z].height;
				hForce += viscosity * hDiff;
				// apply force and update
				particles[x, z].AddForce(hForce);
				particles[x, z].Update();
			}
		}
	}

	void UserInput() {
		// draw on the water
		if (Input.GetMouseButton(0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100)) {
				// determine indices where the user clicked
				int x = (int)(hit.point.x * width);
				int z = (int)(hit.point.z * width);
				if (x < 0 || x > width || z < 0 || z > height) return;
				particles[x, z].AddForce(0.3f);
			}
		}
	}

	void Draw() {
		// visualize water
		for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				float cHeight = particles[x, z].height;
				tex.SetPixel(x, z, new Color(cHeight, cHeight, cHeight));
			}
		}
		tex.Apply(false);
	}
}