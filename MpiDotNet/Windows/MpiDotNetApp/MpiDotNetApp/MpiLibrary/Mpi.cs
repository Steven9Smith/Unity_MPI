using System;
using System.Linq;
using System.Runtime.InteropServices;


namespace MpiLibrary
{
    // Inherit from an abstract class
    public sealed class Mpi : IDisposable
    {
        public struct MPI_Status {
            public int count_lo;
            public int count_hi_and_cancelled;
            public int MPI_SOURCE;
            public int MPI_TAG;
            public int MPI_ERROR;
        }
        public enum MPI_Datatype : uint{
            //Null datatype
            MPI_DATATYPE_NULL          = 0x0c000000,
            //Char
            MPI_CHAR                   = 0x4c000101,
            //uchar
            MPI_UNSIGNED_CHAR          = 0x4c000102,
            //short
            MPI_SHORT                  = 0x4c000203,
            //ushort
            MPI_UNSIGNED_SHORT         = 0x4c000204,
            // int
            MPI_INT                    = 0x4c000405,
            //uint
            MPI_UNSIGNED               = 0x4c000406,
            //long
            MPI_LONG                   = 0x4c000407,
            //ulong
            MPI_UNSIGNED_LONG          = 0x4c000408,
            //long long (some systems may not implement)
            //The following are datatypes for the MPI functions MPI_MAXLOC and MPI_MINLOC . 
            MPI_LONG_LONG_INT          = 0x4c000809,
            // same as MPI_LONG_LONG_INT
            MPI_LONG_LONG              = MPI_LONG_LONG_INT,
            //float
            MPI_FLOAT                  = 0x4c00040a,
            //double
            MPI_DOUBLE                 = 0x4c00080b,
            //long double (some systems may not implement)
            MPI_LONG_DOUBLE            = 0x4c00080c,
            // See standard; like unsigned char 
            MPI_BYTE                   = 0x4c00010d,
            //  is an MPI_Datatype that represents a wide character type in MPI, it corresponds to a wchar_t in C
            MPI_WCHAR                  = 0x4c00020e,
            //For MPI_Pack and MPI_Unpack lookup for more information
            MPI_PACKED                 = 0x4c00010f,
            //For MPI_Type_struct ; a lower-bound indicator
            MPI_LB                     = 0x4c000010,
            //For MPI_Type_struct ; a upper-bound indicator
            MPI_UB                     = 0x4c000011,
            //is an MPI_Datatype that represents a complex type in MPI, it corresponds to a float _Complex in C
            MPI_C_COMPLEX              = 0x4c000812,
            //An MPI_Datatype that represents a floating complex type in MPI, it corresponds to a float _Complex in C.
            MPI_C_FLOAT_COMPLEX        = 0x4c000813,
            //An MPI_Datatype that represents a double precision floating complex type in MPI, it corresponds to a float _Complex in C
            MPI_C_DOUBLE_COMPLEX       = 0x4c001614,
            //An MPI_Datatype that represents a long double precision floating complex type in MPI, it corresponds to a long double _Complex in C
            MPI_C_LONG_DOUBLE_COMPLEX  = 0x4c001615,
            // struct { int, int } 
            MPI_2INT                   = 0x4c000816,
            //is an MPI_Datatype that represents a boolean type in MPI, it corresponds to a bool in C. For the FORTRAN counterpart, please see MPI_LOGICAL.
            MPI_C_BOOL                 = 0x4c000117,
            //MPI_SIGNED_CHAR is an MPI_Datatype that represents a signed character type in MPI, it corresponds to a signed char in C. The difference
            // with MPI_CHAR is that the latter is treated as the printable character, while MPI_SIGNED_CHAR is treated as the integral value. 
            //This is why MPI_CHAR cannot be used in reduction operations for instance while MPI_SIGNED_CHAR can.
            MPI_SIGNED_CHAR            = 0x4c000118,
            //MPI_UNSIGNED_LONG_LONG is an MPI_Datatype that represents an unsigned long long integer type in MPI,
            // it corresponds to an unsigned long long int in C.
            MPI_UNSIGNED_LONG_LONG     = 0x4c000819,
            //MPI_CHARACTER is an MPI_Datatype that represents a character type in MPI, it corresponds to a CHARACTER in FORTRAN.
            MPI_CHARACTER              = 0x4c00011a,
            //MPI_INTEGER is an MPI_Datatype that represents an integer type in MPI, it corresponds to an INTEGER in FORTRAN. 
            //For the C counterpart, please see MPI_INT.
            MPI_INTEGER                = 0x4c00041b,
            //Fortran REAL
            MPI_REAL                   = 0x4c00041c,
            //Fortran LOGICAL
            MPI_LOGICAL                = 0x4c00041d,
            //Fortran COMPLEX
            MPI_COMPLEX                = 0x4c00081e,
            //Fortran DOUBLE_PERCISION 
            MPI_DOUBLE_PRECISION       = 0x4c00081f,

            MPI_2INTEGER               = 0x4c000820,
            MPI_2REAL                  = 0x4c000821,
            MPI_DOUBLE_COMPLEX         = 0x4c001022,
            MPI_2DOUBLE_PRECISION      = 0x4c001023,
            MPI_2COMPLEX               = 0x4c001024,
            MPI_2DOUBLE_COMPLEX        = 0x4c002025,
            MPI_REAL2                  = MPI_DATATYPE_NULL,
            MPI_REAL4                  = 0x4c000427,
            MPI_COMPLEX8               = 0x4c000828,
            MPI_REAL8                  = 0x4c000829,
            MPI_COMPLEX16              = 0x4c00102a,
            MPI_REAL16                 = MPI_DATATYPE_NULL,
            MPI_COMPLEX32              = MPI_DATATYPE_NULL,
            MPI_INTEGER1               = 0x4c00012d,
            MPI_COMPLEX4               = MPI_DATATYPE_NULL,
            MPI_INTEGER2               = 0x4c00022f,
            MPI_INTEGER4               = 0x4c000430,
            MPI_INTEGER8               = 0x4c000831,
            MPI_INTEGER16              = MPI_DATATYPE_NULL,
            //MPI_INT8_T is an MPI_Datatype that represents a 1-byte integer type in MPI, it corresponds to an int8_t in C. For the FORTRAN counterpart, please see MPI_INTEGER1.
             MPI_INT8_T                 = 0x4c000133,
            //MPI_INT16_T is an MPI_Datatype that represents a 2-byte integer type in MPI, it corresponds to an int16_t in C. For the FORTRAN counterpart, please see MPI_INTEGER2.
            MPI_INT16_T                = 0x4c000234,
            //MPI_INT32_T is an MPI_Datatype that represents a 4-byte integer type in MPI, it corresponds to an int32_t in C. For the FORTRAN counterpart, please see MPI_INTEGER4.
            MPI_INT32_T                = 0x4c000435,
            //MPI_INT64_T is an MPI_Datatype that represents an 8-byte integer type in MPI, it corresponds to an int64_t in C. For the FORTRAN counterpart, please see MPI_INTEGER8.
            MPI_INT64_T                = 0x4c000836,
            //MPI_UINT8_T is an MPI_Datatype that represents an unsigned 1-byte integer type in MPI, it corresponds to an uint8_t in C.
            MPI_UINT8_T                = 0x4c000137,
            //MPI_UINT16_T is an MPI_Datatype that represents an unsigned 2-byte integer type in MPI, it corresponds to an uint16_t in C.
            MPI_UINT16_T               = 0x4c000238,
            //MPI_UINT32_T is an MPI_Datatype that represents an unsigned 4-byte integer type in MPI, it corresponds to an uint32_t in C.
            MPI_UINT32_T               = 0x4c000439,
            //MPI_UINT64_T is an MPI_Datatype that represents an unsigned 8-byte integer type in MPI, it corresponds to an uint64_t in C.
            MPI_UINT64_T               = 0x4c00083a,
            // In C, MPI_Aint and MPI_AINT are distinct:
            //MPI_Aint is the type of a variable able to contain a memory address. It is used in heterogeneous datatype creation routines 
            //for instance, such as MPI_Type_create_hindexed, MPI_Type_create_hindexed_block, MPI_Type_create_hvector and MPI_Type_create_struct. 
            //Please see MPI_ADDRESS_KIND for the FORTRAN counterpart.
            //MPI_AINT is an MPI_Datatype used to inform MPI about the type of a variable passed to a routine. Similar to MPI_INT being the M
            //PI_Datatype corresponding to an int, MPI_AINT is the MPI_Datatype corresponding to an MPI_Aint.
            MPI_AINT                   = 0x4c00083b,
            MPI_AINT_WIN64             = 0x4c00043b,
            //is an integer type of size sufficient to represent the size (in bytes) of the largest file supported by MPI.
            MPI_OFFSET                 = 0x4c00083c,
            //struct {float,int}
            MPI_FLOAT_INT              = 0x8c000000,
            //struct {doubl.int}
            MPI_DOUBLE_INT             = 0x8c000001,
            //struct {long,int}
            MPI_LONG_INT               = 0x8c000002,
            //struct {short,int}
            MPI_SHORT_INT              = 0x8c000003,
            //struct {long,double,int} 
            MPI_LONG_DOUBLE_INT        = 0x8c000004
        }
        // The name of the MPI Library to load
        private const string MPI_LIBRARY = "MpiLibrary.dll";

        // The command-line invocation of dotnet
        private const string DOTNET_COMMAND = "dotnet";

        // There is a bug in .NET Core, where if you DLLImport a library that uses dlopen, the imported
        // library won't be able to get the correct version of the underlying library. (See https://github.com/dotnet/coreclr/issues/18599)
        // The workaround we use (from that issue) is to import dl into .NET, and dlopen any required libraries
        // into the .NET runtime here.
        private const int RTLD_LAZY = 0x00001; //Only resolve symbols as needed
        private const int RTLD_GLOBAL = 0x00100; //Make symbols available to libraries loaded later
    //    [DllImport("dl")]
    //    private static extern IntPtr dlopen(string file, int mode);

        // Native implementation of initialize_mpi() for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int initialize_mpi(int argc, string[] argv);

        // Native implementation of finalize_mpi() for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int finalize_mpi();

        // Native implementation of get_world_size() for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int get_world_size();

        // Native implementation of get_world_rank() for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int get_world_rank();

        // Native implementation of AllReduce(int, int, ...) for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int all_reduce_int(int i, ref int j);

        // Native implementation of AllReduce(float[], float[], ...) for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int all_reduce_floatarray(float[] source, float[] dest, int length);

        // Native implementation of GetOne() for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int get_one();

        // Native implementation of AddTwoNumbers() for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int add_two_numbers(int i, int j);

        // Native implementation of ExternalCall() for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int external_call(Functions.AddTwoIntsDelegate func, int i, int j);

        // Native implementation of ExternalReduce() for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int external_reduce(Functions.ReduceIntArrayDelegate reduceFunc, int[] array, int arraySize);

        // Native implementation of ExternalReduceWithCallback() for Linux
        [DllImport(MPI_LIBRARY)]
        private static extern int external_reduce_with_callback(
            Functions.ReduceNativeIntArrayWithFuncDelegate reduceFunc, int[] array, int arraySize);

        [DllImport(MPI_LIBRARY)]
        private static extern double mpi_wtime();

        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_barrier();

    #region MPI_SEND_IMPORTS
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_double(double buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_int(int buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_char(char buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_float(float buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_short(short buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_long(long buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_ushort(ushort buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_ulong(ulong buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_uchar(byte buf, int count, int dest, int tag);
            [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_schar(sbyte buf, int count, int dest, int tag);
    #endregion
    #region MPI_SEND_ARRAY_IMPORTS
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_double_array(double[] buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_int_array(int[] buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_char_array(char[] buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_float_array(float[] buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_short_array(short[] buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_long_array(long[] buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_uchar_array(byte[] buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_schar_array(sbyte[] buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_ushort_array(ushort[] buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_send_ulong_array(ulong[] buf, int count, int dest, int tag);
    #endregion

        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_ssendd(double buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_ssendi(int buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_ssendc(char buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_ssendf(float buf, int count, int dest, int tag);

        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_bsendd(double buf, int count, int dest, int tag,ref int mpi_request);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_bsendi(int buf, int count, int dest, int tag,ref int mpi_request);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_bsendc(char buf, int count, int dest, int tag,ref int mpi_request);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_bsendf(float buf, int count, int dest, int tag,ref int mpi_request);

        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_isendd(double buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_isendi(int buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_isendc(char buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_isendf(float buf, int count, int dest, int tag);

        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_rsendd(double buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_rsendi(int buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_rsendc(char buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_rsendf(float buf, int count, int dest, int tag);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recvi(ref int buf, int count, MPI_Datatype datatype, int source, int tag,ref MPI_Status status);
    #region MPI_Recv
         [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_int(ref int buf, int count, int source, int tag,ref int status_count,ref int status_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_char(ref char buf, int count, int source, int tag,ref int status_count,ref int status_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_double(ref double buf, int count, int source, int tag,ref int status_count,ref int status_cancelled,ref int status_MPI_SOURCE,
         ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_float(ref float buf, int count, int source, int tag,ref int status_count,ref int status_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_short(ref short buf, int count, int source, int tag,ref int status_count,ref int status_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_long(ref long buf, int count, int source, int tag,ref int status_count,ref int status_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_ushort(ref ushort buf, int count, int source, int tag,ref int status_count,ref int status_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
         [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_ulong(ref ulong buf, int count, int source, int tag,ref int status_count,ref int status_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_uchar(ref byte buf, int count, int source, int tag,ref int status_count,ref int status_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_schar(ref sbyte buf, int count, int source, int tag,ref int status_count,ref int status_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
    #endregion
    #region MPI_Recv_Array

        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_int_array(int[] buf, int count, int source, int tag,ref int status_count_lo,ref int status_count_hi_and_cancelled,ref int status_MPI_SOURCE,
    ref int status_MPI_TAG,ref int status_MPI_ERROR);
    
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_char_array(char[] buf, int count, int source, int tag,ref int status_count_lo,ref int status_count_hi_and_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_float_array(float[] buf, int count, int source, int tag,ref int status_count_lo,ref int status_count_hi_and_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_double_array(double[] buf, int count, int source, int tag,ref int status_count_lo,ref int status_count_hi_and_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_short_array(short[] buf, int count, int source, int tag,ref int status_count_lo,ref int status_count_hi_and_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_long_array(long[] buf, int count, int source, int tag,ref int status_count_lo,ref int status_count_hi_and_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_uchar_array(byte[] buf, int count, int source, int tag,ref int status_count_lo,ref int status_count_hi_and_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_schar_array(sbyte[] buf, int count, int source, int tag,ref int status_count_lo,ref int status_count_hi_and_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_ushort_array(ushort[] buf, int count, int source, int tag,ref int status_count_lo,ref int status_count_hi_and_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
        [DllImport(MPI_LIBRARY)]
        private static extern int mpi_recv_ulong_array(ulong[] buf, int count, int source, int tag,ref int status_count_lo,ref int status_count_hi_and_cancelled,ref int status_MPI_SOURCE,
            ref int status_MPI_TAG,ref int status_MPI_ERROR);
    #endregion
        /// <summary>
        /// Initialize the MPI object.
        /// </summary>
        /// <param name="args">The arguments array passed to the program.</param>
        /// <returns>The result of MPI_Init().</returns>
        public Mpi(string[] args)
        {
            // Pre-load dependencies of the wrapping module; necessary to grab the right _version_
            // libmpi.so.12 is for OpenMPI on Ubuntu16.04; libmpi.so.20 for Ubuntu18.04.
        //    dlopen("libmpi.so.12", RTLD_LAZY | RTLD_GLOBAL);
            InitializeMpi(args);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <remarks>
        /// Necessary so that we can open and close the MPI connection with a using statement.
        /// </remarks>
        public void Dispose()
        {
            FinalizeMpi();
        }
    #region MPI_Send
        public int MPI_Send(int buf,int count,int dest,int tag){
            return mpi_send_int(buf,count,dest,tag);
        }
        public int MPI_Send(double buf,int count,int dest,int tag){
            return mpi_send_double(buf,count,dest,tag);
        }
        public int MPI_Send(char buf,int count,int dest,int tag){
            return mpi_send_char(buf,count,dest,tag);
        }
        public int MPI_Send(float buf,int count,int dest,int tag){
            return mpi_send_float(buf,count,dest,tag);
        }
        public int MPI_Send(long buf,int count,int dest,int tag){
            return mpi_send_long(buf,count,dest,tag);
        }
        public int MPI_Send(short buf,int count,int dest,int tag){
            return mpi_send_short(buf,count,dest,tag);
        }
        public int MPI_Send(byte buf,int count,int dest,int tag){
            return mpi_send_uchar(buf,count,dest,tag);
        }
        public int MPI_Send(sbyte buf,int count,int dest,int tag){
            return mpi_send_schar(buf,count,dest,tag);
        }
        public int MPI_Send(ushort buf,int count,int dest,int tag){
            return mpi_send_ushort(buf,count,dest,tag);
        }
        public int MPI_Send(ulong buf,int count,int dest,int tag){
            return mpi_send_ulong(buf,count,dest,tag);
        }
    #endregion
    #region MPI_Send_Array
         public int MPI_Send(int[] buf,int count,int dest,int tag){
            return mpi_send_int_array(buf,count,dest,tag);
        }
        public int MPI_Send(double[] buf,int count,int dest,int tag){
            return mpi_send_double_array(buf,count,dest,tag);
        }
        public int MPI_Send(char[] buf,int count,int dest,int tag){
            return mpi_send_char_array(buf,count,dest,tag);
        }
        public int MPI_Send(float[] buf,int count,int dest,int tag){
            return mpi_send_float_array(buf,count,dest,tag);
        }
        public int MPI_Send(long[] buf,int count,int dest,int tag){
            return mpi_send_long_array(buf,count,dest,tag);
        }
        public int MPI_Send(short[] buf,int count,int dest,int tag){
            return mpi_send_short_array(buf,count,dest,tag);
        }
        public int MPI_Send(byte[] buf,int count,int dest,int tag){
            return mpi_send_uchar_array(buf,count,dest,tag);
        }
        public int MPI_Send(sbyte[] buf,int count,int dest,int tag){
            return mpi_send_schar_array(buf,count,dest,tag);
        }
        public int MPI_Send(ushort[] buf,int count,int dest,int tag){
            return mpi_send_ushort_array(buf,count,dest,tag);
        }
        public int MPI_Send(ulong[] buf,int count,int dest,int tag){
            return mpi_send_ulong_array(buf,count,dest,tag);
        }
    #endregion
    #region MPI_Ssend
        public int MPI_Ssend(int buf,int count,int dest,int tag){
            return mpi_ssendi(buf,count,dest,tag);
        }
        public int MPI_Ssend(double buf,int count,int dest,int tag){
            return mpi_ssendd(buf,count,dest,tag);
        }
        public int MPI_Ssend(char buf,int count,int dest,int tag){
            return mpi_ssendc(buf,count,dest,tag);
        }
        public int MPI_Ssend(float buf,int count,int dest,int tag){
            return mpi_ssendf(buf,count,dest,tag);
        }
    #endregion
    #region MPI_ISend  
        public int MPI_ISend(int buf,int count,int dest,int tag){
            return mpi_isendi(buf,count,dest,tag);
        }
        public int MPI_ISend(double buf,int count,int dest,int tag){
            return mpi_isendd(buf,count,dest,tag);
        }
        public int MPI_ISend(char buf,int count,int dest,int tag){
            return mpi_isendc(buf,count,dest,tag);
        }
        public int MPI_ISend(float buf,int count,int dest,int tag){
            return mpi_isendf(buf,count,dest,tag);
        }
    #endregion
    #region MPI_RSend
         public int MPI_RSend(int buf,int count,int dest,int tag){
            return mpi_rsendi(buf,count,dest,tag);
        }
        public int MPI_RSend(double buf,int count,int dest,int tag){
            return mpi_rsendd(buf,count,dest,tag);
        }
        public int MPI_RSend(char buf,int count,int dest,int tag){
            return mpi_rsendc(buf,count,dest,tag);
        }
        public int MPI_RSend(float buf,int count,int dest,int tag){
            return mpi_rsendf(buf,count,dest,tag);
        }
    #endregion
    #region MPI_BSend
         public int MPI_BSend(int buf,int count,int dest,int tag,ref int mpi_request){
            return mpi_bsendi(buf,count,dest,tag,ref mpi_request);
        }
        public int MPI_BSend(double buf,int count,int dest,int tag,ref int mpi_request){
            return mpi_bsendd(buf,count,dest,tag,ref mpi_request);
        }
        public int MPI_BSend(char buf,int count,int dest,int tag,ref int mpi_request){
            return mpi_bsendc(buf,count,dest,tag,ref mpi_request);
        }
        public int MPI_BSend(float buf,int count,int dest,int tag,ref int mpi_request){
            return mpi_bsendf(buf,count,dest,tag,ref mpi_request);
        }
    #endregion
    #region MPI_Recv
       
        public int MPI_Recv(ref int buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_int(ref buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref char buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_char(ref buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref float buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_float(ref buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref double buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_double(ref buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref short buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_short(ref buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref long buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_long(ref buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref byte buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_uchar(ref buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref sbyte buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_schar(ref buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref ushort buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_ushort(ref buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref ulong buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_ulong(ref buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
    #endregion
    #region MPI_Recv_Array
        public int MPI_Recv(ref int[] buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_int_array(buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);
            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref char[] buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_char_array(buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);

            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref long[] buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_long_array(buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);

            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref float[] buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_float_array(buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);

            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref sbyte[] buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_schar_array(buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);

            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref short[] buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_short_array(buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);

            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref byte[] buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_uchar_array(buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);

            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref ulong[] buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_ulong_array(buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);

            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref double[] buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_double_array(buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);

            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
        public int MPI_Recv(ref ushort[] buf, int count, int source, int tag,out MPI_Status status){

            int status_count_lo = -1;
            int status_count_hi_and_cancelled = -1;
            int status_MPI_SOURCE = -1; 
            int status_MPI_TAG = -1;
            int status_MPI_ERROR = -1;
            int response = mpi_recv_ushort_array(buf,count,source,tag,ref status_count_lo,ref status_count_hi_and_cancelled,ref status_MPI_SOURCE,
            ref status_MPI_TAG,ref status_MPI_ERROR);

            status = new MPI_Status{
                count_lo = status_count_lo,
                count_hi_and_cancelled = status_count_hi_and_cancelled,
                MPI_SOURCE = status_MPI_SOURCE,
                MPI_ERROR = status_MPI_ERROR,
                MPI_TAG = status_MPI_TAG
            };
            return response;
        }
    #endregion
        //calls the MPI_Wtime() function
        public double MPI_Wtime(){
            return mpi_wtime();
        }
        //calls the MPI_Barrier() function
        public int MPI_Barrier(){
            return mpi_barrier();
        }

        private int InitializeMpi(string[] args)
        {
            // Here we recreate the args that a C program would give to MPI; probably not necessary.
            var cStyleArgs = args.ToList();
            cStyleArgs.Insert(0, DOTNET_COMMAND);
            var fileName = Environment.GetCommandLineArgs()[0];
            cStyleArgs.Insert(1, fileName);
            return initialize_mpi(cStyleArgs.Count, cStyleArgs.ToArray());
        }

        /// <summary>
        /// Finalize MPI.
        /// </summary>
        /// <returns>The result of MPI_Finalize().</returns>
        private int FinalizeMpi()
        {
            return finalize_mpi();
        }

        /// <summary>
        /// Get the MPI world size.
        /// </summary>
        /// <returns>The world size.</returns>
        public int GetWorldSize()
        {
            return get_world_size();
        }

        /// <summary>
        /// Get the MPI world rank.
        /// </summary>
        /// <returns>The world rank.</returns>
        public int GetWorldRank()
        {
            return get_world_rank();
        }

        /// <summary>
        /// AllReduce an integer to an integer.
        /// </summary>
        /// <param name="i">The value in the local process.</param>
        /// <param name="j">The sum over all the workers.</param>
        /// <returns>The result of the MPI operation.</returns>
        public int AllReduce(int i, ref int j)
        {
            return all_reduce_int(i, ref j);
        }

        /// <summary>
        /// AllReduce a flaot array to a float array.
        /// </summary>
        /// <param name="i">The value in the local process.</param>
        /// <param name="j">The sum over all the workers.</param>
        /// <returns>The result of the MPI operation.</returns>
        public int AllReduce(float[] source, float[] dest)
        {
            if (source == null || dest == null)
                throw new ArgumentException("Arrays must not be null.");
            if (source.Length != dest.Length)
                throw new ArgumentException("Arrays not of equal length.");

            return all_reduce_floatarray(source, dest, source.Length);
        }

        /// <summary>
        /// Return the number 1
        /// </summary>
        /// <returns>The number 1</returns>
        public int GetOne()
        {
            return get_one();
        }

        /// <summary>
        /// Sum two numbers
        /// </summary>
        /// <param name="i">An integer</param>
        /// <param name="j">An integer</param>
        /// <returns>The sum of <paramref name="i"/> and <paramref name="j"/></returns>
        public int AddTwoNumbers(int i, int j)
        {
            return add_two_numbers(i, j);
        }

        /// <summary>
        /// Evaluate a function with integer inputs
        /// </summary>
        /// <param name="func">A delegate to a function taking two ints as input and returning an int</param>
        /// <param name="i">An integer</param>
        /// <param name="j">An integer</param>
        /// <returns>The result of the function</returns>
        public int ExternalCall(Functions.AddTwoIntsDelegate func, int i, int j)
        {
            return external_call(func, i, j);
        }

        /// <summary>
        /// Evaluate a reduce function
        /// </summary>
        /// <param name="reduceFunc">A delegate to a reduce function taking an integer array</param>
        /// <param name="array">The integer array</param>
        /// <param name="arraySize">The size of the integer array</param>
        /// <returns>The result of the reduce function</returns>
        public int ExternalReduce(Functions.ReduceIntArrayDelegate reduceFunc, int[] array, int arraySize)
        {
            return external_reduce(reduceFunc, array, arraySize);
        }

        /// <summary>
        /// Evaluate a reduce function taking a callback to a custom reducer
        /// </summary>
        /// <param name="reduceFunc">A delegate to a reducer function taking an integer array, the integer array size, and a pointer to a reducing function over the integer array</param>
        /// <param name="array">The integer array</param>
        /// <param name="arraySize">The size of the integer array</param>
        public int ExternalReduceWithCallback(Functions.ReduceNativeIntArrayWithFuncDelegate reduceFunc, int[] array, int arraySize)
        {
            return external_reduce_with_callback(reduceFunc, array, arraySize);
        }
    }
}
