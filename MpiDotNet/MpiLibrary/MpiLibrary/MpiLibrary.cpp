// MpiLibrary.cpp : Defines the exported functions for the DLL.
#include "stdafx.h"
#include "MpiLibrary.h"
#ifdef WINDOWS_BUILD
#include <mpi.h>
#else
#include <mpi/mpi.h>
#endif
#include <iostream>
#pragma region General_Functions
// Initialize MPI
int initialize_mpi(int argc, char *argv[]) {
	return MPI_Init(&argc, &argv);
}

int finalize_mpi()
{
	return MPI_Finalize();
}
/// <summary>
/// Gets the World Size
/// </summary>
/// <returns>world size</returns>
int get_world_size__ws()
{
	int size;
	MPI_Comm_size(MPI_COMM_WORLD, &size);
	return size;
}
/// <summary>
/// Gets the World Size
/// </summary>
/// <param name="comm">MPI_Comm id</param>
/// <returns>error code</returns>
int get_world_size__w(int & size)
{
	return MPI_Comm_size(MPI_COMM_WORLD, &size);
}
/// <summary>
/// Gets the World Size
/// </summary>
/// <param name="comm">MPI_Comm id</param>
/// <param name="size">world size</param>
/// <returns>error code</returns>
int get_world_size(int comm,int & size)
{
	MPI_Comm c = comm;
	return MPI_Comm_size(c, &size);
}
//Get the World Rank
int get_world_rank__wr()
{
	int rank;
	MPI_Comm_rank(MPI_COMM_WORLD, &rank);
	return rank;
}
/// <summary>
/// //Get the World Rank
/// </summary>
/// <param name="rank">world rank</param>
/// <returns>error code</returns>
int get_world_rank__w(int& rank)
{
	return MPI_Comm_rank(MPI_COMM_WORLD, &rank);
}
/// <summary>
/// //Get the World Rank
/// </summary>
/// <param name="comm">comm handle</param>
/// <param name="rank">world rank</param>
/// <returns>error code</returns>
int get_world_rank(int comm,int& rank)
{
	MPI_Comm c = comm;
	return MPI_Comm_rank(c, &rank);
}
/// <summary>
/// performs an MPI_ALLreduce with operation MPI_SUM and MPI_World = MPI_COMM_WORLD
/// </summary>
/// <param name="i">The value in the local process.</param>
/// <param name="j">The sum over all the workers.</param>
/// <returns>error code</returns>
/// <returns>error code</returns>
int all_reduce_int__oc(int i, int& j)
{
	return MPI_Allreduce(&i, &j, 1, MPI_INT, MPI_SUM, MPI_COMM_WORLD);
}
/// <summary>
/// performs an MPI_ALLreduce with operation MPI_SUM and MPI_World = MPI_COMM_WORLD
/// </summary>
/// <param name="i">The value in the local process.</param>
/// <param name="j">The sum over all the workers.</param>
/// <param name="operation">type of operation to execute</param>
/// <param name="comm">MPI_Comm to execute this in</param>
/// <returns>error code</returns>
int all_reduce_int(int i, int& j, int operation,int comm) {
	return MPI_Allreduce(&i, &j, 1, MPI_INT, (MPI_Op)operation, (MPI_Comm)comm);
}
// returns the MPI_COMM_WORLD
int get_mpi_comm_world() {
	return (int)MPI_COMM_WORLD;
}
// returns the MPI_COMM_SELF
int get_mpi_comm_self() {
	return (int)MPI_COMM_SELF;
}
// returns the MPI_COMM_NULL
int get_mpi_comm_null() {
	return (int)MPI_COMM_NULL;
}


/// <summary>
/// Reduces values on all processes to a single value 
/// </summary>
/// <param name="send">address of send buffer (choice) </param>
/// <param name="recv">address of receive buffer (choice, significant only at root) </param>
/// <param name="count">number of elements in send buffer (integer) </param>
/// <param name="operation">reduce operation (handle) </param>
/// <param name="root">rank of root process (integer) </param>
/// <param name="comm">communicator (handle) </param>
/// <returns>error code</returns>
int mpi_reduce_int(int& send,int& recv,int count,int operation,int root,int comm) {
	return MPI_Reduce(&send, &recv, count, MPI_INT, (MPI_Op)operation, root,(MPI_Comm)comm);
}
/*
/// <summary>
/// Reduces values on all processes to a single value 
/// </summary>
/// <param name="send">address of send buffer (choice) </param>
/// <param name="recv">address of receive buffer (choice, significant only at root) </param>
/// <param name="count">number of elements in send buffer (integer) </param>
/// <param name="data_type">expected datatype</param>
/// <param name="operation">reduce operation (handle) </param>
/// <param name="root">rank of root process (integer) </param>
/// <param name="comm">communicator (handle) </param>
/// <returns>error code</returns>
int mpi_reduce(int& send,int& recv,int count,unsigned int data_type,int operation,int root,int comm) {
	return MPI_Reduce(&send, &recv, count,
		(MPI_Datatype)data_type, (MPI_Op)operation, root,(MPI_Comm)comm);
}*/

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


// get the mpi_barrier
int mpi_barrier(){
	return MPI_Barrier(MPI_COMM_WORLD);
}
/// <summary>
/// Accesses the group associated with given communicator 
/// </summary>
/// <param name="group">MPI_Group id</param>
/// <returns>error code</returns>
int mpi_comm_group(int& group) {
	MPI_Group g; // a MPI_Group is an int
	int result = MPI_Comm_group(MPI_COMM_WORLD,&g);
	group = g;
	return result;
}
/// <summary>
/// Produces a group by reordering an existing group and taking only listed members 
/// </summary>
/// <param name="group">group id (NOTE: this is treated as a MPI_GROUP)</param>
/// <param name="n">number of elements in array ranks (and size of newgroup ) (integer) </param>
/// <param name="ranks">ranks of processes in group to appear in newgroup (array of integers) </param>
/// <param name="new_group">new group derived from above, in the order defined by ranks (handle) </param>
/// <returns>error code</returns>
int mpi_group_incl(int group, int n, int ranks[], int & new_group) {
	MPI_Group g = group;
	MPI_Group new_g;
	int result = MPI_Group_incl(g,n,ranks,&new_g);
	new_group = new_g;
	return result;
}
/// <summary>
/// Creates a new communicator 
/// </summary>
/// <param name="comm">communicator (handle) </param>
/// <param name="group">group, which is a subset of the group of comm (handle) </param>
/// <param name="new_comm">new communicator (handle) </param>
/// <returns>error code</returns>
int mpi_comm_create(int comm, int group, int& new_comm) {
	MPI_Group g = group;
	MPI_Comm c = comm;
	MPI_Group new_g;
	int result = MPI_Comm_create(c, g, &new_g);
	new_comm = new_g;
	return result;
}
/// <summary>
/// Creates a new communicator using MPI_COMM_WORLD as the comm parameter
/// </summary>
/// <param name="comm">communicator (handle) </param>
/// <param name="group">group, which is a subset of the group of comm (handle) </param>
/// <param name="new_comm">new communicator (handle) </param>
/// <returns>error code</returns>
int mpi_comm_create__w( int group, int& new_comm) {
	MPI_Group g = group;
	MPI_Group new_g;
	int result = MPI_Comm_create(MPI_COMM_WORLD, g, &new_g);
	new_comm = new_g;
	return result;
}
// get the mpi_wtime
double mpi_wtime(){
	return MPI_Wtime();
}
#pragma endregion
////////////////////////////////////////////////////
// MPI_Send methods ////////////////////////////////
////////////////////////////////////////////////////
#pragma region MPI_Send
//reference
/*int mpi_send(const void *buf, int count, MPI_Datatype datatype, int dest, int tag){
	int a = MPI_Send(buf,count,datatype,dest,tag,MPI_COMM_WORLD);
	return a;
}*/
/*NOTE: be careful for blocking send requests*/
int mpi_send_char_s(char buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_int_s(int buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_INT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_float_s(float buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_FLOAT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_double_s(double buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_DOUBLE,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_short_s(short buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_SHORT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_long_s(long buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_LONG,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_ulong_s(unsigned long buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_UNSIGNED_LONG,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_uchar_s(unsigned char buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_UNSIGNED_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_schar_s(signed char buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_SIGNED_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_ushort_s(unsigned short buf, int count, int dest, int tag){
	return MPI_Send(&buf,count,MPI_UNSIGNED_SHORT,dest,tag,MPI_COMM_WORLD);
}
// arrays
int mpi_send_char_array_s(char buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_int_array_s(int buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_INT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_float_array_s(float buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_FLOAT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_double_array_s(double buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_DOUBLE,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_short_array_s(short buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_SHORT,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_long_array_s(long buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_LONG,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_ulong_array_s(unsigned long buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_UNSIGNED_LONG,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_uchar_array_s(unsigned char buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_UNSIGNED_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_schar_array(signed char buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_SIGNED_CHAR,dest,tag,MPI_COMM_WORLD);
}
int mpi_send_ushort_array(unsigned short buf[], int count, int dest, int tag){
	return MPI_Send(buf,count,MPI_UNSIGNED_SHORT,dest,tag,MPI_COMM_WORLD);
}
// General MPI_Send functions
int mpi_send_char(char buf, int count, int dest, int tag,int comm) {
	return MPI_Send(&buf, count, MPI_CHAR, dest, tag, (MPI_Comm)comm);
}
int mpi_send_int(int buf, int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_INT, dest, tag, (MPI_Comm)comm);
}
int mpi_send_float(float buf, int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_FLOAT, dest, tag, (MPI_Comm)comm);
}
int mpi_send_double(double buf, int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_DOUBLE, dest, tag, (MPI_Comm)comm);
}
int mpi_send_short(short buf, int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_SHORT, dest, tag, (MPI_Comm)comm);
}
int mpi_send_long(long buf, int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_LONG, dest, tag, (MPI_Comm)comm);
}
int mpi_send_ulong(unsigned long buf, int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_UNSIGNED_LONG, dest, tag, (MPI_Comm)comm);
}
int mpi_send_uchar(unsigned char buf, int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count,MPI_UNSIGNED_CHAR, dest, tag, (MPI_Comm)comm);
}
int mpi_send_schar(signed char buf, int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_SIGNED_CHAR, dest, tag, (MPI_Comm)comm);
}
int mpi_send_ushort(unsigned short buf, int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_UNSIGNED_SHORT, dest, tag, (MPI_Comm)comm);
}
// arrays
int mpi_send_char_array(char buf[], int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count,MPI_CHAR, dest, tag, (MPI_Comm)comm);
}
int mpi_send_int_array(int buf[], int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_INT, dest, tag, (MPI_Comm)comm);
}
int mpi_send_float_array(float buf[], int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_FLOAT, dest, tag, (MPI_Comm)comm);
}
int mpi_send_double_array(double buf[], int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_DOUBLE, dest, tag, (MPI_Comm)comm);
}
int mpi_send_short_array(short buf[], int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_SHORT, dest, tag, (MPI_Comm)comm);
}
int mpi_send_long_array(long buf[], int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_LONG, dest, tag, (MPI_Comm)comm);
}
int mpi_send_ulong_array(unsigned long buf[], int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_UNSIGNED_LONG, dest, tag, (MPI_Comm)comm);
}
int mpi_send_uchar_array(unsigned char buf[], int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count,MPI_UNSIGNED_CHAR, dest, tag, (MPI_Comm)comm);
}
int mpi_send_schar_array(signed char buf[], int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_SIGNED_CHAR, dest, tag, (MPI_Comm)comm);
}
int mpi_send_ushort_array(unsigned short buf[], int count, int dest, int tag, int comm) {
	return MPI_Send(&buf, count, MPI_UNSIGNED_SHORT, dest, tag, (MPI_Comm)comm);
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
#pragma endregion
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
void mpi_recv_populate_status(int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    int& status_MPI_TAG,int& status_MPI_ERROR,MPI_Status status){
		status_count_lo = 0;
		status_count_hi_and_cancelled = 0;
		status_MPI_SOURCE = status.MPI_SOURCE;
		status_MPI_TAG = status.MPI_TAG;
		status_MPI_ERROR = status.MPI_ERROR;
}
#pragma region MPI_Recv
	int mpi_recv_int_s(int& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
		int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_INT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_char_s(char& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
		int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_float_s(float& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
		int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_INT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_double_s(double& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
		int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_DOUBLE,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_short_s(short& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
		int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_SHORT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_long_s(long& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
		int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_LONG,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_uchar_s(unsigned char& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
		int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_UNSIGNED_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_schar_s(signed char& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
		int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_SIGNED_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_ushort_s(unsigned short& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
		int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_UNSIGNED_SHORT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_ulong_s(unsigned long& buf, int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
		int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(&buf,count,MPI_UNSIGNED_LONG,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_int_array_s(int buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_INT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_char_array_s(char buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}	
	int mpi_recv_double_array_s(double buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_DOUBLE,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_float_array_s(float buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_FLOAT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_short_array_s(short buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_SHORT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_long_array_s(long buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_LONG,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_uchar_array_s(unsigned char buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_UNSIGNED_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_schar_array_s(signed char buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_SIGNED_CHAR,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_ushort_array_s(unsigned short buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_UNSIGNED_SHORT,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	int mpi_recv_ulong_array_s(unsigned long buf[], int count, int source, int tag,int& status_count_lo,int& status_count_hi_and_cancelled,int& status_MPI_SOURCE,
    	int& status_MPI_TAG,int& status_MPI_ERROR){
		MPI_Status status;
		int a = MPI_Recv(buf,count,MPI_UNSIGNED_LONG,source,tag,MPI_COMM_WORLD,&status);
		mpi_recv_populate_status(status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR,status);
		return a;
	}
	//-break-//
	int mpi_recv_int(int& buf, int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(&buf, count, MPI_INT, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_char(char& buf, int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(&buf, count, MPI_CHAR, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_float(float& buf, int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(&buf, count, MPI_INT, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_double(double& buf, int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(&buf, count, MPI_DOUBLE, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_short(short& buf, int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(&buf, count, MPI_SHORT, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_long(long& buf, int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(&buf, count, MPI_LONG, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_uchar(unsigned char& buf, int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(&buf, count, MPI_UNSIGNED_CHAR, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_schar(signed char& buf, int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(&buf, count, MPI_SIGNED_CHAR, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_ushort(unsigned short& buf, int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(&buf, count, MPI_UNSIGNED_SHORT, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_ulong(unsigned long& buf, int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(&buf, count, MPI_UNSIGNED_LONG, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_int_array(int buf[], int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(buf, count, MPI_INT, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_char_array(char buf[], int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(buf, count, MPI_CHAR, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_double_array(double buf[], int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(buf, count, MPI_DOUBLE, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_float_array(float buf[], int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(buf, count, MPI_FLOAT, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_short_array(short buf[], int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(buf, count, MPI_SHORT, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_long_array(long buf[], int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(buf, count, MPI_LONG, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_uchar_array(unsigned char buf[], int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(buf, count, MPI_UNSIGNED_CHAR, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_schar_array(signed char buf[], int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(buf, count, MPI_SIGNED_CHAR, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_ushort_array(unsigned short buf[], int count, int source,  int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(buf, count, MPI_UNSIGNED_SHORT, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
	int mpi_recv_ulong_array(unsigned long buf[], int count, int source, int tag, int comm, int& status_count_lo, int& status_count_hi_and_cancelled, int& status_MPI_SOURCE,
		int& status_MPI_TAG, int& status_MPI_ERROR) {
		MPI_Status status;
		int a = MPI_Recv(buf, count, MPI_UNSIGNED_LONG, source, tag, (MPI_Comm)comm, &status);
		mpi_recv_populate_status(status_count_lo, status_count_hi_and_cancelled, status_MPI_SOURCE, status_MPI_TAG, status_MPI_ERROR, status);
		return a;
	}
#pragma endregion
////////////////////////////////////////////
/// MPI_Bcast //////////////////////////////
////////////////////////////////////////////
#pragma region MPI_Bcast
	/// <summary>
	/// Broadcasts a message from the process with rank "root" to all other processes of the communicator 
	/// </summary>
	/// <param name="buf">data buffer to be sent</param>
	/// <param name="count">number of entries in buffer (integer) </param>
	/// <param name="root">rank of broadcast root (integer) </param>
	/// <returns></returns>
	int mpi_bcast_char(char& buf, int count, int root) {
		return MPI_Bcast(&buf, count, MPI_CHAR,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_int(int& buf, int count, int root) {
		return MPI_Bcast(&buf, count, MPI_INT,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_float(float& buf, int count, int root) {
		return MPI_Bcast(&buf, count, MPI_FLOAT,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_double(double& buf, int count, int root) {
		return MPI_Bcast(&buf, count, MPI_DOUBLE,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_short(short& buf, int count, int root) {
		return MPI_Bcast(&buf, count, MPI_SHORT,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_long(long& buf, int count, int root) {
		return MPI_Bcast(&buf, count, MPI_LONG,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_ulong(unsigned long& buf, int count, int root) {
		return MPI_Bcast(&buf, count, MPI_UNSIGNED_LONG,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_uchar(unsigned char& buf, int count, int root) {
		return MPI_Bcast(&buf, count, MPI_UNSIGNED_CHAR,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_schar(signed char& buf, int count, int root) {
		return MPI_Bcast(&buf, count, MPI_SIGNED_CHAR,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_ushort(unsigned short& buf, int count, int root) {
		return MPI_Bcast(&buf, count, MPI_UNSIGNED_SHORT,root, MPI_COMM_WORLD);
	}
	// arrays
	int mpi_bcast_char_array(char buf[], int count, int root) {
		return MPI_Bcast(buf, count, MPI_CHAR,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_int_array(int buf[], int count, int root) {
		return MPI_Bcast(buf, count, MPI_INT,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_float_array(float buf[], int count, int root) {
		return MPI_Bcast(buf, count, MPI_FLOAT,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_double_array(double buf[], int count, int root) {
		return MPI_Bcast(buf, count, MPI_DOUBLE,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_short_array(short buf[], int count, int root) {
		return MPI_Bcast(buf, count, MPI_SHORT,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_long_array(long buf[], int count, int root) {
		return MPI_Bcast(buf, count, MPI_LONG,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_ulong_array(unsigned long buf[], int count, int root) {
		return MPI_Bcast(buf, count, MPI_UNSIGNED_LONG,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_uchar_array(unsigned char buf[], int count, int root) {
		return MPI_Bcast(buf, count, MPI_UNSIGNED_CHAR,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_schar_array(signed char buf[], int count, int root) {
		return MPI_Bcast(buf, count, MPI_SIGNED_CHAR,root, MPI_COMM_WORLD);
	}
	int mpi_bcast_ushort_array(unsigned short buf[], int count, int root) {
		return MPI_Bcast(buf, count, MPI_UNSIGNED_SHORT,root, MPI_COMM_WORLD);
	}
#pragma endregion
/////////////////////////////////////////////////////////////////////////////////////////////////
