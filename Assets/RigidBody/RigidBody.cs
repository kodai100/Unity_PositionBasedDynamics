using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Rigid {

    [RequireComponent(typeof(Renderer))]
    public class RigidBody : MonoBehaviour {

        public float dt = 0.005f;

        #region GPU
        public ComputeShader cs;
        ComputeBuffer bufferRead;
        ComputeBuffer bufferWrite;
        static int SIMULATION_BLOCK_SIZE = 32;
        int threadGroupSize;
        #endregion GPU

        List<Particle> particles = new List<Particle>();
        Particle[] particle_array;
        int maxParticleNum;

        public bool random_start;
        public Vector3 range;
        public Vector3 gravity;
        public int iter = 4;

        void Start() {
            MakeBodies();
            InitializeBuffer();
        }

        void Update() {

            cs.SetInt("_MaxParticleNum", maxParticleNum);
            cs.SetFloat("_DT", dt);
            cs.SetVector("_Gravity", gravity);
            cs.SetVector("_Range", range);

            int kernel;

            kernel = cs.FindKernel("Update");
            cs.SetBuffer(kernel, "bufferRead", bufferRead);
            cs.SetBuffer(kernel, "bufferWrite", bufferWrite);
            cs.Dispatch(kernel, threadGroupSize, 1, 1);

            for (int i = 0; i < iter; i++) {
                kernel = cs.FindKernel("Constraint");
                cs.SetBuffer(kernel, "bufferRead", bufferRead);
                cs.SetBuffer(kernel, "bufferWrite", bufferWrite);
                cs.Dispatch(kernel, threadGroupSize, 1, 1);
            }

            SwapBuffer();
        }

        void OnDestroy() {
            if(bufferRead != null) {
                bufferRead.Release();
                bufferRead = null;
            }
            if(bufferWrite != null) {
                bufferWrite.Release();
                bufferWrite = null;
            }
        }

        void OnDrawGizmos() {
            // Gizmos.DrawWireCube(range / 2, range);
        }

        void MakeBodies() {
            if (!random_start) {
                for (int i = 1; i < 9; i++) {
                    for (int j = 1; j < 9; j++) {
                        for (int k = 1; k < 9; k++) {
                            particles.Add(new Particle(new Vector3(i, j, k)));
                        }
                    }
                }
            } else {
                for (int i = 0; i < (range.x-1)* (range.x - 1)* (range.x - 1); i++) {
                    particles.Add(new Particle(new Vector3(Random.value * (float)range.x, Random.value * (float)range.y, Random.value * (float)range.z)));
                }
            }

            particle_array = particles.ToArray();
            maxParticleNum = particle_array.Length;
        }

        void InitializeBuffer() {
            bufferRead = new ComputeBuffer(maxParticleNum, Marshal.SizeOf(typeof(Particle)));
            bufferWrite = new ComputeBuffer(maxParticleNum, Marshal.SizeOf(typeof(Particle)));
            bufferRead.SetData(particle_array);
            bufferWrite.SetData(particle_array);
            threadGroupSize = Mathf.CeilToInt(maxParticleNum / SIMULATION_BLOCK_SIZE) + 1;
        }

        void SwapBuffer() {
            ComputeBuffer tmp = bufferRead;
            bufferRead = bufferWrite;
            bufferWrite = tmp;
        }

        public ComputeBuffer GetBuffer() {
            return bufferRead;
        }

        public int GetMaxParticleNum() {
            return maxParticleNum;
        }

    }

    struct Particle {
        Vector3 oldPos;
        Vector3 newPos;

        public Particle(Vector3 pos) {
            this.oldPos = pos;
            this.newPos = pos;
        }
    }

}