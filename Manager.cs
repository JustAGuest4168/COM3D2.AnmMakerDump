﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.AnmMaker.Plugin.Core
{
    public class Manager : MonoBehaviour
    {
        public bool Initialized { get; private set; }
        public void Initialize()
        {
            //Copied from examples
            if (this.Initialized)
                return;

            Hooks.Initialize();
            this.Initialized = true;
            UnityEngine.Debug.Log("Anm Maker: Manager Initialize");
        }

        public void Awake()
        {
            //Copied from examples
            UnityEngine.Debug.Log("Anm Maker: Manager Awake");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);
        }
    }
}