using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ray.Data.SO
{
    public abstract class RayAbstractSOManager : MonoBehaviour
    {
        private ScriptableObject[] masterBuffer;
        private RaySOChunk[] dataInterfaces;
        private int[] offsets;

        private List<ScriptableObject[]> datalist = new();
        private bool RegisterSwitch;
        private int dICounter;

        public T Access<T>(int key) where T : RaySOChunk
        {
            return (T)dataInterfaces[key];
        }

        void Awake()
        {
            RegisterSwitch = false;
            StructRegistry();
            dataInterfaces = new RaySOChunk[datalist.Count];
            RegisterSwitch = true;
            StructRegistry();
            CompileData();
        }

        protected abstract void StructRegistry();

        protected void Register<T>() where T : RaySOChunk, new()
        {
            if (!RegisterSwitch)
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
            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = datalist[i].Length;
            }
            masterBuffer = new ScriptableObject[lengths.Sum()];
            Memory<ScriptableObject> masterMemory = masterBuffer.AsMemory();

            int currentOffset = 0;
            offsets = new int[lengths.Length];
            for (int i = 0; i < lengths.Length; i++)
            {
                Array.Copy(datalist[i], 0, masterBuffer, currentOffset, lengths[i]);
                dataInterfaces[i].data = masterMemory.Slice(currentOffset, lengths[i]);
                offsets[i] = currentOffset;
                currentOffset += lengths[i];
            }
            datalist = null;
        }
    }

    public abstract class RaySOChunk
    {
        public ReadOnlyMemory<ScriptableObject> data { get; set; }

        public abstract ScriptableObject[] Load();
    }
}
