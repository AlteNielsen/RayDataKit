using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ray.Data
{
    public abstract class RayAbstractDataManager : MonoBehaviour
    {
        private float[] masterBuffer;
        private RayDataChunk[] dataInterfaces;
        private int[] offsets;

        private List<float[]> datalist = new();
        private bool RegisterSwitch;
        private int dICounter;

        public void GetData(int selector, float[] frame)
        {
            Array.Copy(masterBuffer, offsets[selector], frame, 0, frame.Length);
        }

        public void SetData(int selector, float[] values)
        {
            Array.Copy(values, 0, masterBuffer, offsets[selector], values.Length);
        }

        public T Access<T>(int key) where T : RayDataChunk
        {
            return (T)dataInterfaces[key];
        }

        protected void Save()
        {
            for(int i = 0; i < dataInterfaces.Length; i++)
            {
                dataInterfaces[i].Save();
            }
        }

        void Awake()
        {
            RegisterSwitch = false;
            StructRegistry();
            dataInterfaces = new RayDataChunk[datalist.Count];
            RegisterSwitch = true;
            StructRegistry();
            CompileData();
        }

        protected abstract void StructRegistry();

        protected void Register<T>() where T : RayDataChunk, new()
        {
            if(!RegisterSwitch)
            {
                datalist.Add(new T().Load());
            }
            else
            {
                dataInterfaces[dICounter] = new T();
                dICounter++;
            }
        }

        private void CompileData()
        {
            int[] lengths = new int[datalist.Count];
            for(int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = datalist[i].Length;
            }
            masterBuffer = new float[lengths.Sum()];
            Memory<float> masterMemory = masterBuffer.AsMemory();

            int currentOffset = 0;
            offsets = new int[lengths.Length];
            for(int i = 0; i < lengths.Length;  i++)
            {
                Array.Copy(datalist[i], 0, masterBuffer, currentOffset, lengths[i]);
                dataInterfaces[i].data = masterMemory.Slice(currentOffset, lengths[i]);
                offsets[i] = currentOffset;
                currentOffset += lengths[i];
            }
            datalist = null;
        }
    }

    public abstract class RayDataChunk
    {
        public ReadOnlyMemory<float> data { get; set; }

        public abstract void Save();
        public abstract float[] Load();
    }
}
