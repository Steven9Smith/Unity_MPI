using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using System;
using NAudio.Wave;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.OdeSolvers;
using System.Threading;
using System.Threading.Tasks;

public class IntegrateBubbleMultiThreaded
{
	public Vector<double> y0;
	public int nSize;
	public double start, end;
	double d0, r, dt;
	byte interface_type, moving_type;
	int threads,completedThreads;
	public List<Vector<double>> data;

	public IntegrateBubbleMultiThreaded(byte interface_type, byte moving_type, Vector<double> y0, double r, double d0, double dt,int threads = 2, double start = 0, double end = 1, int nSize = 96000)
	{
		this.interface_type = interface_type;
		this.moving_type = moving_type;
		this.y0 = y0;
		this.r = r;
		this.d0 = d0;
		this.dt = dt;
		this.start = start;
		this.end = end;
		this.nSize = nSize;
		this.threads = threads;
		this.completedThreads = 0;
		data = new List<Vector<double>>();
	}
	public async Task<Vector<double>[]> Exec()
	{
	Debug.Log("Starting thread thing");
	
		var result = await new IntegrateBubble_Struct(interface_type, moving_type, y0, r, d0, dt, start, end, nSize).Exec(false);
		data.AddRange(result);
		var a = new List<IntegrateBubble_Struct>();
	/*	for (int i = 0; i < threads; i++)
		{
			double fraction = this.end / threads;
			double start = this.start + fraction * i;
			double end = this.end - (threads - i - 1) * fraction;
			a.Add(new IntegrateBubble_Struct(interface_type,moving_type,y0,r,d0,dt,start,end,nSize/threads));
			var result = await a[a.Count - 1].Exec(false);
			
		//	Thread t = new Thread(a[a.Count-1].Exec);
		//	t.Start();
		}
		int size = a.Count;*/
	/*	for (int i = 0; i < size; i++)
		{
			if (a[i].isReady && !a[i].isRunning)
			{
				Debug.Log(i + " is ready");
				data.AddRange(a[i].data);
				completedThreads++;
				a.RemoveAt(i);
				size--;
			}
		}*/
		
		return data.ToArray();
	}
}
public class IntegrateBubble_Struct
{
	public Vector<double> y0;
	public int nSize;
	public double start, end;
	double d0, r, dt;
	byte interface_type, moving_type;
	public bool isReady,isRunning;
	public Vector<double>[] data;
	public IntegrateBubble_Struct(byte interface_type, byte moving_type, Vector<double> y0, double r, double d0, double dt, double start = 0, double end = 1, int nSize = 96000)
	{
		this.y0 = y0;
		this.nSize = nSize;
		this.start = start;
		this.end = end;
		this.d0 = d0;
		this.r = r;
		this.dt = dt;
		this.interface_type = interface_type;
		this.moving_type = moving_type;
		isReady = false;
		data = new Vector<double>[0]; 
	}
//	(data.interfacetype, data.movingtype, DEFAULT_INITIAL_VALUE, m_radius, m_depth, dt, 0, 1, numsteps);
		
	public void Exec(){
		var m_y0 = y0;
		var m_nSize = nSize;
		var m_start = start;
		var m_end = end;
		var m_d0 = d0;
		var m_r = r;
		var m_dt = dt;
		var m_interface_type = interface_type;
		var m_moving_type = moving_type;
		Func<double, Vector<double>, Vector<double>> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				double f = Double_BubbleSoundData.JetForcing(m_r, (double)t - 0.1f);

				double d = m_d0;

				if (m_moving_type == 2 && t >= 0.1f)
				{
					// rising bubble, calc depth

					double vt = Double_BubbleSoundData.BubbleTerminalVelocity(m_r);

					d = math.max(0.51f * 2f * m_r, m_d0 - ((double)t - 0.1f) * vt);

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
					double p0 = Double_BubbleSoundData.PATM + 2.0f * Double_BubbleSoundData.SIGMA / m_r;
					double v0 = 4f / 3f * math.PI * math.pow(m_r, 3);

					double w0 = Double_BubbleSoundData.ActualFreq(m_interface_type, m_r, d) * 2 * math.PI;
					double k = Double_BubbleSoundData.GAMMA * p0 / v0;

					double m = k / math.pow(w0, 2);

					double beta = Double_BubbleSoundData.CalcBeta(m_r, w0);

					double acc = f / m - 2 * beta * (double)Y[0] - math.pow(w0, 2) * (double)Y[1];

					if (double.IsNaN(acc))
					{
						Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					return Vector<double>.Build.Dense(new[] { acc, Y[0] });
				}
			};
		}
		data = RungeKutta.FourthOrder(y0, start, end, nSize, DerivativeMaker());
		isReady = true;
	}

	public async Task<Vector<double>[]> Exec(bool ad)
	{
		Debug.Log("Executing");
		var m_y0 = y0;
		var m_nSize = nSize;
		var m_start = start;
		var m_end = end;
		var m_d0 = d0;
		var m_r = r;
		var m_dt = dt;
		var m_interface_type = interface_type;
		var m_moving_type = moving_type;
		Func<double, Vector<double>, Vector<double>> DerivativeMaker()
		{
			return (t, Y) =>
			{
				//[f'; f]
				double f = Double_BubbleSoundData.JetForcing(m_r, (double)t - 0.1);

				double d = m_d0;

				if (m_moving_type == 2 && t >= 0.1f)
				{
					// rising bubble, calc depth

					double vt = Double_BubbleSoundData.BubbleTerminalVelocity(m_r);

					d = math.max(0.51 * 2.0 * m_r, m_d0 - ((double)t - 0.1) * vt);

					// print('vt: ' + str(vt))
					// print('d: ' + str(d))
				}
				//if we let it run too long and the values get very small,
				// the scipy integrator has problems. Might be setting the time step too
				// small? So just exit when the oscillator loses enough energy
				if (t > 0.11 && math.sqrt(math.pow(Y[0], 2) + math.pow(Y[1], 2)) < 1e-15)
				{
					return Vector<double>.Build.Dense(new[] { 0.0, 0.0 });
				}
				else
				{
					double p0 = Double_BubbleSoundData.PATM + 2.0 * Double_BubbleSoundData.SIGMA / m_r;
					double v0 = 4.0 / 3.0 * math.PI * math.pow(m_r, 3);

					double w0 = Double_BubbleSoundData.ActualFreq(m_interface_type, m_r, d) * 2.0 * math.PI;
					double k = Double_BubbleSoundData.GAMMA * p0 / v0;

					double m = k / math.pow(w0, 2.0);

					double beta = Double_BubbleSoundData.CalcBeta(m_r, w0);

					double acc = f / m - 2.0 * beta * (double)Y[0] - math.pow(w0, 2.0) * (double)Y[1];

					if (double.IsNaN(acc))
					{
						Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
					}
					return Vector<double>.Build.Dense(new[] { acc, Y[0] });
				}
			};
		}

	//	var result = await Task.Run(()=> { return RungeKutta.FourthOrder(y0, start, end, nSize, DerivativeMaker()); });
		var result = await Task.Run(()=> { return new Vector<double>[0]; });
	
		return result;
	}

}
public class IntegrateBubble
{
	public Vector<double> y0;
	public int nSize;
	public double start, end;
	double d0, r, dt;
	byte interface_type, moving_type;
	List<Vector<double>> data;
	public IntegrateBubble(byte interface_type, byte moving_type, Vector<double> y0, double r, double d0, double dt, double start = 0, double end = 1, int nSize = 96000)
	{
		this.interface_type = interface_type;
		this.moving_type = moving_type;
		this.y0 = y0;
		this.r = r;
		this.d0 = d0;
		this.dt = dt;
		this.start = start;
		this.end = end;
		this.nSize = nSize;
		data = new List<Vector<double>>();
	}
	public Vector<double>[] Exec()
	{
		var a = DerivativeMaker();
		return RungeKutta.FourthOrder(y0, start, end, nSize, a); ;
	}
	Func<double, Vector<double>, Vector<double>> DerivativeMaker()
	{
		return (t, Y) =>
		{
			//[f'; f]
			double f = Double_BubbleSoundData.JetForcing(r, (double)t - 0.1f);

			double d = d0;

			if (moving_type == 2 && t >= 0.1f)
			{
				// rising bubble, calc depth

				double vt = Double_BubbleSoundData.BubbleTerminalVelocity(r);

				d = math.max(0.51f * 2f * r, d0 - ((double)t - 0.1f) * vt);

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
				double p0 = Double_BubbleSoundData.PATM + 2.0f * Double_BubbleSoundData.SIGMA / r;
				double v0 = 4f / 3f * math.PI * math.pow(r, 3);

				double w0 = Double_BubbleSoundData.ActualFreq(interface_type, r, d) * 2 * math.PI;
				double k = Double_BubbleSoundData.GAMMA * p0 / v0;

				double m = k / math.pow(w0, 2);

				double beta = Double_BubbleSoundData.CalcBeta(r, w0);

				double acc = f / m - 2 * beta * (double)Y[0] - math.pow(w0, 2) * (double)Y[1];

				if (double.IsNaN(acc))
				{
					Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + Y[0] + ", y[1]=" + Y[1]);
				}
				return Vector<double>.Build.Dense(new[] { acc, Y[0] });
			}
		};

		
	}
}
public struct Double_BubbleSoundData : IComponentData
{// physical constants
	public static readonly double CF = 1497f;
	public static readonly double MU = 0.00089f;
	public static readonly double RHO_WATER = 998f;
	public static readonly double GTH = 1600000f;
	public static readonly double GAMMA = 1.4f;
	public static readonly double G = 9.8f;
	public static readonly double SIGMA = 0.072f;
	public static readonly double ETA = 0.84f;
	public static readonly double PATM = 101325;
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

	double radius, depth;

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

	public Double_BubbleSoundData(byte interface_type, byte moving_type, double radius, double depth)
	{
		m_interfacetype = interface_type;
		m_movingtype = moving_type;
		this.radius = radius;
		this.depth = depth;
	}

	public Double_BubbleSoundData Default => new Double_BubbleSoundData
	{
		m_interfacetype = fluid_interface,
		m_movingtype = static_moving,
		radius = 1,
		depth = 1
	};

	public static double BubbleCapacitance(byte interface_type, double radius, double depth)
	{
		if (interface_type == rigid_interface)
			return radius / (1.0f - radius / (2f * depth) - math.pow((radius / (2f * depth)), 4f));
		else // Rigid interface
			return radius / (1.0f + radius / (2f * depth) - math.pow((radius / (2f * depth)), 4f));
	}
	public static double MinnaertFreq(double radius)
	{
		double omega = math.sqrt(3 * GAMMA * PATM - 2 * SIGMA * radius) / (radius * math.sqrt(RHO_WATER));
		return omega / 2 / math.PI;
	}
	public static double ActualFreq(byte interface_type, double radius, double depth)
	{

		double bubbleCapacitance = BubbleCapacitance(interface_type, radius, depth);

		double p0 = PATM;
		//	Debug.Log("r = "+radius+", d = "+depth+", C = " + bubbleCapacitance);

		double v0 = 4.0f / 3.0f * math.PI * math.pow(radius, 3);

		//	Debug.Log("C = " + v0+":: "+( 4.0f * math.PI * GAMMA * p0 * bubbleCapacitance )+ "::"+(RHO_WATER * v0)+"::"+ (4.0f * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0)));
		double omega = math.sqrt(4.0f * math.PI * GAMMA * p0 * bubbleCapacitance / (RHO_WATER * v0));

		//	Debug.Log("D = " + omega);
		return omega / 2 / math.PI;
	}
	public static double CalcBeta(double radius, double w0)
	{

		double dr = w0 * radius / CF;
		double dvis = 4f * MU / (RHO_WATER * w0 * math.pow(radius, 2));

		double phi = 16f * GTH * G / (9f * math.pow((GAMMA - 1), 2) * w0);

		double dth = 2f * (math.sqrt(phi - 3f) - (3f * GAMMA - 1f) /
				 (3f * (GAMMA - 1))) / (phi - 4);


		double dtotal = dr + dvis + dth;


		return w0 * dtotal / math.sqrt(math.pow(dtotal, 2) + 4);
	}
	public static double JetForcing(double r, double t)
	{

		double cutoff = math.min(0.0006f, 0.5f / (3.0f / r));

		if (t < 0 || t > cutoff)
			return 0;
		double jval = (-9f * GAMMA * SIGMA * ETA *
				(PATM + 2f * SIGMA / r) * math.sqrt(1f + math.pow(ETA, 2)) /
				(4f * math.pow(RHO_WATER, 2) * math.pow(r, 5)) * math.pow(t, 2));

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
	public static double BubbleTerminalVelocity(double r)
	{

		double d = 2 * r;

		double del_rho = 997f; // Density difference between the phases

		// eq 2
		double vtpot = 1f / 36f * del_rho * G * math.pow(d, 2) / MU;

		// eq 6
		double vt1 = vtpot * math.sqrt(1f + 0.73667f * math.sqrt(G * d) / vtpot);

		// eq 8
		double vt2 = math.sqrt(3 * SIGMA / RHO_WATER / d + G * d * del_rho / 2 / RHO_WATER);

		// eq 1
		double vt = 1f / math.sqrt(1 / math.pow(vt1, 2) + 1 / math.pow(vt2, 2));


		return vt;
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="moving_type"></param>
	/// <param name="y">a double array of size 2</param>
	/// <param name="t"></param>
	/// <param name="r"></param>
	/// <param name="d0"></param>
	/// <param name="dt"></param>
	/// <param name="of">this is a python file...ignore this</param>
	/// <returns></returns>
	public static double[] BubbleIntegrator(byte interface_type, byte moving_type, double[] y, double t, double r, double d0, double dt/*, File of*/)
	{
		//[f'; f]

		double f = JetForcing(r, t - 0.1f);

		double d = d0;

		if (moving_type == 2 && t >= 0.1f)
		{
			// rising bubble, calc depth

			double vt = BubbleTerminalVelocity(r);

			d = math.max(0.51f * 2f * r, d0 - (t - 0.1f) * vt);

			// print('vt: ' + str(vt))
			// print('d: ' + str(d))
		}
		//if we let it run too long and the values get very small,
		// the scipy integrator has problems. Might be setting the time step too
		// small? So just exit when the oscillator loses enough energy
		if (t > 0.11f && math.sqrt(math.pow(y[0], 2) + math.pow(y[1], 2)) < 1e-15f)
		{
			return new double[] { 0, 0 };
		}

		double p0 = PATM + 2.0f * SIGMA / r;
		double v0 = 4f / 3f * math.PI * math.pow(r, 3);

		double w0 = ActualFreq(interface_type, r, d) * 2 * math.PI;
		double k = GAMMA * p0 / v0;

		double m = k / math.pow(w0, 2);

		double beta = CalcBeta(r, w0);

		double acc = f / m - 2 * beta * y[0] - math.pow(w0, 2) * y[1];

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
		if (double.IsNaN(acc))
		{
			Debug.Log("DETECTED NAN! y = " + y + ", f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + y[0] + ", y[1]=" + y[1]);
		}
		return new double[] { acc, y[0] };
	}

	public static double[] LINSPACE_D(double StartValue, double EndValue, int numberofpoints)
	{

		double[] parameterVals = new double[numberofpoints];
		double increment = Math.Abs(StartValue - EndValue) / Convert.ToDouble(numberofpoints - 1);
		int j = 0; //will keep a track of the numbers 
		double nextValue = StartValue;
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
	public static double[] LINSPACE(double StartValue, double EndValue, int numberofpoints)
	{

		double[] parameterVals = new double[numberofpoints];
		double increment = Math.Abs(StartValue - EndValue) / (double)(numberofpoints - 1);
		int j = 0; //will keep a track of the numbers 
		double nextValue = StartValue;
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


	public static void play_bubble(byte interface_type, byte moving_type, double r, double d, bool save_file,out float[] wav_data)
	{
		// modify values 
		d = d * r / 1000f * 2f;
		r /= 1000f;

		int numsteps = 96000;

		double dt = 1f / (numsteps - 1f);

		// Integrate the bubble sound into a buffer
		var sol =  new IntegrateBubble(interface_type, moving_type, DEFAULT_INITIAL_VALUE, r, d, dt, 0, 1, numsteps).Exec();

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
	public static void play_bubble(Double_BubbleSoundData data,bool save_file,out float[] wav_data)
	{
		play_bubble(data.interfacetype, data.movingtype, data.radius, data.depth, save_file,out wav_data);
	}

	public static double[] ModifyRadiusAndDepth(double radius, double depth)
	{
		depth = depth * radius / 1000.0 * 2.0;
		return new double[] { radius / 1000.0,depth };
	}

	public static void GenerateBubble(Double_BubbleSoundData data,out float[] wave_data,out int channels,out int sampleRate)
	{
		// Set Defaults
		channels = 1;
		sampleRate = 41000;
		// modify values 
		// modified depth
		double m_depth = data.depth * data.radius / 1000.0 * 2.0;
		// modified radius
		double m_radius = data.radius / 1000.0;

		int numsteps = 96000;

		double dt = 1.0 / (numsteps - 1);

		// Integrate the bubble sound into a buffer
		var BubbleIntegrator = new IntegrateBubble(data.interfacetype, data.movingtype, DEFAULT_INITIAL_VALUE, m_radius, m_depth, dt, 0, 1, numsteps);
		var sol = BubbleIntegrator.Exec();

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
	}
	public static void GenerateBubbleSplitTest(Double_BubbleSoundData data, out float[] wave_data, out int channels, out int sampleRate)
	{
		// Set Defaults
		channels = 1;
		sampleRate = 41000;
		// modify values 
		// modified depth
		double m_depth = data.depth * data.radius / 1000.0 * 2.0;
		// modified radius
		double m_radius = data.radius / 1000.0;

		int numsteps = 96000;

		double dt = 1.0 / (numsteps - 1);

		// Integrate the bubble sound into a buffer
		var sol = new IntegrateBubble(data.interfacetype, data.movingtype, DEFAULT_INITIAL_VALUE, m_radius, m_depth, dt, 0, 1, numsteps).Exec();
	
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
	}

}
