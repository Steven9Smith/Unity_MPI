using UnityEngine;
using System.Collections;

public class WaterOnTerrain : MonoBehaviour
{
	public Texture2D relief;
	public float heightScale = 10;
	public float flow = 1.5f;
	public int iterations = 5;
	
	Texture2D tex;
	int width;
	int height;
	float[,] liquid;
	float[,] ground;
	
	void Start()
	{
		// duplicate the original texture and assign to the material
		tex = Instantiate(GetComponent<Renderer>().material.mainTexture) as Texture2D;
		GetComponent<Renderer>().material.mainTexture = tex;
		width = tex.width;
		height = tex.height;

		liquid = new float[width, height];
		// read ground texture data
		ground = new float[width, height];
		for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				ground[x, z] = relief.GetPixel(x, z).grayscale * heightScale;
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
		for (int k = 0; k < iterations; k++) {
			for (int z = 1; z < height - 1; z++) {
				for (int x = 1; x < width - 1; x++) {
					float cLiquid = liquid[x, z];
					float cGround = ground[x, z];
					float nHeight, hDiff;
					int i;
					i = x - 1; // left neighbour
					nHeight = ground[i, z] + liquid[i, z];
					hDiff = cGround + cLiquid - nHeight;
					if ((hDiff > 0) && (cLiquid > 0)) {
						float diffuse = Mathf.Min(flow * hDiff * 0.25f, cLiquid);
						liquid[i, z] += diffuse;
						cLiquid -= diffuse;
					}
					i = x + 1; // right neighbour
					nHeight = ground[i, z] + liquid[i, z];
					hDiff = cGround + cLiquid - nHeight;
					if ((hDiff > 0) && (cLiquid > 0)) {
						float diffuse = Mathf.Min(flow * hDiff * 0.50f, cLiquid);
						liquid[i, z] += diffuse;
						cLiquid -= diffuse;
					}
					i = z - 1; // bottom neighbour
					nHeight = ground[x, i] + liquid[x, i];
					hDiff = cGround + cLiquid - nHeight;
					if ((hDiff > 0) && (cLiquid > 0)) {
						float diffuse = Mathf.Min(flow * hDiff * 0.75f, cLiquid);
						liquid[x, i] += diffuse;
						cLiquid -= diffuse;
					}
					i = z + 1; // top neighbour
					nHeight = ground[x, i] + liquid[x, i];
					hDiff = cGround + cLiquid - nHeight;
					if ((hDiff > 0) && (cLiquid > 0)) {
						float diffuse = Mathf.Min(flow * hDiff * 1.00f, cLiquid);
						liquid[x, i] += diffuse;
						cLiquid -= diffuse;
					}
					// sink
					if (ground[x, z] < (0.05f * heightScale)) cLiquid = 0;
					liquid[x, z] = cLiquid;
				}
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
				liquid[x, z] += 10f;
			}
		}
	}
	
	void Draw() {
		// visualize water
		for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				float cLiquid = liquid[x, z];
				float cGround = ground[x, z] / heightScale;
				tex.SetPixel(x, z, new Color(cGround, cGround + cLiquid * 0.5f, cGround + cLiquid));
			}
		}
		tex.Apply(false);
	}
}