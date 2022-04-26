
#define ALWAYS_RUN_BUBBLE_SYSTEM
using NAudio.Wave;
using System.Collections.Generic;
using System.IO;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class DOTS_BubbleGeneratorV1 : MonoBehaviour
{
    public DOTS_Bubble_Data[] bubbles;
    public int sampleRate = 41000;
    public bool PlayOnStart = true;
	public ComputeShader shader;
    [Tooltip("Set to true to save to a file")]
    public bool SaveToFile = false;
    public string SoundFileName = "pop";
    private AudioSource audioSource;
  //  private AudioClip clip;
  //  int currentStep = 0;
  //  System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

    private void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = this.gameObject.AddComponent<AudioSource>();
		audioSource.loop = true;


        //	var a = AudioClip.Create("Test", wav_data.Length, 1, 41000, false);
        //	a.SetData(wav_data, 0);
    }

	private DynamicRungeKuttaBubbleSystem play_bubble_sound_system;
	private EntityManager em;
	private EntityArchetype BubbleArchtype;
    // Start is called before the first frame update
    void Start()
    {
		GetBubbleSystem();
    }
	private void GetBubbleSystem()
    {
		Debug.LogWarning("Getting Play Bubble System");
		em = World.DefaultGameObjectInjectionWorld.EntityManager;
		BubbleArchtype = em.CreateArchetype(typeof(DOTS_Bubble_Data), typeof(BubbleGenerationRequest));
		play_bubble_sound_system = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<DynamicRungeKuttaBubbleSystem>();
		play_bubble_sound_system.Enabled = true;
		if (play_bubble_sound_system != null)
		{
			play_bubble_sound_system.audioSource = audioSource;
			play_bubble_sound_system.sampleRate = sampleRate;
			play_bubble_sound_system.shader = shader;
		}
		else Debug.LogError("bubble system is null!");
	}

    // Update is called once per frame
    void Update()
    {
		if (play_bubble_sound_system != null)
		{
            if (PlayOnStart)
            {
				for(int i = 0; i < bubbles.Length; i++)
                {
					NativeArray<Entity> entities = new NativeArray<Entity>(1, Allocator.TempJob);
					em.CreateEntity(BubbleArchtype,entities);
					for(int j = 0; j < entities.Length; j++)
                    {
						em.SetComponentData(entities[j], new DOTS_Bubble_Data(bubbles[i].m_interfacetype, bubbles[i].m_movingtype, bubbles[i].radius,
							bubbles[i].depth, /*bubbles[i].from, bubbles[i].to,*/ bubbles[i].startTime, bubbles[i].endTime, bubbles[i].steps, bubbles[i].timeLeft,
							entities[j])
						);
					}
					entities.Dispose();
				}
				PlayOnStart = false;
            }
		}
		else GetBubbleSystem();
	}
}
[BurstCompile]
public struct BubbleQueueInfo
{
	private int N_LIMIT, MAX_ALLOWED_BUBBLES_PER_FRAME;
//	[ReadOnly]
//	private NativeList<DOTS_Bubble_Data> bDatas;
	[ReadOnly]
	private NativeList<DOTS_Step_Calculations> bCalculations;
	[NativeDisableParallelForRestriction]
	public NativeList<float2> x;
	[NativeDisableParallelForRestriction]
	private NativeList<float2> x_tmp, k1, k2, k3, k4;
	[NativeDisableParallelForRestriction]
	// x = t, y = dt, z = dt/2,w = dt/6
	private NativeList<float4> dt126;
	[NativeDisableParallelForRestriction]
	// x = startIndex, y = endINdex, z = steps, w = stepsLeft
	public NativeList<int4> stepsLNNLNM1;

	public BubbleQueueInfo(int maxAllowedBubblesPerFrom,int nLimit,Allocator allocator)
    {
		MAX_ALLOWED_BUBBLES_PER_FRAME = maxAllowedBubblesPerFrom;
		N_LIMIT = nLimit;
		int maxStep = MAX_ALLOWED_BUBBLES_PER_FRAME * nLimit;
	//	bDatas = new NativeList<DOTS_Bubble_Data>(allocator);
		bCalculations = new NativeList<DOTS_Step_Calculations>(allocator);
		dt126 = new NativeList<float4>(allocator);
		stepsLNNLNM1 = new NativeList<int4>(allocator);
		x = new NativeList<float2>(allocator);
		x_tmp = new NativeList<float2>(allocator);
		k1 = new NativeList<float2>(allocator);
		k2 = new NativeList<float2>(allocator);
		k3 = new NativeList<float2>(allocator);
		k4 = new NativeList<float2>(allocator);
	}
	public void AddRange(NativeArray<DOTS_Bubble_Data> datas,Allocator allocator)
    {
		for (int i = 0; i < datas.Length; i++)
			AddData(datas[i], allocator);
    }
	public void AddData(DOTS_Bubble_Data data,Allocator allocator)
    {
		int startIndex = bCalculations.Length * N_LIMIT;
		bCalculations.Add(new DOTS_Step_Calculations(data));
		stepsLNNLNM1.Add(new int4(startIndex,startIndex + N_LIMIT-1,data.steps,data.steps));
		float dt = (data.endTime - data.startTime) / (data.steps - 1);
		dt126.Add(new float4(data.startTime, dt, dt / 2, dt / 6));
		for(int i = 0; i < N_LIMIT; i++)
        {
			x.Add(new float2());
			x_tmp.Add(new float2());
			k1.Add(new float2());
			k2.Add(new float2());
			k3.Add(new float2());
			k4.Add(new float2());
        }

	/*	x.AddRange(new NativeArray<float2>(N_LIMIT,allocator));
		x_tmp.AddRange(new NativeArray<float2>(N_LIMIT,allocator));
		k1.AddRange(new NativeArray<float2>(N_LIMIT,allocator));
		k2.AddRange(new NativeArray<float2>(N_LIMIT,allocator));
		k3.AddRange(new NativeArray<float2>(N_LIMIT,allocator));
		k4.AddRange(new NativeArray<float2>(N_LIMIT,allocator));*/
	}
	public void Dispose()
	{
		x.Dispose();
		x_tmp.Dispose();
		k1.Dispose();
		k2.Dispose();
		k3.Dispose();
		k4.Dispose();
		bCalculations.Dispose();
		stepsLNNLNM1.Dispose();
		dt126.Dispose();
	}
	public void ExecuteRungeKutta4(int index)
	{
		//TODO: look into doing a back and forth thing so we don't have
		// to waste time setting variables
		// set the last calculated value to the beggining
		int4 stepData = stepsLNNLNM1[index];
		float4 dts = dt126[index];
		int min = math.min(N_LIMIT, stepData.w);
		int max = stepData.x + min;
	//	Debug.Log($"before x0 = {x[stepData.x]},{x[stepData.y]},{stepData.x},{stepData.y},{max},{x.Length}");
		x[stepData.x] = x[stepData.y];
	//	Debug.Log($"AFTER x0 = {x[stepData.x]},{x[stepData.y]},{stepData.x},{stepData.y},{max},{x.Length}");
		x_tmp[stepData.x] = x_tmp[stepData.y];
		k1[stepData.x] = k1[stepData.y];
		k2[stepData.x] = k2[stepData.y];
		k3[stepData.x] = k3[stepData.y];
		k4[stepData.x] = k4[stepData.y];

		//	float dt23 = dts.x + dts.z;

		for (int i = stepData.x + 1; i < max; i++)
		{
			k1[i] = bCalculations[index].system(x[i - 1], dts.x);
			k2[i] = bCalculations[index].system(x[i - 1] + k1[i] * dts.z, dts.x + dts.z);
			k3[i] = bCalculations[index].system(x[i - 1] + k2[i] * dts.z, dts.x + dts.z);
			k4[i] = bCalculations[index].system(x[i - 1] + k3[i] * dts.y, dts.x + dts.y);
			x[i] = x[i - 1] + dts.w * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]);
			dts.x += dts.y;
		}
		stepData.w -= min;
		stepsLNNLNM1[index] = stepData;
		dt126[index] = dts;
	//	Debug.Log($"t = {dt126[index].x},{stepsLNNLNM1[index].w}");
	}
	public void CleanUp()
    {
		for(int i = 0; i < stepsLNNLNM1.Length; i++)
        {
			var stepInfo = stepsLNNLNM1[i];
			if (stepInfo.w == 0)
			{
			//	Debug.Log($"removeing bubble at index {i}");
				if (stepsLNNLNM1.Length > 1)
				{
					// remove the data at i using a swap back procedure
					//bDatas.RemoveAtSwapBack(i);
					bCalculations.RemoveAtSwapBack(i);
					stepsLNNLNM1.RemoveAtSwapBack(i);
					dt126.RemoveAtSwapBack(i);
					x.RemoveRangeSwapBack(stepInfo.x, N_LIMIT);
					x_tmp.RemoveRangeSwapBack(stepInfo.x, N_LIMIT);
					k1.RemoveRangeSwapBack(stepInfo.x, N_LIMIT);
					k2.RemoveRangeSwapBack(stepInfo.x, N_LIMIT);
					k3.RemoveRangeSwapBack(stepInfo.x, N_LIMIT);
					k4.RemoveRangeSwapBack(stepInfo.x, N_LIMIT);
					// after procedure calculate new start and end index
					if (i == stepsLNNLNM1.Length)
					{
						i--;
						stepInfo.x -= N_LIMIT;
						stepInfo.y -= N_LIMIT;
						stepsLNNLNM1[i] = stepInfo;
					}
					else
					{
						stepsLNNLNM1[i] = new int4(stepInfo.x, stepInfo.y, stepsLNNLNM1[i].z, stepsLNNLNM1[i].w);
						i--;
					}
				}
				else
				{
					//	bDatas.Clear();
					bCalculations.Clear();
					stepsLNNLNM1.Clear();
					dt126.Clear();
					x.Clear();
					x_tmp.Clear();
					k1.Clear();
					k2.Clear();
					k3.Clear();
					k4.Clear();
				}
			}
			else if (stepInfo.w < 0)
				Debug.LogError("CleanUp Error: value is less than 0!");
		}
    }
}

[System.Serializable]
public struct DOTS_Bubble_Data : IComponentData, IComparer<DOTS_Bubble_Data>
{
	// physical constants
	public static readonly float CF = 1497f;
	public static readonly float MU = 0.00089f;
	public static readonly float RHO_WATER = 998f;
	public static readonly float GTH = 1600000f;
	public static readonly float GAMMA = 1.4f;
	public static readonly float G = 9.8f;
	public static readonly float SIGMA = 0.072f;
	public static readonly float ETA = 0.84f;
	public static readonly float PATM = 101325;
	public static readonly float DEFAULT_INITIAL_VALUE_FLOAT = 0;
	public static readonly int DEFAULT_STEPS = 96000;

	public enum InterfaceType : byte
	{
		None,
		Fluid,
		Rigid
	}
	public enum MovingType : byte
	{
		None,
		Static,
		Rising
	}
	public enum Init_Mode
	{
		Raw,
		Formatted,
		Fast_Raw,
		Fast_Formatted
	}

	public MovingType m_movingtype;
	public InterfaceType m_interfacetype;
	[Range(0, 16)]
	public float radius;
	[Range(0, 16)]
	public float depth;
	public float startTime, endTime;
	public int steps;
	//public int from, to;
	public bool IsInitialized;
	public float timeLeft;
	//public float2 minmax;
	public Entity entity;

	public DOTS_Bubble_Data(InterfaceType interface_type,MovingType moving_type,
		float radius, float depth,/* int from, int to,*/ float startTime, float endTime, int steps,float timeLeft,
		Entity e)
	{
		if (steps < 0) steps = DEFAULT_STEPS;
		if (startTime < 0) startTime = 0;
		if (endTime <= startTime) endTime = startTime + 1;

		int multiplier = DEFAULT_STEPS / steps;
		radius *= multiplier;

		m_interfacetype = interface_type;
		m_movingtype = moving_type;
		this.depth = depth * radius / 1000f * 2f;
		this.radius = radius / 1000f;
	//	this.from = from;
	//	this.to = to;
		this.steps = steps;
		this.startTime = startTime;
		this.endTime = endTime;
		this.timeLeft = timeLeft;
		entity = e;
		IsInitialized = false;
	//	minmax = new float2();
	}
	public static float BubbleCapacitance(byte interface_type, float radius, float depth)
	{
		if (interface_type == (byte)InterfaceType.Rigid)
			return radius / (1f - radius / (2f * depth) - math.pow((radius / (2f * depth)), 4));
		else // Rigid interface
			return radius / (1f + radius / (2f * depth) - math.pow((radius / (2f * depth)), 4));
	}
	public static float MinnaertFreq(float radius)
	{
		float omega = math.sqrt(3f * GAMMA * PATM - 2f * SIGMA * radius) / (radius * math.sqrt(RHO_WATER));
		return omega / 2f / math.PI;
	}
	public static float ActualFreq(byte interface_type, float radius, float depth)
	{
		float bubbleCapacitance = BubbleCapacitance(interface_type, radius, depth);

		float p0 = PATM;
		
		float v0 = 4f / 3f * math.PI * math.pow(radius, 3);

		float omega = math.sqrt(4f * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0));

		return omega / 2f / math.PI;
	}
	
	public static float CalcBeta(float radius, float w0)
	{

		float dr = w0 * radius / CF;
		float dvis = 4f * MU / (RHO_WATER * w0 * math.pow(radius, 2));

		float phi = 16f * GTH * G / (9f * math.pow((GAMMA - 1), 2) * w0);

		float dth = 2f * (math.sqrt(phi - 3f) - (3f * GAMMA - 1f) /
				 (3f * (GAMMA - 1))) / (phi - 4);

		float dtotal = dr + dvis + dth;


		return w0 * dtotal / math.sqrt(math.pow(dtotal, 2) + 4f);
	}

	public static float JetForcing(float r, float t)
	{

		float cutoff = math.min(0.0006f, 0.5f / (3f / r));

		if (t < 0 || t > cutoff)
			return 0;
		float jval = (-9f * GAMMA * SIGMA * ETA *
				(PATM + 2f * SIGMA / r) * math.sqrt(1f + math.pow(ETA, 2)) /
				(4f * math.pow(RHO_WATER, 2) * math.pow(r, 5)) * math.pow(t, 2));

		// Convert to radius (instead of fractional radius)
		jval *= r;

		// Convert to pressure
		float mrp = RHO_WATER * r;

		jval *= mrp;


		return jval;
	}


	// Calculate the bubble terminal velocity according to the paper
	// Rising Velocity for Single Bubbles in Pure Liquids
	// Bax-Rodriguez et al. 2012
	public static float BubbleTerminalVelocity(float r)
	{

		float d = 2f * r;

		float del_rho = 997f; // Density difference between the phases

		// eq 2
		float vtpot = 1f / 36f * del_rho * G * math.pow(d, 2) / MU;

		// eq 6
		float vt1 = vtpot * math.sqrt(1f + 0.73667f * math.sqrt(G * d) / vtpot);

		// eq 8
		float vt2 = math.sqrt(3f * SIGMA / RHO_WATER / d + G * d * del_rho / 2f / RHO_WATER);

		// eq 1
		float vt = 1f / math.sqrt(1 / math.pow(vt1, 2) + 1f / math.pow(vt2, 2));


		return vt;
	}


	/*
	public static NativeArray<float> CalculateRawSound(DOTS_Bubble_Data data, bool fastCalculation = false)
	{
		float m_depth = data.depth;
		float m_radius = data.radius;
		// Integrate the bubble sound into a buffer
		var sol = fastCalculation ? RungeKutta.SecondOrder(DEFAULT_INITIAL_VALUE, data.start, data.end, data.steps, DerivativeMaker())
			: RungeKutta.FourthOrder(DEFAULT_INITIAL_VALUE, data.start, data.end, data.steps, DerivativeMaker());
		Func<float, Vector<float>, Vector<float>> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				float f = DOTS_Bubble_Data.JetForcing(m_radius, (float)t - 0.1f);

				float d = m_depth;

				if (data.movingtype == 2 && t >= 0.1f)
				{
					// rising bubble, calc depth

					float vt = DOTS_Bubble_Data.BubbleTerminalVelocity(m_radius);

					d = math.max(0.51f * 2f * m_radius, m_depth - ((float)t - 0.1f) * vt);

					// print('vt: ' + str(vt))
					// print('d: ' + str(d))
				}
				//if we let it run too long and the values get very small,
				// the scipy integrator has problems. Might be setting the time step too
				// small? So just exit when the oscillator loses enough energy
				if (t > 0.11f && math.sqrt(math.pow(Y[0], 2) + math.pow(Y[1], 2)) < 1e-15f)
				{
					return Vector<float>.Build.Dense(new[] { 0.0, 0.0 });
				}
				else
				{
					float p0 = DOTS_Bubble_Data.PATM + 2.0f * DOTS_Bubble_Data.SIGMA / m_radius;
					float v0 = 4f / 3f * math.PI * math.pow(m_radius, 3);

					float w0 = DOTS_Bubble_Data.ActualFreq(data.interfacetype, m_radius, d) * 2 * math.PI;
					float k = DOTS_Bubble_Data.GAMMA * p0 / v0;

					float m = k / math.pow(w0, 2);

					float beta = DOTS_Bubble_Data.CalcBeta(m_radius, w0);

					float acc = f / m - 2 * beta * (float)Y[0] - math.pow(w0, 2) * (float)Y[1];

					if (float.IsNaN(acc))
					{
						//		Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					return Vector<float>.Build.Dense(new[] { acc, Y[0] });
				}
			};
		}

		return sol;
	}
	*/
	public static NativeArray<float> ApplyWavFormat(int from, int to, NativeArray<float> input,Allocator allocator)
	{
		NativeList<float> data = new NativeList<float>(input.Length, allocator);
		for (int i = 0; i < input.Length; i++)
			data[i] = input[i];
		data.RemoveRange(from, to - from);
		return ApplyWavFormat(data);
	}
	/*public static NativeArray<float2> ApplyWavFormat(int from, int to, NativeArray<float2> input, Allocator allocator)
	{
		NativeList<float> data = new NativeList<float>(input.Length, allocator);
		for (int i = 0; i < input.Length; i++)
			data[i] = input[i].y;
		data.RemoveRange(from, to - from);

		return ApplyWavFormat(data);
	}*/
	public static NativeArray<float> ApplyWavFormat(NativeArray<float> input)
	{
		// now we trim
		var minmax = GetMinMax(input);
		return ApplyWavFormat(minmax[0], minmax[1], input);
	}
	public static NativeArray<float2> ApplyWavFormat(NativeArray<float2> input)
	{
		// now we trim
		var minmax = GetMinMax(input);
		return ApplyWavFormat(minmax[0], minmax[1], input);
	}
	public static NativeArray<float> ApplyWavFormat(ref DOTS_Bubble_Data data,NativeArray<float2> input,Allocator allocator)
	{
		// now we trim

		float2 minmax = GetMinMax(input,out var tmp, allocator);
		return ApplyWavFormat(minmax[0], minmax[1], tmp);
	}
	public static NativeArray<float> ApplyWavFormat(float min, float max, NativeArray<float> input)
	{
		// now we trim
		var minmax = GetMinMax(input);
		for (int i = 0; i < input.Length; i++)
			input[i] = input[i] / math.max(minmax[0], minmax[1]) * 1.05f;
		return input;
	}
	public static NativeArray<float2> ApplyWavFormat(float min, float max, NativeArray<float2> input)
	{
		// now we trim
		var minmax = GetMinMax(input);
		for (int i = 0; i < input.Length; i++)
			input[i] = new float2(input[i].x, input[i].y / math.max(minmax[0], minmax[1]) * 1.05f);
		
		return input;
	}
	public static float[] ApplyWavFormat_f(int from, int to, NativeArray<float> input,Allocator allocator)
	{
		NativeList<float> data = new NativeList<float>(input.Length, allocator);
		for (int i = 0; i < input.Length; i++)
			data[i] = input[i];
		data.RemoveRange(from, to - from);
		return ApplyWavFormat_f(data,allocator);
	}
	private static float[] ApplyWavFormat_f(NativeArray<float> input,Allocator allocator)
	{
		var minmax = GetMinMax(input,allocator, out NativeArray<float> tmp);
		return ApplyWavFormat_f((int)minmax[0], (int)minmax[1], tmp,allocator);
	}
	static float[] ApplyWavFormat_f(float min, float max, float[] input)
	{
		for (int i = 0; i < input.Length; i++)
			input[i] = input[i] / math.max(min, max) * 1.05f;
		return input;
	}

	public static float2 GetMinMax(NativeArray<float2> sol )
	{
		float max = float.MinValue;
		float min = float.MaxValue;
		for (int i = 0; i < sol.Length; i++)
		{
			if (sol[i].y > max) max = sol[i].y;
			if (sol[i].y < min) min = sol[i].y;
		}
		return new float2(min, max);
	}
	public static float GetMax(NativeArray<float2> sol)
	{
		float max = float.MinValue;
		for (int i = 0; i < sol.Length; i++)
		{
			if (sol[i].y > max) max = sol[i].y;
		}
		return max;
	}
	public static float GetMax(NativeArray<float> sol)
	{
		float max = float.MinValue;
		for (int i = 0; i < sol.Length; i++)
		{
			if (sol[i] > max) max = sol[i];
		}
		return max;
	}
	public static float2 GetMinMax(NativeArray<float2> sol,
		out NativeArray<float> simplified_arr,Allocator allocator)
	{
		float max = float.MinValue;
		float min = float.MaxValue;
		simplified_arr = new NativeArray<float>(sol.Length, allocator);
		for (int i = 0; i < sol.Length; i++)
		{
			simplified_arr[i] = sol[i].y;
			if (simplified_arr[i] > max) max = simplified_arr[i];
			if (simplified_arr[i] < min) min = simplified_arr[i];
		}
		return new float2(min, max);
	}
	public static float2 GetMinMax(NativeArray<float> sol)
	{
		float max = float.MinValue;
		float min = float.MaxValue;
		for (int i = 0; i < sol.Length; i++)
		{
			if (sol[i] > max) max = sol[i];
			if (sol[i] < min) min = sol[i];
		}
		return new float2(min, max);
	}
	public static float2 GetMinMax(NativeArray<float> sol,Allocator allocator, out NativeArray<float> simplified_arr)
	{
		float max = float.MinValue;
		float min = float.MaxValue;
		simplified_arr = new NativeArray<float>(sol.Length, allocator);
		for (int i = 0; i < sol.Length; i++)
		{
			simplified_arr[i] = (float)sol[i];
			if (simplified_arr[i] > max) max = (float)simplified_arr[i];
			if (simplified_arr[i] < min) min = (float)simplified_arr[i];
		}
		return new float2(min, max);
	}

    public int Compare(DOTS_Bubble_Data x, DOTS_Bubble_Data y)
    {
		if (x.timeLeft < y.timeLeft)
			return -1;
		else if (x.timeLeft == y.timeLeft)
			return 0;
		else
			return 1;
    }
}
public struct BubbleGenerationRequest : IComponentData {
	//public DOTS_Bubble_Data data;
}
[BurstCompile]
public struct DOTS_Step_Calculations
{
	private DOTS_Bubble_Data.InterfaceType interfaceType;
	private DOTS_Bubble_Data.MovingType movingType;
	private float radius, depth;
	private float p0, v0, k;
	// For Jet Forcing
	float cutoff, mrp, jval_initial;
	// Bubble Terminal Velocity
	private const float del_rho = 997f; // Density difference between the phases
	private float vt;
	// rising bubble
	private float rising_d_m1;
	// Actual Freq
	private float AF_v0, AF_omega_a, AF_b;
	// Calculate Beta
	private float B_dr_a, B_dvis_a, B_dvis_b, B_phi_a, B_phi_B, B_dth_a;
	public DOTS_Step_Calculations(DOTS_Bubble_Data data)
	{
		radius = data.radius;
		depth = data.depth;
		interfaceType = data.m_interfacetype;
		movingType = data.m_movingtype;

		p0 = DOTS_Bubble_Data.PATM + 2.0f * DOTS_Bubble_Data.SIGMA / data.radius;
		v0 = 4f / 3f * math.PI * math.pow(data.radius, 3);
		k = DOTS_Bubble_Data.GAMMA * p0 / v0;

		// Jet Forcing
		cutoff = math.min(0.0006f, 0.5f / (3f / data.radius));
		mrp = DOTS_Bubble_Data.RHO_WATER * data.radius;
		jval_initial = (-9f * DOTS_Bubble_Data.GAMMA * DOTS_Bubble_Data.SIGMA * DOTS_Bubble_Data.ETA *
				(DOTS_Bubble_Data.PATM + 2f * DOTS_Bubble_Data.SIGMA / data.radius) * math.sqrt(1f + math.pow(DOTS_Bubble_Data.ETA, 2)) /
				(4f * math.pow(DOTS_Bubble_Data.RHO_WATER, 2) * math.pow(data.radius, 5))) * data.radius * mrp;
		// Bubble Terminal Velocity
		vt = BubbleTerminalVelocity(data.radius);
		// Rising Bubble
		if (data.m_movingtype == DOTS_Bubble_Data.MovingType.Rising)
			rising_d_m1 = 0.51f * 2f * data.radius;
		else
			rising_d_m1 = data.depth;
		// Actual Freq
		AF_v0 = 4f / 3f * math.PI * math.pow(data.radius, 3);
		AF_omega_a = 4f * math.PI * DOTS_Bubble_Data.GAMMA * DOTS_Bubble_Data.PATM / (DOTS_Bubble_Data.RHO_WATER * AF_v0);
		AF_b = 2 * math.PI;
		// Calculate Beta
		B_dr_a = data.radius / DOTS_Bubble_Data.CF;
		B_dvis_a = 4f * DOTS_Bubble_Data.MU;
		B_dvis_b = DOTS_Bubble_Data.RHO_WATER * math.pow(data.radius, 2);
		B_phi_a = 16f * DOTS_Bubble_Data.GTH * DOTS_Bubble_Data.G;
		B_phi_B = 9f * math.pow((DOTS_Bubble_Data.GAMMA - 1), 2);
		B_dth_a = (3f * DOTS_Bubble_Data.GAMMA - 1f) /
				 (3f * (DOTS_Bubble_Data.GAMMA - 1));
	}
	public float JetForcing(float t)
	{
		if (t < 0 || t > cutoff)
			return 0;
		return jval_initial * math.pow(t, 2);
	}
	public static float BubbleTerminalVelocity(float r)
	{

		float d = 2f * r;

		// eq 2
		float vtpot = 1f / 36f * del_rho * DOTS_Bubble_Data.G * math.pow(d, 2) / DOTS_Bubble_Data.MU;

		// eq 6
		float vt1 = vtpot * math.sqrt(1f + 0.73667f * math.sqrt(DOTS_Bubble_Data.G * d) / vtpot);

		// eq 8
		float vt2 = math.sqrt(3f * DOTS_Bubble_Data.SIGMA / DOTS_Bubble_Data.RHO_WATER / d + DOTS_Bubble_Data.G * d * del_rho / 2f / DOTS_Bubble_Data.RHO_WATER);

		// eq 1
		float vt = 1f / math.sqrt(1 / math.pow(vt1, 2) + 1f / math.pow(vt2, 2));


		return vt;
	}
	public static float ActualFreq(DOTS_Bubble_Data.InterfaceType interface_type, float radius, float depth, float v0, float AF_omega_a)
	{
		float bubbleCapacitance = BubbleCapacitance(interface_type, radius, depth);

		float omega = math.sqrt(bubbleCapacitance * AF_omega_a);

		return omega / 2f / math.PI;
	}
	public static float BubbleCapacitance(DOTS_Bubble_Data.InterfaceType interface_type, float radius, float depth)
	{
		if (interface_type == DOTS_Bubble_Data.InterfaceType.Rigid)
			return radius / (1f - radius / (2f * depth) - math.pow((radius / (2f * depth)), 4));
		else // Rigid interface
			return radius / (1f + radius / (2f * depth) - math.pow((radius / (2f * depth)), 4));
	}
	public static float CalcBeta(float w0, float B_dr_a, float B_dvis_a, float B_dvis_b, float B_phi_a, float B_phi_b,
		float B_dth_a)
	{

		float dr = w0 * B_dr_a;
		float dvis = B_dvis_a / (w0 * B_dvis_b);

		float phi = B_phi_a / (B_phi_b * w0);

		float dth = 2f * (math.sqrt(phi - 3f) - B_dth_a) / (phi - 4);

		float dtotal = dr + dvis + dth;


		return w0 * dtotal / math.sqrt(math.pow(dtotal, 2) + 4f);
	}
	public float2 system(float2 Y, float t)
	{
		//[f'; f]
		float f = JetForcing((float)t - 0.1f);

		float d = depth;

		if (movingType == DOTS_Bubble_Data.MovingType.Rising && t >= 0.1f)
		{
			// rising bubble, calc depth

			d = math.max(rising_d_m1, depth - ((float)t - 0.1f) * vt);

		}
		//if we let it run too long and the values get very small,
		// the scipy integrator has problems. Might be setting the time step too
		// small? So just exit when the oscillator loses enough energy
		if (t > 0.11f && math.sqrt(math.pow(Y[0], 2) + math.pow(Y[1], 2)) < 1e-15f)
		{
			return 0;
		}
		else
		{
			float w0 = ActualFreq(interfaceType, radius, d, AF_v0, AF_omega_a) * AF_b;

			float m = k / math.pow(w0, 2);

			float beta = CalcBeta(w0, B_dr_a, B_dvis_a, B_dvis_b, B_phi_a, B_phi_B, B_dth_a);

			float acc = f / m - 2 * beta * (float)Y[0] - math.pow(w0, 2) * (float)Y[1];

			if (float.IsNaN(acc))
			{
				//		Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
			}
			//	if (acc != 0 || Y[0] != 0 || Y[1] != 0)
			//		Debug.Log($"{acc},{Y[0]},{Y[1]}");
			return new float2(acc, Y[0]);
		}
	}

}

// This is the RungeKuttaBubbleSystem with Dynamic Memory Allocation
#if !ALWAYS_RUN_BUBBLE_SYSTEM
[DisableAutoCreation]
#else
[AlwaysUpdateSystem]
#endif
public partial class DynamicRungeKuttaBubbleSystem : SystemBase
{
	[BurstCompile]
	private struct ExecuteQueueOnMultipleCoresV2 : IJobParallelFor
	{
		public BubbleQueueInfo QInfo;
		public void Execute(int index)
		{
			QInfo.ExecuteRungeKutta4(index);
		}
	}
	[BurstCompile]
    private struct ClearPostProcessingNativeArrayJob : IJobParallelFor
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float> PostProcessingOutput;
		public void Execute(int index)
        {
			PostProcessingOutput[index] = 0;
        }
    }
	[BurstCompile]
	private struct MulticoreAcousticPostProcessingJobAV3 : IJobParallelFor
	{
		[ReadOnly]
		public BubbleQueueInfo QInfo;
		[NativeDisableParallelForRestriction]
		public NativeArray<float> PostProcessingOutput;
		public void Execute(int index)
		{
		/*	int i = 0;
			var bInfo = QInfo.stepsLNNLNM1[index];
			for (int j = bInfo.x; j < bInfo.y; j++)
			{
				PostProcessingOutput[i] += QInfo.x[j].y;
				i++;
			}
			*/
			for(int i = 0; i < QInfo.stepsLNNLNM1.Length; i++)
            {
				PostProcessingOutput[index] += QInfo.x[QInfo.stepsLNNLNM1[i].x + index].y;
            }

		}
	}
	[BurstCompile]
	private struct MulticorePostProcessingC : IJobParallelFor
	{
		public NativeArray<float> PostProcessingOutput;
		[ReadOnly]
		public NativeArray<float> data;
		public void Execute(int index)
		{
			PostProcessingOutput[index] *= data[1];
		}
	}
	[BurstCompile]
	private struct PostProcessingJobB : IJob
	{
		[ReadOnly]
		public NativeArray<float> PostProcessingOutput;
		// data[0] = max value
		// data[1] = multiplier
		public NativeArray<float> data;
		public void Execute()
		{
			if (data.Length != 2)
				Debug.LogError($"PostProcessingJobB: given data NativeArray is not equal to 2 which is expected, got {data.Length}");
			else
			{
				//NOTE: we only apply change to raw_data[i].y as the x is useless to us atm
				data[0] = DOTS_Bubble_Data.GetMax(PostProcessingOutput);
				data[1] = 1.05f / data[0];
			}
		}
	}
	private static int ALLOWED_BUBBLES_PER_FRAME = 10;
	public const int AudioSourceDataLimit = 4096;
	EntityQuery BubbleRequestQuery;
	internal ComputeShader shader;
	ComputeBuffer buffer;
	// we need this to bereferenceable from the other job for now

	internal BubbleQueueInfo bubbleQueueInfo;

	NativeArray<float> tmpData;
	protected override void OnCreate()
    {
		PostProcessingOutput = new NativeArray<float>(AudioSourceDataLimit, Allocator.Persistent);
		zeroData = new float[0];
		// Create our Bubble Query
		BubbleRequestQuery = GetEntityQuery(typeof(BubbleGenerationRequest),typeof(DOTS_Bubble_Data));
		bubbleQueueInfo = new BubbleQueueInfo(ALLOWED_BUBBLES_PER_FRAME, AudioSourceDataLimit, Allocator.Persistent);
		tmpData = new NativeArray<float>(2, Allocator.Persistent);
		tmpData[1] = 6666666666;// 1/1.5E-10 = 6666666666
	}
    protected override void OnDestroy()
    {
		bubbleQueueInfo.Dispose();
		PostProcessingOutput.Dispose();
		tmpData.Dispose();
	}
	protected override void OnStartRunning()
	{
		liveBubbleData = new float[AudioSourceDataLimit];
		SetupBubbleSound("Test", 1, sampleRate, true);
	}
	protected override void OnUpdate()
    {
        {
			if (BubbleRequestQuery.CalculateEntityCount() > 0)
			{
				ComputeBuffer
				// collected the new bubble requests
				var bubbles = BubbleRequestQuery.ToComponentDataArray<DOTS_Bubble_Data>(Allocator.TempJob);
				// for debugging purposes only!
				var entities = BubbleRequestQuery.ToEntityArray(Allocator.TempJob);

				bubbleQueueInfo.AddRange(bubbles, Allocator.Persistent);

				// we no longer need this array so we can dispose it
				EntityManager.DestroyEntity(entities);
				entities.Dispose();
				bubbles.Dispose();
			}
			bubbleQueueInfo.CleanUp();
			if (bubbleQueueInfo.stepsLNNLNM1.Length > 0)
			{
				var GenerateJob = new ExecuteQueueOnMultipleCoresV2
				{
					QInfo = bubbleQueueInfo
				}.Schedule(bubbleQueueInfo.stepsLNNLNM1.Length, 1);
				// POST PROCESSING!
				var ResetPostProcessingJob = new ClearPostProcessingNativeArrayJob
				{
					PostProcessingOutput = PostProcessingOutput
				}.Schedule(PostProcessingOutput.Length, 1, GenerateJob);
				var PostProcessingJobHandleA = new MulticoreAcousticPostProcessingJobAV3
				{
					QInfo = bubbleQueueInfo,
					PostProcessingOutput = PostProcessingOutput
				}.Schedule(PostProcessingOutput.Length, 1, ResetPostProcessingJob);
				// TODO: Determine if this is needed?
				/*	var PostProcessingJobHandleB = new PostProcessingJobB
					{
						PostProcessingOutput = PostProcessingOutput,
						data = tmpData
					}.Schedule(PostProcessingJobHandleA);*/
				var PostProcessingJobHandleC = new MulticorePostProcessingC
				{
					PostProcessingOutput = PostProcessingOutput,
					data = tmpData
				}.Schedule(PostProcessingOutput.Length, 1, PostProcessingJobHandleA);


				PostProcessingJobHandleC.Complete();

				//	ExportWaveDataToCSV();

				SetLiveBubbleData(PostProcessingOutput.ToArray());
			}
			else
			{
				//	save++;
				SetLiveBubbleData(zeroData);
				//	ExportWaveDataToCSV();
			}
		}
        {
		/*Version 2 Multicore CPU
		 * if (BubbleRequestQuery.CalculateEntityCount() > 0)
			{
				// collected the new bubble requests
				var bubbles = BubbleRequestQuery.ToComponentDataArray<DOTS_Bubble_Data>(Allocator.TempJob);
				// for debugging purposes only!
				var entities = BubbleRequestQuery.ToEntityArray(Allocator.TempJob);

				bubbleQueueInfo.AddRange(bubbles, Allocator.Persistent);

				// we no longer need this array so we can dispose it
				EntityManager.DestroyEntity(entities);
				entities.Dispose();
				bubbles.Dispose();
			}
			bubbleQueueInfo.CleanUp();
			if (bubbleQueueInfo.stepsLNNLNM1.Length > 0)
			{
				var GenerateJob = new ExecuteQueueOnMultipleCoresV2
				{
					QInfo = bubbleQueueInfo
				}.Schedule(bubbleQueueInfo.stepsLNNLNM1.Length, 1);
				// POST PROCESSING!
				var ResetPostProcessingJob = new ClearPostProcessingNativeArrayJob
				{
					PostProcessingOutput = PostProcessingOutput
				}.Schedule(PostProcessingOutput.Length, 1, GenerateJob);
				var PostProcessingJobHandleA = new MulticoreAcousticPostProcessingJobAV3
				{
					QInfo = bubbleQueueInfo,
					PostProcessingOutput = PostProcessingOutput
				}.Schedule(PostProcessingOutput.Length, 1, ResetPostProcessingJob);
				// TODO: Determine if this is needed?
				//	var PostProcessingJobHandleB = new PostProcessingJobB
				//	{
				//		PostProcessingOutput = PostProcessingOutput,
				//		data = tmpData
				//	}.Schedule(PostProcessingJobHandleA);
				var PostProcessingJobHandleC = new MulticorePostProcessingC
				{
					PostProcessingOutput = PostProcessingOutput,
					data = tmpData
				}.Schedule(PostProcessingOutput.Length, 1, PostProcessingJobHandleA);


				PostProcessingJobHandleC.Complete();

				//	ExportWaveDataToCSV();

				SetLiveBubbleData(PostProcessingOutput.ToArray());
			}
			else
			{
				//	save++;
				SetLiveBubbleData(zeroData);
				//	ExportWaveDataToCSV();
			}*/
		}
		{
			/*
			 * This runs well however there's no handling too many bubbles being created
			 * if(BubbleRequestQuery.CalculateEntityCount() > 0)
			{
				var bubbles = BubbleRequestQuery.ToComponentDataArray<DOTS_Bubble_Data>(Allocator.TempJob);
				var entities = BubbleRequestQuery.ToEntityArray(Allocator.TempJob);
				Dependency = new ExecuteOnMultipleCores
				{
					rk = rk,
					bubbles = bubbles,
					steps = step_calculations,
					convert_to_wave_format = true,
					ALLOCATED_N_PER_BUBBLE = MAX_AMOUNT_OF_STEPS_PER_BUBBLE
				}.Schedule(bubbles.Length,1,Dependency);
				Dependency.Complete();
				EntityManager.RemoveComponent(entities,typeof(BubbleGenerationRequest));
				entities.Dispose();
				bubbles.Dispose();
				playBubbleSoundSystem.bubbleAdditionalInfos = rk.bubbleAdditionalInfos;
				playBubbleSoundSystem.bubble_queue = rk.x;
			}*/
			/*
			 * this is the second version. it used a fixed size buffer and managed to calculate the bubble information in ~6-8ms
			 * I realized that:
			 * 1) the fixed size buffer is an issue in terms of flexibility because if we need to calculate more bubbles that amount of
			 *		processors then the memory will need to be increased and the calculation time will increase
			 * 2) the amounf of audio data that an AudioSource can deal with at a time is set to 4096 float values so calculating more 
			 *		than that can wait until next frame
			if (BubbleRequestQuery.CalculateEntityCount() > 0)
			{
				// collected the new bubble requests
				var bubbles = BubbleRequestQuery.ToComponentDataArray<DOTS_Bubble_Data>(Allocator.TempJob);
				// add them the the queue
				BubbleSoundGenerationQueue.AddRange(bubbles);
				// we need to sort the bubble data so we know which bubble sounds to generate first
				var SortingJobHandle = BubbleSoundGenerationQueue.SortJob(new DOTS_Bubble_Data()).Schedule(Dependency);
				// now we create those sounds using the sort job as a dependency
				Dependency = new ExecuteQueueOnMultipleCores
				{
					rk = rk,
					bubbles = BubbleSoundGenerationQueue,
					steps = step_calculations,
					MAX_ALLOWED_BUBBLE_CALCULATED_PER_FRAME = ALLOWED_BUBBLES_PER_FRAME,
				//	convert_to_wave_format = true,
					ALLOCATED_N_PER_BUBBLE = MAX_AMOUNT_OF_STEPS_PER_BUBBLE
				}.Schedule(bubbles.Length, 1, SortingJobHandle);

				// this is for debuggin purposes only
				Dependency.Complete();

				// remove request from process entities
				int max = math.min(ALLOWED_BUBBLES_PER_FRAME, BubbleSoundGenerationQueue.Length);
				for (int i = 0; i < max; i++)
					EntityManager.RemoveComponent(BubbleSoundGenerationQueue[i].entity, typeof(BubbleGenerationRequest));
				// calculate range remove length
				// remove the processed bubbles
				BubbleSoundGenerationQueue.RemoveRange(0, max);

				//TODO: look at setting data;
				playBubbleSoundSystem.bubbleAdditionalInfos = rk.bubbleAdditionalInfos;
				playBubbleSoundSystem.ProcessedAcousticData = rk.x;

				// we no longer need this array so we can dispose it
				bubbles.Dispose();
				// let the PlaySounBubbleSystem know we processed some bubble sounds
				playBubbleSoundSystem.ProcessNewData = true;
			}*/
		}
	}
    
	private AudioClip clip;
	public AudioSource audioSource;
	public int sampleRate = 48000;
	float[] liveBubbleData;
	float[] zeroData;
	internal bool ProcessNewData = false;
	private NativeArray<float> PostProcessingOutput;
	
	string finalWave = "";
	int save = 0;
	int total = 0;
	private void ExportWaveDataToCSV()
	{
		if (save == 0)
		{
			int i = 0;
			for (; i < PostProcessingOutput.Length; i++)
			{
				//	a += $"{i},{ProcessedAcousticData[bubbleAdditionalInfos[0].startIndex + i].y},{ProcessedAcousticData[bubbleAdditionalInfos[1].startIndex + i].y},{PostProcessingOutput[i]}\n";
				finalWave += $"{i+total},{PostProcessingOutput[i]},{bubbleQueueInfo.x[i].y}," +
					$"{bubbleQueueInfo.x[i+AudioSourceDataLimit].y},{bubbleQueueInfo.x[i + AudioSourceDataLimit*2].y},{bubbleQueueInfo.x[i + AudioSourceDataLimit*3].y},{bubbleQueueInfo.x[i + AudioSourceDataLimit*4].y}\n";

			}
			total += i;
        }
        else if(save == 1)
		{
			save++;
			Debug.Log("Exporting...");
			finalWave = "i,DataA,Data2,Data3,Data3,Data4,Data5\n" + finalWave;
			if (!Directory.Exists(Application.dataPath + "/CSV"))
				Directory.CreateDirectory(Application.dataPath + "/CSV");
			if (!File.Exists(Application.dataPath + "/CSV/data.csv"))
				File.Create(Application.dataPath + "/CSV/data.csv");
			File.WriteAllText(Application.dataPath + "/CSV/data.csv", finalWave);
			
		}
		
	}
	private void SetupBubbleSound(string name, int channels = 1, int sampleRate = 41000, bool stream = true)
	{
		if (clip == null)
			clip = AudioClip.Create(name, sampleRate, channels, sampleRate, stream, OnAudioRead/*,OnAudioSetPosition*/);
		if (audioSource != null)
		{
			audioSource.clip = clip;
			audioSource.Play();
		}
	}
	private void SetLiveBubbleData(float[] new_data)
	{
	//	Debug.Log($"{total}: Setting New Bubble Data! {new_data.Length}");
		DataInQueue.AddRange(new_data);
		//	liveBubbleData = new_data;
		//liveBubbleDataDisabled = new_data.Length == 0 ? true : false;
	//	total++;
	/*		string s = "";
			for (int i = 0; i < new_data.Length; i++)
				s += new_data[i];
			Debug.Log(s);*/
		
	}
	private void saveBubbleData()
	{
		var path = Application.dataPath + "/Wave_Outputs/";
		WaveFormat waveFormat = new WaveFormat(liveBubbleData.Length, 1);
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);

		using (WaveFileWriter writer = new WaveFileWriter(path + "test" + ".wav", waveFormat))
		{
			writer.WriteSamples(liveBubbleData, 0, liveBubbleData.Length);
		}
		Debug.Log("saved file to" + Application.dataPath + "/Wav_Outputs/test.wav");

	}
	private void ClearLiveBubbleData()
	{
		for (int i = 0; i < liveBubbleData.Length; i++)
			liveBubbleData[i] = 0;

	}
	List<float> DataInQueue = new List<float>();
	void OnAudioRead(float[] data) // 4096
	{
		//Debug.Log($"{total}: Read was called!");
		//	total++;
		if (DataInQueue.Count > 0)
		{
			int max = math.min(data.Length, DataInQueue.Count);
			for (int i = 0; i < max; i++)
				data[i] = DataInQueue[i];
			DataInQueue.RemoveRange(0, max);
			//	Debug.Log($"Setting data! {data.Length}");

		}
		else
			for (int i = 0; i < data.Length; i++)
				data[i] = 0;

	}

}
