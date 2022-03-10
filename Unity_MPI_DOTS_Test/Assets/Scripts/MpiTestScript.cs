
using System;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using MpiLibrary;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace UnityMpi.Tests
{
    public class MpiTestScript : MonoBehaviour,IConvertGameObjectToEntity
    {
        public string dataFilePath = "";
        public string[] args;

        void Start(){}

      /*  public static int worldSize;
        public static int availableProcessors;
        public static int output;
        void Start()
        {
            Debug.LogError("open debug console!");
            if(dataFilePath.Length == 0){
                Debug.LogError("dataFilePath is empty!");
            }
        //    Console.WriteLine("Starting up an MPI program!");
            Debug.Log("Starting up an MPI program!");
            
            // Start up MPI
            // We can wrap the MPI helper library in a using statement so that we can instantiate the MPI connection
            // and have MPI_Finalize automatically called when we are done.
            using (var mpi = new Mpi(args))
            {
                // How big is the cluster, and which node are we?
                worldSize = mpi.GetWorldSize();
                int worldRank = mpi.GetWorldRank();
            //    Console.WriteLine($"Rank: {worldRank}, Size: {worldSize}");
                Debug.Log($"Rank: {worldRank}, Size: {worldSize}");
                
                availableProcessors = SystemInfo.processorCount - worldSize;
                Debug.Log("setting at index: "+worldRank);
                mpi.MPI_Barrier();
                double start = mpi.MPI_Wtime();
                
                // Do a quick allreduce to show that everything works
                int sum = 0;
                mpi.AllReduce(worldRank, ref sum);
            //    Console.WriteLine($"Rank: {worldRank}: AllReduce Rank-Sum: {sum}");
                Debug.Log($"Rank: {worldRank}: AllReduce Rank-Sum: {sum}");
                Debug.Log($"Rank: {worldRank}: total cpus: {SystemInfo.processorCount}, available cpus: {availableProcessors}");
                // Now let's give a little demo showing how we can cooperatively build a model.
           //     Console.WriteLine("Training a linear model....");
           //     TrainLinearModel(dataFilePath, (uint)worldRank, out float[] biasAndWeights);

           //     var averageBiasAndWeights = new float[biasAndWeights.Length];
           //     mpi.AllReduce(biasAndWeights, averageBiasAndWeights);
           //     for (int i = 0; i < averageBiasAndWeights.Length; i++)
           //         averageBiasAndWeights[i] /= (float)worldSize;

            //    Console.WriteLine($"Rank-{worldRank}: bias={biasAndWeights[0]} weight[0]={biasAndWeights[1]} | bias={averageBiasAndWeights[0]} weight[0]={averageBiasAndWeights[1]}");
            //    Console.WriteLine($"Rank: {worldRank}: FINISHED!");
                int size = 100000000*worldRank;
                Debug.Log($"Rank: {worldRank}, size: "+size);
                for(int i = 0; i < size;i++)
                    output++;
                mpi.MPI_Barrier();
                double end = mpi.MPI_Wtime();
                Debug.Log($"Rank: {worldRank}: FINISHED 2 after {(end-start)/1000} seconds");
            }
        }
        void Update(){

        }*/
        public void Convert(Entity e,EntityManager dstManager,GameObjectConversionSystem conversionSystem){
            /*      if(MpiDOTSSystemTest.mpi == null)
                      // MpiDOTSSystemTest.mpi = new Mpi(new string[]{});
                      Debug.LogError("mpi is null!");
                  MPI_Data data = new MPI_Data();
                   Debug.LogError("open debug console!");

                  Debug.Log("Starting up an MPI program!");
                  // How big is the cluster, and which node are we?
                      data.worldSize = MpiDOTSSystemTest.mpi.GetWorldSize();
                      data.rank = MpiDOTSSystemTest.mpi.GetWorldRank();
                  //    Console.WriteLine($"Rank: {worldRank}, Size: {worldSize}");
                      Debug.Log($"Rank: {data.rank}, Size: {data.worldSize}");

                      data.availableProcessors = SystemInfo.processorCount - data.worldSize;
                      Debug.Log("setting at index: "+data.rank);

                      // Do a quick allreduce to show that everything works
                      int sum = 0;
                      MpiDOTSSystemTest.mpi.AllReduce(data.rank, ref sum);
                  //    Console.WriteLine($"Rank: {worldRank}: AllReduce Rank-Sum: {sum}");
                      Debug.Log($"Rank: {data.rank}: AllReduce Rank-Sum: {sum}");
                      Debug.Log($"Rank: {data.rank}: total cpus: {SystemInfo.processorCount}, available cpus: {data.availableProcessors}");
                      dstManager.AddComponentData(e,data);
              */
        }
    }
       
        public class MpiDOTSSystemTest : SystemBase{
            // This is static for no becaus eyou can't call new Mpi multiple times
            // because it will cause an Exception
            public static Mpi mpi = new Mpi(new string[]{});
            bool run = true;

            protected override void OnCreate()
            {

            }
            protected override void OnDestroy()
            {
                if(mpi != null)
                    mpi.Dispose();
            }
            protected override void OnUpdate()
            {
                 MpiDOTSSystemTest.mpi.MPI_Barrier();
                 double startTime = MpiDOTSSystemTest.mpi.MPI_Wtime();

                Dependency = Entities
                .WithName("MPI_Test_Job")
                .WithBurst()
                .ForEach((Entity e,MPI_Data data)=>{
                    if(!data.finished){
                        int size = 100000000*(data.rank+1);
                        int output = 0;
                       for(int i = 0; i < size;i++)
                            output++;
                    }
                }).ScheduleParallel(Dependency);
                Dependency.Complete();
                //Dependency =
                Entities
                .WithName("MPICommunicationSystemtest")
                .WithoutBurst()
                .ForEach((Entity e,ref MPI_Message_Data data)=>{
                    if(mpi.GetWorldRank() == 0){
                        Debug.Log(string.Format("Sending \"{0}\"",data.message));
                        mpi.MPI_Send(data.message,1,1,0);
                        Debug.Log("Successfully sent!");
                    }else{
                        Debug.Log("waiting on receive...");
                        int value = -1;
                    //    Mpi.MPI_Status status = new Mpi.MPI_Status();
                        mpi.MPI_RecvI(ref value,1,0,0,out Mpi.MPI_Status status);
                        Debug.Log($"got \"{value}\" from [{status.MPI_SOURCE}] with tag [{status.MPI_TAG}] with error code [{status.MPI_ERROR}]");
                    }
                }).Run(); 



                mpi.MPI_Barrier();
                Debug.Log(string.Format("Rank: {0} finished job in {1} seconds, total avaialble cpus: {2} with type \"{3}\"",
                mpi.GetWorldRank(),MpiDOTSSystemTest.mpi.MPI_Wtime()-startTime,SystemInfo.processorCount,
                    SystemInfo.processorType));
            }
        }
        public struct MPI_Data : IComponentData{
        public int rank;
        public int worldSize;
        public int availableProcessors;
        public double startTime,endTime;
        public bool finished;
        public double getTime(bool inSeconds){
            return inSeconds ? (endTime-startTime)/1000 : endTime-startTime;
        }
    }
}
