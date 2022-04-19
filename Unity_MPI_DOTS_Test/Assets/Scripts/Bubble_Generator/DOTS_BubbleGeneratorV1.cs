using NAudio.Wave;
using System.Collections;
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


        //	var a = AudioClip.Create("Test", wav_data.Length, 1, 41000, false);
        //	a.SetData(wav_data, 0);
    }

	private Play_Bubble_Sound play_bubble_sound_system;
	private EntityManager em;
    // Start is called before the first frame update
    void Start()
    {
		GetBubbleSystem();
    }
	private void GetBubbleSystem()
    {
		Debug.LogWarning("Getting Play Bubble System");
		em = World.DefaultGameObjectInjectionWorld.EntityManager;
		play_bubble_sound_system = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Play_Bubble_Sound>();
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
					Entity e = em.CreateEntity();
					em.AddComponentData(e,new DOTS_Bubble_Data(bubbles[i].m_interfacetype,bubbles[i].m_movingtype,bubbles[i].radius,
						bubbles[i].depth,bubbles[i].from,bubbles[i].to,bubbles[i].start,bubbles[i].end,bubbles[i].steps,bubbles[i].timeLeft));
					var buf = em.AddBuffer<DB_Float>(e);
					for (int j = 0; j < sampleRate; j++)
						buf.Add(new DB_Float());

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


public class DOTS_Bubble_Authoring
{
	
}
[System.Serializable]
public struct DOTS_Bubble_Data : IComponentData
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
	float min, max;
	// to be handled in a Dynamic Buffer
	//public NativeArray<float> raw_data;
	//public float[] formatted_data;

	public DOTS_Bubble_Data(InterfaceType interface_type,MovingType moving_type,
		float radius, float depth, int from, int to, float start, float end, int steps,float timeLeft)
	{
		if (steps < 0) steps = DEFAULT_STEPS;
		if (start < 0) start = 0;
		if (end <= start) end = start + 1;

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
	//	raw_data = new Vector<float>[0];
	//	formatted_data = new float[0];
		IsInitialized = false;
		min = 0;
		max = 0;
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
	public static NativeArray<float> ApplyWavFormat(NativeArray<float2> input,Allocator allocator)
	{
		// now we trim
		var minmax = GetMinMax(input,out var tmp, allocator);
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
	/*
	public static float[] GenerateBubble(DOTS_Bubble_DataCondensedDynamic data, Vector<float> initialValue, float start, float end, int steps, out Vector<float> lastValue)
	{
		//	int numsteps = 96000;

		//	float dt = 1f / (numsteps - 1);

		float m_depth = data.depth;
		float m_radius = data.radius;
		// Integrate the bubble sound into a buffer
		var sol = RungeKutta.FourthOrder(initialValue, start, end, steps, DerivativeMaker());
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
						Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					return Vector<float>.Build.Dense(new[] { acc, Y[0] });
				}
			};
		}

		float[] wave_data = new float[steps];
		for (int i = 0; i < wave_data.Length; i++)
		{
			wave_data[i] = (float)sol[i][1] / math.max(data.min, data.max) * 1.05f;
		}
		lastValue = sol[sol.Length - 1];
		return wave_data;

	}
	*/
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
[UpdateAfter(typeof(MathNetRungeKutta))]
public partial class RungeKuttaBubbleSystem : SystemBase
{
    private struct runge_kutta4_single
    {
        private int N;
        private NativeArray<float2> x_tmp, k1, k2, k3, k4;
        private float dt, dt2, dt3, dt6;
        public runge_kutta4_single(int size_n, float start, float end, Allocator allocator)
        {
            N = size_n;
            x_tmp = new NativeArray<float2>(size_n, allocator);
            k1 = new NativeArray<float2>(size_n, allocator);
            k2 = new NativeArray<float2>(size_n, allocator);
            k3 = new NativeArray<float2>(size_n, allocator);
            k4 = new NativeArray<float2>(size_n, allocator);
            dt = (end - start) / (N - 1);
            dt2 = dt / 2;
            dt3 = dt / 3;
            dt6 = dt / 6;
        }
        public void Dispose()
        {
            k1.Dispose();
            k2.Dispose();
            k3.Dispose();
            k4.Dispose();
            x_tmp.Dispose();
        }
		public void GenerateBubbleWaveForm(ref DOTS_Bubble_Data data, NativeArray<float2> raw_data, 
			bool convert_to_wav_format = true, bool useFromTo = false)
		{
		//	Debug.Log($"{data.depth},{data.radius},{data.from},{data.to},{data.steps},{data.start},{data.end},{(byte)data.m_interfacetype},{(byte)data.m_movingtype}");

			do_step2(data, raw_data, 0);
			//var a = Float_BubbleSoundDataCondensedDynamic.CalculateRawSound(data, true);
			//for (int i = 0; i < raw_data.Length; i++)
			//	raw_data[i] = new float2((float)a[i][0], (float)a[i][1]);
			
			if (convert_to_wav_format)
			{
				NativeArray<float> tmp = DOTS_Bubble_Data.ApplyWavFormat(raw_data,Allocator.Temp);
				for (int i = 0; i < tmp.Length; i++)
				{
					raw_data[i] = tmp[i];
				}
			//	if (!useFromTo) 
			//	else raw_data = DOTS_Bubble_Data.ApplyWavFormat(data.from, data.to, raw_data);
			}
			data.IsInitialized = true;
			/*switch (data.)
			{
				case DOTS_Bubble_Data.Init_Mode.Raw:
					

					break;
				case DOTS_Bubble_Data.Init_Mode.Fast_Raw:
					raw_data = DOTS_Bubble_Data.CalculateRawSound(this, true);
					if (convert_to_wav_format)
					{
						if (!useFromTo) raw_data = ApplyWavFormat(raw_data);
						else raw_data = DOTS_Bubble_Data.ApplyWavFormat(this.from, this.to, raw_data);
					}
					break;
				default:
					Debug.Log("Invalid format detected!");
					break;
					/*	case  Init_Mode.Formatted:
							raw_data = CalculateRawSound(this, false);
							if (convert_to_wav_format)
							{
								if (!useFromTo) formatted_data = ApplyWavFormat_f(raw_data);
								else formatted_data = ApplyWavFormat_f(this.from, this.to, raw_data);
							}
							break;
						case  Init_Mode.Fast_Formatted:
							raw_data = CalculateRawSound(this, true);
							if (convert_to_wav_format)
							{
								if (!useFromTo) formatted_data = ApplyWavFormat_f(raw_data);
								else formatted_data = ApplyWavFormat_f(this.from, this.to, raw_data);
							}
							break;
		}
			IsInitialized = true;*/
		}

		float2 system(DOTS_Bubble_Data data, float2 Y,float t)
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

				if (float.IsNaN(acc))
				{
					//		Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
				}
			//	if (acc != 0 || Y[0] != 0 || Y[1] != 0)
			//		Debug.Log($"{acc},{Y[0]},{Y[1]}");
				return new float2( acc, Y[0] );
			}
		}
        // assum x = [1...n]
        float system(float x, float t)
        {
            return x * x + t;
        }
        public void do_step2(DOTS_Bubble_Data data,
            NativeArray<float2> x, float t)
        {
            for (int i = 1; i < x.Length; i++)
            {
				/*   k1[i] = system(x[i - 1], t);
				   k2[i] = system(x[i - 1] + k1[i] * dt2, t + dt2);
				   k3[i] = system(x[i - 1] + k2[i] * dt2, t + dt2);
				   k4[i] = system(x[i - 1] + k3[i] * dt, t + dt);
				   x[i] = x[i - 1] + dt6 * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]);
				   t += dt;*/
                k1[i] = system(data,x[i - 1], t);
                k2[i] = system(data,x[i - 1] + k1[i] * dt2,  t + dt2);
                k3[i] = system(data,x[i - 1] + k2[i] * dt2,  t + dt2);
                k4[i] = system(data,x[i - 1] + k3[i] * dt,  t + dt);
                x[i] = x[i - 1] + dt6 * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]);
                t += dt;
            }
        }
    }

    [BurstCompile]
    private struct ExecuteOn1Core : IJob
    {
        public NativeArray<float2> input;
        public float deltaTime;
        public runge_kutta4_single rk;
        public DOTS_Bubble_Data data;
        public void Execute()
        {
            rk.do_step2(data,input, 0);
        }
    }

    protected override void OnDestroy()
    {
      
    }

    protected override void OnUpdate()
    {
		Dependency = Entities
			.WithName("Bubble_Generation_System")
			.WithBurst()
			.ForEach((ref DOTS_Bubble_Data data, ref DynamicBuffer<DB_Float> wave_data) => {
				if (!data.IsInitialized)
				{
					var rk = new runge_kutta4_single(data.steps, 0, 1, Allocator.Temp);
					NativeArray<float2> w_data = new NativeArray<float2>(wave_data.Length, Allocator.Temp);
					rk.GenerateBubbleWaveForm(ref data, w_data, false);
					//	string s = "";
					//	for (int i = 0; i < w_data.Length; i++)
					//		s += w_data[i].y + ",";
					//	Debug.Log(s);
					DB_Float.FromNativeArray(w_data, ref wave_data);
					data.IsInitialized = true;
				}
			}).ScheduleParallel(Dependency);
		Dependency.Complete();
		
    }
}
[UpdateAfter(typeof(RungeKuttaBubbleSystem))]
public partial class Play_Bubble_Sound : SystemBase
{

	EntityQuery Bubble_Query;
	private AudioClip clip;
	public AudioSource audioSource;
	public int sampleRate = 48000;
	float[] current_bubble;
	protected override void OnCreate()
	{
		Bubble_Query = GetEntityQuery(typeof(DOTS_Bubble_Data));
	}
    protected override void OnUpdate()
    {
		var deltaTime = Time.DeltaTime;
		int a = Bubble_Query.CalculateEntityCount();
		if(a > 0)
        {
			BufferFromEntity<DB_Float> Get_buf = GetBufferFromEntity<DB_Float>(true);
			ComponentDataFromEntity<DOTS_Bubble_Data> GetBubbleData = GetComponentDataFromEntity<DOTS_Bubble_Data>();
			NativeArray<Entity> entities = Bubble_Query.ToEntityArray(Allocator.TempJob);

			for(int i = 0; i < entities.Length; i++)
			{
				var bubble_data = GetBubbleData[entities[i]];
				if (bubble_data.timeLeft <= 0)
				{
					float[] wave_data = DB_Float.ToArray(Get_buf[entities[i]], true);
					PlayBubbleSound("Test", wave_data, 1, sampleRate, true);
					this.EntityManager.DestroyEntity(entities[i]);
                }
                else
                {
					bubble_data.timeLeft -= deltaTime;
					GetBubbleData[entities[i]] = bubble_data;
				}
			}
			entities.Dispose();
        }
    }
	public void PlayBubbleSound(string name, float[] wave_data, int channels = 1, int sampleRate = 41000, bool stream = false)
	{
		Debug.Log("Playing the sound!");
		current_bubble = wave_data;
		clip = AudioClip.Create(name, wave_data.Length, channels, sampleRate, stream, OnAudioRead/*,OnAudioSetPosition*/);
		//	clip.SetData(wave_data, 0);
		PlayBubbleSound(clip);



	}
	public void PlayBubbleSound(AudioClip audioClip)
	{
		if (audioSource != null)
		{
			audioSource.clip = audioClip;
			audioSource.Play();
		}
		else Debug.LogError("somehow this audio source is null!");
	}
	int currentStep = 0;
	void OnAudioRead(float[] data)
	{
		//	Debug.Log($"data length: {data.Length}, currentStep {currentStep}," +
		//		$" max {bubbles[0].formatted_data.Length}");
		int total = current_bubble.Length - currentStep - data.Length;
		if (total > data.Length) total = data.Length;
		else if (total <= 0 && currentStep < current_bubble.Length)
			total = data.Length + total;

		//if (currentStep + data.Length >= bubbles[0].formatted_data.Length)
		if (total <= 0)
		{
			//		Debug.Log("exceeded data lnegth, no more!");
			for (int i = 0; i < data.Length; i++)
				data[i] = 0;
			return;
		}
		else
		{
			//	watch.Start();
			for (int i = 0; i < total; i++)
			{
				float value = 0;
				for (int j = 0; j < current_bubble.Length; j++)
					value += current_bubble[currentStep + i];
				data[i] = ClampToValidRange(value);

			}/*
		if (currentStep + data.Length >= bubbles[0].formatted_data.Length)
		{
			Debug.Log("Exceeded data length");
			currentStep = bubbles[0].formatted_data.Length;
		}
		else*/
			currentStep += data.Length;
		}
		//	watch.Stop();
		//	Debug.Log("Combining " + bubbles.Length + " bubbles took " + watch.ElapsedMilliseconds + "ms");
		//	watch.Reset();
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
