#include <iostream>
#include "MpiLibrary.h"
#include <mpi/mpi.h>

//using namespace std;



int mpi_tests(int argc, char *argv[]){
    initialize_mpi(argc,argv);

    //Test rank and world size
    int world_size = get_world_size();
	int world_rank = get_world_rank();
    // print results
	std::cout << world_rank << ", " << world_size << std::endl;
    //test sum
//	int sum;
//	all_reduce_int(world_rank, sum);
//	std::cout << "Sum " << sum << std::endl;

    int tag = 0;
    int mpi_test_error = 0;
    //buffer tests
    int int_buffer = 12;
    char char_buffer = 'a';
    float float_buffer = 1.1f;
    double double_buffer = 23.5;
    short short_buffer = 234;
    long long_buffer = 454L;
    unsigned char uchar_buffer = (unsigned char)34;
    signed char schar_buffer = (signed char)3;
    unsigned short ushort_buffer = (unsigned short)84;
    unsigned long ulong_buffer = (unsigned) 837L;
    //array buffer
    int int_buffer_array[] = {12,2,5,7,654};
    char char_buffer_array[] = {'a','b','3','T'};
    float float_buffer_array[] = {1.1f,4.6f,3.6f,2.35f};
    double double_buffer_array[] = {23.5,4,5.6,3.7};
    short short_buffer_array[] = {234,2,3,5,66};
    long long_buffer_array[] = {454L,1L,3L,453L,123L,65L,7L};
    unsigned char uchar_buffer_array[] = {(unsigned char)'0','R','E','"'};
    signed char schar_buffer_array[] = {(signed char)3,4,3,3,1};
    unsigned short ushort_buffer_array[] = {(unsigned short)84,3,532,76,3};
    unsigned long ulong_buffer_array[] = {(unsigned) 837L,2L,7L,432L,};
    int array_size = 0;



	if(world_rank == 0)
    {
        // The "master" MPI process issues the MPI_Bsend.
#pragma region
      /*  //send int test
        printf("MPI process '%d' sending int value '%d' with tag '%d'.\n", world_rank, int_buffer, tag);
        mpi_send_int(int_buffer, 1, 1, tag);
        tag++;
        //Send char test
        printf("MPI process '%d' sending char value '%d' with tag '%d'.\n", world_rank, char_buffer, tag);
        mpi_send_char(char_buffer, 1, 1, tag);
        tag++;
        //Send float test
        printf("MPI process '%d' sending float value '%f' with tag '%d'.\n", world_rank, float_buffer, tag);
        mpi_send_float(float_buffer, 1, 1, tag);
        tag++;
        //Send double test
        printf("MPI process '%d' sending double value '%f' with tag '%d'.\n", world_rank, double_buffer, tag);
        mpi_send_double(double_buffer, 1, 1, tag);
        tag++;
        //Send short test
        printf("MPI process '%d' sending short value '%d' with tag '%d'.\n", world_rank, short_buffer, tag);
        mpi_send_short(short_buffer, 1, 1, tag);
        tag++;
        //Send long test
        printf("MPI process '%d' sending long value '%ld' with tag '%d'.\n", world_rank, long_buffer, tag);
        mpi_send_long(long_buffer, 1, 1, tag);
        tag++;
        //Send unsigned char test
        printf("MPI process '%d' sending unsigned char _array[]value '%u' with tag '%d'.\n", world_rank, uchar_buffer, tag);
        mpi_send_uchar(uchar_buffer, 1, 1, tag);
        tag++;*/
#pragma endregion
        //test array sending
         //send int test
        printf("MPI process [%d] sending int array [%d",world_rank,int_buffer_array[0]); 
        array_size = sizeof(int_buffer_array)/sizeof(int);
        for(int i = 1; i < array_size; i++)
            printf(",%d",int_buffer_array[i]);
        printf("] with tag '%d'.\n", tag);
        mpi_send_int_array(int_buffer_array,array_size, 1, tag);
        tag++;
        //Send char test  
        printf("MPI process [%d] sending char array [%c",world_rank,char_buffer_array[0]); 
        array_size = sizeof(char_buffer_array)/sizeof(char);
        for(int i = 1; i < array_size; i++)
            printf(",%c",char_buffer_array[i]);
        printf("] with tag '%d'.\n", tag);
        mpi_send_char_array(char_buffer_array,array_size, 1, tag);
        tag++;
        //Send float test 
        printf("MPI process [%d] sending float array [%f",world_rank,float_buffer_array[0]); 
        array_size = sizeof(float_buffer_array)/sizeof(float);
        for(int i = 1; i < array_size; i++)
            printf(",%f",float_buffer_array[i]);
        printf("] with tag '%d'.\n", tag);
        mpi_send_float_array(float_buffer_array, array_size, 1, tag);
        tag++;
        /*
        //Send double test
        printf("MPI process [%d] sending double array [%f",world_rank,double_buffer_array[0]); 
        array_size = sizeof(double_buffer_array)/sizeof(double);
        for(int i = 1; i < array_size; i++)
            printf(",%f",double_buffer_array[i]);
        printf("] with tag '%d'.\n", tag);
        mpi_send_double_array(double_buffer_array, array_size, 1, tag);
        tag++;
        //Send short test
        printf("MPI process [%d] sending short array [%d",world_rank,short_buffer_array[0]); 
        array_size = sizeof(short_buffer_array)/sizeof(short);
        for(int i = 1; i < array_size; i++)
            printf(",%d",short_buffer_array[i]);
        printf("] with tag '%d'.\n", tag);
        mpi_send_short_array(short_buffer_array, array_size, 1, tag);
        tag++;
        //Send long test
        printf("MPI process [%d] sending long array [%ld",world_rank,long_buffer_array[0]); 
        array_size = sizeof(long_buffer_array)/sizeof(long);
        for(int i = 1; i < array_size; i++)
            printf(",%ld",long_buffer_array[i]);
        printf("] with tag '%d'.\n", tag);
        mpi_send_long_array(long_buffer_array, array_size, 1, tag);
        tag++;
        //Send unsigned char test
        printf("MPI process [%d] sending unsigned char array [%c",world_rank,uchar_buffer_array[0]); 
        array_size = sizeof(uchar_buffer_array)/sizeof(unsigned char);
        for(int i = 1; i < array_size; i++)
            printf(",%c",uchar_buffer_array[i]);
        printf("] with tag '%d'.\n", tag);
        mpi_send_uchar_array(uchar_buffer_array,array_size, 1, tag);
        tag++;*/


    }
    else
    {
        // The "slave" MPI process receives the message.
		int tmp_int_buffer;
        char tmp_char_buffer;
        float tmp_float_buffer;
        double tmp_double_buffer;
        short tmp_short_buffer;
        long tmp_long_buffer;
        unsigned char tmp_uchar_buffer;
        signed char tmp_schar_buffer;
        unsigned short tmp_ushort_buffer;
        unsigned long tmp_ulong_buffer;
        int status_count_hi_and_cancelled,status_MPI_ERROR,status_MPI_SOURCE,status_MPI_TAG,status_count_lo;
#pragma region
      /*  // test received buffer arraysgma region 
        printf("waiting on int message...\n");
        mpi_recv_int(tmp_int_buffer, 1, 0, tag,status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,
            status_MPI_TAG,status_MPI_ERROR);
        printf("got int '%d' from source '%d' with tag '%d' with error '%d'\n",tmp_int_buffer,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR);
        tag++;
        printf("waiting on char message...\n");
        mpi_recv_char(tmp_char_buffer, 1, 0, tag,status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,
            status_MPI_TAG,status_MPI_ERROR);
        printf("got char '%d' from source '%d' with tag '%d' with error '%d'\n",tmp_char_buffer,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR);
        tag++;
        printf("waiting on float message...\n");
        mpi_recv_float(tmp_float_buffer, 1, 0, tag,status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,
            status_MPI_TAG,status_MPI_ERROR);
        printf("got float '%f' from source '%d' with tag '%d' with error '%d'\n",tmp_float_buffer,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR);
        tag++;
        printf("waiting on double message...\n");
        mpi_recv_double(tmp_double_buffer, 1, 0, tag,status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,
            status_MPI_TAG,status_MPI_ERROR);
        printf("got double '%f' from source '%d' with tag '%d' with error '%d'\n",tmp_double_buffer,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR);
        tag++;
        printf("waiting on short message...\n");
        mpi_recv_short(tmp_short_buffer, 1, 0, tag,status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,
            status_MPI_TAG,status_MPI_ERROR);
        printf("got short '%d' from source '%d' with tag '%d' with error '%d'\n",tmp_short_buffer,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR);
        tag++;
        printf("waiting on long message...\n");
        mpi_recv_long(tmp_long_buffer, 1, 0, tag,status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,
            status_MPI_TAG,status_MPI_ERROR);
        printf("got long '%ld' from source '%d' with tag '%d' with error '%d'\n",tmp_long_buffer,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR);
        tag++;
        printf("waiting on uchar message...\n");
        mpi_recv_uchar(tmp_uchar_buffer, 1, 0, tag,status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,
            status_MPI_TAG,status_MPI_ERROR);
        printf("got long '%ld' from source '%d' with tag '%d' with error '%d'\n",tmp_long_buffer,status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR);
        tag++;*/
#pragma endregion
////////////////////////////////////
// test received buffer arrays /////
////////////////////////////////////
#pragma region 
        // int * tmp_int_buffer_array = (int *)malloc(sizeof(int) * 5);
        // or
        array_size = sizeof(int_buffer_array)/sizeof(int); 
        int tmp_int_buffer_array[array_size];
        printf("waiting on int array message...\n");
        mpi_recv_int_array(tmp_int_buffer_array,array_size, 0, tag,status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,
            status_MPI_TAG,status_MPI_ERROR);
        printf("got int array [%d",tmp_int_buffer_array[0]);
        for(int i = 1; i < array_size; i++){
            printf(",%d",tmp_int_buffer_array[i]);
        }
        printf("] from source '%d' with tag '%d' with error '%d'\n",status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR);
        tag++;
        // get char array
        array_size = sizeof(char_buffer_array)/sizeof(char); 
        char tmp_char_buffer_array[array_size];
        printf("waiting on char array message...\n");
        mpi_recv_char_array(tmp_char_buffer_array,array_size, 0, tag,status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,
            status_MPI_TAG,status_MPI_ERROR);
        printf("got char array [%c",tmp_char_buffer_array[0]);
        for(int i = 1; i < array_size; i++){
            printf(",%c",tmp_char_buffer_array[i]);
        }
        printf("] from source '%d' with tag '%d' with error '%d'\n",status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR);
        tag++;
        // get float array
        array_size = sizeof(float_buffer_array)/sizeof(float); 
        float tmp_float_buffer_array[array_size];
        printf("waiting on float array message...\n");
        mpi_recv_float_array(tmp_float_buffer_array,array_size, 0, tag,status_count_lo,status_count_hi_and_cancelled,status_MPI_SOURCE,
            status_MPI_TAG,status_MPI_ERROR);
        printf("got float array [%f",tmp_float_buffer_array[0]);
        for(int i = 1; i < array_size; i++){
            printf(",%f",tmp_float_buffer_array[i]);
        }
        printf("] from source '%d' with tag '%d' with error '%d'\n",status_MPI_SOURCE,status_MPI_TAG,status_MPI_ERROR);
        tag++;
#pragma endregion
    }





    // finish tests
    finalize_mpi();
    printf("rank %d is done with error '%d'\n",world_rank,mpi_test_error);
    return mpi_test_error;
}

int main(int argc, char *argv[])
{
    return mpi_tests(argc,argv); 
}

