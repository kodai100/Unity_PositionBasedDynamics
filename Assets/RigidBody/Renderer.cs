using UnityEngine;

namespace Rigid {

    public class Renderer : MonoBehaviour {

        public RigidBody GPUScript;

        public Material ParticleRenderMat;

        void OnRenderObject(){
            DrawObject();
        }

        void DrawObject(){
            Material m = ParticleRenderMat;
            m.SetPass(0);
            m.SetBuffer("_Particles", GPUScript.GetBuffer());
            Graphics.DrawProcedural(MeshTopology.Points, GPUScript.GetMaxParticleNum());
        }

    }

}