using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using System.Collections.Generic;
using MpiLibrary;

public class MPICommunicationComponent : MonoBehaviour,IConvertGameObjectToEntity{
        public int Message;
        void Start(){}

        public void Convert(Entity e,EntityManager dstManager,GameObjectConversionSystem conversionSystem){
            dstManager.AddComponentData(e,new UnityMpi.Tests.MPI_Message_Data{message = Message});
        }
    }
namespace UnityMpi.Tests{
    
    public struct MPI_Message_Data : IComponentData{
        public int message;
    }
}