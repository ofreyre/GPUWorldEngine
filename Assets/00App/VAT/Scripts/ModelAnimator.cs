using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimatedModel
{

    public class ModelAnimator : MonoBehaviour
    {
        public enum STATE
        {
            idle,
            walk,
            attack
        }

        public ModelAnimatorController m_controller;

        public STATE m_state;
        public STATE m_nextState;
        int i = 0;

        [SerializeField]List<MeshRenderer> m_renderers = new List<MeshRenderer>();

        public STATE nextState {
            set { 
                m_nextState = value;
                enabled = true;
                i = 0;
            }
        }

        public void Awake()
        {
            m_controller.SetClip(0, m_renderers);
        }

        private void Update()
        {
            i++;
            if (i > 1)
            {
                if (m_nextState != m_state)
                {
                    m_state = m_nextState;
                    m_controller.SetClip((int)m_state, m_renderers);
                }
                enabled = false;
            }
        }
    }
}
