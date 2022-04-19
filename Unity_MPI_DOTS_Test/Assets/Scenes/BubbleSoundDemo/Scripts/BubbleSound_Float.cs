using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.OdeSolvers;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[System.Serializable]

public struct Float_BubbleSoundDataCondensedDynamic
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
	public static readonly Vector<double> DEFAULT_INITIAL_VALUE = Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
	public static readonly int DEFAULT_STEPS = 96000;
	//Todo:  make interfaces boolean

	public static readonly byte fluid_interface = (byte)1;
	public static readonly byte rigid_interface = (byte)2;
	public static readonly byte static_moving = (byte)1;
	public static readonly byte rising_moving = (byte)2;

	// 1 = static moving, 2 = rising moving
	[Tooltip("1 = static moving, 2 = rising moving")]
	[Range(1,2)]
	public byte m_movingtype;
	
	[Tooltip("1 = fluid interface, 2 = rigid interface")]
	[Range(1,2)]
	public byte m_interfacetype;
	[Range(0, 16)]
	public float radius;
	[Range(0, 16)]
	public float depth;
	public double start, end;
	public int steps;
	public int from, to;
	public bool IsInitialized;
	float min, max;
	public Vector<double>[] raw_data;
	public float[] formatted_data;

	public byte interfacetype
	{
		get { return m_interfacetype; }
		set { if (value == (byte)2 || value == (byte)1) m_interfacetype = value; }
	}
	public byte movingtype
	{
		get { return m_movingtype; }
		set { if (value == (byte)2 || value == (byte)1) m_movingtype = value; }
	}
	public enum Init_Mode
	{
		Raw,
		Formatted,
		Fast_Raw,
		Fast_Formatted
	}
	public Float_BubbleSoundDataCondensedDynamic(byte interface_type, byte moving_type, float radius, float depth,int from,int to,double start,double end,int steps)
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
		raw_data = new Vector<double>[0];
		formatted_data = new float[0];
		IsInitialized = false;
		min = 0;
		max = 0;
	}
	public void Init(Init_Mode mode, bool convert_to_wav_format = true,bool useFromTo = false)
	{
		Debug.Log($"{this.depth},{this.radius},{from},{to},{steps},{start},{end},{interfacetype},{movingtype}");
		switch (mode)
		{
			case Init_Mode.Raw:
				raw_data = CalculateRawSound(this, false);
				if (convert_to_wav_format)
				{
					if(!useFromTo)raw_data = ApplyWavFormat(raw_data);
					else raw_data = ApplyWavFormat(this.from, this.to, raw_data);
				}
				
				break;
			case Init_Mode.Fast_Raw:
				raw_data = CalculateRawSound(this, true);
				if (convert_to_wav_format)
				{
					if (!useFromTo) raw_data = ApplyWavFormat(raw_data);
					else raw_data = ApplyWavFormat(this.from, this.to, raw_data);
				}
				break;
			case Init_Mode.Formatted:
				raw_data = CalculateRawSound(this, false);
				if (convert_to_wav_format)
				{
					if (!useFromTo) formatted_data = ApplyWavFormat_f(raw_data);
					else formatted_data = ApplyWavFormat_f(this.from, this.to, raw_data);
				}
				break;
			case Init_Mode.Fast_Formatted:
				raw_data = CalculateRawSound(this, true);
				if (convert_to_wav_format)
				{
					if (!useFromTo) formatted_data = ApplyWavFormat_f(raw_data);
					else formatted_data = ApplyWavFormat_f(this.from, this.to, raw_data);
				}
				break;
		}
		IsInitialized = true;
	}
	public static float BubbleCapacitance(byte interface_type, float radius, float depth)
	{
		if (interface_type == rigid_interface)
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
		//	Debug.Log("r = "+radius+", d = "+depth+", C = " + bubbleCapacitance);

		float v0 = 4f / 3f * math.PI * math.pow(radius, 3);

		//	Debug.Log("C = " + v0+":: "+( 4.0f * math.PI * GAMMA * p0 * bubbleCapacitance )+ "::"+(RHO_WATER * v0)+"::"+ (4.0f * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0)));
		float omega = math.sqrt(4f * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0));

		//	Debug.Log("D = " + omega);
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
	public static Vector<double>[] CalculateRawSound(Float_BubbleSoundDataCondensedDynamic data, bool fastCalculation = false)
	{

		float m_depth = data.depth;
		float m_radius = data.radius;
		// Integrate the bubble sound into a buffer
		var sol = fastCalculation ? RungeKutta.SecondOrder(DEFAULT_INITIAL_VALUE, data.start,data.end, data.steps, DerivativeMaker()) 
			: RungeKutta.FourthOrder(DEFAULT_INITIAL_VALUE, data.start, data.end, data.steps, DerivativeMaker());
		Func<double, Vector<double>, Vector<double>> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				float f = Float_BubbleSoundData.JetForcing(m_radius, (float)t - 0.1f);

				float d = m_depth;

				if (data.movingtype == 2 && t >= 0.1f)
				{
					Debug.LogError("BBBBBB");
					// rising bubble, calc depth

					float vt = Float_BubbleSoundData.BubbleTerminalVelocity(m_radius);

					d = math.max(0.51f * 2f * m_radius, m_depth - ((float)t - 0.1f) * vt);

					// print('vt: ' + str(vt))
					// print('d: ' + str(d))
				}
				//if we let it run too long and the values get very small,
				// the scipy integrator has problems. Might be setting the time step too
				// small? So just exit when the oscillator loses enough energy
				if (t > 0.11f && math.sqrt(math.pow(Y[0], 2) + math.pow(Y[1], 2)) < 1e-15f)
				{
					return Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
				}
				else
				{
					float p0 = Float_BubbleSoundData.PATM + 2.0f * Float_BubbleSoundData.SIGMA / m_radius;
					float v0 = 4f / 3f * math.PI * math.pow(m_radius, 3);

					float w0 = Float_BubbleSoundData.ActualFreq(data.interfacetype, m_radius, d) * 2 * math.PI;
					float k = Float_BubbleSoundData.GAMMA * p0 / v0;

					float m = k / math.pow(w0, 2);

					float beta = Float_BubbleSoundData.CalcBeta(m_radius, w0);

					float acc = f / m - 2 * beta * (float)Y[0] - math.pow(w0, 2) * (float)Y[1];

					if (float.IsNaN(acc))
					{
				//		Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					//if(acc != 0 || Y[0] != 0 || Y[1] != 0)
					//	Debug.Log($"{acc},{Y[0]},{Y[1]}");
					return Vector<double>.Build.Dense(new[] { acc, Y[0] });
				}
			};
		}
		string s = "";
		for (int i = 0; i < sol.Length; i++)
			s += sol[i][1] + ",";
		return sol;
	}
	public static Vector<double>[] CalculateRawSound(DOTS_Bubble_Data data, bool fastCalculation = false)
	{
		Debug.Log($"P|{data.depth},{data.radius},{data.from},{data.to},{data.steps},{data.start},{data.end},{(byte)data.m_interfacetype},{(byte)data.m_movingtype}");

		float m_depth = data.depth;
		float m_radius = data.radius;
		// Integrate the bubble sound into a buffer
		var sol = fastCalculation ? RungeKutta.SecondOrder(DEFAULT_INITIAL_VALUE, data.start, data.end, data.steps, DerivativeMaker())
			: RungeKutta.FourthOrder(DEFAULT_INITIAL_VALUE, data.start, data.end, data.steps, DerivativeMaker());
		Func<double, Vector<double>, Vector<double>> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				float f = Float_BubbleSoundData.JetForcing(m_radius, (float)t - 0.1f);

				float d = m_depth;

				if ((byte)data.m_movingtype == 2 && t >= 0.1f)
				{
					Debug.LogError("BBBBBB");
					// rising bubble, calc depth

					float vt = Float_BubbleSoundData.BubbleTerminalVelocity(m_radius);

					d = math.max(0.51f * 2f * m_radius, m_depth - ((float)t - 0.1f) * vt);

					// print('vt: ' + str(vt))
					// print('d: ' + str(d))
				}
				//if we let it run too long and the values get very small,
				// the scipy integrator has problems. Might be setting the time step too
				// small? So just exit when the oscillator loses enough energy
				if (t > 0.11f && math.sqrt(math.pow(Y[0], 2) + math.pow(Y[1], 2)) < 1e-15f)
				{
					return Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
				}
				else
				{
					float p0 = Float_BubbleSoundData.PATM + 2.0f * Float_BubbleSoundData.SIGMA / m_radius;
					float v0 = 4f / 3f * math.PI * math.pow(m_radius, 3);

					float w0 = Float_BubbleSoundData.ActualFreq((byte)data.m_interfacetype, m_radius, d) * 2 * math.PI;
					float k = Float_BubbleSoundData.GAMMA * p0 / v0;

					float m = k / math.pow(w0, 2);

					float beta = Float_BubbleSoundData.CalcBeta(m_radius, w0);

					float acc = f / m - 2 * beta * (float)Y[0] - math.pow(w0, 2) * (float)Y[1];

					if (float.IsNaN(acc))
					{
						//		Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					//if(acc != 0 || Y[0] != 0 || Y[1] != 0)
					//	Debug.Log($"{acc},{Y[0]},{Y[1]}");
					return Vector<double>.Build.Dense(new[] { acc, Y[0] });
				}
			};
		}
		string s = "";
		for (int i = 0; i < sol.Length; i++)
			s += sol[i][1] + ",";
		Debug.Log(s);
		return sol;
	}

	public static Vector<double>[] ApplyWavFormat(int from,int to,Vector<double>[] input)
	{
		List<Vector<double>> data = new List<Vector<double>>();
		data.AddRange(input);
		data.RemoveRange(from, to - from);
		return ApplyWavFormat(data.ToArray());
	}
	public static Vector<double>[] ApplyWavFormat(Vector<double>[] input)
	{
		// now we trim
		var minmax = GetMinMax(input);
		return ApplyWavFormat(minmax[0], minmax[1],input);
	}
	public static Vector<double>[] ApplyWavFormat(double min,double max,Vector<double>[] input)
	{
		// now we trim
		var minmax = GetMinMax(input);
		for (int i = 0; i < input.Length; i++)
			input[i][1] = input[i][1] / math.max(minmax[0], minmax[1]) * 1.05;
		return input;
	}

	public static float[] ApplyWavFormat_f(int from, int to, Vector<double>[] input)
	{
		List<Vector<double>> data = new List<Vector<double>>();
		data.AddRange(input);
		data.RemoveRange(from, to - from);
		return ApplyWavFormat_f(data.ToArray());
	}
	private static float[] ApplyWavFormat_f(Vector<double>[] input)
	{
		var minmax = GetMinMax(input, out float[] tmp);
		return ApplyWavFormat_f(minmax[0], minmax[1],tmp);
	}
	static float[] ApplyWavFormat_f(float min,float max,float[] input)
	{
		for (int i = 0; i < input.Length; i++)
			input[i] = input[i] / math.max(min, max) * 1.05f;
		return input;
	}

	public static float[] GetMinMax(Vector<double>[] sol)
	{
		float max = float.MinValue;
		float min = float.MaxValue;
		for (int i = 0; i < sol.Length; i++)
		{
			if (sol[i][1] > max) max = (float)sol[i][1];
			if (sol[i][1] < min) min = (float)sol[i][1];
		}
		return new float[2] { min, max };
	}
	public static float[] GetMinMax(Vector<double>[] sol, out float[] simplified_arr)
	{
		float max = float.MinValue;
		float min = float.MaxValue;
		simplified_arr = new float[sol.Length];
		for (int i = 0; i < sol.Length; i++)
		{
			simplified_arr[i] = (float)sol[i][1];
			if (simplified_arr[i] > max) max = (float)simplified_arr[i];
			if (simplified_arr[i] < min) min = (float)simplified_arr[i];
		}
		return new float[2] { min, max };
	}

	public static float[] GenerateBubble(Float_BubbleSoundDataCondensedDynamic data, Vector<double> initialValue, double start, double end, int steps, out Vector<double> lastValue)
	{
		//	int numsteps = 96000;

		//	float dt = 1f / (numsteps - 1);

		float m_depth = data.depth;
		float m_radius = data.radius;
		// Integrate the bubble sound into a buffer
		var sol = RungeKutta.FourthOrder(initialValue, start, end, steps, DerivativeMaker());
		Func<double, Vector<double>, Vector<double>> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				float f = Float_BubbleSoundData.JetForcing(m_radius, (float)t - 0.1f);

				float d = m_depth;

				if (data.movingtype == 2 && t >= 0.1f)
				{
					// rising bubble, calc depth

					float vt = Float_BubbleSoundData.BubbleTerminalVelocity(m_radius);

					d = math.max(0.51f * 2f * m_radius, m_depth - ((float)t - 0.1f) * vt);

					// print('vt: ' + str(vt))
					// print('d: ' + str(d))
				}
				//if we let it run too long and the values get very small,
				// the scipy integrator has problems. Might be setting the time step too
				// small? So just exit when the oscillator loses enough energy
				if (t > 0.11f && math.sqrt(math.pow(Y[0], 2) + math.pow(Y[1], 2)) < 1e-15f)
				{
					return Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
				}
				else
				{
					float p0 = Float_BubbleSoundData.PATM + 2.0f * Float_BubbleSoundData.SIGMA / m_radius;
					float v0 = 4f / 3f * math.PI * math.pow(m_radius, 3);

					float w0 = Float_BubbleSoundData.ActualFreq(data.interfacetype, m_radius, d) * 2 * math.PI;
					float k = Float_BubbleSoundData.GAMMA * p0 / v0;

					float m = k / math.pow(w0, 2);

					float beta = Float_BubbleSoundData.CalcBeta(m_radius, w0);

					float acc = f / m - 2 * beta * (float)Y[0] - math.pow(w0, 2) * (float)Y[1];

					if (float.IsNaN(acc))
					{
						Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					return Vector<double>.Build.Dense(new[] { acc, Y[0] });
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
}
[System.Serializable]
public struct Float_BubbleSoundDataCondensed
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
	public static readonly Vector<double> DEFAULT_INITIAL_VALUE = Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
	//Todo:  make interfaces boolean
	public static readonly byte fluid_interface = (byte)1;
	public static readonly byte rigid_interface = (byte)2;
	public static readonly byte static_moving = (byte)1;
	public static readonly byte rising_moving = (byte)2;
	// 1 = static moving, 2 = rising moving
	public byte m_movingtype;
	// 1 = fluid interface, 2 = rigid interface
	public byte m_interfacetype;

	public float radius, depth;
	public int steps;
	public bool IsInitialized;
	float min,max;
	public Vector<double>[] raw_data;
	public float[] formatted_data;

	public byte interfacetype
	{
		get { return m_interfacetype; }
		set { if (value == (byte)2 || value == (byte)1) m_interfacetype = value; }
	}
	public byte movingtype
	{
		get { return m_movingtype; }
		set { if (value == (byte)2 || value == (byte)1) m_movingtype = value; }
	}
	public enum Init_Mode
	{
		Raw,
		Formatted,
		Fast_Raw,
		Fast_Formatted
	}
	public Float_BubbleSoundDataCondensed(byte interface_type, byte moving_type, float radius, float depth,int steps)
	{
		m_interfacetype = interface_type;
		m_movingtype = moving_type;
		this.depth = depth * radius / 1000f * 2f;
		this.radius = radius / 1000f;
		this.steps = steps;
		raw_data = new Vector<double>[0];
		formatted_data = new float[0];
		IsInitialized = false;
		min = 0;
		max = 0;
	}
	public void Init(Init_Mode mode,bool convert_to_wav_format =true,double start = 0,double end = 1)
	{
		switch (mode)
		{
			case Init_Mode.Raw:
				raw_data = CalculateRawSound(this, mode,start,end);
				if (convert_to_wav_format) raw_data = ApplyWavFormat(raw_data);
				break;
			case Init_Mode.Fast_Raw:
				raw_data = CalculateRawSound(this, mode, start, end);
				if (convert_to_wav_format) raw_data = ApplyWavFormat(raw_data);
				break;
			case Init_Mode.Formatted:
				raw_data = CalculateRawSound(this, mode, start, end);
				formatted_data = ApplyWavFormat_f(raw_data);
				break;
			case Init_Mode.Fast_Formatted:
				raw_data = CalculateRawSound(this, mode, start, end);
				formatted_data = ApplyWavFormat_f(raw_data);
				break;
		}
		IsInitialized = true;
	}
	
	public static float BubbleCapacitance(byte interface_type, float radius, float depth)
	{
		if (interface_type == rigid_interface)
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
		//	Debug.Log("r = "+radius+", d = "+depth+", C = " + bubbleCapacitance);

		float v0 = 4f / 3f * math.PI * math.pow(radius, 3);

		//	Debug.Log("C = " + v0+":: "+( 4.0f * math.PI * GAMMA * p0 * bubbleCapacitance )+ "::"+(RHO_WATER * v0)+"::"+ (4.0f * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0)));
		float omega = math.sqrt(4f * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0));

		//	Debug.Log("D = " + omega);
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
	
	public static  Vector<double>[] CalculateRawSound(Float_BubbleSoundDataCondensed data,
		Float_BubbleSoundDataCondensed.Init_Mode mode,double start = 0,double end = 1)
	{
		float m_depth = data.depth;
		float m_radius = data.radius;
		// Integrate the bubble sound into a buffer
		Vector<double>[] sol = new Vector<double>[] { Vector<double>.Build.Dense(new[] { 0.0, 0.0 }) };
		switch (mode)
        {
			case Init_Mode.Fast_Formatted:
				sol = RungeKutta.SecondOrder(DEFAULT_INITIAL_VALUE, start, end, data.steps, DerivativeMaker());
				break;
			case Init_Mode.Formatted:
				sol = RungeKutta.FourthOrder(DEFAULT_INITIAL_VALUE, start, end, data.steps, DerivativeMaker());
				break;
		}
		
		Func<double, Vector<double>, Vector<double>> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				float f = Float_BubbleSoundData.JetForcing(m_radius, (float)t - 0.1f);

				float d = m_depth;

				if (data.movingtype == 2 && t >= 0.1f)
				{
					// rising bubble, calc depth

					float vt = Float_BubbleSoundData.BubbleTerminalVelocity(m_radius);

					d = math.max(0.51f * 2f * m_radius, m_depth - ((float)t - 0.1f) * vt);

					// print('vt: ' + str(vt))
					// print('d: ' + str(d))
				}
				//if we let it run too long and the values get very small,
				// the scipy integrator has problems. Might be setting the time step too
				// small? So just exit when the oscillator loses enough energy
				if (t > 0.11f && math.sqrt(math.pow(Y[0], 2) + math.pow(Y[1], 2)) < 1e-15f)
				{
					return Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
				}
				else
				{
					float p0 = Float_BubbleSoundData.PATM + 2.0f * Float_BubbleSoundData.SIGMA / m_radius;
					float v0 = 4f / 3f * math.PI * math.pow(m_radius, 3);

					float w0 = Float_BubbleSoundData.ActualFreq(data.interfacetype, m_radius, d) * 2 * math.PI;
					float k = Float_BubbleSoundData.GAMMA * p0 / v0;

					float m = k / math.pow(w0, 2);

					float beta = Float_BubbleSoundData.CalcBeta(m_radius, w0);

					float acc = f / m - 2 * beta * (float)Y[0] - math.pow(w0, 2) * (float)Y[1];

					if (float.IsNaN(acc))
					{
						Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					return Vector<double>.Build.Dense(new[] { acc, Y[0] });
				}
			};
		}
		return sol;
	}
	//This doesn't work don't use
	public static double[] CalculateRawSound(Float_BubbleSoundDataCondensed data,
		Float_BubbleSoundDataCondensed.Init_Mode mode,bool a)
	{
		float m_depth = data.depth;
		float m_radius = data.radius;
		// Integrate the bubble sound into a buffer
		double[] sol = new double[] { };
		switch (mode)
		{
			case Init_Mode.Fast_Formatted:
				sol = AdamsBashforth.FourthOrder(0, 0, 1, data.steps, DerivativeMaker());
				break;
			case Init_Mode.Formatted:
				sol = AdamsBashforth.SecondOrder(0, 0, 1, data.steps, DerivativeMaker());
				break;
		}

		Func<double, double, double> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				float f = Float_BubbleSoundData.JetForcing(m_radius, (float)t - 0.1f);

				float d = m_depth;

				if (data.movingtype == 2 && t >= 0.1f)
				{
					// rising bubble, calc depth

					float vt = Float_BubbleSoundData.BubbleTerminalVelocity(m_radius);

					d = math.max(0.51f * 2f * m_radius, m_depth - ((float)t - 0.1f) * vt);

					// print('vt: ' + str(vt))
					// print('d: ' + str(d))
				}
				//if we let it run too long and the values get very small,
				// the scipy integrator has problems. Might be setting the time step too
				// small? So just exit when the oscillator loses enough energy
				if (t > 0.11f && math.sqrt(math.pow(Y, 2) + math.pow(Y, 2)) < 1e-15f)
				{
					return 0.0;
				}
				else
				{
					float p0 = Float_BubbleSoundData.PATM + 2.0f * Float_BubbleSoundData.SIGMA / m_radius;
					float v0 = 4f / 3f * math.PI * math.pow(m_radius, 3);

					float w0 = Float_BubbleSoundData.ActualFreq(data.interfacetype, m_radius, d) * 2 * math.PI;
					float k = Float_BubbleSoundData.GAMMA * p0 / v0;

					float m = k / math.pow(w0, 2);

					float beta = Float_BubbleSoundData.CalcBeta(m_radius, w0);

					float acc = f / m - 2 * beta * (float)Y - math.pow(w0, 2) * (float)Y;

					if (float.IsNaN(acc))
					{
						Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y + ", y[1]=" + Y);
					}
					return Y;
				}
			};
		}
		return sol;
	}

	public static Vector<double>[] ApplyWavFormat(Vector<double>[] input)
	{
		var minmax = GetMinMax(input);
		for (int i = 0; i < input.Length; i++)
		{
			input[i][1] = input[i][1] / math.max(minmax[0], minmax[1]) * 1.05;
		}
		return input;
	}
	private static float[] ApplyWavFormat_f(Vector<double>[] input)
	{
		var minmax = GetMinMax(input,out float[] tmp);
		for (int i = 0; i < input.Length; i++)
		{
			tmp[i] = tmp[i] / math.max(minmax[0], minmax[1]) * 1.05f;
		}
		return tmp;
	}
	private static float[] ApplyWavFormat_f(double[] input)
	{
		var minmax = GetMinMax(input, out float[] tmp);
		for (int i = 0; i < input.Length; i++)
		{
			tmp[i] = tmp[i] / math.max(minmax[0], minmax[1]) * 1.05f;
		}
		return tmp;
	}
	public static float[] GetMinMax(Vector<double>[] sol)
	{
		float max = float.MinValue;
		float min = float.MaxValue;
		for (int i = 0; i <sol.Length; i++)
		{
			if (sol[i][1] > max) max = (float)sol[i][1];
			if (sol[i][1] < min) min = (float)sol[i][1];
		}
		return new float[2] { min,max};
	}
	public static float[] GetMinMax(double[] sol)
	{
		float max = float.MinValue;
		float min = float.MaxValue;
		for (int i = 0; i < sol.Length; i++)
		{
			if (sol[i] > max) max = (float)sol[i];
			if (sol[i] < min) min = (float)sol[i];
		}
		return new float[2] { min, max };
	}
	public static float[] GetMinMax(Vector<double>[] sol,out float[] simplified_arr)
	{
		float max = float.MinValue;
		float min = float.MaxValue;
		simplified_arr = new float[sol.Length];
		for (int i = 0; i < sol.Length; i++)
		{
			simplified_arr[i] = (float)sol[i][1];
			if (simplified_arr[i] > max) max = (float)simplified_arr[i];
			if (simplified_arr[i] < min) min = (float)simplified_arr[i];
		}
		return new float[2] { min, max };
	}
	public static float[] GetMinMax(double[] sol, out float[] simplified_arr)
	{
		float max = float.MinValue;
		float min = float.MaxValue;
		simplified_arr = new float[sol.Length];
		for (int i = 0; i < sol.Length; i++)
		{
			simplified_arr[i] = (float)sol[i];
			if (simplified_arr[i] > max) max = (float)simplified_arr[i];
			if (simplified_arr[i] < min) min = (float)simplified_arr[i];
		}
		return new float[2] { min, max };
	}

	public static float[] GenerateBubble(Float_BubbleSoundDataCondensed data,Vector<double> initialValue, double start, double end,int steps,out Vector<double> lastValue)
	{
		//	int numsteps = 96000;

		//	float dt = 1f / (numsteps - 1);

		float m_depth = data.depth;
		float m_radius = data.radius;
		// Integrate the bubble sound into a buffer
		var sol = RungeKutta.FourthOrder(initialValue, start, end, steps, DerivativeMaker());
		Func<double, Vector<double>, Vector<double>> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				float f = Float_BubbleSoundData.JetForcing(m_radius, (float)t - 0.1f);

				float d = m_depth;

				if (data.movingtype == 2 && t >= 0.1f)
				{
					// rising bubble, calc depth

					float vt = Float_BubbleSoundData.BubbleTerminalVelocity(m_radius);

					d = math.max(0.51f * 2f * m_radius, m_depth - ((float)t - 0.1f) * vt);

					// print('vt: ' + str(vt))
					// print('d: ' + str(d))
				}
				//if we let it run too long and the values get very small,
				// the scipy integrator has problems. Might be setting the time step too
				// small? So just exit when the oscillator loses enough energy
				if (t > 0.11f && math.sqrt(math.pow(Y[0], 2) + math.pow(Y[1], 2)) < 1e-15f)
				{
					return Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
				}
				else
				{
					float p0 = Float_BubbleSoundData.PATM + 2.0f * Float_BubbleSoundData.SIGMA / m_radius;
					float v0 = 4f / 3f * math.PI * math.pow(m_radius, 3);

					float w0 = Float_BubbleSoundData.ActualFreq(data.interfacetype, m_radius, d) * 2 * math.PI;
					float k = Float_BubbleSoundData.GAMMA * p0 / v0;

					float m = k / math.pow(w0, 2);

					float beta = Float_BubbleSoundData.CalcBeta(m_radius, w0);

					float acc = f / m - 2 * beta * (float)Y[0] - math.pow(w0, 2) * (float)Y[1];

					if (float.IsNaN(acc))
					{
						Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					return Vector<double>.Build.Dense(new[] { acc, Y[0] });
				}
			};
		}

		float[] wave_data = new float[steps];
		for (int i = 0; i < wave_data.Length; i++)
		{
			wave_data[i] = (float)sol[i][1] / math.max(data.min, data.max) * 1.05f;
		}
		lastValue = sol[sol.Length-1];
		return wave_data;

	}


	public static float GenerateBubble(Float_BubbleSoundDataCondensed data,double start,double end)
	{
	//	int numsteps = 96000;

	//	float dt = 1f / (numsteps - 1);

		float m_depth = data.depth;
		float m_radius = data.radius;
		// Integrate the bubble sound into a buffer
		var sol = RungeKutta.FourthOrder(DEFAULT_INITIAL_VALUE, start, end, 1, DerivativeMaker());
		Func<double, Vector<double>, Vector<double>> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				float f = Float_BubbleSoundData.JetForcing(m_radius, (float)t - 0.1f);

				float d = m_depth;

				if (data.movingtype == 2 && t >= 0.1f)
				{
					// rising bubble, calc depth

					float vt = Float_BubbleSoundData.BubbleTerminalVelocity(m_radius);

					d = math.max(0.51f * 2f * m_radius, m_depth - ((float)t - 0.1f) * vt);

					// print('vt: ' + str(vt))
					// print('d: ' + str(d))
				}
				//if we let it run too long and the values get very small,
				// the scipy integrator has problems. Might be setting the time step too
				// small? So just exit when the oscillator loses enough energy
				if (t > 0.11f && math.sqrt(math.pow(Y[0], 2) + math.pow(Y[1], 2)) < 1e-15f)
				{
					return Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
				}
				else
				{
					float p0 = Float_BubbleSoundData.PATM + 2.0f * Float_BubbleSoundData.SIGMA / m_radius;
					float v0 = 4f / 3f * math.PI * math.pow(m_radius, 3);

					float w0 = Float_BubbleSoundData.ActualFreq(data.interfacetype, m_radius, d) * 2 * math.PI;
					float k = Float_BubbleSoundData.GAMMA * p0 / v0;

					float m = k / math.pow(w0, 2);

					float beta = Float_BubbleSoundData.CalcBeta(m_radius, w0);

					float acc = f / m - 2 * beta * (float)Y[0] - math.pow(w0, 2) * (float)Y[1];

					if (float.IsNaN(acc))
					{
						Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					return Vector<double>.Build.Dense(new[] { acc, Y[0] });
				}
			};
		}

		return (float)sol[0][1];

	/*	wave_data = new float[sol.Length];
		// Get Max And Min Value
		float max = float.MinValue;
		float min = float.MaxValue;
		for (int i = 0; i < wave_data.Length; i++)
		{
			if (sol[i][1] != 0)
				wave_data[i] = (float)sol[i][1];
			if (wave_data[i] > max) max = wave_data[i];
			if (wave_data[i] < min) min = wave_data[i];
		}
		for (int i = 0; i < wave_data.Length; i++)
		{
			wave_data[i] /= math.max(min, max) * 1.05f;
		}*/
	
	}
}
public struct Float_BubbleSoundData
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
	public static readonly Vector<double> DEFAULT_INITIAL_VALUE = Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
	//Todo:  make interfaces boolean
	public static readonly byte fluid_interface = (byte)1;
	public static readonly byte rigid_interface = (byte)2;
	public static readonly byte static_moving = (byte)1;
	public static readonly byte rising_moving = (byte)2;
	// 1 = static moving, 2 = rising moving
	byte m_movingtype;
	// 1 = fluid interface, 2 = rigid interface
	byte m_interfacetype;

	float radius, depth;

	public byte interfacetype
	{
		get { return m_interfacetype; }
		set { if (value == (byte)2 || value == (byte)1) m_interfacetype = value; }
	}
	public byte movingtype
	{
		get { return m_movingtype; }
		set { if (value == (byte)2 || value == (byte)1) m_movingtype = value; }
	}

	public Float_BubbleSoundData(byte interface_type, byte moving_type, float radius, float depth)
	{
		m_interfacetype = interface_type;
		m_movingtype = moving_type;
		this.radius = radius;
		this.depth = depth;
	}

	public Float_BubbleSoundData Default => new Float_BubbleSoundData
	{
		m_interfacetype = fluid_interface,
		m_movingtype = static_moving,
		radius = 1,
		depth = 1
	};

	public static float BubbleCapacitance(byte interface_type, float radius, float depth)
	{
		if (interface_type == rigid_interface)
			return radius / (1f - radius / (2f * depth) - math.pow((radius / (2f * depth)), 4));
		else // Rigid interface
			return radius / (1f + radius / (2f * depth) - math.pow((radius / (2f * depth)), 4));
	}
	public static double BubbleCapacitance(byte interface_type, double radius, double depth)
	{
		if (interface_type == rigid_interface)
			return radius / (1 - radius / (2 * depth) - math.pow((radius / (2 * depth)), 4));
		else // Rigid interface
			return radius / (1 + radius / (2 * depth) - math.pow((radius / (2 * depth)), 4));
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
		//	Debug.Log("r = "+radius+", d = "+depth+", C = " + bubbleCapacitance);

		float v0 = 4f / 3f * math.PI * math.pow(radius, 3);

		//	Debug.Log("C = " + v0+":: "+( 4.0f * math.PI * GAMMA * p0 * bubbleCapacitance )+ "::"+(RHO_WATER * v0)+"::"+ (4.0f * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0)));
		float omega = math.sqrt(4f * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0));

		//	Debug.Log("D = " + omega);
		return omega / 2f / math.PI;
	}
	public static double ActualFreq(byte interface_type, double radius, double depth)
	{
		double bubbleCapacitance = BubbleCapacitance(interface_type, radius, depth);

		double p0 = PATM;
		//	Debug.Log("r = "+radius+", d = "+depth+", C = " + bubbleCapacitance);

		double v0 = 4 / 3 * math.PI * math.pow(radius, 3);

		//	Debug.Log("C = " + v0+":: "+( 4.0f * math.PI * GAMMA * p0 * bubbleCapacitance )+ "::"+(RHO_WATER * v0)+"::"+ (4.0f * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0)));
		double omega = math.sqrt(4 * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0));

		//	Debug.Log("D = " + omega);
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
	public static double CalcBeta(double radius, double w0)
	{

		double dr = w0 * radius / CF;
		double dvis = 4 * MU / (RHO_WATER * w0 * math.pow(radius, 2));

		double phi = 16 * GTH * G / (9f * math.pow((GAMMA - 1), 2) * w0);

		double dth = 2 * (math.sqrt(phi - 3) - (3 * GAMMA - 1) /
				 (3 * (GAMMA - 1))) / (phi - 4);


		double dtotal = dr + dvis + dth;


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
	public static double JetForcing(double r, double t,bool a)
	{

		double cutoff = math.min(0.0006, 0.5 / (3 / r));

		if (t < 0 || t > cutoff)
			return 0;
		double jval = (-9 * GAMMA * SIGMA * ETA *
				(PATM + 2 * SIGMA / r) * math.sqrt(1 + math.pow(ETA, 2)) /
				(4 * math.pow(RHO_WATER, 2) * math.pow(r, 5)) * math.pow(t, 2));

		// Convert to radius (instead of fractional radius)
		jval *= r;

		// Convert to pressure
		double mrp = RHO_WATER * r;

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
	/// <summary>
	/// 
	/// </summary>
	/// <param name="moving_type"></param>
	/// <param name="y">a float array of size 2</param>
	/// <param name="t"></param>
	/// <param name="r"></param>
	/// <param name="d0"></param>
	/// <param name="dt"></param>
	/// <param name="of">this is a python file...ignore this</param>
	/// <returns></returns>
	public static float[] BubbleIntegrator(byte interface_type, byte moving_type, float[] y, float t, float r, float d0, float dt/*, File of*/)
	{
		//[f'; f]

		float f = JetForcing(r, t - 0.1f);

		float d = d0;

		if (moving_type == 2 && t >= 0.1f)
		{
			// rising bubble, calc depth

			float vt = BubbleTerminalVelocity(r);

			d = math.max(0.51f * 2f * r, d0 - (t - 0.1f) * vt);

			// print('vt: ' + str(vt))
			// print('d: ' + str(d))
		}
		//if we let it run too long and the values get very small,
		// the scipy integrator has problems. Might be setting the time step too
		// small? So just exit when the oscillator loses enough energy
		if (t > 0.11f && math.sqrt(math.pow(y[0], 2) + math.pow(y[1], 2)) < 1e-15f)
		{
			return new float[] { 0, 0 };
		}

		float p0 = PATM + 2.0f * SIGMA / r;
		float v0 = 4f / 3f * math.PI * math.pow(r, 3);

		float w0 = ActualFreq(interface_type, r, d) * 2 * math.PI;
		float k = GAMMA * p0 / v0;

		float m = k / math.pow(w0, 2);

		float beta = CalcBeta(r, w0);

		float acc = f / m - 2 * beta * y[0] - math.pow(w0, 2) * y[1];

#if DEBUG_MODE
		CommonFunctions.Log("BubbleIntegrator: acc = "+acc);
#endif
		//	if of:
		//        of.write(str(w0 / 2 / math.pi) + ' ' + str(y[0]) + '\n')

		// print(y)
		/*	if np.isnan(acc) or max(y) > 1e-4:
				print('y: ' + str(y))

				print('f: ' + str(f))

				print('m: ' + str(m))

				print('w0: ' + str(w0))

				print('beta: ' + str(beta))

				print('t: ' + str(t))


				raise Exception('nan')
			*/
		if (float.IsNaN(acc))
		{
			Debug.Log("DETECTED NAN! y = " + y + ", f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + y[0] + ", y[1]=" + y[1]);
		}
		return new float[] { acc, y[0] };
	}

	public static float[] LINSPACE(float StartValue, float EndValue, int numberofpoints)
	{

		float[] parameterVals = new float[numberofpoints];
		float increment = Math.Abs(StartValue - EndValue) / (float)(numberofpoints - 1);
		int j = 0; //will keep a track of the numbers 
		float nextValue = StartValue;
		for (int i = 0; i < numberofpoints; i++)
		{


			parameterVals.SetValue(nextValue, j);
			j++;
			if (j > numberofpoints)
			{
				throw new IndexOutOfRangeException();
			}
			nextValue = nextValue + increment;
		}
		return parameterVals;



	}

	public static void play_bubble(byte interface_type, byte moving_type, float r, float d, bool save_file, out float[] wav_data)
	{
		// modify values 
		d = d * r / 1000f * 2f;
		r /= 1000f;

		int numsteps = 96000;

		float dt = 1f / (numsteps - 1f);

		// Integrate the bubble sound into a buffer
		var sol = new IntegrateBubble(interface_type, moving_type, DEFAULT_INITIAL_VALUE, r, d, dt, 0, 1, numsteps).Exec();

		wav_data = new float[sol.Length];
		float max = float.MinValue;
		float min = float.MaxValue;
		for (int i = 0; i < wav_data.Length; i++)
		{
			if (sol[i][1] != 0)
				wav_data[i] = (float)sol[i][1];
			if (wav_data[i] > max) max = wav_data[i];
			if (wav_data[i] < min) min = wav_data[i];
		}
		for (int i = 0; i < wav_data.Length; i++)
		{
			wav_data[i] /= math.max(min, max) * 1.05f;
		}
		if (save_file)
		{
			WaveFormat waveFormat = new WaveFormat(numsteps, 1);
			using (WaveFileWriter writer = new WaveFileWriter(Application.dataPath + "/Wav_Outputs/test.wav", waveFormat))
			{
				writer.WriteSamples(wav_data, 0, wav_data.Length);
			}
			Debug.Log("saved file to" + Application.dataPath + "/Wav_Outputs/text.wav");
		}
	}
	public static void play_bubble(Float_BubbleSoundData data, bool save_file, out float[] wav_data)
	{
		play_bubble(data.interfacetype, data.movingtype, data.radius, data.depth, save_file, out wav_data);
	}

	public static float[] ModifyRadiusAndDepth(float radius, float depth)
	{
		depth = depth * radius / 1000f * 2f;
		return new float[] { radius / 1000f, depth };
	}

	public static void GenerateBubble(Float_BubbleSoundData data, out float[] wave_data, out int channels, out int sampleRate)
	{

		// Set Defaults
		channels = 1;
		sampleRate = 41000;
		// modify values 
		// modified depth
		float m_depth = data.depth * data.radius / 1000f * 2f;
		// modified radius
		float m_radius = data.radius / 1000f;

		int numsteps = 96000;

		float dt = 1f / (numsteps - 1);

		// Integrate the bubble sound into a buffer
		var sol = RungeKutta.FourthOrder(DEFAULT_INITIAL_VALUE, 0, 1, numsteps, DerivativeMaker());

		wave_data = new float[sol.Length];
		// Get Max And Min Value
		float max = float.MinValue;
		float min = float.MaxValue;
		for (int i = 0; i < wave_data.Length; i++)
		{
			if (sol[i][1] != 0)
				wave_data[i] = (float)sol[i][1];
			if (wave_data[i] > max) max = wave_data[i];
			if (wave_data[i] < min) min = wave_data[i];
		}
		for (int i = 0; i < wave_data.Length; i++)
		{
			wave_data[i] /= math.max(min, max) * 1.05f;
		}
		Func<double, Vector<double>, Vector<double>> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				float f = Float_BubbleSoundData.JetForcing(m_radius, (float)t - 0.1f);

				float d = m_depth;

				if (data.movingtype == 2 && t >= 0.1f)
				{
					// rising bubble, calc depth

					float vt = Float_BubbleSoundData.BubbleTerminalVelocity(m_radius);

					d = math.max(0.51f * 2f * m_radius, m_depth - ((float)t - 0.1f) * vt);

					// print('vt: ' + str(vt))
					// print('d: ' + str(d))
				}
				//if we let it run too long and the values get very small,
				// the scipy integrator has problems. Might be setting the time step too
				// small? So just exit when the oscillator loses enough energy
				if (t > 0.11f && math.sqrt(math.pow(Y[0], 2) + math.pow(Y[1], 2)) < 1e-15f)
				{
					return Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
				}
				else
				{
					float p0 = Float_BubbleSoundData.PATM + 2.0f * Float_BubbleSoundData.SIGMA / m_radius;
					float v0 = 4f / 3f * math.PI * math.pow(m_radius, 3);

					float w0 = Float_BubbleSoundData.ActualFreq(data.interfacetype, m_radius, d) * 2 * math.PI;
					float k = Float_BubbleSoundData.GAMMA * p0 / v0;

					float m = k / math.pow(w0, 2);

					float beta = Float_BubbleSoundData.CalcBeta(m_radius, w0);

					float acc = f / m - 2 * beta * (float)Y[0] - math.pow(w0, 2) * (float)Y[1];

					if (float.IsNaN(acc))
					{
						Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					return Vector<double>.Build.Dense(new[] { acc, Y[0] });
				}
			};
		}

	}
}