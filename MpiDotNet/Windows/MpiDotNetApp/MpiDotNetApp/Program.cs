
using System;
using System.Linq;
using MpiLibrary;

namespace DotNetClient
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Console.WriteLine("Starting up an MPI program!");
            var dataFilePath = args[0];

            // Start up MPI
            // We can wrap the MPI helper library in a using statement so that we can instantiate the MPI connection
            // and have MPI_Finalize automatically called when we are done.
            using (var mpi = new Mpi(args))
            {
                // How big is the cluster, and which node are we?
                int worldSize = mpi.GetWorldSize();
                int worldRank = mpi.GetWorldRank();

                Console.WriteLine($"Rank: {worldRank}, Size: {worldSize}");

                // Do a quick allreduce to show that everything works
                int sum = 0;
                mpi.AllReduce(worldRank, ref sum);
                Console.WriteLine($"AllReduce Rank-Sum: {sum}");

                // Now let's give a little demo showing how we can cooperatively build a model.
                Console.WriteLine("Training a linear model....");
                TrainLinearModel(dataFilePath, (uint)worldRank, out float[] biasAndWeights);

                var averageBiasAndWeights = new float[biasAndWeights.Length];
                mpi.AllReduce(biasAndWeights, averageBiasAndWeights);
                for (int i = 0; i < averageBiasAndWeights.Length; i++)
                    averageBiasAndWeights[i] /= (float)worldSize;

                Console.WriteLine($"Rank-{worldRank}: bias={biasAndWeights[0]} weight[0]={biasAndWeights[1]} | bias={averageBiasAndWeights[0]} weight[0]={averageBiasAndWeights[1]}");
            }*/
          //  Test_Suite();
         //   MPI_COMMUNICATION_TEST();
            MpiManagerTest();
            Console.WriteLine("Finished!");
        }
        static void MpiManagerTest(){
            MpiManager manager = new MpiManager(new string[]{});
            
            if (manager.worldRank == 0){
                Console.WriteLine("MpiManager Info:\n"+manager.ToString());
            
            }
            manager.Dispose();
        }
        static void Test_Suite(){
            using (var mpi = new Mpi(new string[]{}))
            {
                // How big is the cluster, and which node are we?
                int world_size = mpi.GetWorldSize();
                int world_rank = mpi.GetWorldRank();

                Console.WriteLine($"Rank: {world_rank}, Size: {world_size}");

                // Do a quick allreduce to show that everything works
                int sum = 0;
                mpi.AllReduce(world_rank, ref sum);
                Console.WriteLine($"AllReduce Rank-Sum: {sum}");
                int tag = 0;
                 int int_buffer = 0;
                char char_buffer = (char)0; 
                float float_buffer = 0;
                double double_buffer =0;
                short short_buffer=0;
                long long_buffer=0;
                byte uchar_buffer=0;
                sbyte schar_buffer=0;
                ushort ushort_buffer=0;
                ulong ulong_buffer=0;
                int[] int_buffer_array = new int[]{12,2,5,7,654};
                char[] char_buffer_array = new char[]{'a','b','3','T'};
                float[] float_buffer_array = {1.1f,4.6f,3.6f,2.35f};
                double[] double_buffer_array = {23.5,4,5.6,3.7};
                short[] short_buffer_array = {234,2,3,5,66};
                long[] long_buffer_array = {454L,1L,3L,453L,123L,65L,7L};
                byte[] uchar_buffer_array = new byte[]{(byte)'0',(byte)'R',(byte)'E',(byte)'"'};
                sbyte[] schar_buffer_array = new sbyte[]{(sbyte)3,(sbyte)4,(sbyte)3,(sbyte)3,(sbyte)1};
                ushort[] ushort_buffer_array = {84,3,532,76,3};
                ulong[] ulong_buffer_array = {837L,2L,7L,432L,};
                if(world_rank == 0)
                {
                #region Send_Tests
                    int_buffer = 12;
                    char_buffer = 'a';
                    float_buffer = 1.1f;
                    double_buffer = 23.5;
                    short_buffer = 234;
                    long_buffer = 454L;
                    uchar_buffer = (byte)34;
                    schar_buffer = (sbyte)3;
                    ushort_buffer = (ushort)84;
                    ulong_buffer = (ulong) 837L;
                //send int test
                    DO($"MPI process '{world_rank}' sending int value '{int_buffer}' with tag '{tag}'");
                    mpi.MPI_Send(int_buffer, 1, 1, tag);
                    tag++;
                    //Send char test
                    DO($"MPI process '{world_rank}' sending char value '{char_buffer}' with tag '{tag}'");
                    mpi.MPI_Send(char_buffer, 1, 1, tag);
                    tag++;
                    //Send float test
                    DO($"MPI process '{world_rank}' sending float value '{float_buffer}' with tag '{tag}'");
                    mpi.MPI_Send(float_buffer, 1, 1, tag);
                    tag++;
                    //Send double test
                    DO($"MPI process '{world_rank}' sending double value '{double_buffer}' with tag '{tag}'");
                    mpi.MPI_Send(double_buffer, 1, 1, tag);
                    tag++;
                    //Send short test
                    DO($"MPI process '{world_rank}' sending short value '{short_buffer}' with tag '{tag}'");
                    mpi.MPI_Send(short_buffer, 1, 1, tag);
                    tag++;
                    //Send long test
                    DO($"MPI process '{world_rank}' sending long value '{long_buffer}' with tag '{tag}'");
                    mpi.MPI_Send(long_buffer, 1, 1, tag);
                    tag++;
                    //Send unsigned char test
                    DO($"MPI process '{world_rank}' sending uchar value '{uchar_buffer}' with tag '{tag}'");
                    mpi.MPI_Send(uchar_buffer, 1, 1, tag);
                    tag++;
                    //Send signed char test
                    DO($"MPI process '{world_rank}' sending schar value '{schar_buffer}' with tag '{tag}'");
                    mpi.MPI_Send(schar_buffer, 1, 1, tag);
                    tag++;
                    //Send unsigned long test
                    DO($"MPI process '{world_rank}' sending ulong value '{ulong_buffer}' with tag '{tag}'");
                    mpi.MPI_Send(ulong_buffer, 1, 1, tag);
                    tag++;
                    //Send unsigned short test
                    DO($"MPI process '{world_rank}' sending ushort value '{ushort_buffer}' with tag '{tag}'");
                    mpi.MPI_Send(ushort_buffer, 1, 1, tag);
                    tag++;
                #endregion
                #region MPI_Send_Array_Tests
                    int array_size = 0;
                    // Send int array test
                    array_size =int_buffer_array.Length;
                    DO($"MPI process [{world_rank}] sending int array [{String.Join(",",int_buffer_array)}] with tag '{tag}'.\n");
                    mpi.MPI_Send(int_buffer_array,array_size, 1, tag);
                    tag++;
                    //Send char array test
                    array_size =char_buffer_array.Length;
                    DO($"MPI process [{world_rank}] sending char array [{String.Join(",",char_buffer_array)}] with tag '{tag}'.\n");
                    mpi.MPI_Send(char_buffer_array,array_size, 1, tag);
                    tag++;
                    //Send float array test 
                    array_size =float_buffer_array.Length;
                    DO($"MPI process [{world_rank}] sending float array [{String.Join(",",float_buffer_array)}] with tag '{tag}'.\n");
                    mpi.MPI_Send(float_buffer_array,array_size, 1, tag);
                    tag++;
                #endregion

                }
                else
                {
                    Mpi.MPI_Status status;

                    #region MPI_Recv_Tests
                        DO($"waiting on int from 0 with tag {tag}");
                        mpi.MPI_Recv(ref int_buffer, 1, 0, tag,out status);
                        DO($"MPI process '{world_rank}'  got int buffer {int_buffer} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref char_buffer, 1, 0, tag,out status);
                        DO($"MPI process '{world_rank}'  got char buffer {char_buffer} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref float_buffer, 1, 0, tag,out status);
                        DO($"MPI process '{world_rank}'  got float buffer {float_buffer} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref double_buffer, 1, 0, tag,out status);
                        DO($"MPI process '{world_rank}'  got double buffer {double_buffer} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref short_buffer, 1, 0, tag,out status);
                        DO($"MPI process '{world_rank}'  got short buffer {short_buffer} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref long_buffer, 1, 0, tag,out status);
                        DO($"MPI process '{world_rank}'  got long buffer {long_buffer} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref uchar_buffer, 1, 0, tag,out status);
                        DO($"MPI process '{world_rank}'  got uchar buffer {uchar_buffer} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref schar_buffer, 1, 0, tag,out status);
                        DO($"MPI process '{world_rank}'  got schar buffer {schar_buffer} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref ulong_buffer, 1, 0, tag,out status);
                        DO($"MPI process '{world_rank}'  got ulong buffer {ulong_buffer} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref ushort_buffer, 1,0, tag,out status);
                        DO($"MPI process '{world_rank}'  got ushort buffer {ushort_buffer} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                    #endregion
                    #region MPI_Recv_Array_Tests
                        mpi.MPI_Recv(ref int_buffer_array, 5,0, tag,out status);
                        DO($"MPI process '{world_rank}'  got int array buffer {String.Join(",",int_buffer_array)} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref char_buffer_array, 4,0, tag,out status);
                        DO($"MPI process '{world_rank}'  got char array buffer {String.Join(",",char_buffer_array)} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                        mpi.MPI_Recv(ref float_buffer_array,4,0, tag,out status);
                        DO($"MPI process '{world_rank}'  got float array buffer {String.Join(",",float_buffer_array)} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                        tag++;
                    #endregion
                }
            }
     
        }
        //Display Output
        static void DO(object o){Console.WriteLine(o.ToString());}
        static void MPI_COMMUNICATION_TEST(){
             // Start up MPI
            // We can wrap the MPI helper library in a using statement so that we can instantiate the MPI connection
            // and have MPI_Finalize automatically called when we are done.
            using (var mpi = new Mpi(new string[]{}))
            {
                // How big is the cluster, and which node are we?
                int worldSize = mpi.GetWorldSize();
                int worldRank = mpi.GetWorldRank();

                Console.WriteLine($"Rank: {worldRank}, Size: {worldSize}");

                // Do a quick allreduce to show that everything works
                int sum = 0;
                mpi.AllReduce(worldRank, ref sum);
                Console.WriteLine($"AllReduce Rank-Sum: {sum}");
                int tag = 0;
                if(worldRank == 0)
                {
                    // The "master" MPI process issues the MPI_Bsend.
                //    int buffer_sent = 12345;
                    int[] buffer_array = new int[]{11,2,3}; 
                 //   char[] buffer_array = new char[]{'a','g','f'}; 
                    string tmp = "["+buffer_array[0];
                    for(int i = 1; i < buffer_array.Length; i++)
                        tmp+=","+buffer_array[i];
                    tmp+="]";

                    Console.WriteLine($"MPI process {worldRank} sends value {tmp} with {tag} .\n" );
                    mpi.MPI_Send(buffer_array, 3, 1, tag);
                }
                else
                {
                    // The "slave" MPI process receives the message.
                 //   int buffer_sent = -1;
                    int[] buffer_array = new int[3];
                   // char[] buffer_array = new char[3];
                    Console.WriteLine("Attempting to receive...");
                    

                    mpi.MPI_Recv(ref buffer_array,3,0,tag,out Mpi.MPI_Status status);
                    string tmp = "["+buffer_array[0];
                    for(int i = 1; i < buffer_array.Length; i++)
                        tmp+=","+buffer_array[i];
                    tmp+="]";
                    
                    Console.WriteLine($"got buffer {tmp} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");

                /*    await System.Threading.Tasks.Task.Run(()=>{

                        mpi.MPI_Recv(ref buffer_array,3,0,tag,out Mpi.MPI_Status status);
                        string tmp = "["+buffer_array[0];
                        for(int i = 1; i < buffer_array.Length; i++)
                            tmp+=","+buffer_array[i];
                        tmp+="]";
                        
                        Console.WriteLine($"got buffer {tmp} from source {status.MPI_SOURCE} with tag {status.MPI_TAG} with error {status.MPI_ERROR}");
                    });*/
                }
            }
        }

        /// <summary>
        /// Train a linear model and return the bias and weights as an array.
        /// </summary>
        /// <remarks>
        /// Mostly copied from the ML.NET samples, e.g. https://github.com/dotnet/machinelearning/blob/master/docs/samples/Microsoft.ML.Samples/Dynamic/GeneralizedAdditiveModels.cs
        /// </remarks>
        /// <param name="dataFilePath">The path to the housing regression data file.</param>
        /// <param name="seed">Seed for the random number generator</param>
        /// <param name="biasAndWeights">The bias and weights of the model</param>
        /*static void TrainLinearModel(string dataFilePath, uint seed, out float[] biasAndWeights)
        {
            // Create a new context for ML.NET operations. It can be used for exception tracking and logging, 
            // as a catalog of available operations and as the source of randomness.
            var mlContext = new MLContext();

            // Step 1: Read the data as an IDataView.
            // First, we define the reader: specify the data columns and where to find them in the text file.
            var reader = mlContext.Data.CreateTextReader(
                columns: new[]
                    {
                        new TextLoader.Column("MedianHomeValue", DataKind.R4, 0),
                        new TextLoader.Column("CrimesPerCapita", DataKind.R4, 1),
                        new TextLoader.Column("PercentResidental", DataKind.R4, 2),
                        new TextLoader.Column("PercentNonRetail", DataKind.R4, 3),
                        new TextLoader.Column("CharlesRiver", DataKind.R4, 4),
                        new TextLoader.Column("NitricOxides", DataKind.R4, 5),
                        new TextLoader.Column("RoomsPerDwelling", DataKind.R4, 6),
                        new TextLoader.Column("PercentPre40s", DataKind.R4, 7),
                        new TextLoader.Column("EmploymentDistance", DataKind.R4, 8),
                        new TextLoader.Column("HighwayDistance", DataKind.R4, 9),
                        new TextLoader.Column("TaxRate", DataKind.R4, 10),
                        new TextLoader.Column("TeacherRatio", DataKind.R4, 11),
                    },
                hasHeader: true
            );
            // Read the data
            var data = reader.Read(dataFilePath);

            var labelName = "MedianHomeValue";
            var featureNames = data.Schema
                .Select(column => column.Name) // Get the column names
                .Where(name => name != labelName) // Drop the Label
                .ToArray();

            // Sample with TrainTest split with different seeds
            var (trainSet, testSet) = mlContext.Regression.TrainTestSplit(data, testFraction: 0.2, seed: seed);

            // Step 2: Pipeline
            // Concatenate the features to create a Feature vector.
            // Normalize the data set so that for each feature, its maximum value is 1 while its minimum value is 0.
            // Then append a linear regression trainer.
            var pipeline = mlContext.Transforms.Concatenate("Features", featureNames)
                    .Append(mlContext.Regression.Trainers.StochasticDualCoordinateAscent(
                        labelColumn: labelName, featureColumn: "Features"));
            var model = pipeline.Fit(trainSet);

            biasAndWeights = new float[1 + featureNames.Length];

            // Extract the model from the pipeline
            var linearPredictor = model.LastTransformer;
            biasAndWeights[0] = linearPredictor.Model.Bias;
            linearPredictor.Model.Weights.ToArray().CopyTo(biasAndWeights, 1);
        }
    
        */
    }
}
