using UnityEngine;
using System.Collections;

public class FluidFlow : MonoBehaviour {
	public Texture2D relief;
	public bool showNormalmap = false;
	public float heightScale = 10f;
	public float diffStrength = 1f;
	public int iterations = 5;
	public float influenceRelief = 1f;
	public float influenceDiff = 1f;
	public float influenceLiquid = 1f;
	public float influenceSpread = 0.05f;
	public float turbulence = 0f;
	public int userRadius = 5;
	public float userStrength = 5f;
	public float source = 0.5f;
	
	Texture2D tex;
	int width;
	int height;
	float[,] liquid;
	float[,] ground, riverbed;
	float[,] users;
	float[,] velX, velY;
	int numParticles = 2048;
	Particle[] particles;
	int partInd = 0;
	float[,] partGradX, partGradY;
	int userPosX, userPosY;
	
	void Start() {
		// duplicate the original texture and assign to the material
		tex = Instantiate(GetComponent<Renderer>().material.mainTexture) as Texture2D;
		GetComponent<Renderer>().material.mainTexture = tex;
		width = tex.width;
		height = tex.height;
		// liquid arrays
		liquid = new float[width, height];
		velX = new float[width, height];
		velY = new float[width, height];
		// read ground texture data
		riverbed = new float[width, height];
		ground = new float[width, height];
		users = new float[width, height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				riverbed[x, y] = relief.GetPixel(x, y).grayscale * heightScale;
				ground[x, y] = riverbed[x, y];
				liquid[x, y] = 0.0005f * (width - x);
			}
		}
		// particles
		particles = new Particle[numParticles];
		for (int i = 0; i < numParticles; i++) {
			particles[i].posX = 2;
			particles[i].posY = 2;
		}
		partGradX = new float[width, height];
		partGradY = new float[width, height];
	}
	
	void Update() {
		Source();
		UserInput();
		Fluid();
		VelocityField();
		MoveParticles();
		Draw();
	}
	
	void Source() {
		liquid[width - 10, height / 2] += source;
		ParticleReset(partInd++ % numParticles);
	}
	
	void Fluid() {
		for (int k = 0; k < iterations; k++) {
			for (int y = 1; y < height - 1; y++) {
				for (int x = 1; x < width - 1; x++) {
					float cLiquid = liquid[x, y];
					float cGround = ground[x, y];
					float hDelta;
					int i;
					// flood neighbours if possible
					i = x - 1; // left neighbour
					hDelta = cGround + cLiquid - (ground[i, y] + liquid[i, y]);
					if ((hDelta > 0) && (cLiquid > 0)) {
						float diffuse = Mathf.Min(diffStrength * hDelta * 0.25f, cLiquid);
						liquid[i, y] += diffuse;
						cLiquid -= diffuse;
					}
					i = x + 1; // right neighbour
					hDelta = cGround + cLiquid - (ground[i, y] + liquid[i, y]);
					if ((hDelta > 0) && (cLiquid > 0)) {
						float diffuse = Mathf.Min(diffStrength * hDelta * 0.50f, cLiquid);
						liquid[i, y] += diffuse;
						cLiquid -= diffuse;
					}
					i = y - 1; // bottom neighbour
					hDelta = cGround + cLiquid - (ground[x, i] + liquid[x, i]);
					if ((hDelta > 0) && (cLiquid > 0)) {
						float diffuse = Mathf.Min(diffStrength * hDelta * 0.75f, cLiquid);
						liquid[x, i] += diffuse;
						cLiquid -= diffuse;
					}
					i = y + 1; // top neighbour
					hDelta = cGround + cLiquid - (ground[x, i] + liquid[x, i]);
					if ((hDelta > 0) && (cLiquid > 0)) {
						float diffuse = Mathf.Min(diffStrength * hDelta * 1.00f, cLiquid);
						liquid[x, i] += diffuse;
						cLiquid -= diffuse;
					}
					// sink
					if (ground[x, y] < (0.05f * heightScale)) cLiquid = 0;
					liquid[x, y] = cLiquid;
				}
			}
		}
	}
	
	void VelocityField() {
		// build velocity field (for particle movement)
		for (int y = 1; y < height - 1; y++) {
			for (int x = 1; x < width - 1; x++) {
				float cGround = ground[x, y];
				float cLiquid = liquid[x, y];
				float cHeight = cGround + cLiquid;
				
				float xGradDiff = 0;
				float yGradDiff = 0;
				// velocity gradient from the diffusion
				if (influenceDiff != 0) {
					if (cLiquid > 0) {
						float hDelta;
						int i;
						i = x - 1; // left neighbour
						hDelta = cHeight - (ground[i, y] + liquid[i, y]);
						if (hDelta > 0) xGradDiff -= hDelta;
						i = x + 1; // right neighbour
						hDelta = cHeight - (ground[i, y] + liquid[i, y]);
						if (hDelta > 0) xGradDiff += hDelta;
						i = y - 1; // bottom neighbour
						hDelta = cHeight - (ground[x, i] + liquid[x, i]);
						if (hDelta > 0) yGradDiff -= hDelta;
						i = y + 1; // top neighbour
						hDelta = cHeight - (ground[x, i] + liquid[x, i]);
						if (hDelta > 0) yGradDiff += hDelta;
						// almost the same (but small artifacts):
						/*xGradDiff = (ground[x-1, y] + liquid[x-1, y]) - cHeight;
						yGradDiff = (ground[x, y-1] + liquid[x, y-1]) - cHeight;*/
					}
				}
				// velocity gradient for the level
				cHeight = cGround + cLiquid * influenceLiquid;
				float xGradHeight = (ground[x-1, y] + liquid[x-1, y] * influenceLiquid) - cHeight;
				float yGradHeight = (ground[x, y-1] + liquid[x, y-1] * influenceLiquid) - cHeight;
				// store in array
				velX[x, y] = xGradHeight * influenceRelief + xGradDiff * influenceDiff + partGradX[x, y] * influenceSpread;
				velY[x, y] = yGradHeight * influenceRelief + yGradDiff * influenceDiff + partGradY[x, y] * influenceSpread;
				// reset particle gradients
				partGradX[x, y] = 0;
				partGradY[x, y] = 0;
			}
		}
	}
	
	void UserInput() {
		bool mouseL = Input.GetMouseButton(0);
		bool mouseR = Input.GetMouseButton(1);
		if (mouseL || mouseR) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100)) {
				// determine indices where the user clicked
				int x = (int)(hit.point.x * width);
				int y = (int)(hit.point.z * width);
				if (x < 0 || x > width || y < 0 || y > height) return;
				if (mouseL) {
					// add liquid and partiles
					liquid[x, y] += 10f;
					ParticleReset(partInd++ % numParticles);
				}
				if (mouseR) {
					// build usermap
					UserMap(x, y);
					// combine with relief
					for (int yi = 0; yi < height; yi++) {
						for (int xi = 0; xi < width; xi++) {
							ground[xi, yi] = riverbed[xi, yi] + users[xi, yi];
						}
					}
				}
			}
		}
	}
	
	void UserMap(int x, int y) {
		// clear map
		for (int yi = 0; yi < height; yi++) {
			for (int xi = 0; xi < width; xi++) {
				users[xi, yi] = 0;
			}
		}
		// circle around
		for (int yi = y-userRadius; yi < (y+userRadius); yi++) {
			for (int xi = x-userRadius; xi < (x+userRadius); xi++) {
				// only apply withing valid riverbed area
				if ((xi >= 0) && (xi < width) && (yi >= 0) && (yi < height)) {
					float dist = Mathf.Sqrt((yi-y)*(yi-y)+(xi-x)*(xi-x));
					float rf = (float)userRadius;
					if (dist <= rf) {
						float lift = 1-dist/rf;
						users[xi, yi] += Mathf.Min(lift*userStrength, userStrength * 0.2f);
					}
				}
			}
		}
	}
	
	void Draw() {
		// visualize ground and liquid
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				if (showNormalmap) {
					tex.SetPixel(x, y, new Color(velX[x, y] + 0.5f, velY[x, y] + 0.5f, 1)); // normal map
				} else {
					float cLiquid = liquid[x, y];
					float cGround = ground[x, y] / heightScale;
					tex.SetPixel(x, y, new Color(cGround, cGround + cLiquid * 0.5f, cGround + cLiquid));
				}
			}
		}
		// visualize particles
		foreach (Particle p in particles) {
			tex.SetPixel((int)p.posX, (int)p.posY, new Color(1, 1, 1));
		}
		// apply changes to texture
		tex.Apply(false);
	}
	
	void ParticleReset(int i) {
		particles[i].posX = width - 20 + Random.Range(-3, 3);
		particles[i].posY = height / 2 + Random.Range(-3, 3);
		particles[i].velX = 0;
		particles[i].velY = 0;
	}
	
	void MoveParticles() {
		for (int i = 0; i < numParticles; i++) {
			float pX = particles[i].posX;
			float pY = particles[i].posY;
			int x = Mathf.Clamp((int)pX, 1, width-2);
			int y = Mathf.Clamp((int)pY, 1, height-2);
			// particles influenced by interpolated velocity
			particles[i].AddForce(velX[x, y], velY[x, y]);
			if (pX < 1 || pX > width-2 || pY < 1 || pY > height-2) {
				ParticleReset(i);
			}
			// particles transport fluid (seems strange, like turbulence)
			if (turbulence != 0) {
				int indX = (particles[i].velX > 0) ? x+1 : x-1;
				int indY = (particles[i].velY > 0) ? y+1 : y-1;
				float minLiq = Mathf.Min(Mathf.Min(Mathf.Min(liquid[x, y], liquid[indX, y]), liquid[x, indY]), turbulence);
				liquid[x, y] += minLiq; liquid[indX, y] -= minLiq;
				liquid[x, y] += minLiq; liquid[x, indY] -= minLiq;
			}
			// particles make repulsive forces around them (unfortunately, this influences also them self)
			if (influenceSpread != 0) {
				partGradX[x, y] += -velX[x, y];
				partGradY[x, y] += -velY[x, y];
				partGradX[x-1, y] = partGradY[x, y-1] += -0.2f;
				partGradX[x+1, y] = partGradY[x, y+1] += 0.2f;
				partGradX[x-1, y-1] = partGradY[x-1, y-1] = partGradX[x-1, y+1] = partGradY[x+1, y-1] += -0.1f;
				partGradX[x+1, y+1] = partGradY[x+1, y+1] = partGradY[x-1, y+1] = partGradX[x+1, y-1] += 0.1f;
			}
		}
	}
	
	struct Particle {
		public float posX, posY;
		public float velX, velY;
		
		const float inertia = 0.9f;
		
		public void AddForce(float forceX, float forceY) {
			velX += forceX; //Mathf.Clamp(forceX, -2, 2);
			velY += forceY; //Mathf.Clamp(forceY, -2, 2);
			posX += velX;
			posY += velY;
			velX *= inertia;
			velY *= inertia;
		}
	}
}