using System;
using System.Collections.Generic;
using System.Text;

namespace MpiDotNetApp.MPI_Tests
{
    class Heat_MPI
    {
        double cfl;
        double[] h;
        double[] h_new;
        int i;
        int j;
        int j_min = 0;
        int j_max = 400;
        double k = 0.002;
        int n = 11;
        MpiLibrary.Mpi.MPI_Status status;
        int tag;
        double time;
        double time_delta;
        double time_max = 10.0;
        double time_min = 0.0;
        double time_new;
        double[] x;
        double x_delta;
        double x_max = 1.0;
        double x_min = 0.0;
        public const int UNINITIALIZED_INT = int.MinValue;
        string h_file = "";
        /// <summary>
        /// ///  Purpose:
        ///    MAIN is the main program for HEAT_MPI.
        ///  Licensing:
        ///    This code is distributed under the GNU LGPL license. 
        ///  Modified:
        ///    15 June 2016
        ///  Author:
        ///    John Burkardt
        ///  Reference:
        ///
        ///    William Gropp, Ewing Lusk, Anthony Skjellum,
        ///    Using MPI: Portable Parallel Programming with the
        ///    Message-Passing Interface,
        ///    Second Edition,
        ///    MIT Press, 1999,
        ///    ISBN: 0262571323,
        ///    LC: QA76.642.G76.
        ///
        ///    Marc Snir, Steve Otto, Steven Huss-Lederman, David Walker, 
        ///    Jack Dongarra,
        ///    MPI: The Complete Reference,
        ///    Volume I: The MPI Core,
        ///    Second Edition,
        ///    MIT Press, 1998,
        ///    ISBN: 0-262-69216-3,
        ///     LC: QA76.642.M65.
        /// </summary>
        public void Run()
        {
            using (MpiLibrary.Mpi mpi = new MpiLibrary.Mpi(new string[] { }))
            {
                {
                    int id = mpi.GetWorldRank();
                    int p = mpi.GetWorldSize() ;
                    double wtime = UNINITIALIZED_INT;

                    if (id == 0)
                    {
                        timestamp(id);
                        Console.WriteLine("\n");
                        Console.WriteLine("HEAT_MPI:\n");
                        Console.WriteLine("  C++/MPI version\n");
                        Console.WriteLine("  Solve the 1D time-dependent heat equation.\n");
                    }
                    //
                    //  Record the starting time.
                    //
                    if (id == 0)
                    {
                        wtime = mpi.MPI_Wtime();
                    }

                    update(mpi,id, p);
                    //
                    //  Record the final time.
                    //
                    if (id == 0)
                    {
                        wtime = mpi.MPI_Wtime() - wtime;

                        Console.WriteLine("\n");
                        Console.WriteLine($"\tWall clock elapsed seconds = {wtime}\n");
                    }
                    //
                    //  Terminate.
                    //
                    if (id == 0)
                    {
                        Console.WriteLine("\n");
                        Console.WriteLine("HEAT_MPI:\n");
                        Console.WriteLine("\tNormal end of execution.\n");
                        Console.WriteLine("\n");
                        timestamp(id);
                    }
                }
            }
        }
        /// <summary>
        /// TIMESTAMP prints the current YMDHMS date as a time stamp.
        /// Example: 31 May 2001 09:45:54 AM
        ///  Licensing: This code is distributed under the GNU LGPL license.
        ///  Modified: 08 July 2009 
        ///  Author: John Burkardt
        /// </summary>
        /// <param name="worldRank">worldRank that called this</param>

        void timestamp(int worldRank)
        {
            Console.WriteLine($"{worldRank}::timestamp:: {DateTime.Now.ToString()}");
        }

        /// <summary>
        /// Purpose:  UPDATE computes the solution of the heat equation.
        ///  Discussion:
        ///    If there is only one processor ( P == 1 ), then the program writes the
        ///    values of X and H to files.
        ///  Licensing: This code is distributed under the GNU LGPL license. 
        /// Modified:  14 June 2016
        ///  Author: John Burkardt
        /// </summary>
        /// <param name="id"> the id of this processor.</param>
        /// <param name="p">the number of processors</param>
        void update(MpiLibrary.Mpi mpi,int id, int p){
           
            //
            //  Have process 0 print out some information.
            //
            if (id == 0)
            {   
                Console.WriteLine($"\tCompute an approximate solution to the time dependent\n");
                Console.WriteLine($"\tone dimensional heat equation:\n");
                Console.WriteLine($"\t\tdH/dt - K * d2H/dx2 = f(x,t)\n");
                Console.WriteLine($"\tfor {x_min} = x_min < x < x_max = {x_max}\n");
                Console.WriteLine($"\tand {time_min} = time_min < t <= t_max = {time_max}\n");
                Console.WriteLine($"\tBoundary conditions are specified at x_min and x_max.\n");
                Console.WriteLine($"\tInitial conditions are specified at time_min.\n");
                Console.WriteLine($"\tThe finite difference method is used to discretize the\n");
                Console.WriteLine($"\tdifferential equation.\n");
                Console.WriteLine($"\tThis uses {p * n} equally spaced points in X\n");
                Console.WriteLine($"\tand {j_max} equally spaced points in time.\n");
                Console.WriteLine($"\tParallel execution is done using {p} processors.\n");
                Console.WriteLine($"\tDomain decomposition is used.\n");
                Console.WriteLine($"\tEach processor works on {n} nodes, \n");
                Console.WriteLine($"\tand shares some information with its immediate neighbors.\n");
            }
            //
            //  Set the X coordinates of the N nodes.
            //  We don't actually need ghost values of X but we'll throw them in
            //  as X[0] and X[N+1].
            //
            x = new double[n + 2];

            for (i = 0; i <= n + 1; i++)
            {
                x[i] = ((double)(id * n + i - 1) * x_max
                       + (double)(p * n - id * n - i) * x_min)
                       / (double)(p * n - 1);
            }
            //
            //  In single processor mode, write out the X coordinates for display.
            //
            string text = "";
            if (p == 1)
            {
                text = System.IO.File.ReadAllText("x_data.txt");
            }
            //
            //  Set the values of H at the initial time.
            //
            time = time_min;
            h = new double[n + 2];
            h_new = new double[n + 2];
            h[0] = 0.0;
            for (i = 1; i <= n; i++)
            {
                h[i] = initial_condition(x[i], time);
            }
            h[n + 1] = 0.0;
            // 10 - 0 / 400 - 1 = 10/400 = 0.025
            time_delta = (time_max - time_min) / (double)(j_max - j_min);
            // 1 - 0 / p * 11 - 1 = 1/((11*p)-1)
            // if p = 20 then -> 1/219 ~ 0.00456
            // if p = 8 then -> 1/87 ~ 0.01149
            x_delta = (x_max - x_min) / (double)(p * n - 1);
            //
            //  Check the CFL condition, have processor 0 print out its value,
            //  and quit if it is too large.
            //

            // if p == 20
            // 0.002 * 0.025 / 0.00456 / 0.00456 = 2.404586
            // if p == 8
            // 0.002 * 0.025 / 0.01149 / 0.01149 = 0.037873
            cfl = k * time_delta / x_delta / x_delta;

            if (id == 0)
            {
                Console.WriteLine("\n");
                Console.WriteLine("UPDATE\n");
                Console.WriteLine($"\tCFL stability criterion value = {cfl}\n"); ;
            }

            if (0.5 <= cfl)
            {
                if (id == 0)
                {
                    Console.WriteLine("\n");
                    Console.WriteLine("UPDATE - Warning!\n");
                    Console.WriteLine("\tComputation cancelled!\n");
                    Console.WriteLine("\tCFL condition failed.\n");
                    Console.WriteLine($"\t0.5 <= K * dT / dX / dX = {cfl}\n");
                }
                return;
            }
            //
            //  In single processor mode, write out the values of H.
            //
            if (p == 1)
            {
                h_file = System.IO.File.ReadAllText("h_data.txt");
            }
            //
            //  Compute the values of H at the next time, based on current data.
            //
            for (j = 1; j <= j_max; j++)
            {

                time_new = ((double)(j - j_min) * time_max
                           + (double)(j_max - j) * time_min)
                           / (double)(j_max - j_min);
                //
                //  Send H[1] to ID-1.
                //
                if (0 < id)
                {
                    tag = 1;
                    mpi.MPI_Send(h[1], 1, id - 1, tag);
                }
                //
                //  Receive H[N+1] from ID+1.
                //
                if (id < p - 1)
                {
                    tag = 1;
                    mpi.MPI_Recv(ref h[n + 1], 1, id + 1, tag, out status);
                }
                //
                //  Send H[N] to ID+1.
                //
                if (id < p - 1)
                {
                    tag = 2;
                    mpi.MPI_Send(h[n], 1, id + 1, tag);
                }
                //
                //  Receive H[0] from ID-1.
                //
                if (0 < id)
                {
                    tag = 2;
                    mpi.MPI_Recv(ref h[0], 1, id - 1, tag, out status);
                }
                //
                //  Update the temperature based on the four point stencil.
                //
                for (i = 1; i <= n; i++)
                {
                    h_new[i] = h[i]
                    + (time_delta * k / x_delta / x_delta) * (h[i - 1] - 2.0 * h[i] + h[i + 1])
                    + time_delta * rhs(x[i], time);
                }
                //
                //  H at the extreme left and right boundaries was incorrectly computed
                //  using the differential equation.  Replace that calculation by
                //  the boundary conditions.
                //
                if (0 == id)
                {
                    h_new[1] = boundary_condition(x[1], time_new);
                }
                if (id == p - 1)
                {
                    h_new[n] = boundary_condition(x[n], time_new);
                }
                //
                //  Update time and temperature.
                //
                time = time_new;

                for (i = 1; i <= n; i++)
                {
                    h[i] = h_new[i];
                }
                //
                //  In single processor mode, add current solution data to output file.
                //
                if (p == 1)
                {
                    for (i = 1; i <= n; i++)
                    {
                        h_file+= " "+h[i];
                    }
                    h_file+= "\n";
                }
            }

           
        }

        /// <summary>
        ///  Purpose:
        ///    BOUNDARY_CONDITION evaluates the boundary condition of the differential equation.
        ///  Licensing:
        ///    This code is distributed under the GNU LGPL license. 
        ///  Modified:
        ///    23 April 2008
        ///  Author:
        ///    John Burkardt
        /// </summary>
        /// <param name="x">position</param>
        /// <param name="time">time</param>
        /// <returns> the value of the boundary condition.</returns>
        double boundary_condition(double x, double time){
            double value;
            //
            //  Left condition:
            //
            if (x < 0.5)
            {
                value = 100.0 + 10.0 * Math.Sin(time);
            }
            else
            {
                value = 75.0;
            }
            return value;
        }
        /// <summary>
        ///  Purpose:
        ///    INITIAL_CONDITION evaluates the initial condition of the differential equation.
        ///  Licensing:
        ///    This code is distributed under the GNU LGPL license. 
        ///  Modified:
        ///    23 April 2008
        ///  Author:
        ///    John Burkardt
        /// </summary>
        /// <param name="x">the position</param>
        /// <param name="time">time</param>
        /// <returns>the value of the initial condition</returns>
        double initial_condition(double x, double time){
            double value;

            value = 95.0;

            return value;
        }
        /// <summary>
        ///  Purpose:
        ///    RHS evaluates the right hand side of the differential equation.
        ///  Licensing:
        ///    This code is distributed under the GNU LGPL license. 
        ///  Modified:
        ///    23 April 2008
        ///  Author:
        ///    John Burkardt
        /// </summary>
        /// <param name="x">the position</param>
        /// <param name="time">time</param>
        /// <returns>the value of the right hand side function.</returns>
        double rhs(double x, double time)
        {
            double value;

            value = 0.0;

            return value;
        }
    }
}
    

