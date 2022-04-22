using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using System;
using MathNet.Numerics.OdeSolvers;
using MathNet.Numerics.LinearAlgebra;
using Unity.Mathematics;
using Unity.Burst;

public class DOTSMultiThreadTest : MonoBehaviour
{
    public static int N = 1;
    public static int max_for_display = 10;
    public static int iterations = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public struct RK_Tag : IComponentData { }
[UpdateAfter(typeof(RungeKuttaSystem))]
public partial class MathNetRungeKutta : SystemBase
{
    System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
  //  double m3, t3;
    float dt = 0.02f;
    string o = "";
    double[] sol;
    protected override void OnUpdate()
    {
        for (int a = 0; a < DOTSMultiThreadTest.iterations; a++)
        {
         //   s.Start();
            sol = RungeKutta.FourthOrder(0, 0, dt, DOTSMultiThreadTest.N, DerivativeMaker());
        //    s.Stop();

   //         m3 += s.ElapsedMilliseconds;
   //         t3 += s.ElapsedTicks;
        }
    //    m3 /= DOTSMultiThreadTest.iterations;
    //    t3 /= DOTSMultiThreadTest.iterations;

    /*    if(DOTSMultiThreadTest.N <= DOTSMultiThreadTest.max_for_display)
        {
            for (int i = 0; i < DOTSMultiThreadTest.N; i++)
                o += sol[i]*10000 + ",";
        }*/

      //  Debug.Log($"Math.Net: {m3} milliseconds, {t3} ticks, [{o}]");
    ///    Debug.Log($"Math.Net: [{o}]");
    }

    Func<double, double, double> DerivativeMaker()
    {
        return (t, Y) =>
        {
            //  Debug.Log($"{a}: t={t},Y={Y}");
            //  a++;
            return Y * Y + t;
        };
    }
}
[UpdateAfter(typeof(MathNetRungeKutta))]
public partial class RungeKuttaSingleJob : SystemBase
{
    private struct runge_kutta4_single
    {
        private int N;
        private NativeArray<double> x_tmp, k1, k2, k3, k4;
        private double dt;
        private double dt2, dt3, dt6;
        public runge_kutta4_single(int size_n,double start,double end,Allocator allocator)
        {
            N = size_n;
            x_tmp = new NativeArray<double>(size_n, allocator);
            k1 = new NativeArray<double>(size_n, allocator);
            k2 = new NativeArray<double>(size_n, allocator);
            k3 = new NativeArray<double>(size_n, allocator);
            k4 = new NativeArray<double>(size_n, allocator);
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
        void system(NativeArray<double> x,NativeArray<double> k,double t)
        {
            for(int i = 1; i < x.Length; i++)
            {
                k[i] = k[i-1] + system(x[i],t);
                t += dt;
            }
        }
        double systemBubble(Float_BubbleSoundDataCondensedDynamic data, float x,float x1,
            float m_depth,float m_radius,double t)
        {
            //[f'; f]
            float f = Float_BubbleSoundData.JetForcing(m_radius, (float)t - 0.1f);

            float d = m_depth;

            if (data.movingtype == 2 && t >= 0.1f)
            {
                // rising bubble, calc depth

                float vt = Float_BubbleSoundData.BubbleTerminalVelocity(m_radius);

                d = math.max(0.51f * 2f * m_radius, m_depth - ((float)t - 0.1f) * vt);

            }
            //if we let it run too long and the values get very small,
            // the scipy integrator has problems. Might be setting the time step too
            // small? So just exit when the oscillator loses enough energy
            if (t > 0.11f && math.sqrt(math.pow(x, 2) + math.pow(x1, 2)) < 1e-15f)
                return 0;
            
            else
            {
                float p0 = Float_BubbleSoundData.PATM + 2.0f * Float_BubbleSoundData.SIGMA / m_radius;
                float v0 = 4f / 3f * math.PI * math.pow(m_radius, 3);

                float w0 = Float_BubbleSoundData.ActualFreq(data.interfacetype, m_radius, d) * 2 * math.PI;
                float k = Float_BubbleSoundData.GAMMA * p0 / v0;

                float m = k / math.pow(w0, 2);

                float beta = Float_BubbleSoundData.CalcBeta(m_radius, w0);

                float acc = f / m - 2 * beta * (float)x - math.pow(w0, 2) * (float)x1;

                //    if (float.IsNaN(acc))
                //    {
                //        Debug.Log("DETECTED NAN! f: " + f + ", m: " + m + ", w0: " + w0 + ", beta: " + beta + ", t: " + t + ", y[0]=" + x + ", y[1]=" + x1);
                //    }
                //   return Vector<double>.Build.Dense(new[] { acc, Y[0] });
                return acc;
            }
        }
        // assum x = [1...n]
        double system(double x,double t)
        {
            return x*x+t;
        }
        public void do_step2(NativeArray<double> x, double t)
        {
            for (int i = 1; i < x.Length; i++)
            {
                k1[i] = system(x[i-1], t);
                k2[i] = system( x[i-1] + k1[i] * dt2, t + dt2);
                k3[i] = system( x[i-1] + k2[i] * dt2, t + dt2);
                k4[i] = system( x[i-1] + k3[i] * dt,t + dt);
                x[i] = x[i-1] + dt6 * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]);
                t += dt;
            }
        }
        public void do_step(NativeArray<double> x, double t)
        {
            system(x, k1, t);
            for (int i = 0; i < N; ++i)
                x_tmp[i] = x[i] + dt2 * k1[i];
            system(x_tmp, k2, t + dt2);
            for (int i = 0; i < N; ++i)
                x_tmp[i] = x[i] + dt2 * k2[i];
            system(x_tmp, k3, t + dt2);
            for (int i = 0; i < N; ++i)
                x_tmp[i] = x[i] + dt * k3[i];
            system(x_tmp, k4, t + dt);
            for (int i = 0; i < N; ++i)
                x[i] += dt6 * k1[i] + dt3 * k2[i] + dt3 * k3[i] + dt6 * k4[i];
        }

    }
   
    [BurstCompile]
    private struct ExecuteOn1Core : IJob
    {
        public NativeArray<double> input;
        public double deltaTime;
        public runge_kutta4_single rk;
        public void Execute()
        {
            rk.do_step2(input, 0);
        }
    }

    float dt = 0.02f;
    System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
  //  double m3, t3;
    runge_kutta4_single rk_single;
    ExecuteOn1Core rk_single_job;
    NativeArray<double> input;
    string o = "";
    protected override void OnCreate()
    {
        if (!input.IsCreated || input.Length == 0)
        {
            //   Debug.Log("Creating Input...");
            input = new NativeArray<double>(DOTSMultiThreadTest.N, Allocator.Persistent);

        }
        // Initialize our Different Methods
        var rk_single = new runge_kutta4_single(DOTSMultiThreadTest.N, 0, dt, Allocator.Persistent);
     
        rk_single_job = new ExecuteOn1Core
        {
            input = input,
            deltaTime = dt,
            rk = rk_single
        };
    }
    protected override void OnDestroy()
    {
        if (input.IsCreated || input.Length > 0)
            input.Dispose();


        rk_single.Dispose();
    }

    protected override void OnUpdate()
    {
        for (int a = 0; a < DOTSMultiThreadTest.iterations; a++)
        {
            rk_single_job.Schedule().Complete();
        }
    }
}
[UpdateAfter(typeof(RungeKuttaSingleJob))]
public partial class RungeKutaParallel_A : SystemBase
{
    NativeArray<double> input;
    NativeArray<JobHandle> order;
    int max_processors;
    System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();

    int max = (int)math.pow(2, 10) + 1;
    int c_b = 1;
    int b = 0;
    int initializer = 1;
    float dt = 0.02f;
  //  double m3 = 0;
  //  double t3 = 0;
    private struct runge_kutta_p2
    {
        private int N;
        [NativeDisableParallelForRestriction]
        private NativeArray<double> x_tmp, k1, k2, k3, k4, t;
        //   private NativeArray<double> k12, k22, k32, k42;
        private double dt;

        private double dt2, dt3, dt6;
        public runge_kutta_p2(int size_n, double start, double end, Allocator allocator)
        {
            N = size_n;
            x_tmp = new NativeArray<double>(size_n, allocator);
            k1 = new NativeArray<double>(size_n, allocator);
            k2 = new NativeArray<double>(size_n, allocator);
            k3 = new NativeArray<double>(size_n, allocator);
            k4 = new NativeArray<double>(size_n, allocator);
            t = new NativeArray<double>(size_n, allocator);
            /*    k12 = new NativeArray<double>(k1, Allocator.TempJob);
                k22 = new NativeArray<double>(k2, Allocator.TempJob);
                k32 = new NativeArray<double>(k3, Allocator.TempJob);
                k42 = new NativeArray<double>(k4, Allocator.TempJob);*/
            dt = (end - start) / (N - 1);
            for (int i = 1; i < N - 1; i++)
            {
                // do addition instead of 
                // start + dt * i
                // to save computation time
                t[i] = t[i - 1] + dt;
            }
            dt2 = dt / 2;
            dt3 = dt / 3;
            dt6 = dt / 6;
        }
        public void Dispose()
        {
            x_tmp.Dispose();
            k1.Dispose();
            k2.Dispose();
            k3.Dispose();
            k4.Dispose();
            t.Dispose();
        }
        void system(NativeArray<double> x, NativeArray<double> k, double t)
        {
            for (int i = 1; i < N; i++)
            {
                k[i] = k[i - 1] + system(x[i], t);
                t += dt;
            }
        }
        // assum x = [1...n]
        double system(double x, double t)
        {
            return x * x + t;
        }

        /*    public void do_step(NativeArray<double> x,double t,int index)
            {
                double _t = t + (dt * index);
                int ii = index ;
                k1[index] = k1[ii] + system(x[index], _t);  
                x_tmp[index] = x[index] + dt2 * k1[index];
                k2[index] = k2[ii] + system(x_tmp[index], _t + dt2);
                x_tmp[index] = x[index] + dt2 * k2[index];
                k3[index] = k3[ii] + system(x_tmp[index], _t + dt2);
                x_tmp[index] = x[index] + dt * k3[index];
                k4[index] = k4[ii] + system(x_tmp[index], _t + dt);
                x[index] += dt6 * k1[index] + dt3 * k2[index] + dt3 * k3[index] + dt6 * k4[index];
            }
            */
        public void do_step1_p(NativeArray<double> x, int index)
        {
            if(index > 0)
                k1[index] = system(x[index], t[index]);
        }
        public void do_step2_s()
        {
            for (int i = 1; i < N; i++)
                k1[i] = k1[i - 1] + k1[i];
        }
        public void do_step3_p(NativeArray<double> x, int index)
        {
            if (index > 0)
            {
                x_tmp[index] = x[index] + dt2 * k1[index];
                k2[index] = system(x_tmp[index], t[index] + dt2);
            }
        }
        public void do_step4_s()
        {
            for (int i = 1; i < N; i++)
                k2[i] = k2[i - 1] + k2[i];
        }
        public void do_step5_p(NativeArray<double> x, int index)
        {
            if (index > 0)
            {
                x_tmp[index] = x[index] + dt2 * k2[index];
                k3[index] = system(x_tmp[index], t[index] + dt2);
            }
        }
        public void do_step6_s()
        {
            for (int i = 1; i < N; i++)
                k3[i] = k3[i - 1] + k3[i];
        }
        public void do_step7_p(NativeArray<double> x, int index)
        {
            if (index > 0)
            {
                x_tmp[index] = x[index] + dt * k3[index];
                k4[index] = system(x_tmp[index], t[index] + dt);
            }
        }
        public void do_step8_s()
        {
            for (int i = 1; i < N; i++)
                k4[i] = k4[i - 1] + k4[i];
        }
        public void do_step9_p(NativeArray<double> x, int index)
        {
            if (index > 0)
                x[index] += dt6 * k1[index] + dt3 * k2[index] + dt3 * k3[index] + dt6 * k4[index];
        }
    }
    [BurstCompile]
    private struct RKP_Step1 : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<double> input;
        public runge_kutta_p2 rk;
        public void Execute(int index)
        {
            rk.do_step1_p(input, index);
        }
    }
    [BurstCompile]
    private struct RKP_Step2 : IJob
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<double> input;
        public runge_kutta_p2 rk;
        public void Execute()
        {
            rk.do_step2_s();
        }
    }
    [BurstCompile]
    private struct RKP_Step3 : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<double> input;
        public runge_kutta_p2 rk;
        public void Execute(int index)
        {
            rk.do_step3_p(input, index);
        }
    }
    [BurstCompile]
    private struct RKP_Step4 : IJob
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<double> input;
        public runge_kutta_p2 rk;
        public void Execute()
        {
            rk.do_step4_s();
        }
    }
    [BurstCompile]
    private struct RKP_Step5 : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<double> input;
        public runge_kutta_p2 rk;
        public void Execute(int index)
        {
            rk.do_step5_p(input, index);
        }
    }
    [BurstCompile]
    private struct RKP_Step6 : IJob
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<double> input;
        public runge_kutta_p2 rk;
        public void Execute()
        {
            rk.do_step6_s();
        }
    }
    [BurstCompile]
    private struct RKP_Step7 : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<double> input;
        public runge_kutta_p2 rk;
        public void Execute(int index)
        {
            rk.do_step7_p(input, index);
        }
    }
    [BurstCompile]
    private struct RKP_Step8 : IJob
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<double> input;
        public runge_kutta_p2 rk;
        public void Execute()
        {
            rk.do_step8_s();
        }
    }
    [BurstCompile]
    private struct RKP_Step9 : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<double> input;
        public runge_kutta_p2 rk;
        public void Execute(int index)
        {
            rk.do_step9_p(input, index);
        }
    }

    runge_kutta_p2 rk;
    RKP_Step1 s1;
    RKP_Step2 s2;
    RKP_Step3 s3;
    RKP_Step4 s4;
    RKP_Step5 s5;
    RKP_Step6 s6;
    RKP_Step7 s7;
    RKP_Step8 s8;
    RKP_Step9 s9;
    string o = "";
    protected override void OnCreate()
    {
        input = new NativeArray<double>(DOTSMultiThreadTest.N, Allocator.Persistent);
        rk = new runge_kutta_p2(DOTSMultiThreadTest.N, 0, dt, Allocator.Persistent);
        order = new NativeArray<JobHandle>(9, Allocator.Persistent);
        s1 = new RKP_Step1
        {
            input = input,
            rk = rk
        };
        s2 = new RKP_Step2
        {
            input = input,
            rk = rk
        };
        s3 = new RKP_Step3
        {
            input = input,
            rk = rk
        }; 
        s4 = new RKP_Step4
        {
            input = input,
            rk = rk
        };
        s5 = new RKP_Step5
        {
            input = input,
            rk = rk
        }; 
        s6 = new RKP_Step6
        {
            input = input,
            rk = rk
        }; 
        s7 = new RKP_Step7
        {
            input = input,
            rk = rk
        }; 
        s8 = new RKP_Step8
        {
            input = input,
            rk = rk
        }; 
        s9 = new RKP_Step9
        {
            input = input,
            rk = rk
        };
    }

    protected override void OnDestroy()
    {
        input.Dispose();
        rk.Dispose();
    }


    protected override void OnUpdate()
    {
        if (dt != 0 && c_b < max)
        {
            c_b = initializer == 1 ? (int)math.pow(2, 0) : (int)math.pow(2, b);
            initializer = 0;
            b++;
       //     Debug.Log("Running Tests with batch size stuff = " + c_b);
      ///      if (c_b == 1)
       //     {
      //          Debug.Log("dt = " + dt.ToString("F8"));
       //     }

            for (int a = 0; a < DOTSMultiThreadTest.iterations; a++)
            {
         //       s = new System.Diagnostics.Stopwatch();
         //       s.Start();
                order[0] = s1.Schedule(input.Length, c_b, Dependency);
                order[1] = s2.Schedule(order[0]);
                order[2] = s3.Schedule(input.Length, c_b, order[1]);
                order[3] = s4.Schedule(order[2]);
                order[4] = s5.Schedule(input.Length, c_b, order[3]);
                order[5] = s6.Schedule(order[4]);
                order[6] = s7.Schedule(input.Length, c_b, order[5]);
                order[7] = s8.Schedule(order[6]);
                order[8] = s9.Schedule(input.Length, c_b, order[7]);
                JobHandle finished = JobHandle.CombineDependencies(order);
                finished.Complete();
         //       s.Stop();
        //        m3 += s.ElapsedMilliseconds;
        //        t3 += s.ElapsedTicks;
            }

        /*    if (DOTSMultiThreadTest.N <= DOTSMultiThreadTest.max_for_display)
            {
                for (int i = 0; i < DOTSMultiThreadTest.N; i++)
                    o += input[i]*10000 + ",";
            }*/
         //   m3 /= DOTSMultiThreadTest.iterations;
         //      t3 /= DOTSMultiThreadTest.iterations;

            //Debug.Log($"{mm0},{iterations},{mm0/iterations}") ;
        //    Debug.Log($"MultiCore P Jobs: {m3.ToString("F8")} milliseceonds, {t3} ticks,[{o}]");*
        //    Debug.Log($"MultiCore P Jobs: [{o}]");
        }
    }

}
public partial class RungeKuttaSystem : SystemBase
{
    private struct runge_kutta4_p
    {
        private int N;
        private NativeArray<double> x_tmp, k1, k2, k3, k4;
        private double dt,dt2, dt3, dt6;
        public runge_kutta4_p(int size_n, double start, double end,int max_cores,Allocator allocator)
        {
            N = size_n / max_cores;
            //   Debug.Log(size_n+", N = "+N+", max_core = "+max_cores);
            x_tmp = new NativeArray<double>(size_n, allocator);
            k1 = new NativeArray<double>(size_n, allocator);
            k2 = new NativeArray<double>(size_n, allocator);
            k3 = new NativeArray<double>(size_n, allocator);
            k4 = new NativeArray<double>(size_n, allocator);
            dt = (end - start) / (size_n - 1);
            dt2 = dt / 2;
            dt3 = dt / 3; 
            dt6 = dt / 6;
        }
        public void Dispose()
        {
            x_tmp.Dispose();
            k1.Dispose();
            k2.Dispose();
            k3.Dispose();
            k4.Dispose();

        }
        void system(NativeArray<double> x, NativeArray<double> k, double t,
            int start_index,int end_index)
        {
            for (int i = start_index+1; i < end_index; i++)
            {
                k[i] = k[i - 1] + system2(x[i], t);
                t += dt;
            }
        }
        double system2(double x, double t)
        {
            return x * x + t;
        }
        public void do_step(NativeArray<double> x, double t,int start_index,int count)
        {
            int end_index = start_index + count;
      //      Debug.Log("start_index: "+start_index+", "+end_index+", "+N);
            system(x, k1, t,start_index,end_index);
            for (int i = start_index; i < end_index; ++i)
                x_tmp[i] = x[i] + dt2 * k1[i];
            system(x_tmp, k2, t + dt2, start_index, end_index);
            for (int i = start_index; i < end_index; ++i)
                x_tmp[i] = x[i] + dt2 * k2[i];
            system(x_tmp, k3, t + dt2, start_index, end_index);
            for (int i = start_index; i < end_index; ++i)
                x_tmp[i] = x[i] + dt * k3[i];
            system(x_tmp, k4, t + dt, start_index, end_index);
            for (int i = start_index; i < end_index; ++i)
                x[i] += dt6 * k1[i] + dt3 * k2[i] + dt3 * k3[i] + dt6 * k4[i];
        }

    }
  



    [BurstCompile]
    private struct EIP2 : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<double> input;
      //  public runge_kutta_p2 rk;
        public void Execute(int index)
        {
     //       rk.do_step(input,0, index);
        }
    }
    [BurstCompile]
    private struct ExecuteInParallel : IJobParallelForBatch
    {
        public NativeArray<double> input;
        public double deltaTime;
        public runge_kutta4_p rk;
        public void Execute(int index)
        {
        }

        public void Execute(int startIndex, int count)
        {
            rk.do_step(input, deltaTime, startIndex,count);
        }
    }

    NativeArray<double> input;
    int max_processors;

    int max = (int)math.pow(2, 10) + 1;
    int c_b = 1;
    int b = 0;
    int initializer = 1;
    float dt = 0.02f;
    int N = 1000000;// 1000000;
    int iterations = 10;


    // Initialize the time and tick variables
    double m1 = 0;
    double t1 = 0;

    runge_kutta4_p rk_p1;
    ExecuteInParallel rk_p1_job;

    System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
    protected override void OnCreate()
    {
        max_processors = Environment.ProcessorCount;   
        if (!input.IsCreated || input.Length == 0)
        {
            //   Debug.Log("Creating Input...");
            input = new NativeArray<double>(N, Allocator.Persistent);

        }
        // Initialize our Different Methods
        var rk_p1 = new runge_kutta4_p(N, 0, dt, max_processors, Allocator.Persistent);

        rk_p1_job = new ExecuteInParallel
        {
            input = input,
            deltaTime = dt,
            rk = rk_p1
        };
    }
    protected override void OnDestroy()
    {
        if(input.IsCreated || input.Length > 0)
            input.Dispose();

        rk_p1.Dispose();

    }

    private void ResetForNextTest(System.Diagnostics.Stopwatch s,NativeArray<double> input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            input[i] = 0;
        }
        s = new System.Diagnostics.Stopwatch();
    }
    
    protected override void OnUpdate()
    {
        if (dt != 0 && c_b < max)
        {
            c_b = initializer == 1 ? (int)math.pow(2, 0) : (int)math.pow(2,b);
            initializer = 0;
            b++;
         //   Debug.Log("Running Tests with batch size stuff = " + c_b);
            if (c_b == 1)
            {
         //       Debug.Log("dt = " + dt.ToString("F8"));
            }

            for (int a = 0; a < iterations; a++)
            {
       
         /*       //////////////////////////////////////
                // Multicore 1 ///////////////////////
                //////////////////////////////////////
                ResetForNextTest(s,input);
                rk_p1_job.input = input;
                s.Start();
                rk_p1_job.ScheduleBatch(input.Length, c_b).Complete();
                s.Stop();
                if (a == iterations - 1)
                    input.CopyTo(o1);
                //TODO: make runge kutta correct;
                m1 += s.ElapsedMilliseconds;
                t1 += s.ElapsedTicks;

                /////////////////////////////////////
                // Multicore 2 //////////////////////
                /////////////////////////////////////

                ResetForNextTest(s,input);
                rk_p2_job.input = input;
                s.Start();
                rk_p2_job.Schedule(input.Length, c_b).Complete();
                s.Stop();

                m2 += s.ElapsedMilliseconds;
                t2 += s.ElapsedTicks;
                if (a == iterations - 1)
                    input.CopyTo(o2);
                */

          
            }

         //   m1 /= iterations;
         //   t1 /= iterations;

            //Debug.Log($"{mm0},{iterations},{mm0/iterations}") ;
         //   Debug.Log($"MultiCore A: {m1.ToString("F8")} milliseceonds, {t1} ticks");
        }
    }
}
