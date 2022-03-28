
using System;
using System.Collections.Generic;
using System.Text;


namespace MpiDotNetApp.MPI_Tests
{
    //
    //  Purpose:
    //
    //    MAIN is the main program for COMMUNICATOR_MPI.
    //
    //  Discussion:
    //
    //    This program demonstrates how an MPI program can start with the
    //    default communicator MPI_COMM_WORLD, and create new communicators
    //    referencing a subset of the total number of processes.
    //
    //  Licensing:
    //
    //    This code is distributed under the GNU LGPL license. 
    //
    //  Modified:
    //
    //    15 June 2016
    //
    //  Author:
    //
    //    John Burkardt
    //
    //  Reference:
    //
    //    William Gropp, Ewing Lusk, Anthony Skjellum,
    //    Using MPI: Portable Parallel Programming with the
    //    Message-Passing Interface,
    //    Second Edition,
    //    MIT Press, 1999,
    //    ISBN: 0262571323,
    //    LC: QA76.642.G76.
    //
    class Communicator_Test
    {
        public const int TIME_SIZE = 40;
        public const int UNINITIALIZED_INT = int.MinValue;
      
        public static void Run()
        {
            using (MpiLibrary.Mpi mpi = new MpiLibrary.Mpi(new string[] { }))
            {
                int even_comm_id = UNINITIALIZED_INT;
                int even_group_id = UNINITIALIZED_INT;
                int even_id = UNINITIALIZED_INT;
                int even_id_sum = UNINITIALIZED_INT;
                int even_p = UNINITIALIZED_INT;
                int[] even_rank;
                int i = UNINITIALIZED_INT;
                int id = UNINITIALIZED_INT;
                int ierr = UNINITIALIZED_INT;
                int j = UNINITIALIZED_INT;
                int odd_comm_id = UNINITIALIZED_INT;
                int odd_group_id = UNINITIALIZED_INT;
                int odd_id = UNINITIALIZED_INT;
                int odd_id_sum = UNINITIALIZED_INT;
                int odd_p = UNINITIALIZED_INT;
                int[] odd_rank;
                int p = UNINITIALIZED_INT;
                int world_group_id = UNINITIALIZED_INT;

                p = mpi.GetWorldSize();
                id = mpi.GetWorldRank();
                //  Process 0 prints an introductory message.
                if (id == 0)
                {
                    timestamp(id);
                    Console.WriteLine($"{id}::COMMUNICATOR_MPI - Master process:\n\t" +
                        $"C++ -> C#/MPI version\n\tAn MPI example program.\n\tThe number of processes is {p}");

                }

                // call barrier to make the output cleaner
                mpi.MPI_Barrier();

                //  Every process prints a hello.
                // NOTE: this line seems to only work sometimes
                Console.WriteLine($"Process {(id+1)} says 'Hello, world!'\n");
                //  Get a group identifier for MPI_COMM_WORLD.
                mpi.MPI_Comm_group(ref world_group_id);
                if (world_group_id == UNINITIALIZED_INT)
                {
                    Console.WriteLine("{id}:: falied to get MPI_Group id!");
                    return;
                }
                //  List the even processes, and create their group.
                even_p = (p + 1) / 2;
                even_rank = new int[even_p];
                j = 0;
                for (i = 0; i < p; i = i + 2)
                {
                    even_rank[j] = i;
                    j = j + 1;
                }
                mpi.MPI_Group_incl(world_group_id, even_p, ref even_rank, ref even_group_id);
                if (even_group_id == UNINITIALIZED_INT)
                {
                    Console.WriteLine($"{id}::ERROR::Failed to create Even Group Id");
                    return;
                }
                mpi.MPI_Comm_create__w(even_group_id, ref even_comm_id);
                if (even_comm_id == UNINITIALIZED_INT)
                {
                    Console.WriteLine($"{id}::ERROR::Failed to create Even Comm Id");
                    return;
                }
                //  List the odd processes, and create their group.
                odd_p = p / 2;
                odd_rank = new int[odd_p];
                j = 0;
                for (i = 1; i < p; i = i + 2)
                {
                    odd_rank[j] = i;
                    j = j + 1;
                }
                mpi.MPI_Group_incl(world_group_id, odd_p, ref odd_rank, ref odd_group_id);
                if (odd_group_id == UNINITIALIZED_INT)
                {
                    Console.WriteLine($"{id}::ERROR::Failed to create Odd Group Id");
                    return;
                }
                mpi.MPI_Comm_create__w(odd_group_id, ref odd_comm_id);
                if (odd_comm_id == UNINITIALIZED_INT)
                {
                    Console.WriteLine($"{id}::ERROR::Failed to create Odd Comm Id");
                    return;
                }
                //
                //  Try to get ID of each process in both groups.  
                //  If a process is not in a communicator, what is its ID?
                //
                if (id % 2 == 0)
                {
                    ierr = mpi.GetWorldRank(even_comm_id, ref even_id);
                    odd_id = UNINITIALIZED_INT;
                }
                else
                {
                    ierr = mpi.GetWorldRank(odd_comm_id, ref odd_id);
                    even_id = UNINITIALIZED_INT;
                }
                //
                //  Use MPI_Reduce to sum the global ID of each process in the even group.
                //  Assuming 4 processes: EVEN_SUM = 0 + 2 = 2;
                //
                if (even_id != UNINITIALIZED_INT)
                {
                    mpi.MPI_Reduce(ref id, ref even_id_sum, 1, MpiLibrary.Mpi.MPI_Op.MPI_SUM, 0, even_comm_id);
                }
                if (even_id == 0)
                {
                    Console.WriteLine($"{id}::Number of processes in even communicator = {even_p}");
                    Console.WriteLine($"{id}::Sum of global ID's in even communicator  = {even_id_sum}");
                }
                //
                //  Use MPI_Reduce to sum the global ID of each process in the odd group.
                //  Assuming 4 processes: ODD_SUM = 1 + 3 = 4;
                //
                if (odd_id != UNINITIALIZED_INT)
                {
                    mpi.MPI_Reduce(ref id, ref odd_id_sum, 1, MpiLibrary.Mpi.MPI_Op.MPI_SUM, 0, odd_comm_id);
                }
                if (odd_id == 0)
                {
                    Console.WriteLine($"{id}::Number of processes in odd communicator = {odd_p}");
                    Console.WriteLine($"{id}::Sum of global ID's in odd communicator  = {odd_id_sum}");

                }
                //
                //  Terminate
                //
                if (id == 0)
                {
                    Console.WriteLine("COMMUNICATOR_MPI:\n\tNormal end of execution.");
                    timestamp(id);
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
        static void timestamp(int worldRank)
        {
            Console.WriteLine($"{worldRank}::timestamp:: {DateTime.Now.ToString()}");
        }
    }
}