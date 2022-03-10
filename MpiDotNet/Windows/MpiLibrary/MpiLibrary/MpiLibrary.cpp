// MpiLibrary.cpp : Defines the exported functions for the DLL.
#include "stdafx.h"
#include <mpi.h>
#include "MpiLibrary.h"
#include <iostream>

// Initialize MPI
int initialize_mpi(int argc, char *argv[]) {
	return MPI_Init(&argc, &argv);
}

int finalize_mpi()
{
	return MPI_Finalize();
}

// Get the World Size
int get_world_size()
{
	int size;
	MPI_Comm_size(MPI_COMM_WORLD, &size);
	return size;
}
// get the mpi_barrier
int mpi_barrier(){
	int a = MPI_Barrier(MPI_COMM_WORLD);
	return a;
}
// get the mpi_wtime
double mpi_wtime(){
	return MPI_Wtime();
}
////////////////////////////////////////////////////
// MPI_Send methods ////////////////////////////////
////////////////////////////////////////////////////
//reference
/*int mpi_send(const void *buf, int count, MPI_Datatype datatype, int dest, int tag){
	int a = MPI_Send(buf,count,datatype,dest,tag,MPI_COMM_WORLD);
	return a;
}*/
/*NOTE: these are blocking sends!*/
int mpi_send_char(char buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_int(int buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_INT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_float(float buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_FLOAT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_double(double buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_DOUBLE,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_short(short buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_SHORT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_long(long buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_LONG,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_ulong(unsigned long buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_UNSIGNED_LONG,dest,tag,MPI_COMM_WORLD);
}

int mpi_send_uchar(unsigned char buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_UNSIGNED_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_schar(signed char buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_SIGNED_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_ushort(unsigned short buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_UNSIGNED_SHORT,dest,tag,MPI_COMM_WORLD);
}
// arrays
int mpi_send_char_array(char buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_int_array(int buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_INT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_float_array(float buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_FLOAT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_double_array(double buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_DOUBLE,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_short_array(short buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_SHORT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_long_array(long buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_LONG,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_ulong_array(unsigned long buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_UNSIGNED_LONG,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_uchar_array(unsigned char buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_UNSIGNED_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_schar_array(signed char buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_SIGNED_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_ushort_array(unsigned short buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_UNSIGNED_SHORT,dest,tag,MPI_COMM_WORLD);
}
////////////////////////////////////////////////////////////
// MPI_Ssend methods ///////////////////////////////////////
////////////////////////////////////////////////////////////
int mpi_ssendc(char buf, int count, int dest, int tag){
	return MPI_Ssend(&buf,count,MPI_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_ssendi(int buf, int count, int dest, int tag){
	return MPI_Ssend(&buf,count,MPI_INT,dest,tag,MPI_COMM_WORLD);
}
int mpi_ssendf(float buf, int count, int dest, int tag){
	return MPI_Ssend(&buf,count,MPI_FLOAT,dest,tag,MPI_COMM_WORLD);
}int mpi_ssendd(double buf, int count, int dest, int tag){
	return MPI_Ssend(&buf,count,MPI_DOUBLE,dest,tag,MPI_COMM_WORLD);
}

// MPI_Bsend methods
int mpi_bsendc(char buf, int count, int dest, int tag){
	return MPI_Bsend(&buf,count,MPI_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_bsendi(int buf, int count, int dest, int tag){
	return MPI_Bsend(&buf,count,MPI_INT,dest,tag,MPI_COMM_WORLD);
}
int mpi_bsendf(float buf, int count, int dest, int tag){
	return MPI_Bsend(&buf,count,MPI_FLOAT,dest,tag,MPI_COMM_WORLD);
}int mpi_bsendd(double buf, int count, int dest, int tag){
	return MPI_Bsend(&buf,count,MPI_DOUBLE,dest,tag,MPI_COMM_WORLD);
}
// MPI_Isend methods
int mpi_isendc(char buf, int count, int dest, int tag,int& mpi_request){
	return MPI_Isend(&buf,count,MPI_CHAR,dest,tag,MPI_COMM_WORLD,&mpi_request);
}
int mpi_isendi(int buf, int count, int dest, int tag,int& mpi_request){
	return MPI_Isend(&buf,count,MPI_INT,dest,tag,MPI_COMM_WORLD,&mpi_request);
}
int mpi_isendf(float buf, int count, int dest, int tag,int& mpi_request){
	return MPI_Isend(&buf,count,MPI_FLOAT,dest,tag,MPI_COMM_WORLD,&mpi_request);
}
int mpi_isendd(double buf, int count, int dest, int tag,int& mpi_request){
	return MPI_Isend(&buf,count,MPI_DOUBLE,dest,tag,MPI_COMM_WORLD,&mpi_request);
}
// MPI_Rsend methods
int mpi_rsendc(char buf, int count, int dest, int tag){
	return MPI_Rsend(&buf,count,MPI_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_rsendi(int buf, int count, int dest, int tag){
	return MPI_Rsend(&buf,count,MPI_INT,dest,tag,MPI_COMM_WORLD);
}
int mpi_rsendf(float buf, int count, int dest, int tag){
	return MPI_Rsend(&buf,count,MPI_FLOAT,dest,tag,MPI_COMM_WORLD);
}int mpi_rsendd(double buf, int count, int dest, int tag){
	return MPI_Rsend(&buf,count,MPI_DOUBLE,dest,tag,MPI_COMM_WORLD);
}

/////////////////////////////////////////////////
// Call MPI_Recv ////////////////////////////////
/////////////////////////////////////////////////

int mpi_recv(void *buf, int count, MPI_Datatype datatype, int source, int tag, MPI_Status *status){
	return MPI_Recv(buf,count,datatype,source,tag,MPI_COMM_WORLD,status);
}
int mpi_recvi_legacy_not_working(int& buf, int count, int source, int tag, MPI_Status& status){
	return MPI_Recv(&buf, count, MPI_INT, source, tag, MPI_COMM_WORLD, &status);
     /*   printf("received value %d from rank %d, with tag %d and error code %d.\n", 
               buf,
               status.MPI_SOURCE,
               status.MPI_TAG,
               status.MPI_ERROR);*/
}

// extracts status values and assigns them to the referenced ints
void mpi_recv_populate_status(int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
	int& status_MPI_TAG, int& status_MPI_ERROR, MPI_Status status) {
	//	status_count_lo = status.count_hi_and_cancelled;
	//	status_count_hi_and_cancelled = status.count_hi_and_cancelled;
	status_count_lo = -1;
	status_count_hi_and_cancelled = -1;
	status_MPI_SOURCE = status.MPI_SOURCE;
	status_MPI_TAG = status.MPI_TAG;
	status_MPI_ERROR = status.MPI_ERROR;
}
#pragma region
int mpi_recv_int(int& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_INT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
int mpi_recv_char(char& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
int mpi_recv_float(float& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_INT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
int mpi_recv_double(double& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_DOUBLE,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
int mpi_recv_short(short& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_SHORT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
int mpi_recv_long(long& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_LONG,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
int mpi_recv_uchar(unsigned char& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_UNSIGNED_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
int mpi_recv_schar(signed char& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_SIGNED_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
int mpi_recv_ushort(unsigned short& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_UNSIGNED_SHORT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
int mpi_recv_ulong(unsigned long& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_UNSIGNED_LONG,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
#pragma endregion
	int mpi_recv_int_array(int buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_INT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_char_array(char buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_double_array(double buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_DOUBLE,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_float_array(float buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_FLOAT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_short_array(short buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_SHORT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_long_array(long buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_LONG,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_uchar_array(unsigned char buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_UNSIGNED_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_schar_array(signed char buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_SIGNED_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_ushort_array(unsigned short buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_UNSIGNED_SHORT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_ulong_array(unsigned long buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_UNSIGNED_LONG,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
/////////////////////////////////////////////////////////////////////////////////////////////////
//Get the World Rank
int get_world_rank()
{
	int rank;
	MPI_Comm_rank(MPI_COMM_WORLD, &rank);
	return rank;
}

int all_reduce_int(int i, int& j)
{
	return MPI_Allreduce(&i, &j, 1, MPI_INT, MPI_SUM, MPI_COMM_WORLD);
}

int all_reduce_floatarray(float source[], float dest[], int length)
{
	return MPI_Allreduce(source, dest, length, MPI_FLOAT, MPI_SUM, MPI_COMM_WORLD);
}

// Return 1
int get_one()
{
	return 1;
}

// Add two numbers
int add_two_numbers(int i, int j)
{
	return i + j;
}

// Call a function pointer with two ints, and return the result
int external_call(TwoIntReduceDelegate addTwoNumbers, int i, int j)
{
	return addTwoNumbers(i, j);
}

// Call a reduce function pointer with an int array and return the result
int external_reduce(ReduceIntArrayDelegate reduceFunc, int i[], int i_size)
{
	return reduceFunc(i, i_size);
}

// Reduce an int array by summation
int reduce_int_array(int v[], int v_size)
{
	int sum = 0;
	for (int i = 0; i < v_size; i++)
	{
		sum += v[i];
	}
	return sum;
}

// Call a function pointer with an array of integers and a custom reducing function and return the results
int external_reduce_with_callback(ReduceIntArrayWithFuncDelegate reduceHostFunc, int i[], int i_size)
{
	return reduceHostFunc(i, i_size, *reduce_int_array);
}
