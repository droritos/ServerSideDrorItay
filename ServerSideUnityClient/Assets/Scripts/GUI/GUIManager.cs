using System;
using Data;
using Scriptable_Objects;
using UnityEngine;

namespace GameGUI
{
    public class GUIManager : MonoBehaviour
    {
        [SerializeField] LoginUIManager loginUIManager;
        [SerializeField] ChatGUIHandler chatGUIHandler;
        
        [SerializeField] ServicesChannel  servicesChannel;

        private void Awake()
        {
            //servicesChannel.Subscribe(ServiceEventType.Login);
        }

        public void HandleMenuFlow()
        {
            
        }

        private void Start()
        {
            
        }
    }
}
