// MpiLibrary.h - Contains declarations of functions and delegates
#define WINDOWS_BUILD // comment this out on linux

#ifdef WINDOWS_BUILD
#define MpiLIBRARY_EXPORTS
#endif
#pragma once
#include <mpi.h>
#ifndef _WINDOWS
#define __declspec(dllexport)
#endif
#ifdef MpiLIBRARY_EXPORTS  
#define MpiLIBRARY_API __declspec(dllexport)   
#else  
#define MpiLIBRARY_API __declspec(dllimport)   
#endif  
/////////////////////////////////////////////////////
// General_Functions ////////////////////////////////
/////////////////////////////////////////////////////
#pragma region General_Functions
// Initialize MPI
extern "C" MpiLIBRARY_API int initialize_mpi(int argc, char *argv[]);

extern "C" MpiLIBRARY_API int finalize_mpi();

// Get the World Size
extern "C" MpiLIBRARY_API int get_world_size__ws();

extern "C" MpiLIBRARY_API int get_world_size__w(int & size);

extern "C" MpiLIBRARY_API int get_world_size(int comm,int & size);

// Get the World Rank
extern "C" MpiLIBRARY_API int get_world_rank__wr();
extern "C" MpiLIBRARY_API int get_world_rank__w(int& rank);
extern "C" MpiLIBRARY_API int get_world_rank(int comm,int& rank);

extern "C" MpiLIBRARY_API int all_reduce_int__oc(int i, int& j);
extern "C" MpiLIBRARY_API int all_reduce_int(int i, int& j,int opertaion,int comm);
extern "C" MpiLIBRARY_API int reduce_int_array(int v[],int v_size);
extern "C" MpiLIBRARY_API int mpi_reduce_int(int& send, int& recv, int count, int operation, int root, int comm);

extern "C" MpiLIBRARY_API int all_reduce_floatarray(float source[], float dest[], int length);

extern "C" MpiLIBRARY_API int get_mpi_comm_world();
extern "C" MpiLIBRARY_API int get_mpi_comm_self();
extern "C" MpiLIBRARY_API int get_mpi_comm_null();
// Get the number 1
extern "C" MpiLIBRARY_API int get_one();

// Add two numbers
extern "C" MpiLIBRARY_API int add_two_numbers(int i, int j);
// call MPI_Barrier()
extern "C" MpiLIBRARY_API int mpi_barrier();
// call MPI_Wtime()
extern "C" MpiLIBRARY_API double mpi_wtime();

extern "C" MpiLIBRARY_API void mpi_recv_populate_status(int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR,MPI_Status status);

// MPI_Send place holder, this isn't used in pratice
extern "C" MpiLIBRARY_API int mpi_send(const void *buf, int count, int dest, int tag);

extern "C" MpiLIBRARY_API int mpi_comm_group(int& group);

extern "C" MpiLIBRARY_API int mpi_group_incl(int group, int n, int ranks[], int& new_group);

extern "C" MpiLIBRARY_API int mpi_comm_create(int comm, int group, int& new_comm);

extern "C" MpiLIBRARY_API int mpi_comm_create__w(int group, int& new_comm);
#pragma endregion
/////////////////////////////////////////////////////
// MPI_Send /////////////////////////////////////////
/////////////////////////////////////////////////////
#pragma region MPI_Send
extern "C" MpiLIBRARY_API int mpi_send_double_s(double buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_int_s(int buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_char_s(char buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_float_s(float buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_short_s(short buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_long_s(long buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_ushort_s(unsigned short buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_ulong_s(unsigned long buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_uchar_s(unsigned char buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_schar_s(signed char buf, int count, int dest, int tag);

extern "C" MpiLIBRARY_API int mpi_send_double(double buf, int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_int(int buf, int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_char(char buf, int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_float(float buf, int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_short(short buf, int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_long(long buf, int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_ushort(unsigned short buf, int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_ulong(unsigned long buf, int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_uchar(unsigned char buf, int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_schar(signed char buf, int count, int dest, int tag, int comm);

// MPI_Send array 
extern "C" MpiLIBRARY_API int mpi_send_double_array_s(double buf[], int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_int_array_s(int buf[], int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_char_array_s(char buf[], int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_float_array_s(float buf[], int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_short_array_s(short buf[], int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_long_array_s(long buf[], int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_uchar_array_s(unsigned char buf[], int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_schar_array_s(signed char buf[], int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_ushort_array_s(unsigned short buf[], int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_send_ulong_array_s(unsigned long buf[], int count, int dest, int tag);

extern "C" MpiLIBRARY_API int mpi_send_double_array(double buf[], int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_int_array(int buf[], int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_char_array(char buf[], int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_float_array(float buf[], int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_short_array(short buf[], int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_long_array(long buf[], int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_uchar_array(unsigned char buf[], int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_schar_array(signed char buf[], int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_ushort_array(unsigned short buf[], int count, int dest, int tag, int comm);
extern "C" MpiLIBRARY_API int mpi_send_ulong_array(unsigned long buf[], int count, int dest, int tag, int comm);


extern "C" MpiLIBRARY_API int mpi_ssendd(double buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_ssendi(int buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_ssendc(char buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_ssendf(float buf, int count, int dest, int tag);

extern "C" MpiLIBRARY_API int mpi_bsendd(double buf, int count, int dest, int tag,int& mpi_request);
extern "C" MpiLIBRARY_API int mpi_bsendi(int buf, int count, int dest, int tag,int& mpi_request);
extern "C" MpiLIBRARY_API int mpi_bsendc(char buf, int count, int dest, int tag,int& mpi_request);
extern "C" MpiLIBRARY_API int mpi_bsendf(float buf, int count, int dest, int tag,int& mpi_request);

extern "C" MpiLIBRARY_API int mpi_isendd(double buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_isendi(int buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_isendc(char buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_isendf(float buf, int count, int dest, int tag);

extern "C" MpiLIBRARY_API int mpi_rsendd(double buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_rsendi(int buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_rsendc(char buf, int count, int dest, int tag);
extern "C" MpiLIBRARY_API int mpi_rsendf(float buf, int count, int dest, int tag);
#pragma endregion

/////////////////////////////////////////////////////
// Call MPI_Recv ////////////////////////////////////
/////////////////////////////////////////////////////
#pragma region MPI_Recv
extern "C" MpiLIBRARY_API int mpi_recv(void *buf, int count, MPI_Datatype datatype, int source, int tag,MPI_Status *status);
extern "C" MpiLIBRARY_API int mpi_recvi_legacy_not_working(int& buf, int count, int source, int tag,MPI_Status& status);
extern "C" MpiLIBRARY_API int mpi_recv_int_s(int& buf, int count, int source, int tag,int& status_count,int& status_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_char_s(char& buf, int count, int source, int tag,int& status_count,int& status_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_double_s(double& buf, int count, int source, int tag,int& status_count,int& status_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_float_s(float& buf, int count, int source, int tag,int& status_count,int& status_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_short_s(short& buf, int count, int source, int tag,int& status_count,int& status_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_long_s(long& buf, int count, int source, int tag,int& status_count,int& status_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_ushort_s(unsigned short& buf, int count, int source, int tag,int& status_count,int& status_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_ulong_s(unsigned long& buf, int count, int source, int tag,int& status_count,int& status_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_uchar_s(unsigned char& buf, int count, int source, int tag,int& status_count,int& status_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_schar_s(signed char& buf, int count, int source, int tag,int& status_count,int& status_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);

extern "C" MpiLIBRARY_API int mpi_recv_int_array_s(int buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_char_array_s(char buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_float_array_s(float buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_double_array_s(double buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_short_array_s(short buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_long_array_s(long buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_uchar_array_s(unsigned char buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API  int mpi_recv_schar_array_s(signed char buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_ushort_array_s(unsigned short buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_ulong_array_s(unsigned long buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR);

extern "C" MpiLIBRARY_API int mpi_recv_int(int& buf, int count, int source, int tag, int comm, int& status_count, int& status_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_char(char& buf, int count, int source, int tag, int comm, int& status_count, int& status_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_double(double& buf, int count, int source, int tag, int comm, int& status_count, int& status_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_float(float& buf, int count, int source, int tag, int comm, int& status_count, int& status_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_short(short& buf, int count, int source, int tag, int comm, int& status_count, int& status_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_long(long& buf, int count, int source, int tag, int comm, int& status_count, int& status_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_ushort(unsigned short& buf, int count, int source, int tag, int comm, int& status_count, int& status_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_ulong(unsigned long& buf, int count, int source, int tag, int comm, int& status_count, int& status_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_uchar(unsigned char& buf, int count, int source, int tag, int comm, int& status_count, int& status_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_schar(signed char& buf, int count, int source, int tag, int comm, int& status_count, int& status_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);

extern "C" MpiLIBRARY_API int mpi_recv_int_array(int buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_char_array(char buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_float_array(float buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_double_array(double buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_short_array(short buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_long_array(long buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_uchar_array(unsigned char buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API  int mpi_recv_schar_array(signed char buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_ushort_array(unsigned short buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
extern "C" MpiLIBRARY_API int mpi_recv_ulong_array(unsigned long buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
    int& status_MPI_TAG, int& status_MPI_ERROR);
#pragma endregion
/////////////////////////////////////////////////////
/// MPI_Bcast ///////////////////////////////////////
/////////////////////////////////////////////////////
#pragma region MPI_Bcast
extern "C" MpiLIBRARY_API int mpi_bcast_double(double& buf, int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_int(int& buf, int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_char(char& buf, int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_float(float& buf, int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_short(short& buf, int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_long(long& buf, int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_ushort(unsigned short& buf, int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_ulong(unsigned long& buf, int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_uchar(unsigned char& buf, int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_schar(signed char& buf, int count, int root);

// MPI_Bcast array 
extern "C" MpiLIBRARY_API int mpi_bcast_double_array(double buf[], int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_int_array(int buf[], int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_char_array(char buf[], int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_float_array(float buf[], int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_short_array(short buf[], int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_long_array(long buf[], int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_uchar_array(unsigned char buf[], int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_schar_array(signed char buf[], int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_ushort_array(unsigned short buf[], int count, int root);
extern "C" MpiLIBRARY_API int mpi_bcast_ulong_array(unsigned long buf[], int count, int root);

#pragma endregion


// Call a function with two integers
typedef int(*TwoIntReduceDelegate)(int a, int b);
extern "C" MpiLIBRARY_API int external_call(TwoIntReduceDelegate addTwoNumbers, int i, int j);

// Call a function that reduces an array
typedef int(*ReduceIntArrayDelegate)(int v[], int v_size);
extern "C" MpiLIBRARY_API int external_reduce(ReduceIntArrayDelegate reduceFunc, int i[], int i_size);

// Call a function that reduces an array using a supplied C++ function
typedef int(*ReduceIntArrayWithFuncDelegate)(int v[], int v_size, int(*ReduceIntArray)(int v[], int v_size));
extern "C" MpiLIBRARY_API int external_reduce_with_callback(ReduceIntArrayWithFuncDelegate reduceHostFunc, int i[], int i_size);
