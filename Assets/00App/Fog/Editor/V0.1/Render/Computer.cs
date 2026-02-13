using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Texture3DBaker
{
    public class Computer
    {
        protected int[] m_compArg;
        public ComputeBuffer m_currentResultBuffer;
        protected bool m_bussy;
        protected IRenderComputeInit m_computeInit;
        public int m_kernelHandle;
        public DetailSettings m_currentDetail;
        public TextureBakerSettings m_editorSettings;
        protected Color[] m_result;
        public ComputeShader m_compute;
        public string m_kernel;

        public Computer(TextureBakerSettings editorSettings, ComputeShader compute, string kernel)
        {
            m_editorSettings = editorSettings;
            m_compute = compute;
            m_kernel = kernel;
            Init();
        }

        public int[] compArg
        {
            get { return m_compArg; }
            set {
                m_compArg = value; 
            }
        }

        protected virtual void Init()
        {
        }

        public virtual void Run()
        {
            if (!m_bussy)
            {
                m_bussy = true;
                m_kernelHandle = m_compute.FindKernel(m_kernel);
                ComputeInit();
                Dispatch();
            }
        }

        protected virtual void ComputeInit()
        {
            m_currentDetail = m_editorSettings.SelectedDetail;
            m_computeInit.Run(this);
            m_compute.SetBuffer(m_kernelHandle, "result", m_currentResultBuffer);
        }

        protected virtual void Dispatch()
        {
            m_compute.Dispatch(m_kernelHandle, m_compArg[0], m_compArg[1], m_compArg[2]);
            AsyncGPUReadback.Request(m_currentResultBuffer, ResultBufferCallback);
        }

        void ResultBufferCallback(AsyncGPUReadbackRequest readbackRequest)
        {
            m_bussy = false;
            if (!readbackRequest.hasError)
            {
                m_result = readbackRequest.GetData<Color>().ToArray();
                ReadyAction(readbackRequest);
            }
        }

        protected virtual void ReadyAction(AsyncGPUReadbackRequest readbackRequest)
        {

        }

        public virtual void Release()
        {
            if (m_currentResultBuffer != null)
                m_currentResultBuffer.Release();
        }
    }
}
