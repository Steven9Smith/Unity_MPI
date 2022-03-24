
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

        void Start(){
            DOTSMpiManager manager = new DOTSMpiManager(new string[] { });

            if (manager.worldRank == 0)
            {
                Debug.Log("MpiManager Info:\n" + manager.ToString());

            }
            manager.Dispose();
        }


      /*  public static int worldSize;
        public static int availableProcessors;
        public static int output;
        void Start()
        {
            Debug.LogError("open debug console!");
            if(dataFilePath.Length == 0){
                Debug.LogError("dataFilePath is empty!");
            }
        //    Debug.Log("Starting up an MPI program!");
            Debug.Log("Starting up an MPI program!");
            
            // Start up MPI
            // We can wrap the MPI helper library in a using statement so that we can instantiate the MPI connection
            // and have MPI_Finalize automatically called when we are done.
            using (var mpi = new Mpi(args))
            {
                // How big is the cluster, and which node are we?
                worldSize = mpi.GetWorldSize();
                int worldRank = mpi.GetWorldRank();
            //    Debug.Log($"Rank: {worldRank}, Size: {worldSize}");
                Debug.Log($"Rank: {worldRank}, Size: {worldSize}");
                
                availableProcessors = SystemInfo.processorCount - worldSize;
                Debug.Log("setting at index: "+worldRank);
                mpi.MPI_Barrier();
                double start = mpi.MPI_Wtime();
                
                // Do a quick allreduce to show that everything works
                int sum = 0;
                mpi.AllReduce(worldRank, ref sum);
            //    Debug.Log($"Rank: {worldRank}: AllReduce Rank-Sum: {sum}");
                Debug.Log($"Rank: {worldRank}: AllReduce Rank-Sum: {sum}");
                Debug.Log($"Rank: {worldRank}: total cpus: {SystemInfo.processorCount}, available cpus: {availableProcessors}");
                // Now let's give a little demo showing how we can cooperatively build a model.
           //     Debug.Log("Training a linear model....");
           //     TrainLinearModel(dataFilePath, (uint)worldRank, out float[] biasAndWeights);

           //     var averageBiasAndWeights = new float[biasAndWeights.Length];
           //     mpi.AllReduce(biasAndWeights, averageBiasAndWeights);
           //     for (int i = 0; i < averageBiasAndWeights.Length; i++)
           //         averageBiasAndWeights[i] /= (float)worldSize;

            //    Debug.Log($"Rank-{worldRank}: bias={biasAndWeights[0]} weight[0]={biasAndWeights[1]} | bias={averageBiasAndWeights[0]} weight[0]={averageBiasAndWeights[1]}");
            //    Debug.Log($"Rank: {worldRank}: FINISHED!");
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
                  //    Debug.Log($"Rank: {worldRank}, Size: {worldSize}");
                      Debug.Log($"Rank: {data.rank}, Size: {data.worldSize}");

                      data.availableProcessors = SystemInfo.processorCount - data.worldSize;
                      Debug.Log("setting at index: "+data.rank);

                      // Do a quick allreduce to show that everything works
                      int sum = 0;
                      MpiDOTSSystemTest.mpi.AllReduce(data.rank, ref sum);
                  //    Debug.Log($"Rank: {worldRank}: AllReduce Rank-Sum: {sum}");
                      Debug.Log($"Rank: {data.rank}: AllReduce Rank-Sum: {sum}");
                      Debug.Log($"Rank: {data.rank}: total cpus: {SystemInfo.processorCount}, available cpus: {data.availableProcessors}");
                      dstManager.AddComponentData(e,data);
              */
        }
    }

    public class DOTSMpiManager : IDisposable
    {
        public static Mpi mpi = null;
        // MPI Manager is set to use tags 12345 - (12345+worldSize) so please try not to use them for data passing
        public static readonly int MPI_MANAGER_MIN_COMMUNICATION_TAG = 12345;
        public static int MPI_MANAGER_MAX_COMMINICATION_TAG;
        public int worldRank;
        public int worldSize;
        public MpiSystemInformation[] SYSTEM_INFO;
        public DOTSMpiManager(string[] args)
        {
            Debug.Log("Initializing MpiManager");
            mpi = new Mpi(args);
            worldRank = mpi.GetWorldRank();
            worldSize = mpi.GetWorldSize();
            Debug.Log($"got {worldRank} of {(worldSize - 1)}");
            SYSTEM_INFO = new MpiSystemInformation[worldSize];
            SYSTEM_INFO[worldRank] = new MpiSystemInformation();
            SYSTEM_INFO[worldRank].Initialize();
            MPI_MANAGER_MAX_COMMINICATION_TAG = MPI_MANAGER_MIN_COMMUNICATION_TAG + (worldSize * 2);
            // send information data 
            string systemInformation = SYSTEM_INFO[worldRank].ToString().Replace("\t", "");
            char[] arr = systemInformation.ToCharArray();
            // IMPORTANT!!!!!
            // Using Ubuntu a char in C# and C++ is 1 byte
            // But in Windows 10 a char in C# = 2 and C++ = 1
            // KEEP THIS IN MIND WHEN SENDING DATA!
            // I recommend sending data with a length of 
            // [array_length] * sizeof(type)
            // results may vary
            int arr_length = arr.Length * sizeof(char);
            //send size of string buffer
            for (int i = 0; i < worldSize; i++)
            {
                if (i != worldRank)
                {
                    Debug.Log($"Sending buffer length {arr.Length} and buffer to node {i} at tag {MpiManager.MPI_MANAGER_MIN_COMMUNICATION_TAG + i} & {MpiManager.MPI_MANAGER_MIN_COMMUNICATION_TAG + i + worldSize }");
                    mpi.MPI_Send(arr_length, 1, i, MpiManager.MPI_MANAGER_MIN_COMMUNICATION_TAG + i);
                    // send string buffer
                    mpi.MPI_Send(arr, arr_length, i, MpiManager.MPI_MANAGER_MIN_COMMUNICATION_TAG + worldSize + i);
                }
            }
            for (int i = 0; i < worldSize; i++)
            {
                if (i != worldRank)
                {
                    int buffer_size = -1;
                 //   Debug.Log($"Recieving data from node {i} from tag {MpiManager.MPI_MANAGER_MIN_COMMUNICATION_TAG + worldRank}");
                    mpi.MPI_Recv(ref buffer_size, 1, i, MpiManager.MPI_MANAGER_MIN_COMMUNICATION_TAG + worldRank, out Mpi.MPI_Status status);
                    char[] buffer = new char[buffer_size];
                    Debug.Log($"Got a buffer of size {buffer_size} with status \n{status}");
                    mpi.MPI_Recv(ref buffer, buffer_size, i, MpiManager.MPI_MANAGER_MIN_COMMUNICATION_TAG + worldRank + worldSize, out status);
                    Debug.Log($"done receiving data from node {i} from tag {MpiManager.MPI_MANAGER_MIN_COMMUNICATION_TAG + worldRank + worldSize}...Now populating SYSTEN_INFO {i} of {worldSize - 1} with status \n {status}");
                //    Debug.Log($"recevied buffer = {new string(buffer)}");
                    SYSTEM_INFO[i] = new MpiSystemInformation();
                    if (buffer == null) MpiExtensions.DE("buffer is null!");
                    else SYSTEM_INFO[i].Initialize(buffer);
                }
            }
            Debug.Log("Finished setting up MpiManager!");
        }

        ///<summary>Goes through the stored SYSYTEM_INFO and compares the PROCESSOR_COUNT with the minPRocessCount.</summary>
        ///<param name="minProcessorCount">minimum desired amount of Processor Cores within a system</param>
        ///<param name="returnFirstFoundOnly">set to true so that the first system that satifies the criteria is returned immediately at index 0</param>
        ///<returns>an int[] of worldRanks that meet the criteria</returns>
        public int[] FindSystemsWithMinOfXProcessors(int minProcessorCount, bool returnFirstFoundOnly = false)
        {
            int[] tmp = new int[worldSize];
            int index = 0;
            int[] indicies = new int[] { -1 };
            for (int i = 0; i < worldSize; i++)
            {
                if (SYSTEM_INFO[i].PROCESSOR_COUNT >= minProcessorCount)
                {
                    if (returnFirstFoundOnly)
                    {
                        return new int[] { i };
                    }
                    else
                    {
                        tmp[index] = i;
                        index++;
                    }
                }
            }
            if (index > 0)
            {
                indicies = new int[index + 1];
                for (int i = 0; i < indicies.Length; i++)
                    indicies[i] = tmp[i];
            }
            return indicies;
        }

        public void Dispose()
        {
            mpi.Dispose();
        }

        public override string ToString()
        {
            if (SYSTEM_INFO == null) Debug.LogError("SYSTEM_INFO is null!");
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder(string.Empty);
                for (int i = 0; i < SYSTEM_INFO.Length; i++)
                {
                    sb.Append($"System Info {i} of {worldSize - 1}:\n" + SYSTEM_INFO[i].ToString() + "\n");
                }
                return sb.ToString();
            }
            return "";
        }
        public struct MpiSystemInformation
        {
            public int worldRank;
            public OperatingSystem OS;
            public string PROCESSOR_ARCHITECTURE;
            public string PROCESSOR_IDENTIFIER;
            public string PROCESSOR_LEVEL;
            public string SYSTEM_DIRECTORY;
            public int PROCESSOR_COUNT;
            public string DOMAIN_NAME;
            public string USER_NAME;

            public System.IO.DriveInfo[] LOGIACAL_DRIVES;

            public void Initialize(char[] arr)
            {
                try
                {
                    if (arr == null)
                    {
                        MpiExtensions.DE("given arr is null");
                        return;
                    }
                    string a = new string(arr);
                    string[] s = a.Split(new string[] { "\n" },StringSplitOptions.RemoveEmptyEntries);
                    if (s.Length < 8)
                    {
                        string b = "";
                        foreach (string ss in s)
                            b += ss+"|\n";
                        MpiExtensions.DO($"MpiSystemInformation: Given data string has less data than expeceted! size: {s.Length}\ngot: {b}");
                    }
                    else
                    {
                        MpiExtensions.DO($"MpiSystemInformation: Initializing with string of size {s.Length}");
                        // Get OS Information
                        string[] ss = s[0].Split(new string[] { "." }, StringSplitOptions.None);
                        string[] sa = ss[0].Split(new string[] { " " }, StringSplitOptions.None);
                        int major = 0;
                        int minor = 0;
                        int build = 0;
                        int revision = 0;
                        if (ss.Length > 0)
                            int.TryParse(sa[sa.Length - 1], out major);
                        if (ss.Length > 1)
                            int.TryParse(ss[1], out minor);
                        if (ss.Length > 2)
                            int.TryParse(ss[2], out build);
                        if (ss.Length > 3)
                            int.TryParse(ss[3], out revision);
                        //  MpiExtensions.DO($"{major}, {minor}, {build}, {revision}");
                        OS = new OperatingSystem(s[0].Contains("Unix") ? PlatformID.Unix : PlatformID.Win32NT, new Version(major, minor, build, revision));
                        PROCESSOR_ARCHITECTURE = s[1];
                        PROCESSOR_IDENTIFIER = s[2];
                        PROCESSOR_LEVEL = s[3];
                        SYSTEM_DIRECTORY = s[4];
                        ss = s[5].Split(new string[] { " " }, StringSplitOptions.None); ;
                        PROCESSOR_COUNT = int.Parse(ss[ss.Length - 1]);
                        DOMAIN_NAME = s[6];
                        USER_NAME = s[7];
                        //     LOGIACAL_DRIVES = new System.IO.DriveInfo[s.Length-8];
                        //     for(int i = 0; i < LOGIACAL_DRIVES.Length; i++)
                        //         LOGIACAL_DRIVES[i] = new System.IO.DriveInfo(s[7+i][0]+"");
                        MpiExtensions.DO("finished updating other system info!");
                    }
                }
                catch (Exception e) { MpiExtensions.DE(e.Message); }
            }

            public void Initialize()
            {
                OS = Environment.OSVersion;
                PROCESSOR_ARCHITECTURE = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                PROCESSOR_IDENTIFIER = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
                PROCESSOR_LEVEL = Environment.GetEnvironmentVariable("PROCESSOR_LEVEL");
                SYSTEM_DIRECTORY = Environment.SystemDirectory;
                PROCESSOR_COUNT = Environment.ProcessorCount;
                DOMAIN_NAME = Environment.UserDomainName;
                USER_NAME = Environment.UserName;

                LOGIACAL_DRIVES = System.IO.DriveInfo.GetDrives();
            }

            public override string ToString()
            {
                // this method is a modified verions of the method found at this site:
                //https://morgantechspace.com/2015/08/get-system-information-in-c-sharp.html
                System.Text.StringBuilder systemInfo = new System.Text.StringBuilder(string.Empty);

                systemInfo.AppendFormat("Operation System:  {0}\n", OS == null ? "NULL" : OS.ToString());
                systemInfo.AppendFormat("Processor Architecture:  {0}\n", PROCESSOR_ARCHITECTURE);
                systemInfo.AppendFormat("Processor Model:  {0}\n", PROCESSOR_IDENTIFIER);
                systemInfo.AppendFormat("Processor Level:  {0}\n", PROCESSOR_LEVEL);
                systemInfo.AppendFormat("SystemDirectory:  {0}\n", SYSTEM_DIRECTORY);
                systemInfo.AppendFormat("ProcessorCount:  {0}\n", PROCESSOR_COUNT);
                systemInfo.AppendFormat("UserDomainName:  {0}\n", DOMAIN_NAME);
                systemInfo.AppendFormat("UserName: {0}\n", USER_NAME);
                /*if(LOGIACAL_DRIVES != null){
                    //Drives
                    foreach(System.IO.DriveInfo DriveInfo1 in LOGIACAL_DRIVES){
                        try{
                            if(DriveInfo1 == null){
                                systemInfo.AppendFormat("NULL {0}",0);
                            }
                            else{
                                // i really don't care about this atm
                                
                                systemInfo.AppendFormat("\tDrive: {0}\n\t\t VolumeLabel: " +
                                        "{1}\n\t\t DriveType: {2}\n\t\t DriveFormat: {3}\n\t\t " +
                                        "TotalSize: {4}\n\t\t AvailableFreeSpace: {5}\n",
                                        DriveInfo1.Name, DriveInfo1.VolumeLabel, DriveInfo1.DriveType,
                                            DriveInfo1.DriveFormat, DriveInfo1.TotalSize, DriveInfo1.AvailableFreeSpace);
                            }
                        }catch(Exception e){systemInfo.AppendFormat("{0}",e.Message);}
                    }
                }*/
                systemInfo.AppendFormat("Version:  {0}", Environment.Version);

                return systemInfo.ToString();
            }
            public string GetThisSystemInformation()
            {
                // this method is a modified verions of the method found at this site:
                //https://morgantechspace.com/2015/08/get-system-information-in-c-sharp.html
                System.Text.StringBuilder systemInfo = new System.Text.StringBuilder(string.Empty);

                systemInfo.AppendFormat("Operation System:  {0}\n", Environment.OSVersion);
                systemInfo.AppendFormat("Processor Architecture:  {0}\n", Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine));
                systemInfo.AppendFormat("Processor Model:  {0}\n", Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER", EnvironmentVariableTarget.Machine));
                systemInfo.AppendFormat("Processor Level:  {0}\n", Environment.GetEnvironmentVariable("PROCESSOR_LEVEL", EnvironmentVariableTarget.Machine));
                systemInfo.AppendFormat("SystemDirectory:  {0}\n", Environment.SystemDirectory);
                systemInfo.AppendFormat("ProcessorCount:  {0}\n", Environment.ProcessorCount);
                systemInfo.AppendFormat("UserDomainName:  {0}\n", Environment.UserDomainName);
                systemInfo.AppendFormat("UserName: {0}\n", Environment.UserName);
                if (LOGIACAL_DRIVES != null)
                {
                    //Drives
                    foreach (System.IO.DriveInfo DriveInfo1 in System.IO.DriveInfo.GetDrives())
                    {
                        try
                        {
                            systemInfo.AppendFormat("t Drive: {0}\n\t\t VolumeLabel: " +
                                    "{1}\n\t\t DriveType: {2}\n\t\t DriveFormat: {3}\n\t\t " +
                                    "TotalSize: {4}\n\t\t AvailableFreeSpace: {5}\n",
                                    DriveInfo1.Name, DriveInfo1.VolumeLabel, DriveInfo1.DriveType,
                                        DriveInfo1.DriveFormat, DriveInfo1.TotalSize, DriveInfo1.AvailableFreeSpace);
                        }
                        catch (Exception e) { }
                    }
                    systemInfo.AppendFormat("Version:  {0}", Environment.Version);
                }
                Debug.Log(systemInfo);
                return systemInfo.ToString();
            }
        }
    }


    /*
    public class MpiDOTSSystemTest : SystemBase
    {
        // This is static for no becaus eyou can't call new Mpi multiple times
        // because it will cause an Exception
        public static Mpi mpi = new Mpi(new string[] { });

        protected override void OnCreate()
        {

        }
        protected override void OnDestroy()
        {
            if (mpi != null)
                mpi.Dispose();
        }
        protected override void OnUpdate()
        {
            mpi.MPI_Barrier();
            double startTime = mpi.MPI_Wtime();

            Dependency = Entities
            .WithName("MPI_Test_Job")
            .WithBurst()
            .ForEach((Entity e, MPI_Data data) =>
            {
                if (!data.finished)
                {
                    int size = 100000000 * (data.rank + 1);
                    int output = 0;
                    for (int i = 0; i < size; i++)
                        output++;
                }
            }).ScheduleParallel(Dependency);
            Dependency.Complete();
            //Dependency =
            Entities
            .WithName("MPICommunicationSystemtest")
            .WithoutBurst()
            .ForEach((Entity e, ref MPI_Message_Data data) =>
            {
                if (mpi.GetWorldRank() == 0)
                {
                    Debug.Log(string.Format("Sending \"{0}\"", data.message));
                    mpi.MPI_Send(data.message, 1, 1, 0);
                    Debug.Log("Successfully sent!");
                }
                else
                {
                    Debug.Log("waiting on receive...");
                    int value = -1;
                        //    Mpi.MPI_Status status = new Mpi.MPI_Status();
                        mpi.MPI_Recv(ref value, 1, 0, 0, out Mpi.MPI_Status status);
                    Debug.Log($"got \"{value}\" from [{status.MPI_SOURCE}] with tag [{status.MPI_TAG}] with error code [{status.MPI_ERROR}]");
                }
            }).Run();



            mpi.MPI_Barrier();
            Debug.Log(string.Format("Rank: {0} finished job in {1} seconds, total avaialble cpus: {2} with type \"{3}\"",
            mpi.GetWorldRank(), mpi.MPI_Wtime() - startTime, SystemInfo.processorCount,
                SystemInfo.processorType));
        }
    }
    public struct MPI_Data : IComponentData
    {
        public int rank;
        public int worldSize;
        public int availableProcessors;
        public double startTime, endTime;
        public bool finished;
        public double getTime(bool inSeconds)
        {
            return inSeconds ? (endTime - startTime) / 1000 : endTime - startTime;
        }
    }*/
}
