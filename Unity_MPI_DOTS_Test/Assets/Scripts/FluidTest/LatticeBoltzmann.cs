using UnityEngine;
using System.Collections;

public class LatticeBoltzmann : MonoBehaviour {
	public Texture2D obstacles;
	public bool showVel = false;
	public bool showDens = true;
	public Particle part;

	Texture2D tex;

	// Definition of D2Q9 lattice
	/*
	6   2   5
	  \ | /
	3 - 0 - 1
	  / | \
	7   4   8
	*/
	int[] ex = {0, 1, 0, -1, 0, 1, -1, -1, 1};
	int[] ey = {0, 0, 1, 0, -1, 1, 1, -1, -1};
	int Q = 9;
	float[] rt = {4f/9, 1f/9, 1f/9, 1f/9, 1f/9, 1f/36, 1f/36, 1f/36, 1f/36};
	float cs2 = 1f/3;

	bool[] obst;
	float[] f, ftemp;
	float[] density;
	float[] ux, uy;
	
	float density0 = 1;
	float ux0;
	float omegaf;
	int t;
	int lx, ly;

	void Start() {
		// duplicate the original texture and assign to the material
		tex = Instantiate(GetComponent<Renderer>().material.mainTexture) as Texture2D;
		GetComponent<Renderer>().material.mainTexture = tex;
		// get grid dimensions from texture
		lx = tex.width;
		ly = tex.height;
		// scale platform
		transform.position = new Vector3(0.5f*lx, 0, 0.5f*ly);
		transform.localScale = new Vector3((float)lx, (float)ly, 1);
		// zoom camera on platform
		Camera cam = Camera.main;
		cam.orthographicSize = lx*0.5f*Screen.height/Screen.width;
		cam.transform.position = new Vector3(0.5f*lx, 10, 0.5f*ly);

		// initializations for LBM
		Initialization();
	}

	void Update() {
		UserInput();
		Propagation(f, ftemp); // LHS of Boltzmann equation
		Boundary(ux0, f, ftemp, obst);	// boundary conditions
		CalcMacroQuantities(density, ux, uy, ftemp, obst); // calculation of macroscopic quantities
		CollisionSRT(density, ux, uy, f, ftemp, obst, omegaf); // RHS of Boltzmann equation: SRT
		Draw();
	}

	void Initialization() {
		// initialize fluid parameters
		float reynolds = 100f;
		float kin_visc_lb = 0.05f;
		ux0 = reynolds * kin_visc_lb / (ly-1);
		omegaf = 1f / (3f * kin_visc_lb + 0.5f);
		
		// declare fluid arrays
		int gridSize = lx * ly;
		int fullSize = gridSize * Q;
		obst = new bool[gridSize];
		f = new float[fullSize];
		ftemp = new float[fullSize];
		density = new float[gridSize];
		ux = new float[gridSize];
		uy = new float[gridSize];
		
		// initialize arrays
		for (int y = 0; y < ly; ++y) {
			for (int x = 0; x < lx; ++x) {
				int pos = x+lx*y;
				obst[pos] = obstacles.GetPixel(x, y).grayscale > 0.5f ? true : false;
				density[pos] = density0;
				ux[pos] = ux0;
				uy[pos] = 0f;
				float u2 = ux[pos]*ux[pos] + uy[pos]*uy[pos];
				for (int i = 0; i < Q; ++i) {
					int posQ = Q*pos+i;
					f[posQ] = ftemp[posQ] = density[pos]*rt[i]*(1f + (ex[i]*ux[pos]+ey[i]*uy[pos])/cs2 + (ex[i]*ux[pos]+ey[i]*uy[pos])*(ex[i]*ux[pos]+ey[i]*uy[pos])/(2f*cs2*cs2) - u2/(2f*cs2));
				}
			}
		}
	}

	void Propagation(float[] f, float[] ftemp) {
		for (int y = 0; y < ly; ++y) {
			for (int x = 0; x < lx; ++x) {
				int pos = x+lx*y;
				
				int y_n = (y == ly-1) ? -1 : y+1; // avoiding periodic bc
				int x_e = (x == lx-1) ? -1 : x+1; // avoiding periodic bc
				int y_s = (y == 0   ) ? -1 : y-1; // avoiding periodic bc
				int x_w = (x == 0   ) ? -1 : x-1; // avoiding periodic bc
				
				ftemp[Q*pos] = f[Q*pos];
				if (x_e!=-1)			{ftemp[Q*(x_e  + y   *lx) + 1] = f[Q*pos+1];}
				if (y_n!=-1)			{ftemp[Q*(x    + y_n *lx) + 2] = f[Q*pos+2];}
				if (x_w!=-1)			{ftemp[Q*(x_w  + y   *lx) + 3] = f[Q*pos+3];}
				if (y_s!=-1)			{ftemp[Q*(x    + y_s *lx) + 4] = f[Q*pos+4];}
				if (x_e!=-1 && y_n!=-1)	{ftemp[Q*(x_e  + y_n *lx) + 5] = f[Q*pos+5];}
				if (x_w!=-1 && y_n!=-1)	{ftemp[Q*(x_w  + y_n *lx) + 6] = f[Q*pos+6];}
				if (x_w!=-1 && y_s!=-1)	{ftemp[Q*(x_w  + y_s *lx) + 7] = f[Q*pos+7];}
				if (x_e!=-1 && y_s!=-1)	{ftemp[Q*(x_e  + y_s *lx) + 8] = f[Q*pos+8];}
			}
		}
	}

	void Boundary(float ux0, float[] f, float[] ftemp, bool[] obst) {
		int x, y, pos;
		float density_loc;
		float aa = 1f, ab = 0f; // only first order accuracy
		//float aa = 2f, ab = -1f; // second order accuracy
		
		// momentum at left end of domain (inlets)
		x = 0; // velocity inlet at top of the domain, Zou and He
		for (y = 1; y < ly-1; ++y) {
			pos = x+lx*y;
			float vf = ux0;
			//float vf = ux0*1.5f*(4f*(y-0.5f)/(ly-2) - (2f*(y-0.5f)/(ly-2))*(2f*(y-0.5f)/(ly-2))); // parabolic profile
			//float vf = ux0*1.5f*(4f*y/(ly-1) - (2f*y/(ly-1))*(2f*y/(ly-1))); // parabolic profile
			//float vf = ux0*3f/2*(2f*y/(ly-1) - (1f*y/(ly-1))*(1f*y/(ly-1))); // Nusselt's velocity profile
			float ru = (ftemp[Q*pos+0] + ftemp[Q*pos+2] + ftemp[Q*pos+4] + 2f*(ftemp[Q*pos+3] + ftemp[Q*pos+6] + ftemp[Q*pos+7]))/(1f-vf)*vf;
			ftemp[Q*pos+1] = ftemp[Q*pos+3]+2f/3*ru;
			ftemp[Q*pos+5] = ftemp[Q*pos+7]+1f/6*ru+0.5f*(ftemp[Q*pos+4]-ftemp[Q*pos+2]);
			ftemp[Q*pos+8] = ftemp[Q*pos+6]+1f/6*ru+0.5f*(ftemp[Q*pos+2]-ftemp[Q*pos+4]);
		}

		// momentum at right end of the domain (outlet)
		x = lx-1;
		for (y = 0; y < ly; ++y) {
			pos = x+lx*y;
			for (int i = 0; i < Q; i++) {
				ftemp[Q*pos+i] = aa*ftemp[Q*pos-Q+i] + ab*ftemp[Q*pos-2*Q+i]; // takes values from x=lx-2
			}
		}

		// momentum bounce back at top wall
		y = ly-1;
		for(x = 1; x < lx-1; ++x) {
			pos = x+y*lx;
			ftemp[Q*pos+4] = ftemp[Q*pos+2];
			ftemp[Q*pos+7] = ftemp[Q*pos+5];
			ftemp[Q*pos+8] = ftemp[Q*pos+6];
		}
		
		// momentum bounce back at bottom wall
		y = 0;
		for(x = 1; x < lx-1; ++x) {
			pos = x+y*lx;
			ftemp[Q*pos+2] = ftemp[Q*pos+4];
			ftemp[Q*pos+5] = ftemp[Q*pos+7];
			ftemp[Q*pos+6] = ftemp[Q*pos+8];
		}

		// south-west corner of the inlet has to be defined
		pos = lx; // = 0+lx*1
		density_loc = 0f;
		for(int i = 0; i < Q; ++i) {
			density_loc += ftemp[Q*pos+i];
		}
		pos = 0;
		ftemp[Q*pos+2] = ftemp[Q*pos+4];
		ftemp[Q*pos+1] = ftemp[Q*pos+3];
		ftemp[Q*pos+5] = ftemp[Q*pos+7];
		ftemp[Q*pos+6] = 0.5f*(density_loc - ftemp[Q*pos] - 2f*(ftemp[Q*pos+2] + ftemp[Q*pos+1] + ftemp[Q*pos+5]));
		ftemp[Q*pos+8] = ftemp[Q*pos+6];

		// north-west corner of the inlet has to be defined
		pos = lx*(ly-2); // = 0+lx*(ly-2)
		density_loc = 0f;
		for(int i = 0; i < Q; ++i) {
			density_loc += ftemp[Q*pos + i];
		}
		pos = lx*(ly-1);
		ftemp[Q*pos+4] = ftemp[Q*pos+2];
		ftemp[Q*pos+1] = ftemp[Q*pos+3];
		ftemp[Q*pos+8] = ftemp[Q*pos+6];
		ftemp[Q*pos+7] = 0.5f*(density_loc - ftemp[Q*pos] - 2f*(ftemp[Q*pos+2] + ftemp[Q*pos+3] + ftemp[Q*pos+6]));
		ftemp[Q*pos+5] = ftemp[Q*pos+7];

		// south-east corner of the outlet has to be defined
		pos = (lx-1)+lx; // = (lx-1)+lx*1
		density_loc = 0f;
		for(int i = 0; i < Q; ++i) {
			density_loc += ftemp[Q*pos+i];
		}
		pos = lx-1; // = (lx-1)+lx*0
		ftemp[Q*pos+2] = ftemp[Q*pos+4];
		ftemp[Q*pos+3] = ftemp[Q*pos+1];
		ftemp[Q*pos+6] = ftemp[Q*pos+8];
		ftemp[Q*pos+5] = 0.5f*(density_loc - ftemp[Q*pos] - 2f*(ftemp[Q*pos+2] + ftemp[Q*pos+3] + ftemp[Q*pos+6]));
		ftemp[Q*pos+7] = ftemp[Q*pos+5];

		// north-east corner of the inlet has to be defined
		pos = (lx-1)+lx*(ly-2); // = (lx-1)+lx*(ly-2)
		density_loc = 0f;
		for(int i = 0; i < Q; ++i) {
			density_loc += ftemp[Q*pos+i];
		}
		pos = (lx-1)+lx*(ly-1);
		ftemp[Q*pos+4] = ftemp[Q*pos+2];
		ftemp[Q*pos+3] = ftemp[Q*pos+1];
		ftemp[Q*pos+7] = ftemp[Q*pos+5];
		ftemp[Q*pos+8] = 0.5f*(density_loc - ftemp[Q*pos] - 2f*(ftemp[Q*pos+4] + ftemp[Q*pos+3] + ftemp[Q*pos+7]));
		ftemp[Q*pos+6] = ftemp[Q*pos+8];

		// default bounce back at all inner wall nodes
		for (y = 1; y < ly-1; ++y) {
			for (x = 1; x < lx-1; ++x) {
				pos = x+lx*y;
				if (obst[pos]) {
					// bounce-back at all inner obstacle nodes
					f[Q*pos+1] = ftemp[Q*pos+3];
					f[Q*pos+2] = ftemp[Q*pos+4];
					f[Q*pos+3] = ftemp[Q*pos+1];
					f[Q*pos+4] = ftemp[Q*pos+2];
					f[Q*pos+5] = ftemp[Q*pos+7];
					f[Q*pos+6] = ftemp[Q*pos+8];
					f[Q*pos+7] = ftemp[Q*pos+5];
					f[Q*pos+8] = ftemp[Q*pos+6];
				}
			}
		}
	}

	void CalcMacroQuantities(float[] density, float[] ux, float[] uy, float[] ftemp, bool[] obst) {
		for (int y = 0; y < ly; ++y) {
			for (int x = 0; x < lx; ++x) {
				int pos = x+lx*y;
				if (!obst[pos]) {
					float density_loc = 0f;
					float ux_loc = 0f;
					float uy_loc = 0f;
					for (int i = 0; i < Q; ++i) {
						density_loc += ftemp[Q*pos+i];
						ux_loc += ex[i]*ftemp[Q*pos+i];
						uy_loc += ey[i]*ftemp[Q*pos+i];
					}
					density[pos] = density_loc;
					ux[pos] = ux_loc/density_loc;
					uy[pos] = uy_loc/density_loc;
				} else {
					density[pos] = 0f;
					ux[pos] = 0f;
					uy[pos] = 0f;
				}
			}
		}
	}

	void CollisionSRT(float[] density, float[] ux, float[] uy, float[] f, float[] ftemp, bool[] obst, float omegaf) {
		float[] feq = new float[Q];
		
		for (int y = 0; y < ly; ++y) {
			for (int x = 0; x < lx; ++x) {
				int pos = x+lx*y;
				if (!obst[pos]) {
					float density_loc = density[pos];
					float ux_loc = ux[pos];
					float uy_loc = uy[pos];
					float u2 = ux_loc*ux_loc + uy_loc*uy_loc; // square of velocity
					
					float check_f = 0f;
					float check_ftemp = 0f;
					for (int i = 0; i < Q; ++i) {
						// calculating equilibrium distribution, e. (2)
						feq[i] = density_loc*rt[i]*(1f + (ex[i]*ux_loc+ey[i]*uy_loc)/cs2 + (ex[i]*ux_loc+ey[i]*uy_loc)*(ex[i]*ux_loc+ey[i]*uy_loc)/(2f*cs2*cs2) - u2/(2f*cs2));
						// solving rhs of Boltzmann equation
						f[Q*pos+i] = ftemp[Q*pos+i]+omegaf*(feq[i]-ftemp[Q*pos+i]);
						// summ up density distribution functions to check for negative densities
						check_f += f[Q*pos+i];
						check_ftemp += ftemp[Q*pos+i];
					}
					if (check_f < 0 || check_ftemp < 0) {
						print("error: negative density");
						break;
					}
				}
			}
		}
	}

	void UserInput() {
		if (Input.GetMouseButton(0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100)) {
				// determine indices where the user clicked
				int x = (int)(hit.point.x);
				int y = (int)(hit.point.z);
				if (x < 1 || x >= lx || y < 1 || y >= ly) return;
				// add particles
				Particle newPart = Instantiate(part) as Particle;
				newPart.transform.position = hit.point + Vector3.up;
				newPart.Init(ux, uy, lx, ly);
			}
		}
	}

	void Draw() {
		// visualize water
		for (int y = 0; y < ly; ++y) {
			for (int x = 0; x < lx; ++x) {
				int pos = x+lx*y;
				if (showDens) {
					float d = density[pos];
					tex.SetPixel(x, y, new Color(0, d*0.5f, d));
				}
				if (showVel) {
					tex.SetPixel(x, y, new Color(ux[pos]*5f + 0.5f, uy[pos]*5f + 0.5f, 1));
				}
			}
		}
		tex.Apply(false);
	}
}
