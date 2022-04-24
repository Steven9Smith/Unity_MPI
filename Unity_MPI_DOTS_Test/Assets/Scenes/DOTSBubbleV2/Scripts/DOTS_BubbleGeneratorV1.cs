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

	private PlayBubbleSoundSystem play_bubble_sound_system;
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
		play_bubble_sound_system = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PlayBubbleSoundSystem>();
		if(play_bubble_sound_system != null)
        {
			play_bubble_sound_system.audioSource = audioSource;
			play_bubble_sound_system.sampleRate = sampleRate;
        }
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
							bubbles[i].depth, bubbles[i].from, bubbles[i].to, bubbles[i].start, bubbles[i].end, bubbles[i].steps, bubbles[i].timeLeft,
							entities[j])
						);
						//	var buf = em.AddBuffer<DB_Float>(e);
						//	for (int j = 0; j < sampleRate; j++)
						//		buf.Add(new DB_Float());

					}
					entities.Dispose();
				}
				PlayOnStart = false;
            }
		}
		else GetBubbleSystem();
	}
	/*
    private string path;
	
    public void PlayBubbleSound()
    {
        watch.Start();
        for (int i = 0; i < bubbles.Length; i++)
        {
            bubbles[i] = new DOTS_Bubble_DataCondensedDynamic(bubbles[i].interfacetype, bubbles[i].movingtype, bubbles[i].radius, bubbles[i].depth, bubbles[i].from, bubbles[i].to, bubbles[i].start, bubbles[i].end, bubbles[i].steps);
            bubbles[i].Init(DOTS_Bubble_DataCondensedDynamic.Init_Mode.Fast_Formatted, true, true);
            if (SaveToFile)
            {
                path = Application.dataPath + "/Wave_Outputs/";
                WaveFormat waveFormat = new WaveFormat(bubbles[i].steps, 1);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                using (WaveFileWriter writer = new WaveFileWriter(path + SoundFileName + ".wav", waveFormat))
                {
                    writer.WriteSamples(bubbles[i].formatted_data, 0, bubbles[i].formatted_data.Length);
                }
                Debug.Log("saved file to" + Application.dataPath + "/Wav_Outputs/text" + i + ".wav");
            }
        }
        watch.Stop();
        Debug.Log("AAAAAA " + watch.ElapsedMilliseconds + "ms");
        watch.Reset();
        PlayBubbleSound(name, bubbles[0].formatted_data, 1, sampleRate, true);
    }
   */
}
public struct BubbleAdditionalInfo : IComponentData
{
	public int start,end;
	public float dt, dt2, dt6,timeLeft;
	public Entity entity;
	public bool available;
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
	public float start, end;
	public int steps;
	public int from, to;
	public bool IsInitialized;
	public float timeLeft;
	//public float2 minmax;
	public Entity entity;

	public DOTS_Bubble_Data(InterfaceType interface_type,MovingType moving_type,
		float radius, float depth, int from, int to, float start, float end, int steps,float timeLeft,
		Entity e)
	{
		if (steps < 0) steps = DEFAULT_STEPS;
		if (from < 0) from = 0;
		if (to <= from) to = from + 1;

		int multiplier = DEFAULT_STEPS / steps;
		radius *= multiplier;

		m_interfacetype = interface_type;
		m_movingtype = moving_type;
		this.depth = depth * radius / 1000f * 2f;
		this.radius = radius / 1000f;
		this.from = from;
		this.to = to;
		this.steps = steps;
		this.start = start;
		this.end = end;
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
	public static void ApplyWavFormat(NativeArray<float2> raw_data,BubbleAdditionalInfo bInfo)
	{
		//NOTE: we only apply change to raw_data[i].y as the x is useless to us atm
		float max = GetMax(raw_data);
		float m = 1.05f / max; 
		for (int i = bInfo.start; i < bInfo.end; i++)
		{
			float2 d = raw_data[i];
		//	d.y = d.y / math.max(minmax[0], minmax[1]) * 1.05f;
			d.y = d.y * m;
			raw_data[i] = d;
		}
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
public struct DB_Float : IBufferElementData
{
	public float2 value;
	public static void FromNativeArray(NativeArray<float2> d,ref DynamicBuffer<DB_Float> dd)
    {
		for (int i = 0; i < d.Length; i++)
			dd[i] = new DB_Float { value = d[i] };
	}
	public static NativeArray<float2> ToNativeArray(DynamicBuffer<DB_Float> d, Allocator allocator)
	{
		NativeArray<float2> dd = new NativeArray<float2>(d.Length, allocator);
		for (int i = 0; i < d.Length; i++)
			dd[i] = d[i].value;
		return dd;
	}
	public static float2[] ToArray(DynamicBuffer<DB_Float> d, Allocator allocator)
	{
		float2[] dd = new float2[d.Length];
		for (int i = 0; i < d.Length; i++)
			dd[i] = d[i].value;
		return dd;
	}

	public static float[] ToArray(DynamicBuffer<DB_Float> d, bool X)
	{
		float[] dd = new float[d.Length];
		for (int i = 0; i < d.Length; i++)
			dd[i] = (float)(X ? d[i].value.x : d[i].value.y);
		return dd;
	}
}
public struct BubbleGenerationRequest : IComponentData {
	//public DOTS_Bubble_Data data;
}


[UpdateAfter(typeof(MathNetRungeKutta))]
public partial class RungeKuttaBubbleSystem : SystemBase
{
	[BurstCompile]
	private struct DOTS_Step_Calculations
    {
		private DOTS_Bubble_Data.InterfaceType interfaceType;
		private DOTS_Bubble_Data.MovingType movingType;
		private float radius, depth;
		private float p0,v0,k;
		// For Jet Forcing
		float cutoff,mrp,jval_initial;
		// Bubble Terminal Velocity
		private const float del_rho = 997f; // Density difference between the phases
		private float vt;
		// rising bubble
		private float rising_d_m1;
		// Actual Freq
		private float AF_v0,AF_omega_a,AF_b;
		// Calculate Beta
		private float B_dr_a,B_dvis_a,B_dvis_b,B_phi_a,B_phi_B,B_dth_a;
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
		public static float ActualFreq(DOTS_Bubble_Data.InterfaceType interface_type,float radius,float depth,float v0,float AF_omega_a)
		{
			float bubbleCapacitance = BubbleCapacitance(interface_type, radius, depth);

			float omega = math.sqrt( bubbleCapacitance * AF_omega_a);

			return omega / 2f / math.PI;
		}
		public static float BubbleCapacitance(DOTS_Bubble_Data.InterfaceType interface_type, float radius, float depth)
		{
			if (interface_type == DOTS_Bubble_Data.InterfaceType.Rigid)
				return radius / (1f - radius / (2f * depth) - math.pow((radius / (2f * depth)), 4));
			else // Rigid interface
				return radius / (1f + radius / (2f * depth) - math.pow((radius / (2f * depth)), 4));
		}
		public static float CalcBeta(float w0,float B_dr_a,float B_dvis_a,float B_dvis_b,float B_phi_a,float B_phi_b,
			float B_dth_a)
		{

			float dr = w0 * B_dr_a;
			float dvis = B_dvis_a / ( w0 * B_dvis_b);

			float phi = B_phi_a / (B_phi_b * w0);

			float dth = 2f * ( math.sqrt(phi - 3f) - B_dth_a  ) / (phi - 4);

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
				float w0 = ActualFreq(interfaceType, radius, d,AF_v0,AF_omega_a) * AF_b;

				float m = k / math.pow(w0, 2);

				float beta = CalcBeta( w0,B_dr_a,B_dvis_a,B_dvis_b,B_phi_a,B_phi_B,B_dth_a);

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
	[BurstCompile]
    private struct runge_kutta4_single
    {
        private int N,internal_N;
		[NativeDisableParallelForRestriction]
        private NativeArray<float2> x_tmp, k1, k2, k3, k4;
        [NativeDisableParallelForRestriction]
		public NativeArray<float2> x;
		[NativeDisableParallelForRestriction]
		public NativeArray<BubbleAdditionalInfo> bubbleAdditionalInfos;
        public runge_kutta4_single(int size_n,int size_k, float start, float end, Allocator allocator)
        {
            N = size_n;
            x_tmp = new NativeArray<float2>(size_n, allocator);
            k1 = new NativeArray<float2>(size_n, allocator);
            k2 = new NativeArray<float2>(size_n, allocator);
            k3 = new NativeArray<float2>(size_n, allocator);
            k4 = new NativeArray<float2>(size_n, allocator);
            x = new NativeArray<float2>(size_n, allocator);
			bubbleAdditionalInfos = new NativeArray<BubbleAdditionalInfo>(size_k,allocator);
			internal_N = 0;
        }
		public void SetData(int allocated_n_per_bubble,int index,int n,float from,float to,float timeLeft,Entity e)
        {
			if (index < bubbleAdditionalInfos.Length)
			{
				int s = index * allocated_n_per_bubble;
				x[s] = 0;
				k1[s] = 0;
				k2[s] = 0;
				k3[s] = 0;
				k4[s] = 0;
				x_tmp[s] = 0;
				int2 se = new int2(s, s + n);
				float _dt = (to - from) / (n - 1);
				var bubbleAdditionalInfo = new BubbleAdditionalInfo
				{
					start = s,
					end = s + n,
					dt = _dt,
					dt2 = _dt / 2,
					dt6 = _dt / 6,
					timeLeft = timeLeft,
					entity = e
				};
				bubbleAdditionalInfos[index] = bubbleAdditionalInfo;
			}
			else Debug.LogError("index exceeds allocated amount of additional info!");
		}
        public void Dispose()
        {
            k1.Dispose();
            k2.Dispose();
            k3.Dispose();
            k4.Dispose();
			x.Dispose();
            x_tmp.Dispose();
			bubbleAdditionalInfos.Dispose();
        }
		float2 system(DOTS_Bubble_Data data, float2 Y, float t)
		{
			//[f'; f]
			float f = DOTS_Bubble_Data.JetForcing(data.radius, (float)t - 0.1f);

			float d = data.depth;

			if (data.m_movingtype == DOTS_Bubble_Data.MovingType.Rising && t >= 0.1f)
			{
				// rising bubble, calc depth

				float vt = DOTS_Bubble_Data.BubbleTerminalVelocity(data.radius);

				d = math.max(0.51f * 2f * data.radius, data.depth - ((float)t - 0.1f) * vt);

				// print('vt: ' + str(vt))
				// print('d: ' + str(d))
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
				float p0 = DOTS_Bubble_Data.PATM + 2.0f * DOTS_Bubble_Data.SIGMA / data.radius;
				float v0 = 4f / 3f * math.PI * math.pow(data.radius, 3);

				float w0 = DOTS_Bubble_Data.ActualFreq((byte)data.m_interfacetype, data.radius, d) * 2 * math.PI;
				float k = DOTS_Bubble_Data.GAMMA * p0 / v0;

				float m = k / math.pow(w0, 2);

				float beta = DOTS_Bubble_Data.CalcBeta(data.radius, w0);

				float acc = f / m - 2 * beta * (float)Y[0] - math.pow(w0, 2) * (float)Y[1];

			//	if (float.IsNaN(acc))
			//	{
					//		Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
			//	}
				//	if (acc != 0 || Y[0] != 0 || Y[1] != 0)
				//		Debug.Log($"{acc},{Y[0]},{Y[1]}");
				return new float2(acc, Y[0]);
			}
		}
		///////////////////////////////////////////////////
		public void do_step3(DOTS_Step_Calculations data, float t, int index)
		{
			//	if (index < bubbleStartEnd.Length)
			//	{
			var bInfo = bubbleAdditionalInfos[index];
			for (int i = bInfo.start + 1; i < bInfo.end; i++)
			{
				k1[i] = data.system(x[i - 1], t);
				k2[i] = data.system(x[i - 1] + k1[i] * bInfo.dt2, t + bInfo.dt2);
				k3[i] = data.system( x[i - 1] + k2[i] * bInfo.dt2, t + bInfo.dt2);
				k4[i] = data.system( x[i - 1] + k3[i] * bInfo.dt, t + bInfo.dt);
				x[i] = x[i - 1] + bInfo.dt6 * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]);
				t += bInfo.dt;
			}
			//	}
			//	else Debug.LogError($"runge_kutta_single: given index exceeds prepared datas {index}:{bubbleStartEnd.Length}");

		}
	}

	[BurstCompile]
    private struct ExecuteOnMultipleCores : IJobParallelFor
    {
		[NativeDisableParallelForRestriction]
		public NativeArray<DOTS_Bubble_Data> bubbles;
		[ReadOnly]
		public NativeArray<DOTS_Step_Calculations> steps;
		public runge_kutta4_single rk;
		public bool convert_to_wave_format;
		[ReadOnly]
		public int ALLOCATED_N_PER_BUBBLE;
		public void Execute(int index)
        {
			steps[index] = new DOTS_Step_Calculations(bubbles[index]);
			rk.SetData(ALLOCATED_N_PER_BUBBLE,index, bubbles[index].steps, bubbles[index].from, bubbles[index].to,
				bubbles[index].timeLeft,bubbles[index].entity);
			rk.do_step3(steps[index], 0, index);
			if (convert_to_wave_format)
			{
				DOTS_Bubble_Data.ApplyWavFormat(rk.x,rk.bubbleAdditionalInfos[index]);
			}
		}
    }
	[BurstCompile]
	private struct ExecuteQueueOnMultipleCores : IJobParallelFor
	{
		[ReadOnly]
		public NativeList<DOTS_Bubble_Data> bubbles;
		[NativeDisableParallelForRestriction]
		public NativeArray<DOTS_Step_Calculations> steps;
		[NativeDisableParallelForRestriction]
		public runge_kutta4_single rk;
	//	[ReadOnly]
	//	public bool convert_to_wave_format;
		[ReadOnly]
		public int MAX_ALLOWED_BUBBLE_CALCULATED_PER_FRAME;
		[ReadOnly]
		public int ALLOCATED_N_PER_BUBBLE;
		public void Execute(int index)
		{
			if (index > MAX_ALLOWED_BUBBLE_CALCULATED_PER_FRAME)
				return;
			else
			{
				steps[index] = new DOTS_Step_Calculations(bubbles[index]);
				rk.SetData(ALLOCATED_N_PER_BUBBLE, index, bubbles[index].steps, 
					bubbles[index].from, bubbles[index].to,
					bubbles[index].timeLeft, bubbles[index].entity);
				rk.do_step3(steps[index], 0, index);
			//	if (convert_to_wave_format)
			//		DOTS_Bubble_Data.ApplyWavFormat(rk.x, rk.bubbleAdditionalInfos[index]);
				
			}
		}
	}



	runge_kutta4_single rk;
	private static int ALLOWED_BUBBLES_PER_FRAME = 10;
	public const int MAX_AMOUNT_OF_STEPS_PER_BUBBLE = 48000;
	public static int rk_hard_cap = 48000;
	EntityQuery BubbleRequestQuery;
	NativeArray<DOTS_Step_Calculations> step_calculations;
	NativeList<DOTS_Bubble_Data> BubbleSoundGenerationQueue;
	PlayBubbleSoundSystem playBubbleSoundSystem;
    protected override void OnCreate()
    {
		// Calculate limit of data points
		rk_hard_cap *= ALLOWED_BUBBLES_PER_FRAME;
		// Get a reference to the PlayBubbleSoundSystem
		playBubbleSoundSystem = World.GetExistingSystem<PlayBubbleSoundSystem>();
		// Create our Bubble Query
		BubbleRequestQuery = GetEntityQuery(typeof(BubbleGenerationRequest),typeof(DOTS_Bubble_Data));
		// allocate our NativeArrays with peristant memory
		rk = new runge_kutta4_single(rk_hard_cap,ALLOWED_BUBBLES_PER_FRAME,0,1,Allocator.Persistent);
		step_calculations = new NativeArray<DOTS_Step_Calculations>(ALLOWED_BUBBLES_PER_FRAME, Allocator.Persistent);
		BubbleSoundGenerationQueue = new NativeList<DOTS_Bubble_Data>(Allocator.Persistent);

    }
    protected override void OnDestroy()
    {
		rk.Dispose();
		step_calculations.Dispose();
		BubbleSoundGenerationQueue.Dispose();
    }

    protected override void OnUpdate()
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
		}
	}
}
[UpdateAfter(typeof(RungeKuttaBubbleSystem))]
public partial class PlayBubbleSoundSystem : SystemBase
{
    [BurstCompile]
    private struct MulticoreAcousticPostProcessingJobA : IJobParallelFor
    {
		[ReadOnly]
		public NativeArray<float2> ProcessedAcousticDatas;
		[ReadOnly]
		public NativeArray<BubbleAdditionalInfo> bubbleAdditionalInfos;
		[NativeDisableParallelForRestriction]
		public NativeArray<float> PostProcessingOutput;
		public void Execute(int index)
		{
			var bInfo = bubbleAdditionalInfos[index];
			for (int j = bInfo.start; j < bInfo.end; j++)
			{
				PostProcessingOutput[j - bInfo.start] += ProcessedAcousticDatas[j].y;
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


    private AudioClip clip;
	public AudioSource audioSource;
	public int sampleRate = 48000;
	float[] liveBubbleData;
	bool liveBubbleDataDisabled = true;
	internal bool ProcessNewData = false;
	internal NativeArray<float2> ProcessedAcousticData;
	internal NativeArray<BubbleAdditionalInfo> bubbleAdditionalInfos;
	private NativeArray<float> PostProcessingOutput;
	protected override void OnCreate()
	{
		// NOTE: this assumes that the rk_hard_cap = 1 second
		PostProcessingOutput = new NativeArray<float>(RungeKuttaBubbleSystem.MAX_AMOUNT_OF_STEPS_PER_BUBBLE, Allocator.Persistent);

		float min = -1f;
		float max = 1;

		DebugGUI.SetGraphProperties("liveBubbleDataA","FinalBubbleDataA",min,max,1, new Color(0, 1, 1), false);
		DebugGUI.SetGraphProperties("liveBubbleDataB","FinalBubbleDataB",min,max,2, new Color(0, 1, 1), false);
		DebugGUI.SetGraphProperties("liveBubbleDataC","FinalBubbleDataC",min,max,3, new Color(0, 1, 1), false);
	}
    protected override void OnStartRunning()
	{

		liveBubbleData = new float[RungeKuttaBubbleSystem.MAX_AMOUNT_OF_STEPS_PER_BUBBLE];
		SetupBubbleSound("Test",1,sampleRate,true);
	}
    protected override void OnDestroy()
    {
		PostProcessingOutput.Dispose();
		
	}
    protected override void OnUpdate()
	{
		if (ProcessNewData)
		{
			//	Debug.Log($"{ProcessedAcousticData.Length},{bubbleAdditionalInfos.Length},{PostProccesingOutput.Length}");
			// POST PROCESSING!
			// first we have to combine and normalize the sounds based on thier expected 
			//TODO: implement post processing whie audio is being played live
			NativeArray<float> tmpData = new NativeArray<float>(2, Allocator.TempJob);
			var PostProcessingJobHandleA = new MulticoreAcousticPostProcessingJobA
			{
				ProcessedAcousticDatas = ProcessedAcousticData,
				bubbleAdditionalInfos = bubbleAdditionalInfos,
				PostProcessingOutput = PostProcessingOutput
			}.Schedule(bubbleAdditionalInfos.Length, 1,Dependency);
			var PostProcessingJobHandleB = new PostProcessingJobB
			{
				PostProcessingOutput = PostProcessingOutput,
				data = tmpData
			}.Schedule(PostProcessingJobHandleA);
			var PostProcessingJobHandleC = new MulticorePostProcessingC
			{
				PostProcessingOutput = PostProcessingOutput,
				data = tmpData
			}.Schedule(PostProcessingOutput.Length,1,PostProcessingJobHandleB);
			PostProcessingJobHandleC.Complete();

			//ExportWaveDataToCSV();

			SetLiveBubbleData(PostProcessingOutput.ToArray());
			tmpData.Dispose();
			ProcessNewData = false;
        }
		
	}
	private void ExportWaveDataToCSV()
    {
		string a = "x,DataA,DataB,DataC\n";
		for (int i = 0; i < PostProcessingOutput.Length; i++)
		{
			a += $"{i},{ProcessedAcousticData[bubbleAdditionalInfos[0].start + i].y},{ProcessedAcousticData[bubbleAdditionalInfos[1].start + i].y},{PostProcessingOutput[i]}\n";
			

		}
		if (!Directory.Exists(Application.dataPath + "/CSV"))
			Directory.CreateDirectory(Application.dataPath + "/CSV");
		if (!File.Exists(Application.dataPath + "/CSV/data.csv"))
			File.Create(Application.dataPath + "/CSV/data.csv");
		File.WriteAllText(Application.dataPath + "/CSV/data.csv", a); 
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
		Debug.Log("Setting New Bubble Data!");
	/*	string s = "";
		for (int i = 0; i < new_data.Length; i++)
			s += new_data[i];
		Debug.Log(s);*/
		currentStep = 0;
		stepsLeft = new_data.Length;
		liveBubbleData = new_data;
		liveBubbleDataDisabled = false;
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
		liveBubbleDataDisabled = true;
		
		for (int i = 0; i < liveBubbleData.Length; i++)
			liveBubbleData[i] = 0;

	}
	int currentStep = 0;
	int stepsLeft = 0;
	int tmpMaxStep = 0;
	void OnAudioRead(float[] data)
	{
		if (!liveBubbleDataDisabled)
		{
			tmpMaxStep = math.clamp(stepsLeft, 0, data.Length);
			for (int i = 0; i < tmpMaxStep; i++)
			{
				//	data[i] = math.remap( minmax.x,minmax.y,-1,1,liveBubbleData[currentStep + i]);
				data[i] = liveBubbleData[currentStep + i];// math.clamp(, -1,1);

			}
			if (stepsLeft < data.Length)
			{
				Debug.Log("Clearing Bubble Data!");
				// reached end of data stream
				ClearLiveBubbleData();
				stepsLeft = 0;
			}
			else
			{
				currentStep += data.Length;
				stepsLeft -= data.Length;
			}
		}

		/*
		int total = liveBubbleData.Length - currentStep - data.Length;
		if (total > data.Length) total = data.Length;
		else if (total <= 0 && currentStep < liveBubbleData.Length)
			total = data.Length + total;

		if (total <= 0)
		{
		//	Debug.Log("exceeded data lnegth, no more!");
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = 0;
			}
			return;
		}
		else
		{
			//	watch.Start();
			for (int i = 0; i < total; i++)
			{
				float value = 0;
				for (int j = 0; j < liveBubbleData.Length; j++)
					value += liveBubbleData[currentStep + i];
				data[i] = ClampToValidRange(value);

			}	
			currentStep += data.Length;
		}
		//	watch.Stop();
		//	Debug.Log("Combining " + bubbles.Length + " bubbles took " + watch.ElapsedMilliseconds + "ms");
		//	watch.Reset();*/
	}

	void OnAudioSetPosition(int newPosition)
	{

	}
	private float ClampToValidRange(float value)
	{
		float min = -1.0f;
		float max = 1.0f;
		return (value < min) ? min : (value > max) ? max : value;
	}

	private float[] MixAndClampFloatBuffers(float[] bufferA, float[] bufferB)
	{
		int maxLength = math.min(bufferA.Length, bufferB.Length);
		float[] mixedFloatArray = new float[maxLength];

		for (int i = 0; i < maxLength; i++)
		{
			mixedFloatArray[i] = ClampToValidRange((bufferA[i] + bufferB[i]) / 2);
		}
		return mixedFloatArray;
	}
}
