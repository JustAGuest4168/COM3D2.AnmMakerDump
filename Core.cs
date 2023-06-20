using BepInEx;
using COM3D2API;
using MaidStatus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.AnmMaker.Plugin.Core
{
    [BepInPlugin("org.guest4168.plugins.anmmakerplugin", "Plug-In", "1.0.0.0")]
    public class AnmMaker : BaseUnityPlugin
    {
        private UnityEngine.GameObject managerObject;

        public void Awake()
        {
            //Copied from examples
            UnityEngine.Debug.Log("Anm Maker: Core Awake");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);

            this.managerObject = new UnityEngine.GameObject("anmMakerManager");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this.managerObject);
            this.managerObject.AddComponent<Manager>().Initialize();

            SceneManager.sceneLoaded += SceneLoaded;
        }

        //Data
        private static string pathAnm = UTY.gameProjectPath + "\\Mod\\[AnmMaker]\\[Anm]";
        private static string pathPrj = UTY.gameProjectPath + "\\Mod\\[AnmMaker]\\[Prj]";
        private bool buttonAdded = false;
        private bool inPhotoMode = false;

        public void SceneLoaded(Scene s, LoadSceneMode lsm)
        {
            inPhotoMode = false;

            //Add the button
            if (GameMain.Instance != null && s != null && s.name.Equals("SceneTitle") && GameMain.Instance.SysShortcut != null && !buttonAdded)
            {
                SystemShortcutAPI.AddButton("AnmMaker", OnMenuButtonClickCallback, "Make Animations", GearIcon);
                buttonAdded = true;
            }

            //Special boolean for displaying info message in edit mode
            if (GameMain.Instance != null && s.name.Equals("ScenePhotoMode"))
            {
                inPhotoMode = true;
                displayUI = false;
                displayPage = 0;
            }
        }

        private void OnMenuButtonClickCallback()
        {
            if (!System.IO.Directory.Exists(pathAnm))
            {
                System.IO.Directory.CreateDirectory(pathAnm);
            }
            if (!System.IO.Directory.Exists(pathPrj))
            {
                System.IO.Directory.CreateDirectory(pathPrj);
            }

            infoMessage = "";

            if (inPhotoMode)
            {
                //Toggle UI
                displayUI = !displayUI;
            }
            else
            {
                displayUI = false;
            }
        }

        public static byte[] GearIcon
        {
            get
            {
                if (png == null)
                {
                    png = Convert.FromBase64String(gearIconBase64);
                }
                return png;
            }
        }

        static byte[] png = null;

        private static string gearIconBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABl0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC4xNkRpr/UAAAO2SURBVFhH7ZdnyM1RHMcfe0TZeyRJXhghu7w0XtgrsiVlz2RmUyQRInukJCJlpmRlZCY7IiJk7/X58D/6u557e3CVF7716Z57zv/87+/5rXOejP9KUDbIE33mgDXwOWIb5ATX8kafaZU/WBZGQEdoALfhOlyDR9AQOsFMqAPuSYuyQw2YBZfhOVyCJzAcBsFjuAKuvYDd0Bjcm0yFYCX0/vothXRtffDh5TAGNkIHKA0lo/EmGAWL4H00zgWZqQCsAsP3FsZCbkiq6nAU9kN5KAp65FyE42JgmHaC3jBMmckcmQufoDsMhfugQb73J/nSlnAHdoAGHAAtfxDh2Dmf3QLmxzSoCYkyZK9hJGiM6guG1PeXcyKuD2BcT0Nb6AEfYR34sDh2zrVWcBLc4964WoNufwlLID8ow+yaRhyCivBdbjDb+0FxmA7GuDMEOXbONd3YE66Ce4MMo++xfLvCQzBZK0FQLbgAhrWgE8q/RIud1ANmbTIPuNYGzoB73Kt8sfv1okmr9NRdOAXVnEC+y7I210o5oZwcBjfBhqN7TEjjbvKIY+cqwGa4BZaoz1aOvp8F1+MyUU1Y1xqBHtH4EJrvqgcXQQPKQKiC8xGOnbMsN4CubgLKxNL4ZFWhkSfA+Bsy+8cPspataWO8AEaDNd8edFMJaBfNjYc58A6mwEKwSenuZLIn2FesjC5OJMpuplV7wJjGO+EQ6A+2Yuf8sVdwBGxc98CcSCXb+1MYAEmbkX29Luhq+7193/o3YWzP4SwwSX1mNmjsBEgljdNbU8FSTKlw0ukRDVoPxkzCaeiaSfcGnM8HyWRe3YAVX7+lUXZKSysYl5n0pp47DkWcSKc85fZBMCDRCD1k6VluVlTaZQ0fBA8bf9wY+2l4xFrX9Xrhr0iXejDFPWBX9HMpWBnN4K+pMOwFf9D2rCcszWdg620BykT+o6ubR+92sInEpZsHwzKYARPBM6I51IagEJLflp0uuNjEiivUtGXrOO2XVGXz8UjVAFt0kFc2//pu4C3HJqM8iJy3Lc8Hm5UemAzOh7MjSzKO/rA3GZuOsQ5aC65thcXReCBUicbeliZF4zhWRpa1C9yke8PL+oDSAEsvyFuR4QoGeLFRx+Dwt2HGOIjvSSmzPFgdx+NaaUC4iCjX5kEwoBco7w/+Icq7g2tZktcuH/ZS6WVVwn9JVSGEIMix979ggCeo0mC9oH7JAB9MjFdTcH41ZNUAe4L3PxXCmBZ5AsZPQcfh+p04n/j9X1JGxhcfKhgHSlQ8rAAAAABJRU5ErkJggg==";

        //Data
        private static string infoMessage = "";

        //1
        private Vector2 page1_scroll_man;
        private static Maid baseMaidMan;

        //5
        private Vector2 page5_scroll_maid;
        private static Maid recordingMaid;
        private static Dictionary<MPN, Dictionary<string, float>> recordMaidBaseSks;

        //6
        private Vector2 page6_scroll_projFiles;
        private static List<String> projFiles = new List<string>();

        private static string anmAnimationSearch;
        private static string anmAnimationFilter; 
        private static Vector2 anmAnimationMotionsScroll; 
        private static string anmAnimationSelectedName;
        private static string anmAnimationTimeText = "0.00";
        private static string timeStepText = "0";
        private static bool timeStepPressed = false;
        private static List<float> autoFrames = new List<float>();
        private static bool autoPoseNext = false;
        private static bool autoSnapNext = false;
        private static NewAnm newAnm;
        private static string anmFile = "";
        private static bool captureBones = false;
        private static bool captureSks = false;

        //7
        private static NewAnmFrame newAnmCurrentFrame = null;
        private static string frameTimeText = null;
        private static float frameTimeCached = -1f;

        //GUI
        private static bool displayUI = false;
        private static int iconWidth = 80;
        private Rect uiWindow = new Rect((float)(Screen.width * 3.5 / 5), 120, 120, 50);
        
        private Vector2 page6_scroll_frames;
        private static int displayPage = 0;

        private void Update()
        {
            //Automatic frame capturing
            {
                //Order is important, must check snap before pose to ensure pose is triggered on separate frame from snap on first Update
                if (autoSnapNext)
                {
                    autoSnapNext = false;

                    //Record the Maid's data
                    NewAnmFrame newFrame = new NewAnmFrame();
                    newFrame.timestamp = DateTime.Now.ToString("yy-MM-dd HH:mm:ss:f");
                    newFrame.frameTime = autoFrames[0];
                    newFrame.RecordBones(new CacheBoneDataArray.BoneData(recordingMaid.body0.GetBone("Bip01"), null));

                    //Add to the list
                    newAnm.frames.Add(newFrame);

                    //Remove the autoframe
                    autoFrames.RemoveAt(0);

                    //Check if another frame waits
                    if (autoFrames.Count > 0)
                    {
                        autoPoseNext = true;
                    }
                }

                if (autoPoseNext)
                {
                    autoPoseNext = false;

                    //Determine who the auto frames are from
                    Maid autoFrameMaidOrMan = (baseMaidMan != null) ? baseMaidMan : recordingMaid;
                    //String anmStateName = (baseMaidMan != null) ? anmStateNameMan : anmStateNameMaid;
                    String anmStateName = anmAnimationSelectedName;

                    //Find the animation in the cache
                    Animation anim = autoFrameMaidOrMan.GetAnimation();
                    if (anim != null)
                    {
                        foreach (AnimationState anmState in anim)
                        {
                            //Match on state name
                            if (anmState.name.Equals(anmStateName))
                            {
                                //Set the time according to the frame data
                                anmState.time = autoFrames[0];

                                //Sample
                                anmState.enabled = true;
                                anim.Sample();
                                anmState.enabled = false;

                                //Tell next Update to take a snapshot
                                autoSnapNext = true;
                                break;
                            }
                        }
                    }
                }

                if(timeStepPressed)
                {
                    timeStepPressed = false;

                    //Prep data
                    newAnm.frames.Clear();
                    autoFrames = new List<float>();

                    //Determine who the time step frames are from
                    Maid autoFrameMaidOrMan = (baseMaidMan != null) ? baseMaidMan : recordingMaid;
                    //String anmStateName = (baseMaidMan != null) ? anmStateNameMan : anmStateNameMaid;
                    String anmStateName = anmAnimationSelectedName;

                    //Find the animation in the cache
                    Animation anim = autoFrameMaidOrMan.GetAnimation();
                    if (anim != null)
                    {
                        foreach (AnimationState anmState in anim)
                        {
                            //Match on state name
                            if (anmState.name.Equals(anmStateName))
                            {
                                //Build a list of frame times to record
                                int frame = 0;
                                bool keepGoing = true;
                                while (keepGoing)
                                {
                                    float nextFrameTime = Math.Min(anmState.length, anmState.time + (frame * float.Parse(timeStepText)));
                                    autoFrames.Add(nextFrameTime);
                                    frame = frame + 1;
                                    keepGoing = (nextFrameTime < anmState.length);
                                }
                                autoPoseNext = true;
                                break;
                            }
                        }
                    }
                }
            }

            if(captureBones || captureSks)
            {
                captureBones = false;
                captureSks = false;

                //Record the Maid's data
                NewAnmFrame newFrame = new NewAnmFrame();
                newFrame.timestamp = DateTime.Now.ToString("yy-MM-dd HH:mm:ss:f");
                newFrame.frameTime = newAnm.frames.Count == 0 ? 0.0f : (newAnm.frames[newAnm.frames.Count - 1].frameTime + 0.1f);

                if (captureBones)
                {
                    newFrame.RecordBones(new CacheBoneDataArray.BoneData(recordingMaid.body0.GetBone("Bip01"), null));
                }
                if(captureSks)
                {
                    newFrame.RecordSks(recordingMaid, recordMaidBaseSks);
                }

                //Add to the list
                newAnm.frames.Add(newFrame);
            }
        }

        private AnimationState getAnimState(Animation anim, string anmStateName)
        {
            if (anim != null && anmStateName != null && !anmStateName.Equals(""))
            {
                foreach (AnimationState anmState in anim)
                {
                    //Match on state name
                    if (anmState.name.Equals(anmStateName))
                    {
                        return anmState;
                    }
                }
            }
            return null;
        }
        private void OnGUI()
        {
            if (displayUI)
            {
                uiWindow = GUILayout.Window(416807, uiWindow, DisplayUIWindow, "Animation Maker", GUILayout.Width((iconWidth * 5) + 35), GUILayout.Height(Screen.height * 2 / 3 - 40));
            }
        }
        private void DisplayUIWindow(int windowId)
        {
            GUILayout.BeginVertical("box");
            {
                switch (displayPage)
                {
                    case 0:
                        {
                            GUILayout.Label("INFO:");
                            GUILayout.Label(infoMessage);

                            GUILayout.Label("Animation Type");

                            if (GUILayout.Button("XTMS Transfer"))
                            {
                                displayPage = 1;
                            }
                            if (GUILayout.Button("Custom Creator"))
                            {
                                displayPage = 5;
                            }
                            
                            break;
                        }
                    case 1:
                        {
                            GUILayout.Label("INFO:");
                            GUILayout.Label(infoMessage);

                            GUILayout.Label("Select Linked Man");
                            page1_scroll_man = GUILayout.BeginScrollView(page1_scroll_man, false, true, GUILayout.Width((iconWidth * 5) + 20), GUILayout.Height(Screen.height * 2 / 3 - 60));
                            {
                                //Loop available men
                                for (int i = 0; i < GameMain.Instance.CharacterMgr.GetManCount(); i++)
                                {
                                    Maid nextMaid = GameMain.Instance.CharacterMgr.GetMan(i);
                                    if (nextMaid != null && nextMaid.isActiveAndEnabled)
                                    {
                                        //Draw a button
                                        if (GUILayout.Button(new GUIContent(nextMaid.GetThumIcon()), GUILayout.Width(iconWidth), GUILayout.Height(iconWidth)))
                                        {
                                            baseMaidMan = nextMaid;
                                            displayPage = 5;
                                            newAnm = new NewAnm();
                                        }
                                    }
                                }
                            }
                            GUILayout.EndScrollView();

                            if (GUILayout.Button("Back"))
                            {
                                baseMaidMan = null;
                                displayPage = 0;
                            }
                            break;
                        }
                    case 2:
                        {
                            break;
                        }
                    case 3:
                        {
                            break;
                        }
                    case 4:
                        {
                            break;
                        }
                    case 5:
                        {
                            GUILayout.Label("INFO:");
                            GUILayout.Label(infoMessage);

                            GUILayout.Label("Select Maid to Record");
                            page5_scroll_maid = GUILayout.BeginScrollView(page5_scroll_maid, false, true, GUILayout.Width((iconWidth * 5) + 20), GUILayout.Height(Screen.height * 2 / 3 - 60));
                            {
                                //Loop available maids
                                for(int i=0; i< GameMain.Instance.CharacterMgr.GetMaidCount(); i++)
                                {
                                    Maid nextMaid = GameMain.Instance.CharacterMgr.GetMaid(i);
                                    if (nextMaid != null)
                                    {
                                        //string[] maidNameSplit = nextMaid.Parts.name.Split(']');
                                        //string maidName = maidNameSplit[maidNameSplit.Length - 1];

                                        //Draw a button
                                        if (GUILayout.Button(new GUIContent(nextMaid.GetThumIcon()), GUILayout.Width(iconWidth), GUILayout.Height(iconWidth)))
                                        {
                                            recordingMaid = nextMaid;
                                            displayPage = 6;
                                            projFiles = new List<String>();
                                            projFiles.AddRange(Directory.GetFiles(pathPrj));
                                            foreach(string anmFilePathTemp in Directory.GetFiles(pathAnm))
                                            {
                                                PhotoMotionData.AddMyPose(anmFilePathTemp);
                                            }
                                            newAnm = new NewAnm();
                                        }
                                    }
                                }
                            }
                            GUILayout.EndScrollView();

                            if (GUILayout.Button("Back"))
                            {
                                recordingMaid = null;
                                displayPage = 0;

                                if(baseMaidMan != null)
                                {
                                    baseMaidMan = null;
                                    displayPage = 1;
                                }
                            }
                            break;
                        }
                    case 6:
                        {
                            GUILayout.Label("INFO:");
                            GUILayout.Label(infoMessage);

                            GUILayout.Label("Load:");
                            page6_scroll_projFiles = GUILayout.BeginScrollView(page6_scroll_projFiles, false, true, GUILayout.Width((iconWidth * 5) + 20), GUILayout.Height(Screen.height * 1 / 16));
                            {
                                for (int i = 0; i < projFiles.Count; i++)
                                {
                                    string nextProjFilePath = projFiles[i];
                                    string[] splitProjFilePath = nextProjFilePath.Split('\\');
                                    if (GUILayout.Button(splitProjFilePath[splitProjFilePath.Length - 1]))
                                    {
                                        newAnm = Newtonsoft.Json.JsonConvert.DeserializeObject<NewAnm>(System.IO.File.ReadAllText(nextProjFilePath));
                                        anmFile = Path.Combine(pathAnm, (newAnm.name) + ".anm");
                                    }
                                }
                            }
                            GUILayout.EndScrollView();
                            
                            GUILayout.Label("Name:");
                            newAnm.name = GUILayout.TextField(newAnm.name);

                            //Man
                            if (baseMaidMan != null)
                            {
                                GUILayout.BeginVertical();
                                {
                                    GUILayout.Label("Man:");
                                    UIHelper.UI_5_DrawMotionControls(baseMaidMan, ref anmAnimationSearch, ref anmAnimationFilter, ref anmAnimationMotionsScroll, ref anmAnimationSelectedName, ref anmAnimationTimeText, ref timeStepText, ref timeStepPressed, ref autoFrames, ref autoPoseNext, ref newAnm, ref infoMessage, iconWidth);
                                }
                                GUILayout.EndVertical();
                            }

                            //Maid
                            if (baseMaidMan == null && recordingMaid != null)
                            {
                                GUILayout.BeginVertical();
                                {
                                    GUILayout.Label("Maid:");
                                    UIHelper.UI_5_DrawMotionControls(recordingMaid, ref anmAnimationSearch, ref anmAnimationFilter, ref anmAnimationMotionsScroll, ref anmAnimationSelectedName, ref anmAnimationTimeText, ref timeStepText, ref timeStepPressed, ref autoFrames, ref autoPoseNext, ref newAnm, ref infoMessage, iconWidth);
                                }
                                GUILayout.EndVertical();
                            }

                            GUILayout.Label("Capture:");
                            GUILayout.BeginHorizontal();
                            {
                                //schedule_icon_gravure.tex
                                if (GUILayout.Button("Bones")) //|| autoSnapNow)
                                {
                                    captureBones = true;
                                }
                                if(GUILayout.Button("ShapeKeys"))
                                {
                                    captureSks = true;
                                }
                                if(GUILayout.Button("All"))
                                {
                                    captureBones = true;
                                    captureSks = true;
                                }
                            }
                            page6_scroll_frames = GUILayout.BeginScrollView(page6_scroll_frames, false, true, GUILayout.Width((iconWidth * 5) + 20), GUILayout.Height(Screen.height * 1/8));
                            {
                                //Loop the frames made
                                int frameIndex = -1;
                                int frameButtonPress = -1;

                                //Sort the frames first
                                newAnm.frames.Sort(delegate (NewAnmFrame x, NewAnmFrame y)
                                {
                                    return x.frameTime.CompareTo(y.frameTime);
                                });
                                for (int i = 0; i<newAnm.frames.Count; i++)
                                {
                                    NewAnmFrame nextFrame = newAnm.frames[i];

                                    GUILayout.BeginHorizontal();
                                    {
                                        if(GUILayout.Button(nextFrame.timestamp + "\n" + nextFrame.frameTime) && frameButtonPress == -1)
                                        {
                                            //Drilldown into this frame
                                            frameIndex = i;
                                            frameButtonPress = 0;
                                        }
                                        GUI.enabled = (i != 0);
                                        if(GUILayout.Button("↑") && frameButtonPress == -1)
                                        {
                                            //Move the frame up
                                            frameIndex = i;
                                            frameButtonPress = 1;
                                        }
                                        GUI.enabled = (i != (newAnm.frames.Count-1));
                                        if (GUILayout.Button("↓") && frameButtonPress == -1)
                                        {
                                            //Move the frame down
                                            frameIndex = i;
                                            frameButtonPress = 2;
                                        }
                                        GUI.enabled = true;
                                        if (GUILayout.Button("X") && frameButtonPress == -1)
                                        {
                                            //Delete the frame
                                            frameIndex = i;
                                            frameButtonPress = 3;
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                }

                                //Perform after the loop to avoid issue with concurrancy
                                switch (frameButtonPress)
                                {
                                    case 0:
                                        {
                                            //Drilldown into this frame
                                            newAnmCurrentFrame = newAnm.frames[frameIndex];

                                            //Pause animations
                                            Animation anim = recordingMaid.GetAnimation();
                                            if (anim != null)
                                            {
                                                foreach (AnimationState anmState in anim)
                                                {
                                                    anmState.enabled = false;
                                                }
                                            }

                                            //Loop available maids
                                            //for (int i = 0; i < GameMain.Instance.CharacterMgr.GetMaidCount(); i++)
                                            //{
                                            //    Maid nextMaid = GameMain.Instance.CharacterMgr.GetMaid(i);
                                            //    if (nextMaid != null)
                                            //    {
                                            //        Animation anim = nextMaid.GetAnimation();
                                            //        if (anim != null)
                                            //        {
                                            //            foreach (AnimationState anmState in anim)
                                            //            {
                                            //                anmState.enabled = false;
                                            //            }
                                            //        }
                                            //    }
                                            //}
                                            ////Loop available men
                                            //for (int i = 0; i < GameMain.Instance.CharacterMgr.GetManCount(); i++)
                                            //{
                                            //    Maid nextMaid = GameMain.Instance.CharacterMgr.GetMan(i);
                                            //    if (nextMaid != null && nextMaid.isActiveAndEnabled)
                                            //    {
                                            //        Animation anim = nextMaid.GetAnimation();
                                            //        if (anim != null)
                                            //        {
                                            //            foreach (AnimationState anmState in anim)
                                            //            {
                                            //                anmState.enabled = false;
                                            //            }
                                            //        }
                                            //    }
                                            //}

                                            //Load the current frame into the Maid
                                            newAnmCurrentFrame.PlaybackBones(new CacheBoneDataArray.BoneData(recordingMaid.body0.GetBone("Bip01"), null), recordingMaid);
                                            displayPage = 7;
                                            break;
                                        }
                                    case 1:
                                        {
                                            //Move the frame up
                                            float swapTime = newAnm.frames[frameIndex].frameTime;
                                            newAnm.frames[frameIndex].frameTime = newAnm.frames[frameIndex - 1].frameTime;
                                            newAnm.frames[frameIndex - 1].frameTime = swapTime;
                                            break;
                                        }
                                    case 2:
                                        {
                                            //Move the frame down (more like move next frame up)
                                            float swapTime = newAnm.frames[frameIndex+1].frameTime;
                                            newAnm.frames[frameIndex+1].frameTime = newAnm.frames[frameIndex].frameTime;
                                            newAnm.frames[frameIndex].frameTime = swapTime;
                                            break;
                                        }
                                    case 3:
                                        {
                                            //Delete the frame
                                            newAnm.frames.RemoveAt(frameIndex);
                                            break;
                                        }
                                }
                            }
                            GUILayout.EndScrollView();

                            if (GUILayout.Button("Save Project"))
                            {
                                string newPath = Path.Combine(pathPrj, (newAnm.name + ".json"));
                                File.WriteAllText(newPath, Newtonsoft.Json.JsonConvert.SerializeObject(newAnm));
                            }
                            if (GUILayout.Button("Export .anm"))
                            {
                                if (newAnm.WriteToBinary())
                                {
                                    anmFile = Path.Combine(pathAnm, (newAnm.name) + ".anm");
                                }
                            }
                            { 
                                GUI.enabled = File.Exists(anmFile);
                                if (GUILayout.Button("Test .anm"))
                                {
                                    if (recordingMaid != null)
                                    {
                                        foreach(PhotoMotionData motionData in PhotoMotionData.data)
                                        {
                                            if(motionData.direct_file.Equals(anmFile))
                                            {
                                                motionData.Apply(recordingMaid);
                                            }
                                        }
                                        //recordingMaid.CrossFade(anmFile,, false, true, false, 0f, 1f);
                                    }
                                }
                                GUI.enabled = true;
                            }
                            if (GUILayout.Button("Back"))
                            {
                                anmAnimationSelectedName = null;
                                anmAnimationTimeText = "0.00";
                                timeStepText = "0";
                                autoFrames = new List<float>();
                                newAnm = null;
                                anmFile = "";
                                displayPage = 5;
                            }
                            break;
                        }
                    case 7:
                        {
                            GUILayout.Label("INFO:");
                            GUILayout.Label(infoMessage);

                            GUILayout.Label("Name:");
                            newAnmCurrentFrame.timestamp = GUILayout.TextField(newAnmCurrentFrame.timestamp);

                            GUILayout.Label("Frame Time:");
                            GUILayout.BeginHorizontal();
                            {
                                //I made this stupid complicated
                                if (frameTimeText == null)
                                {
                                    frameTimeText = newAnmCurrentFrame.frameTime.ToString();
                                }
                                if(frameTimeCached == -1)
                                {
                                    frameTimeCached = newAnmCurrentFrame.frameTime;
                                }

                                frameTimeText = GUILayout.TextField(frameTimeText);

                                if (GUILayout.Button("Apply"))
                                {
                                    if (System.Text.RegularExpressions.Regex.IsMatch(frameTimeText, @"[\+]?\d*\.?\d*") && float.Parse(frameTimeText) >= 0)
                                    {
                                        newAnmCurrentFrame.frameTime = float.Parse(frameTimeText);
                                    }
                                    else
                                    {
                                        infoMessage = "Invalid Frame Time float";
                                    }
                                }
                                if (GUILayout.Button("Original"))
                                {
                                    newAnmCurrentFrame.frameTime = frameTimeCached;
                                    frameTimeText = null;
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Label("Re-Position/Rotate:");
                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button("SNAP"))
                                {
                                    //Record the Maid's data
                                    newAnmCurrentFrame.RecordBones(new CacheBoneDataArray.BoneData(recordingMaid.body0.GetBone("Bip01"), null));
                                }
                            }
                            GUILayout.EndHorizontal();

                            if (GUILayout.Button("Back"))
                            {
                                newAnmCurrentFrame = null;
                                frameTimeText = null;
                                displayPage = 6;
                            }
                            break;
                        }
                }
            }
            GUILayout.EndVertical();

            //Make it draggable this must be last always
            GUI.DragWindow();
        }

        public static List<float> readBoneFromBinary(string fileName)
        {
            List<float> frameTimes = new List<float>();

            using (AFileBase afileBase = GameUty.FileOpen(fileName))
            {
                if (ImportCM.m_aniTempFile == null)
                    ImportCM.m_aniTempFile = new byte[Math.Max(500000, afileBase.GetSize())];
                else if (ImportCM.m_aniTempFile.Length < afileBase.GetSize())
                    ImportCM.m_aniTempFile = new byte[afileBase.GetSize()];
                afileBase.Read(ref ImportCM.m_aniTempFile, afileBase.GetSize());
            }

            BinaryReader br = new BinaryReader((Stream)new MemoryStream(ImportCM.m_aniTempFile), System.Text.Encoding.UTF8);
            br.ReadString();
            int int1001 = br.ReadInt32();

            //Read byte
            byte keepGoing = br.ReadByte();

            while (keepGoing == (byte)1)
            {
                //Write bone path
                string bonePath = br.ReadString();
                UnityEngine.Debug.Log("Path " + bonePath);

                while(true)//for (int index1 = 0; index1 < 7; index1++)
                {
                    //Write the channel
                    byte channel = br.ReadByte();
                    if(channel == (byte)1)
                    {
                        break;
                    }
                    UnityEngine.Debug.Log("Channel " + channel);

                    //Write the number of frames for this bone
                    int frames = br.ReadInt32();
                    UnityEngine.Debug.Log("Frames" + frames);

                    //Write the data for coordinates
                    for (int index2 = 0; index2 < frames; ++index2)
                    {
                        //Write the frame time
                        float time = br.ReadSingle();
                        UnityEngine.Debug.Log("Time " + time);

                        //Write the channel's value for this frame
                        float f0 = br.ReadSingle(); //UnityEngine.Debug.Log("f0:" + f0);
                        float f1 = br.ReadSingle(); //UnityEngine.Debug.Log("f1:" + f1);
                        float f2 = br.ReadSingle(); //UnityEngine.Debug.Log("f2:" + f2);

                        if(!frameTimes.Contains(time))
                        {
                            frameTimes.Add(time);
                        }
                    }

                    //Check if there is another channel
                    byte nextChannel = br.ReadByte();
                    br.BaseStream.Position = br.BaseStream.Position - 1;
                    if (nextChannel == (byte)1)
                    {
                        break;
                    }
                }

                keepGoing = br.ReadByte();
            }

            br.Close();

            frameTimes.Sort();
            return frameTimes;
        }


        public class NewAnm
        {
            public string name { get; set; } //fill in when exporting
            public List<NewAnmFrame> frames { get; set; }
            float boobStiffnessL { get; set; } // 0 mean lots of jiggle -- boo using the built in reader can only set 0 and 1
            float boobStiffnessR { get; set; } // 0 mean lots of jiggle -- boo using the built in reader can only set 0 and 1

            private static HashSet<string> _save_bone_path_set_ = null;
            private static HashSet<string> save_bone_path_set_
            {
                get
                {
                    if(_save_bone_path_set_ == null)
                    {
                        Type type = typeof(CacheBoneDataArray);
                        FieldInfo info = type.GetField("save_bone_path_set_", BindingFlags.NonPublic | BindingFlags.Static);
                        _save_bone_path_set_ = (HashSet<string>)info.GetValue(null);
                    }

                    return _save_bone_path_set_;
                }
            }

            public NewAnm()
            {
                name = "";
                frames = new List<NewAnmFrame>();
                boobStiffnessL = 0f;
                boobStiffnessR = 0f;
            }

            public bool WriteToBinary()
            {
                MemoryStream memoryStream = new MemoryStream();
                BinaryWriter binaryWriter = new BinaryWriter((Stream)memoryStream);

                //Get the object that is used to write the .anm file
                NewAnmWriteBone bip01 = NewAnmWriteBone.getWriteAnmData(this);

                //Write CM3D2_ANIM
                binaryWriter.Write("CM3D2_ANIM");
                //Write 1001 to ensure boob jiggle can be read
                binaryWriter.Write(1001);

                //Start Writing bone data
                writeBoneToBinary(bip01, binaryWriter);

                //Write buffer
                binaryWriter.Write((byte)0);
                //Write if booble jiggles enabled
                binaryWriter.Write(boobStiffnessL > 0f ? (byte)0 : (byte)1);
                binaryWriter.Write(boobStiffnessR > 0f ? (byte)0 : (byte)1);

                //Cleanup
                binaryWriter.Close();
                memoryStream.Close();
                byte[] array = memoryStream.ToArray();
                memoryStream.Dispose();

                //Write the actual file
                string filePath = Path.Combine(pathAnm, (name + ".anm"));
                if (!File.Exists(filePath))
                {
                    File.WriteAllBytes(filePath, array);

                    if (File.Exists(filePath))
                    {
                        infoMessage = ".anm File Saved";

                        PhotoMotionData photoMotionData = PhotoMotionData.AddMyPose(filePath);
                        if (photoMotionData != null)
                        {
                            PhotoWindowManager mgr = (UnityEngine.GameObject.Find("PhotoWindowManager").GetComponent<PhotoWindowManager>());
                            if (mgr != null)
                            {
                                MotionWindow motionWindow = mgr.GetWindow(PhotoWindowManager.WindowType.Motion) as MotionWindow;
                                if (motionWindow != null)
                                {
                                    motionWindow.PopupAndTabList.AddData("マイポーズ", new KeyValuePair<string, object>(photoMotionData.name, (object)photoMotionData));
                                }
                            }
                        }
                    }
                }
                else
                {
                    infoMessage = "Save Failed: .anm File Already Exists";
                    return false;
                }

                return true;
            }

            private void writeBoneToBinary(NewAnmWriteBone bone, BinaryWriter bw)
            {
                if (NewAnm.save_bone_path_set_.Contains(bone.path) || bone.path.Contains("chinko") || bone.path.Contains("tamabukuro"))
                {
                    //Write byte that indicates bone path
                    bw.Write((byte)1);
                    //Write bone path
                    bw.Write(bone.path);
                    UnityEngine.Debug.Log("BONE:" + bone.path);

                    //Loop Channels - index = 1 bc first entry is not a channel
                    for (int index1 = 1; index1 < bone.numArray[0].Count; index1++)
                    {
                        //Write the channel
                        bw.Write((byte)(100 + index1 - 1));
                        UnityEngine.Debug.Log("CHANNEL:" + (100 + index1 - 1));

                        //Sort the frames by time
                        bone.numArray.Sort(delegate (List<float> x, List<float> y)
                        {
                            return x[0].CompareTo(y[0]);
                        });

                        //Improve Data
                        float lastVal = 0f;
                        float lastInTan = 0f;
                        float lastOutTan = 0f;
                        List<int> indexesToRemove = new List<int>();
                        for (int index2 = bone.numArray.Count - 1; index2 >= 0; index2--)
                        {
                            float currentVal = bone.numArray[index2][index1];

                            //In-tangent and out-tangent (always 0 for rotations, maybe can be calculated for locations)
                            float currentInTan = 0f;
                            float currentOutTan = 0f;

                            //Check for duplicate frames, we always need first and last tho
                            if (index2 != 0 && index2 != (bone.numArray.Count - 1))
                            {
                                //If no change from the previous frame, no need to write
                                if (currentVal == lastVal && currentInTan == lastInTan && currentOutTan == lastOutTan)
                                {
                                    bone.numArray.RemoveAt(index2);
                                    continue;
                                }
                            }

                            //Save values for next
                            lastVal = currentVal;
                            lastInTan = currentInTan;
                            lastOutTan = currentOutTan;
                        }

                        //Write the number of frames for this bone
                        bw.Write(bone.numArray.Count);
                        UnityEngine.Debug.Log("FRAMES:" + bone.numArray.Count);

                        //Write the data for coordinates
                        for (int index2 = 0; index2 < bone.numArray.Count; ++index2)
                        {
                            float currentVal = bone.numArray[index2][index1];

                            //In-tangent and out-tangent (always 0 for rotations, maybe can be calculated for locations)
                            float currentInTan = 0f;
                            float currentOutTan = 0f;

                            //Write the frame time
                            bw.Write(bone.numArray[index2][0]);
                            UnityEngine.Debug.Log("FRAME:" + index2 + " TIME: " + bone.numArray[index2][0]);

                            //Write the channel's value for this frame
                            bw.Write(currentVal); UnityEngine.Debug.Log("f0:" + currentVal);
                            bw.Write(currentInTan); UnityEngine.Debug.Log("f1:" + currentInTan);
                            bw.Write(currentOutTan); UnityEngine.Debug.Log("f2:" + currentOutTan);
                        }
                    }

                    //if(bone.path.Trim().ToLower().Equals("bip01"))
                    //{
                    //    //Write the channel
                    //    bw.Write((byte)(121));

                    //    //Write the number of frames for this bone
                    //    bw.Write(bone.numArray.Count);

                    //    //Write the data for coordinates
                    //    bool val = false;
                    //    for (int index2 = 0; index2 < bone.numArray.Count; ++index2)
                    //    {
                    //        //Write the frame time
                    //        bw.Write(bone.numArray[index2][0]);
                    //        bw.Write(val? 1f:0f);
                    //        bw.Write(0f);
                    //        bw.Write(0f);

                    //        val = !val;
                    //    }
                    //}
                }

                //Process Children
                for (int index = 0; index < bone.children.Count; ++index)
                {
                    writeBoneToBinary(bone.children[index], bw);
                }

                if (bone.path.Trim().ToLower().Equals("bip01"))
                {
                    //Write byte that indicates bone path
                    bw.Write((byte)1);
                    //Write bone path
                    bw.Write("SK_munel");

                    //Write the channel
                    bw.Write((byte)(201));

                    float increment = bone.numArray[bone.numArray.Count - 1][0] / 10f;

                    //Write the number of frames for this bone
                    bw.Write(10);//bone.numArray.Count);

                    //Write the data for coordinates
                    bool val = false;
                    for (int index2 = 0; index2 < 10; ++index2)
                    {
                        //Write the frame time
                        bw.Write(increment * index2);
                        bw.Write(val ? 1f : 0f);
                        bw.Write(0f);
                        bw.Write(0f);

                        val = !val;
                    }
                }
            }
        }
        public class NewAnmFrame
        {
            public string timestamp { get; set; }
            public float frameTime { get; set; }
            public Dictionary<string, NewAnmBone> bones { get; set; } //path, obj
            public Dictionary<MPN, Dictionary<string, NewAnmSks>> sks { get; set; }

            public NewAnmFrame()
            {
                timestamp = "";
                frameTime = 0f;
                bones = new Dictionary<string, NewAnmBone>();
                sks = new Dictionary<string, float>();
            }

            public NewAnmBone RecordBones(CacheBoneDataArray.BoneData target_bone_data)
            {
                Vector3 localPosition = target_bone_data.transform.localPosition;
                Quaternion localRotation = target_bone_data.transform.localRotation;

                //Create new bone for the target
                NewAnmBone anmBone = new NewAnmBone();
                anmBone.bonePath = target_bone_data.path;

                //Add the coordinate data
                anmBone.coordinates.AddRange(new float[] { localRotation.x, localRotation.y, localRotation.z, localRotation.w });
                anmBone.coordinates.AddRange(new float[] { localPosition.x, localPosition.y, localPosition.z });

                UnityEngine.Debug.Log(anmBone.coordinates[0] + ", " + anmBone.coordinates[1] + ", " + anmBone.coordinates[2] + ", " + anmBone.coordinates[3] + ", " +
                                      anmBone.coordinates[4] + ", " + anmBone.coordinates[5] + ", " + anmBone.coordinates[6] );

                //Loop the children
                for (int index = 0; index < target_bone_data.child_bone_array.Length; ++index)
                {
                    NewAnmBone childBone = RecordBones(target_bone_data.child_bone_array[index]);

                    //Add heirarchy information -- should never be null unless in future decide to remove non-rexported bones
                    if (childBone != null)
                    {
                        anmBone.childPaths.Add(childBone.bonePath);
                    }
                }

                //Add to the frame's dictionary
                bones[anmBone.bonePath] = anmBone;

                //Return in case this was called from child loop
                return anmBone;
            }

            public void RecordSks(Maid maid, Dictionary<MPN, Dictionary<string,float>> sksDict)
            {
                foreach(MPN mpn in sksDict.Keys)
                {
                    Dictionary<string, float> baseSks = sksDict[mpn];
                    TBodySkin skin = maid.body0.goSlot.Find(slot=> slot.m_ParentMPN.Equals(mpn));
                    if(skin != null)
                    {
                        TMorph morph = skin.morph;
                        
                        foreach (string key in morph.hash.Keys)
                        {
                            int f_nIdx = (int)morph.hash[(object)key];

                            float absoluteVal = morph.GetBlendValues(f_nIdx);
                            float additiveDiff = absoluteVal - baseSks[key];
                            float percentDiff = additiveDiff / baseSks[key];

                            if(!this.sks.ContainsKey(mpn))
                            {
                                this.sks[mpn] = new Dictionary<string, NewAnmSks>();
                            }
                            this.sks[mpn][key] = new NewAnmSks();
                            this.sks[mpn][key].name = key;
                            this.sks[mpn][key].absoluteValue = absoluteVal;
                            this.sks[mpn][key].additiveValue = additiveDiff;
                            this.sks[mpn][key].percentValue = percentDiff;
                        }
                    }
                }
            }

            public void PlaybackBones(CacheBoneDataArray.BoneData target_bone_data, Maid maid)
            {
                
                string bonePath = target_bone_data.path;
                string[] boneNameSplit = bonePath.Split('/');
                string boneName = boneNameSplit[boneNameSplit.Length - 1];

                if (bones.ContainsKey(bonePath))
                {
                    NewAnmBone anmBone = bones[bonePath];
                    if (anmBone != null)
                    {
                        Transform transform = target_bone_data.transform;//(Transform)typeof(CacheBoneDataArray.BoneData).GetField("transform", BindingFlags.Public | BindingFlags.Instance).GetValue(target_bone_data);

                        if (transform != null)
                        {
                            Transform transform2 = maid.body0.GetBone(boneName);
                            if (transform2 != null)
                            {
                                transform.localRotation = new Quaternion(anmBone.coordinates[0], anmBone.coordinates[1], anmBone.coordinates[2], anmBone.coordinates[3]);
                                transform2.localRotation = new Quaternion(anmBone.coordinates[0], anmBone.coordinates[1], anmBone.coordinates[2], anmBone.coordinates[3]);

                                if (anmBone.coordinates.Count > 4)
                                {
                                    transform.localPosition = new Vector3(anmBone.coordinates[4], anmBone.coordinates[5], anmBone.coordinates[6]);
                                    transform2.localPosition = new Vector3(anmBone.coordinates[4], anmBone.coordinates[5], anmBone.coordinates[6]);
                                }

                                typeof(CacheBoneDataArray.BoneData).GetProperty("transform", BindingFlags.Public | BindingFlags.Instance).SetValue(target_bone_data, transform, null);
                            }
                            else
                            {
                                UnityEngine.Debug.Log(boneName + " null");
                            }
                        }
                        else
                        {
                            UnityEngine.Debug.Log("transform null");
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.Log("bone null");
                    }
                }

                for (int index = 0; index < target_bone_data.child_bone_array.Length; ++index)
                {
                    PlaybackBones(target_bone_data.child_bone_array[index], maid);
                }
            }
        }
        public class NewAnmBone
        {
            public string boneName { get; set; }
            public string bonePath { get; set; }
            public List<float> coordinates { get; set; }
            public List<string> childPaths { get; set; }

            public NewAnmBone()
            {
                boneName = "";
                bonePath = "";
                coordinates = new List<float>();
                childPaths = new List<string>();
            }
        }
        public class NewAnmWriteBone
        {
            public string path { get; set; }
            public List<List<float>> numArray { get; set; }
            public List<NewAnmWriteBone> children { get; set; }

            public NewAnmWriteBone()
            {
                numArray = new List<List<float>>();
                children = new List<NewAnmWriteBone>();
            }

            public static NewAnmWriteBone getWriteAnmData(NewAnm newAnm)
            {
                //Get a sample
                newAnm.frames.Sort(delegate (NewAnmFrame x, NewAnmFrame y)
                {
                    return x.frameTime.CompareTo(y.frameTime);
                });
                NewAnmFrame firstFrame = newAnm.frames[0];

                //Build out the saving object
                NewAnmBone bip01 = firstFrame.bones["Bip01"];

                //Setup the heirarchy
                return buildBoneHeirarchy(newAnm, firstFrame, bip01);
            }

            private static NewAnmWriteBone buildBoneHeirarchy(NewAnm newAnm, NewAnmFrame sampleFrame, NewAnmBone newAnmBone)
            {
                NewAnmWriteBone saveBone = new NewAnmWriteBone();
                saveBone.path = newAnmBone.bonePath;
                saveBone.numArray = new List<List<float>>();

                //Loop the frames
                for (int i = 0; i < newAnm.frames.Count; i++)
                {
                    NewAnmFrame nextFrame = newAnm.frames[i];

                    //Storage
                    List<float> data = new List<float>();

                    //Add the frame time
                    data.Add(nextFrame.frameTime);

                    //Add the coordinate values -- maybe add additional control values in the future (custom in/out tangent)
                    data.AddRange(nextFrame.bones[saveBone.path].coordinates);

                    saveBone.numArray.Add(data);
                }

                //Setup the children
                for (int i = 0; i < newAnmBone.childPaths.Count; i++)
                {
                    NewAnmBone newAnmChildBone = sampleFrame.bones[newAnmBone.childPaths[i]];
                    saveBone.children.Add(buildBoneHeirarchy(newAnm, sampleFrame, newAnmChildBone));
                }

                return saveBone;
            }
        }
        public class NewAnmSks
        {
            public MPN mpn { get; set; }
            public string name { get; set; }
            public float absoluteValue { get; set; }
            public float additiveValue { get; set; }
            public float percentValue { get; set; }

            public NewAnmSks()
            {
                mpn = MPN.null_mpn;
                name = "";
            }
        }



        private static string[] maidBonesPaths = {
            "ArmL",
            "Bip01",
            "Bip01/Bip01 Footsteps",
            "Bip01/Bip01 Pelvis",
            "Bip01/Bip01 Pelvis/_IK_anal",
            "Bip01/Bip01 Pelvis/_IK_hipL",
            "Bip01/Bip01 Pelvis/_IK_hipR",
            "Bip01/Bip01 Pelvis/_IK_hutanari",
            "Bip01/Bip01 Pelvis/_IK_vagina",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/_IK_thighL",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/_IK_calfL",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/_IK_footL",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe0",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe0/Bip01 L Toe01",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe0/Bip01 L Toe01/Bip01 L Toe0Nub",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe1",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe1/Bip01 L Toe11",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe1/Bip01 L Toe11/Bip01 L Toe1Nub",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe2",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe2/Bip01 L Toe21",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe2/Bip01 L Toe21/Bip01 L Toe2Nub",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/momotwist_L",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/momotwist_L/momoniku_L",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/momotwist_L/momoniku_L/momoniku_L_nub",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/momotwist_L/momotwist2_L",
            "Bip01/Bip01 Pelvis/Bip01 L Thigh/momotwist_L/momotwist_L_nub",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/_IK_thighR",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/_IK_calfR",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/_IK_footR",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe0",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe0/Bip01 R Toe01",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe0/Bip01 R Toe01/Bip01 R Toe0Nub",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe1",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe1/Bip01 R Toe11",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe1/Bip01 R Toe11/Bip01 R Toe1Nub",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe2",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe2/Bip01 R Toe21",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe2/Bip01 R Toe21/Bip01 R Toe2Nub",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/momotwist_R",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/momotwist_R/momoniku_R",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/momotwist_R/momoniku_R/momoniku_R_nub",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/momotwist_R/momotwist2_R",
            "Bip01/Bip01 Pelvis/Bip01 R Thigh/momotwist_R/momotwist_R_nub",
            "Bip01/Bip01 Pelvis/Hip_L",
            "Bip01/Bip01 Pelvis/Hip_L/Hip_L_nub",
            "Bip01/Bip01 Pelvis/Hip_R",
            "Bip01/Bip01 Pelvis/Hip_R/Hip_R_nub",
            "Bip01/Bip01 Spine",
            "Bip01/Bip01 Spine/_IK_hara",
            "Bip01/Bip01 Spine/Bip01 Spine0a",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/_IK_UpperArmL",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/_IK_ForeArmL",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/_IK_handL",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger0",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger0/Bip01 L Finger01",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger0/Bip01 L Finger01/Bip01 L Finger02",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger0/Bip01 L Finger01/Bip01 L Finger02/Bip01 L Finger0Nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger1",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger1/Bip01 L Finger11",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger1/Bip01 L Finger11/Bip01 L Finger12",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger1/Bip01 L Finger11/Bip01 L Finger12/Bip01 L Finger1Nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger2",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger2/Bip01 L Finger21",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger2/Bip01 L Finger21/Bip01 L Finger22",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger2/Bip01 L Finger21/Bip01 L Finger22/Bip01 L Finger2Nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger3",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger3/Bip01 L Finger31",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger3/Bip01 L Finger31/Bip01 L Finger32",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger3/Bip01 L Finger31/Bip01 L Finger32/Bip01 L Finger3Nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger4",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger4/Bip01 L Finger41",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger4/Bip01 L Finger41/Bip01 L Finger42",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger4/Bip01 L Finger41/Bip01 L Finger42/Bip01 L Finger4Nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Foretwist1_L",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Foretwist_L",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Uppertwist1_L",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Uppertwist_L",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Kata_L",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Kata_L/Kata_L_nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 Neck",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 Neck/Bip01 Head",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 Neck/Bip01 Head/_IK_hohoL",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 Neck/Bip01 Head/_IK_hohoR",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 Neck/Bip01 Head/Bip01 HeadNub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/_IK_UpperArmR",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/_IK_ForeArmR",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/_IK_handR",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger0",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger0/Bip01 R Finger01",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger0/Bip01 R Finger01/Bip01 R Finger02",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger0/Bip01 R Finger01/Bip01 R Finger02/Bip01 R Finger0Nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger1",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger1/Bip01 R Finger11",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger1/Bip01 R Finger11/Bip01 R Finger12",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger1/Bip01 R Finger11/Bip01 R Finger12/Bip01 R Finger1Nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger2",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger2/Bip01 R Finger21",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger2/Bip01 R Finger21/Bip01 R Finger22",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger2/Bip01 R Finger21/Bip01 R Finger22/Bip01 R Finger2Nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger3",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger3/Bip01 R Finger31",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger3/Bip01 R Finger31/Bip01 R Finger32",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger3/Bip01 R Finger31/Bip01 R Finger32/Bip01 R Finger3Nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger4",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger4/Bip01 R Finger41",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger4/Bip01 R Finger41/Bip01 R Finger42",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger4/Bip01 R Finger41/Bip01 R Finger42/Bip01 R Finger4Nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Foretwist1_R",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Foretwist_R",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Uppertwist1_R",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Uppertwist_R",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Kata_R",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Kata_R/Kata_R_nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_L",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_L/_IK_muneL",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_L/Mune_L_sub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_L/Mune_L_sub/Mune_L_nub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_R",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_R/_IK_muneR",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_R/Mune_R_sub",
            "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_R/Mune_R_sub/Mune_R_nub",
            "body",
            "center",
            "Hara",
            "MuneL",
            "MuneS",
            "MuneTare",
            "RegFat",
            "RegMeet",

        };

        private static string[] maidBones = {
            "ArmL",
            "Bip01",
            "Bip01 Footsteps",
            "Bip01 Pelvis",
            "_IK_anal",
            "_IK_hipL",
            "_IK_hipR",
            "_IK_hutanari",
            "_IK_vagina",
            "Bip01 L Thigh",
            "_IK_thighL",
            "Bip01 L Calf",
            "_IK_calfL",
            "Bip01 L Foot",
            "_IK_footL",
            "Bip01 L Toe0",
            "Bip01 L Toe01",
            "Bip01 L Toe0Nub",
            "Bip01 L Toe1",
            "Bip01 L Toe11",
            "Bip01 L Toe1Nub",
            "Bip01 L Toe2",
            "Bip01 L Toe21",
            "Bip01 L Toe2Nub",
            "momotwist_L",
            "momoniku_L",
            "momoniku_L_nub",
            "momotwist2_L",
            "momotwist_L_nub",
            "Bip01 R Thigh",
            "_IK_thighR",
            "Bip01 R Calf",
            "_IK_calfR",
            "Bip01 R Foot",
            "_IK_footR",
            "Bip01 R Toe0",
            "Bip01 R Toe01",
            "Bip01 R Toe0Nub",
            "Bip01 R Toe1",
            "Bip01 R Toe11",
            "Bip01 R Toe1Nub",
            "Bip01 R Toe2",
            "Bip01 R Toe21",
            "Bip01 R Toe2Nub",
            "momotwist_R",
            "momoniku_R",
            "momoniku_R_nub",
            "momotwist2_R",
            "momotwist_R_nub",
            "Hip_L",
            "Hip_L_nub",
            "Hip_R",
            "Hip_R_nub",
            "Bip01 Spine",
            "_IK_hara",
            "Bip01 Spine0a",
            "Bip01 Spine1",
            "Bip01 Spine1a",
            "Bip01 L Clavicle",
            "Bip01 L UpperArm",
            "_IK_UpperArmL",
            "Bip01 L Forearm",
            "_IK_ForeArmL",
            "Bip01 L Hand",
            "_IK_handL",
            "Bip01 L Finger0",
            "Bip01 L Finger01",
            "Bip01 L Finger02",
            "Bip01 L Finger0Nub",
            "Bip01 L Finger1",
            "Bip01 L Finger11",
            "Bip01 L Finger12",
            "Bip01 L Finger1Nub",
            "Bip01 L Finger2",
            "Bip01 L Finger21",
            "Bip01 L Finger22",
            "Bip01 L Finger2Nub",
            "Bip01 L Finger3",
            "Bip01 L Finger31",
            "Bip01 L Finger32",
            "Bip01 L Finger3Nub",
            "Bip01 L Finger4",
            "Bip01 L Finger41",
            "Bip01 L Finger42",
            "Bip01 L Finger4Nub",
            "Foretwist1_L",
            "Foretwist_L",
            "Uppertwist1_L",
            "Uppertwist_L",
            "Kata_L",
            "Kata_L_nub",
            "Bip01 Neck",
            "Bip01 Head",
            "_IK_hohoL",
            "_IK_hohoR",
            "Bip01 HeadNub",
            "Bip01 R Clavicle",
            "Bip01 R UpperArm",
            "_IK_UpperArmR",
            "Bip01 R Forearm",
            "_IK_ForeArmR",
            "Bip01 R Hand",
            "_IK_handR",
            "Bip01 R Finger0",
            "Bip01 R Finger01",
            "Bip01 R Finger02",
            "Bip01 R Finger0Nub",
            "Bip01 R Finger1",
            "Bip01 R Finger11",
            "Bip01 R Finger12",
            "Bip01 R Finger1Nub",
            "Bip01 R Finger2",
            "Bip01 R Finger21",
            "Bip01 R Finger22",
            "Bip01 R Finger2Nub",
            "Bip01 R Finger3",
            "Bip01 R Finger31",
            "Bip01 R Finger32",
            "Bip01 R Finger3Nub",
            "Bip01 R Finger4",
            "Bip01 R Finger41",
            "Bip01 R Finger42",
            "Bip01 R Finger4Nub",
            "Foretwist1_R",
            "Foretwist_R",
            "Uppertwist1_R",
            "Uppertwist_R",
            "Kata_R",
            "Kata_R_nub",
            "Mune_L",
            "_IK_muneL",
            "Mune_L_sub",
            "Mune_L_nub",
            "Mune_R",
            "_IK_muneR",
            "Mune_R_sub",
            "Mune_R_nub",
            "body",
            "center",
            "Hara",
            "MuneL",
            "MuneS",
            "MuneTare",
            "RegFat",
            "RegMeet"
        };

        //private static calculateTangents(Vector2 p1, Vector2 p2)
        //{
        //    float tangLengthX = Mathf.Abs(p1.x - p2.x) / 3.0f;
        //    float tangLengthY = tangLengthX;
        //    c1 = p1;
        //    c2 = p2;
        //    c1.x += tangLengthX;
        //    c1.y += tangLengthY * tgOut;
        //    c2.x -= tangLengthX;
        //    c2.y -= tangLengthY * tgIn;

        //    float tangLength = Mathf.Abs(p1.x - p2.x) / 3.0f;
        //    tgOut = (c1.y - p1.y) / tangLength;
        //    tgIn = (p2.y - c2.y) / tangLength;



        //    float d = (p2.x - p1.x) / 3.0f;
        //    float a = 1;//h / w;
        //    Vector2 st = p1 + new Vector2(d, d * a * K1.outTangent);
        //    Vector2 et = p2 + new Vector2(-d, -d * a * K2.inTangent);
        //    Drawing.BezierLineGL(start, st, end, et, Color.red, 20);
        //}
        //private static float getInTangent(float t, float time0, float time1, float val0, float val1)
        //{

        //    float dt = time1 - time0;

        //    t = (t - time0) / dt;

        //    float t2 = t * t;
        //    float t3 = t2 * t;

        //    float a = 2 * t3 - 3 * t2 + 1;
        //    float b = t3 - 2 * t2 + t;
        //    float c = t3 - t2;
        //    float d = -2 * t3 + 3 * t2;

        //    float m0 = keyframe0.outTangent * dt;
        //    float m1 = keyframe1.inTangent * dt;

        //    float outT = m0 / dt;
        //    float inT = m1 / dt;



        //    //return a * val0 + b * m0 + c * m1 + d * val1;



        //    (2 * .0000029427665140246972)/ 0.0000000298023224 = ((1 - t) * (1 - c) * (1 + b)) * (-1) + ((1 - t) * (1 + c) * (1 - b));
        //}

        public class AnmJson
        {
            string name { get; set; }
            float boobStiffnessL { get; set; } // 0 mean lots of jiggle
            float boobStiffnessRL { get; set; } // 0 mean lots of jiggle

            public AnmJson()
            {

            }
        }
        public class AnmJsonData
        {
            #region Bones
            public AnmJsonBoneData ArmL { get; set; }
            public AnmJsonBoneData Bip01 { get; set; }
            public AnmJsonBoneData Bip01_Footsteps { get; set; }
            public AnmJsonBoneData Bip01_Pelvis { get; set; }
            public AnmJsonBoneData _IK_anal { get; set; }
            public AnmJsonBoneData _IK_hipL { get; set; }
            public AnmJsonBoneData _IK_hipR { get; set; }
            public AnmJsonBoneData _IK_hutanari { get; set; }
            public AnmJsonBoneData _IK_vagina { get; set; }
            public AnmJsonBoneData Bip01_L_Thigh { get; set; }
            public AnmJsonBoneData _IK_thighL { get; set; }
            public AnmJsonBoneData Bip01_L_Calf { get; set; }
            public AnmJsonBoneData _IK_calfL { get; set; }
            public AnmJsonBoneData Bip01_L_Foot { get; set; }
            public AnmJsonBoneData _IK_footL { get; set; }
            public AnmJsonBoneData Bip01_L_Toe0 { get; set; }
            public AnmJsonBoneData Bip01_L_Toe01 { get; set; }
            public AnmJsonBoneData Bip01_L_Toe0Nub { get; set; }
            public AnmJsonBoneData Bip01_L_Toe1 { get; set; }
            public AnmJsonBoneData Bip01_L_Toe11 { get; set; }
            public AnmJsonBoneData Bip01_L_Toe1Nub { get; set; }
            public AnmJsonBoneData Bip01_L_Toe2 { get; set; }
            public AnmJsonBoneData Bip01_L_Toe21 { get; set; }
            public AnmJsonBoneData Bip01_L_Toe2Nub { get; set; }
            public AnmJsonBoneData momotwist_L { get; set; }
            public AnmJsonBoneData momoniku_L { get; set; }
            public AnmJsonBoneData momoniku_L_nub { get; set; }
            public AnmJsonBoneData momotwist2_L { get; set; }
            public AnmJsonBoneData momotwist_L_nub { get; set; }
            public AnmJsonBoneData Bip01_R_Thigh { get; set; }
            public AnmJsonBoneData _IK_thighR { get; set; }
            public AnmJsonBoneData Bip01_R_Calf { get; set; }
            public AnmJsonBoneData _IK_calfR { get; set; }
            public AnmJsonBoneData Bip01_R_Foot { get; set; }
            public AnmJsonBoneData _IK_footR { get; set; }
            public AnmJsonBoneData Bip01_R_Toe0 { get; set; }
            public AnmJsonBoneData Bip01_R_Toe01 { get; set; }
            public AnmJsonBoneData Bip01_R_Toe0Nub { get; set; }
            public AnmJsonBoneData Bip01_R_Toe1 { get; set; }
            public AnmJsonBoneData Bip01_R_Toe11 { get; set; }
            public AnmJsonBoneData Bip01_R_Toe1Nub { get; set; }
            public AnmJsonBoneData Bip01_R_Toe2 { get; set; }
            public AnmJsonBoneData Bip01_R_Toe21 { get; set; }
            public AnmJsonBoneData Bip01_R_Toe2Nub { get; set; }
            public AnmJsonBoneData momotwist_R { get; set; }
            public AnmJsonBoneData momoniku_R { get; set; }
            public AnmJsonBoneData momoniku_R_nub { get; set; }
            public AnmJsonBoneData momotwist2_R { get; set; }
            public AnmJsonBoneData momotwist_R_nub { get; set; }
            public AnmJsonBoneData Hip_L { get; set; }
            public AnmJsonBoneData Hip_L_nub { get; set; }
            public AnmJsonBoneData Hip_R { get; set; }
            public AnmJsonBoneData Hip_R_nub { get; set; }
            public AnmJsonBoneData Bip01_Spine { get; set; }
            public AnmJsonBoneData _IK_hara { get; set; }
            public AnmJsonBoneData Bip01_Spine0a { get; set; }
            public AnmJsonBoneData Bip01_Spine1 { get; set; }
            public AnmJsonBoneData Bip01_Spine1a { get; set; }
            public AnmJsonBoneData Bip01_L_Clavicle { get; set; }
            public AnmJsonBoneData Bip01_L_UpperArm { get; set; }
            public AnmJsonBoneData _IK_UpperArmL { get; set; }
            public AnmJsonBoneData Bip01_L_Forearm { get; set; }
            public AnmJsonBoneData _IK_ForeArmL { get; set; }
            public AnmJsonBoneData Bip01_L_Hand { get; set; }
            public AnmJsonBoneData _IK_handL { get; set; }
            public AnmJsonBoneData Bip01_L_Finger0 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger01 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger02 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger0Nub { get; set; }
            public AnmJsonBoneData Bip01_L_Finger1 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger11 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger12 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger1Nub { get; set; }
            public AnmJsonBoneData Bip01_L_Finger2 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger21 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger22 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger2Nub { get; set; }
            public AnmJsonBoneData Bip01_L_Finger3 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger31 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger32 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger3Nub { get; set; }
            public AnmJsonBoneData Bip01_L_Finger4 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger41 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger42 { get; set; }
            public AnmJsonBoneData Bip01_L_Finger4Nub { get; set; }
            public AnmJsonBoneData Foretwist1_L { get; set; }
            public AnmJsonBoneData Foretwist_L { get; set; }
            public AnmJsonBoneData Uppertwist1_L { get; set; }
            public AnmJsonBoneData Uppertwist_L { get; set; }
            public AnmJsonBoneData Kata_L { get; set; }
            public AnmJsonBoneData Kata_L_nub { get; set; }
            public AnmJsonBoneData Bip01_Neck { get; set; }
            public AnmJsonBoneData Bip01_Head { get; set; }
            public AnmJsonBoneData _IK_hohoL { get; set; }
            public AnmJsonBoneData _IK_hohoR { get; set; }
            public AnmJsonBoneData Bip01_HeadNub { get; set; }
            public AnmJsonBoneData Bip01_R_Clavicle { get; set; }
            public AnmJsonBoneData Bip01_R_UpperArm { get; set; }
            public AnmJsonBoneData _IK_UpperArmR { get; set; }
            public AnmJsonBoneData Bip01_R_Forearm { get; set; }
            public AnmJsonBoneData _IK_ForeArmR { get; set; }
            public AnmJsonBoneData Bip01_R_Hand { get; set; }
            public AnmJsonBoneData _IK_handR { get; set; }
            public AnmJsonBoneData Bip01_R_Finger0 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger01 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger02 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger0Nub { get; set; }
            public AnmJsonBoneData Bip01_R_Finger1 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger11 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger12 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger1Nub { get; set; }
            public AnmJsonBoneData Bip01_R_Finger2 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger21 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger22 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger2Nub { get; set; }
            public AnmJsonBoneData Bip01_R_Finger3 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger31 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger32 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger3Nub { get; set; }
            public AnmJsonBoneData Bip01_R_Finger4 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger41 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger42 { get; set; }
            public AnmJsonBoneData Bip01_R_Finger4Nub { get; set; }
            public AnmJsonBoneData Foretwist1_R { get; set; }
            public AnmJsonBoneData Foretwist_R { get; set; }
            public AnmJsonBoneData Uppertwist1_R { get; set; }
            public AnmJsonBoneData Uppertwist_R { get; set; }
            public AnmJsonBoneData Kata_R { get; set; }
            public AnmJsonBoneData Kata_R_nub { get; set; }
            public AnmJsonBoneData Mune_L { get; set; }
            public AnmJsonBoneData _IK_muneL { get; set; }
            public AnmJsonBoneData Mune_L_sub { get; set; }
            public AnmJsonBoneData Mune_L_nub { get; set; }
            public AnmJsonBoneData Mune_R { get; set; }
            public AnmJsonBoneData _IK_muneR { get; set; }
            public AnmJsonBoneData Mune_R_sub { get; set; }
            public AnmJsonBoneData Mune_R_nub { get; set; }
            public AnmJsonBoneData body { get; set; }
            public AnmJsonBoneData center { get; set; }
            public AnmJsonBoneData Hara { get; set; }
            public AnmJsonBoneData MuneL { get; set; }
            public AnmJsonBoneData MuneS { get; set; }
            public AnmJsonBoneData MuneTare { get; set; }
            public AnmJsonBoneData RegFat { get; set; }
            public AnmJsonBoneData RegMeet { get; set; }
            #endregion

            public AnmJsonData()
            {
                #region
                ArmL = new AnmJsonBoneData();
                Bip01 = new AnmJsonBoneData();
                Bip01_Footsteps = new AnmJsonBoneData();
                Bip01_Pelvis = new AnmJsonBoneData();
                _IK_anal = new AnmJsonBoneData();
                _IK_hipL = new AnmJsonBoneData();
                _IK_hipR = new AnmJsonBoneData();
                _IK_hutanari = new AnmJsonBoneData();
                _IK_vagina = new AnmJsonBoneData();
                Bip01_L_Thigh = new AnmJsonBoneData();
                _IK_thighL = new AnmJsonBoneData();
                Bip01_L_Calf = new AnmJsonBoneData();
                _IK_calfL = new AnmJsonBoneData();
                Bip01_L_Foot = new AnmJsonBoneData();
                _IK_footL = new AnmJsonBoneData();
                Bip01_L_Toe0 = new AnmJsonBoneData();
                Bip01_L_Toe01 = new AnmJsonBoneData();
                Bip01_L_Toe0Nub = new AnmJsonBoneData();
                Bip01_L_Toe1 = new AnmJsonBoneData();
                Bip01_L_Toe11 = new AnmJsonBoneData();
                Bip01_L_Toe1Nub = new AnmJsonBoneData();
                Bip01_L_Toe2 = new AnmJsonBoneData();
                Bip01_L_Toe21 = new AnmJsonBoneData();
                Bip01_L_Toe2Nub = new AnmJsonBoneData();
                momotwist_L = new AnmJsonBoneData();
                momoniku_L = new AnmJsonBoneData();
                momoniku_L_nub = new AnmJsonBoneData();
                momotwist2_L = new AnmJsonBoneData();
                momotwist_L_nub = new AnmJsonBoneData();
                Bip01_R_Thigh = new AnmJsonBoneData();
                _IK_thighR = new AnmJsonBoneData();
                Bip01_R_Calf = new AnmJsonBoneData();
                _IK_calfR = new AnmJsonBoneData();
                Bip01_R_Foot = new AnmJsonBoneData();
                _IK_footR = new AnmJsonBoneData();
                Bip01_R_Toe0 = new AnmJsonBoneData();
                Bip01_R_Toe01 = new AnmJsonBoneData();
                Bip01_R_Toe0Nub = new AnmJsonBoneData();
                Bip01_R_Toe1 = new AnmJsonBoneData();
                Bip01_R_Toe11 = new AnmJsonBoneData();
                Bip01_R_Toe1Nub = new AnmJsonBoneData();
                Bip01_R_Toe2 = new AnmJsonBoneData();
                Bip01_R_Toe21 = new AnmJsonBoneData();
                Bip01_R_Toe2Nub = new AnmJsonBoneData();
                momotwist_R = new AnmJsonBoneData();
                momoniku_R = new AnmJsonBoneData();
                momoniku_R_nub = new AnmJsonBoneData();
                momotwist2_R = new AnmJsonBoneData();
                momotwist_R_nub = new AnmJsonBoneData();
                Hip_L = new AnmJsonBoneData();
                Hip_L_nub = new AnmJsonBoneData();
                Hip_R = new AnmJsonBoneData();
                Hip_R_nub = new AnmJsonBoneData();
                Bip01_Spine = new AnmJsonBoneData();
                _IK_hara = new AnmJsonBoneData();
                Bip01_Spine0a = new AnmJsonBoneData();
                Bip01_Spine1 = new AnmJsonBoneData();
                Bip01_Spine1a = new AnmJsonBoneData();
                Bip01_L_Clavicle = new AnmJsonBoneData();
                Bip01_L_UpperArm = new AnmJsonBoneData();
                _IK_UpperArmL = new AnmJsonBoneData();
                Bip01_L_Forearm = new AnmJsonBoneData();
                _IK_ForeArmL = new AnmJsonBoneData();
                Bip01_L_Hand = new AnmJsonBoneData();
                _IK_handL = new AnmJsonBoneData();
                Bip01_L_Finger0 = new AnmJsonBoneData();
                Bip01_L_Finger01 = new AnmJsonBoneData();
                Bip01_L_Finger02 = new AnmJsonBoneData();
                Bip01_L_Finger0Nub = new AnmJsonBoneData();
                Bip01_L_Finger1 = new AnmJsonBoneData();
                Bip01_L_Finger11 = new AnmJsonBoneData();
                Bip01_L_Finger12 = new AnmJsonBoneData();
                Bip01_L_Finger1Nub = new AnmJsonBoneData();
                Bip01_L_Finger2 = new AnmJsonBoneData();
                Bip01_L_Finger21 = new AnmJsonBoneData();
                Bip01_L_Finger22 = new AnmJsonBoneData();
                Bip01_L_Finger2Nub = new AnmJsonBoneData();
                Bip01_L_Finger3 = new AnmJsonBoneData();
                Bip01_L_Finger31 = new AnmJsonBoneData();
                Bip01_L_Finger32 = new AnmJsonBoneData();
                Bip01_L_Finger3Nub = new AnmJsonBoneData();
                Bip01_L_Finger4 = new AnmJsonBoneData();
                Bip01_L_Finger41 = new AnmJsonBoneData();
                Bip01_L_Finger42 = new AnmJsonBoneData();
                Bip01_L_Finger4Nub = new AnmJsonBoneData();
                Foretwist1_L = new AnmJsonBoneData();
                Foretwist_L = new AnmJsonBoneData();
                Uppertwist1_L = new AnmJsonBoneData();
                Uppertwist_L = new AnmJsonBoneData();
                Kata_L = new AnmJsonBoneData();
                Kata_L_nub = new AnmJsonBoneData();
                Bip01_Neck = new AnmJsonBoneData();
                Bip01_Head = new AnmJsonBoneData();
                _IK_hohoL = new AnmJsonBoneData();
                _IK_hohoR = new AnmJsonBoneData();
                Bip01_HeadNub = new AnmJsonBoneData();
                Bip01_R_Clavicle = new AnmJsonBoneData();
                Bip01_R_UpperArm = new AnmJsonBoneData();
                _IK_UpperArmR = new AnmJsonBoneData();
                Bip01_R_Forearm = new AnmJsonBoneData();
                _IK_ForeArmR = new AnmJsonBoneData();
                Bip01_R_Hand = new AnmJsonBoneData();
                _IK_handR = new AnmJsonBoneData();
                Bip01_R_Finger0 = new AnmJsonBoneData();
                Bip01_R_Finger01 = new AnmJsonBoneData();
                Bip01_R_Finger02 = new AnmJsonBoneData();
                Bip01_R_Finger0Nub = new AnmJsonBoneData();
                Bip01_R_Finger1 = new AnmJsonBoneData();
                Bip01_R_Finger11 = new AnmJsonBoneData();
                Bip01_R_Finger12 = new AnmJsonBoneData();
                Bip01_R_Finger1Nub = new AnmJsonBoneData();
                Bip01_R_Finger2 = new AnmJsonBoneData();
                Bip01_R_Finger21 = new AnmJsonBoneData();
                Bip01_R_Finger22 = new AnmJsonBoneData();
                Bip01_R_Finger2Nub = new AnmJsonBoneData();
                Bip01_R_Finger3 = new AnmJsonBoneData();
                Bip01_R_Finger31 = new AnmJsonBoneData();
                Bip01_R_Finger32 = new AnmJsonBoneData();
                Bip01_R_Finger3Nub = new AnmJsonBoneData();
                Bip01_R_Finger4 = new AnmJsonBoneData();
                Bip01_R_Finger41 = new AnmJsonBoneData();
                Bip01_R_Finger42 = new AnmJsonBoneData();
                Bip01_R_Finger4Nub = new AnmJsonBoneData();
                Foretwist1_R = new AnmJsonBoneData();
                Foretwist_R = new AnmJsonBoneData();
                Uppertwist1_R = new AnmJsonBoneData();
                Uppertwist_R = new AnmJsonBoneData();
                Kata_R = new AnmJsonBoneData();
                Kata_R_nub = new AnmJsonBoneData();
                Mune_L = new AnmJsonBoneData();
                _IK_muneL = new AnmJsonBoneData();
                Mune_L_sub = new AnmJsonBoneData();
                Mune_L_nub = new AnmJsonBoneData();
                Mune_R = new AnmJsonBoneData();
                _IK_muneR = new AnmJsonBoneData();
                Mune_R_sub = new AnmJsonBoneData();
                Mune_R_nub = new AnmJsonBoneData();
                body = new AnmJsonBoneData();
                center = new AnmJsonBoneData();
                Hara = new AnmJsonBoneData();
                MuneL = new AnmJsonBoneData();
                MuneS = new AnmJsonBoneData();
                MuneTare = new AnmJsonBoneData();
                RegFat = new AnmJsonBoneData();
                RegMeet = new AnmJsonBoneData();
                #endregion
            }

            public AnmJsonBoneData getBoneByName(string name)
            {
                switch (name)
                {
                    case "ArmL":
                        return this.ArmL;
                    case "Bip01":
                        return this.Bip01;
                    case "Bip01 Footsteps":
                        return this.Bip01_Footsteps;
                    case "Bip01 Pelvis":
                        return this.Bip01_Pelvis;
                    case "_IK_anal":
                        return this._IK_anal;
                    case "_IK_hipL":
                        return this._IK_hipL;
                    case "_IK_hipR":
                        return this._IK_hipR;
                    case "_IK_hutanari":
                        return this._IK_hutanari;
                    case "_IK_vagina":
                        return this._IK_vagina;
                    case "Bip01 L Thigh":
                        return this.Bip01_L_Thigh;
                    case "_IK_thighL":
                        return this._IK_thighL;
                    case "Bip01 L Calf":
                        return this.Bip01_L_Calf;
                    case "_IK_calfL":
                        return this._IK_calfL;
                    case "Bip01 L Foot":
                        return this.Bip01_L_Foot;
                    case "_IK_footL":
                        return this._IK_footL;
                    case "Bip01 L Toe0":
                        return this.Bip01_L_Toe0;
                    case "Bip01 L Toe01":
                        return this.Bip01_L_Toe01;
                    case "Bip01 L Toe0Nub":
                        return this.Bip01_L_Toe0Nub;
                    case "Bip01 L Toe1":
                        return this.Bip01_L_Toe1;
                    case "Bip01 L Toe11":
                        return this.Bip01_L_Toe11;
                    case "Bip01 L Toe1Nub":
                        return this.Bip01_L_Toe1Nub;
                    case "Bip01 L Toe2":
                        return this.Bip01_L_Toe2;
                    case "Bip01 L Toe21":
                        return this.Bip01_L_Toe21;
                    case "Bip01 L Toe2Nub":
                        return this.Bip01_L_Toe2Nub;
                    case "momotwist_L":
                        return this.momotwist_L;
                    case "momoniku_L":
                        return this.momoniku_L;
                    case "momoniku_L_nub":
                        return this.momoniku_L_nub;
                    case "momotwist2_L":
                        return this.momotwist2_L;
                    case "momotwist_L_nub":
                        return this.momotwist_L_nub;
                    case "Bip01 R Thigh":
                        return this.Bip01_R_Thigh;
                    case "_IK_thighR":
                        return this._IK_thighR;
                    case "Bip01 R Calf":
                        return this.Bip01_R_Calf;
                    case "_IK_calfR":
                        return this._IK_calfR;
                    case "Bip01 R Foot":
                        return this.Bip01_R_Foot;
                    case "_IK_footR":
                        return this._IK_footR;
                    case "Bip01 R Toe0":
                        return this.Bip01_R_Toe0;
                    case "Bip01 R Toe01":
                        return this.Bip01_R_Toe01;
                    case "Bip01 R Toe0Nub":
                        return this.Bip01_R_Toe0Nub;
                    case "Bip01 R Toe1":
                        return this.Bip01_R_Toe1;
                    case "Bip01 R Toe11":
                        return this.Bip01_R_Toe11;
                    case "Bip01 R Toe1Nub":
                        return this.Bip01_R_Toe1Nub;
                    case "Bip01 R Toe2":
                        return this.Bip01_R_Toe2;
                    case "Bip01 R Toe21":
                        return this.Bip01_R_Toe21;
                    case "Bip01 R Toe2Nub":
                        return this.Bip01_R_Toe2Nub;
                    case "momotwist_R":
                        return this.momotwist_R;
                    case "momoniku_R":
                        return this.momoniku_R;
                    case "momoniku_R_nub":
                        return this.momoniku_R_nub;
                    case "momotwist2_R":
                        return this.momotwist2_R;
                    case "momotwist_R_nub":
                        return this.momotwist_R_nub;
                    case "Hip_L":
                        return this.Hip_L;
                    case "Hip_L_nub":
                        return this.Hip_L_nub;
                    case "Hip_R":
                        return this.Hip_R;
                    case "Hip_R_nub":
                        return this.Hip_R_nub;
                    case "Bip01 Spine":
                        return this.Bip01_Spine;
                    case "_IK_hara":
                        return this._IK_hara;
                    case "Bip01 Spine0a":
                        return this.Bip01_Spine0a;
                    case "Bip01 Spine1":
                        return this.Bip01_Spine1;
                    case "Bip01 Spine1a":
                        return this.Bip01_Spine1a;
                    case "Bip01 L Clavicle":
                        return this.Bip01_L_Clavicle;
                    case "Bip01 L UpperArm":
                        return this.Bip01_L_UpperArm;
                    case "_IK_UpperArmL":
                        return this._IK_UpperArmL;
                    case "Bip01 L Forearm":
                        return this.Bip01_L_Forearm;
                    case "_IK_ForeArmL":
                        return this._IK_ForeArmL;
                    case "Bip01 L Hand":
                        return this.Bip01_L_Hand;
                    case "_IK_handL":
                        return this._IK_handL;
                    case "Bip01 L Finger0":
                        return this.Bip01_L_Finger0;
                    case "Bip01 L Finger01":
                        return this.Bip01_L_Finger01;
                    case "Bip01 L Finger02":
                        return this.Bip01_L_Finger02;
                    case "Bip01 L Finger0Nub":
                        return this.Bip01_L_Finger0Nub;
                    case "Bip01 L Finger1":
                        return this.Bip01_L_Finger1;
                    case "Bip01 L Finger11":
                        return this.Bip01_L_Finger11;
                    case "Bip01 L Finger12":
                        return this.Bip01_L_Finger12;
                    case "Bip01 L Finger1Nub":
                        return this.Bip01_L_Finger1Nub;
                    case "Bip01 L Finger2":
                        return this.Bip01_L_Finger2;
                    case "Bip01 L Finger21":
                        return this.Bip01_L_Finger21;
                    case "Bip01 L Finger22":
                        return this.Bip01_L_Finger22;
                    case "Bip01 L Finger2Nub":
                        return this.Bip01_L_Finger2Nub;
                    case "Bip01 L Finger3":
                        return this.Bip01_L_Finger3;
                    case "Bip01 L Finger31":
                        return this.Bip01_L_Finger31;
                    case "Bip01 L Finger32":
                        return this.Bip01_L_Finger32;
                    case "Bip01 L Finger3Nub":
                        return this.Bip01_L_Finger3Nub;
                    case "Bip01 L Finger4":
                        return this.Bip01_L_Finger4;
                    case "Bip01 L Finger41":
                        return this.Bip01_L_Finger41;
                    case "Bip01 L Finger42":
                        return this.Bip01_L_Finger42;
                    case "Bip01 L Finger4Nub":
                        return this.Bip01_L_Finger4Nub;
                    case "Foretwist1_L":
                        return this.Foretwist1_L;
                    case "Foretwist_L":
                        return this.Foretwist_L;
                    case "Uppertwist1_L":
                        return this.Uppertwist1_L;
                    case "Uppertwist_L":
                        return this.Uppertwist_L;
                    case "Kata_L":
                        return this.Kata_L;
                    case "Kata_L_nub":
                        return this.Kata_L_nub;
                    case "Bip01 Neck":
                        return this.Bip01_Neck;
                    case "Bip01 Head":
                        return this.Bip01_Head;
                    case "_IK_hohoL":
                        return this._IK_hohoL;
                    case "_IK_hohoR":
                        return this._IK_hohoR;
                    case "Bip01 HeadNub":
                        return this.Bip01_HeadNub;
                    case "Bip01 R Clavicle":
                        return this.Bip01_R_Clavicle;
                    case "Bip01 R UpperArm":
                        return this.Bip01_R_UpperArm;
                    case "_IK_UpperArmR":
                        return this._IK_UpperArmR;
                    case "Bip01 R Forearm":
                        return this.Bip01_R_Forearm;
                    case "_IK_ForeArmR":
                        return this._IK_ForeArmR;
                    case "Bip01 R Hand":
                        return this.Bip01_R_Hand;
                    case "_IK_handR":
                        return this._IK_handR;
                    case "Bip01 R Finger0":
                        return this.Bip01_R_Finger0;
                    case "Bip01 R Finger01":
                        return this.Bip01_R_Finger01;
                    case "Bip01 R Finger02":
                        return this.Bip01_R_Finger02;
                    case "Bip01 R Finger0Nub":
                        return this.Bip01_R_Finger0Nub;
                    case "Bip01 R Finger1":
                        return this.Bip01_R_Finger1;
                    case "Bip01 R Finger11":
                        return this.Bip01_R_Finger11;
                    case "Bip01 R Finger12":
                        return this.Bip01_R_Finger12;
                    case "Bip01 R Finger1Nub":
                        return this.Bip01_R_Finger1Nub;
                    case "Bip01 R Finger2":
                        return this.Bip01_R_Finger2;
                    case "Bip01 R Finger21":
                        return this.Bip01_R_Finger21;
                    case "Bip01 R Finger22":
                        return this.Bip01_R_Finger22;
                    case "Bip01 R Finger2Nub":
                        return this.Bip01_R_Finger2Nub;
                    case "Bip01 R Finger3":
                        return this.Bip01_R_Finger3;
                    case "Bip01 R Finger31":
                        return this.Bip01_R_Finger31;
                    case "Bip01 R Finger32":
                        return this.Bip01_R_Finger32;
                    case "Bip01 R Finger3Nub":
                        return this.Bip01_R_Finger3Nub;
                    case "Bip01 R Finger4":
                        return this.Bip01_R_Finger4;
                    case "Bip01 R Finger41":
                        return this.Bip01_R_Finger41;
                    case "Bip01 R Finger42":
                        return this.Bip01_R_Finger42;
                    case "Bip01 R Finger4Nub":
                        return this.Bip01_R_Finger4Nub;
                    case "Foretwist1_R":
                        return this.Foretwist1_R;
                    case "Foretwist_R":
                        return this.Foretwist_R;
                    case "Uppertwist1_R":
                        return this.Uppertwist1_R;
                    case "Uppertwist_R":
                        return this.Uppertwist_R;
                    case "Kata_R":
                        return this.Kata_R;
                    case "Kata_R_nub":
                        return this.Kata_R_nub;
                    case "Mune_L":
                        return this.Mune_L;
                    case "_IK_muneL":
                        return this._IK_muneL;
                    case "Mune_L_sub":
                        return this.Mune_L_sub;
                    case "Mune_L_nub":
                        return this.Mune_L_nub;
                    case "Mune_R":
                        return this.Mune_R;
                    case "_IK_muneR":
                        return this._IK_muneR;
                    case "Mune_R_sub":
                        return this.Mune_R_sub;
                    case "Mune_R_nub":
                        return this.Mune_R_nub;
                    case "body":
                        return this.body;
                    case "center":
                        return this.center;
                    case "Hara":
                        return this.Hara;
                    case "MuneL":
                        return this.MuneL;
                    case "MuneS":
                        return this.MuneS;
                    case "MuneTare":
                        return this.MuneTare;
                    case "RegFat":
                        return this.RegFat;
                    case "RegMeet":
                        return this.RegMeet;
                }
                return null;
            }
        }
        public class AnmJsonBoneData
        {
            string path { get; set; }
            public AnmJsonChannelData channels { get; set; }

            public AnmJsonBoneData()
            {
                path = "";
                channels = new AnmJsonChannelData();
            }
        }
        public class AnmJsonChannelData
        {
            [JsonProperty("100")]
            public List<AnmJsonFrameData> _100 { get; set; }
            [JsonProperty("101")]
            public List<AnmJsonFrameData> _101 { get; set; }
            [JsonProperty("102")]
            public List<AnmJsonFrameData> _102 { get; set; }
            [JsonProperty("103")]
            public List<AnmJsonFrameData> _103 { get; set; }
            [JsonProperty("104")]
            public List<AnmJsonFrameData> _104 { get; set; }
            [JsonProperty("105")]
            public List<AnmJsonFrameData> _105 { get; set; }
            [JsonProperty("106")]
            public List<AnmJsonFrameData> _106 { get; set; }

            public AnmJsonChannelData()
            {
                _100 = new List<AnmJsonFrameData>();
                _101 = new List<AnmJsonFrameData>();
                _102 = new List<AnmJsonFrameData>();
                _103 = new List<AnmJsonFrameData>();
                /*_104 = new List<AnmJsonFrameData>();
                _105 = new List<AnmJsonFrameData>();
                _106 = new List<AnmJsonFrameData>();*/
            }

            public AnmJsonFrameData getFrameOrNew(float frame, List<AnmJsonFrameData> channel)
            {
                for (int i = 0; i<channel.Count; i++)
                {
                    if(channel[i].frame == frame)
                    {
                        return channel[i];
                    }
                }

                return new AnmJsonFrameData();
            }
        }
        public class AnmJsonFrameData
        {
            public float frame { get; set; }
            public float f0 { get; set; }
            public float f1 { get; set; }
            public float f2 { get; set; }

            public AnmJsonFrameData()
            {
                frame = 0f;
                f0 = 0f;
                f1 = 0f;
                f2 = 0f;
            }
        }
    }

    public static class UIHelper
    {
        private static List<string> _animations;
        private static List<string> animations 
        { 
            get 
            { 
                if(_animations == null)
                {
                    _animations = new List<string>();
                    _animations.AddRange(GameUty.FileSystem.GetList("motion", AFileSystemBase.ListType.AllFile));
                    for(int i = _animations.Count - 1; i >= 0; i--)
                    {
                        if(!_animations[i].EndsWith(".anm"))
                        {
                            _animations.RemoveAt(i);
                        }
                    }
                    _animations.Sort();
                }

                return _animations;
            } 
        }
        public static void UI_5_DrawMotionControls(Maid maidMan, ref string anmAnimationSearch, ref string anmAnimationFilter, ref Vector2 anmAnimationScroll, ref string anmAnimationSelectedName, ref string anmTimeText, ref string timeStepText, ref bool timeStepPressed, ref List<float> autoFrames, ref bool autoPoseNext, ref AnmMaker.NewAnm newAnm, ref string infoMessage, int iconWidth)
        {
            Animation anim = maidMan.GetAnimation();
            AnimationState animState = null;

            //Animation selection
            {
                GUILayout.Label("Animation:");
                GUILayout.BeginHorizontal();
                {
                    //Setup
                    anmAnimationSearch = (anmAnimationSearch == null) ? "" : anmAnimationSearch;

                    //Search text
                    anmAnimationSearch = GUILayout.TextField(anmAnimationSearch);

                    //Search Button
                    if (GUILayout.Button("Search"))
                    {
                        anmAnimationFilter = anmAnimationSearch;
                    }
                }
                GUILayout.EndHorizontal();

                //List of Animations
                anmAnimationScroll = GUILayout.BeginScrollView(anmAnimationScroll, false, true, GUILayout.Width((iconWidth * 5) + 20), GUILayout.Height(Screen.height * 1 / 8));
                {
                    for (int i = 0; i < animations.Count; i++)
                    {
                        string nextAnmPath = animations[i];
                        string[] splitAnmPath = nextAnmPath.Split('\\');
                        if (anmAnimationFilter != null && !anmAnimationFilter.Equals("") && nextAnmPath.Contains(anmAnimationFilter))
                        {
                            if (GUILayout.Button(nextAnmPath))
                            {
                                if (anim != null)
                                {
                                    anmAnimationSelectedName = splitAnmPath[splitAnmPath.Length - 1];
                                    maidMan.body0.CrossFade(splitAnmPath[splitAnmPath.Length - 1], GameUty.FileSystem, false, true);
                                }
                                else
                                {
                                    anmAnimationSelectedName = null;
                                }
                            }
                        }
                    }
                }
                GUILayout.EndScrollView();
            }

            //GUI ENABLE
            {
                GUI.enabled = false;
                //If an animation was selected and it has been loaded into the Animation we can enable the following UI
                if (anmAnimationSelectedName != null && !anmAnimationSelectedName.Equals(""))
                {
                    //First check if animation exists
                    if (anim != null)
                    {
                        foreach (AnimationState anmState in anim)
                        {
                            if (anmState.name.Equals(anmAnimationSelectedName))
                            {
                                animState = anmState;
                                GUI.enabled = true;
                                break;
                            }
                        }
                    }

                }
            }

            //Timeline
            GUILayout.Label("Jump to Time:");
            GUILayout.BeginHorizontal();
            {
                //Setup
                anmTimeText = (anmTimeText == null) ? "0.00" : anmTimeText;
                float maxTime = (animState == null) ? 0f : animState.length;

                //Text Edit
                anmTimeText = GUILayout.TextField(anmTimeText);
                GUILayout.Label(@" / " + maxTime + " seconds");

                //Apply button
                if (GUILayout.Button("Apply"))
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(anmTimeText, @"[\+]?\d*\.?\d*") && float.Parse(anmTimeText) >= 0)
                    {
                        anmTimeText = Math.Min(float.Parse(anmTimeText), maxTime).ToString();
                        animState.time = float.Parse(anmTimeText);
                        animState.enabled = true;
                        anim.Sample();
                        animState.enabled = false;
                    }
                    else
                    {
                        infoMessage = "Invalid Frame Time float";
                    }
                }
            }
            GUILayout.EndHorizontal();

            //Pause
            {
                if (animState == null)
                {
                    GUILayout.Toggle(false, "Pause");
                }
                else
                {
                    animState.enabled = !GUILayout.Toggle(!animState.enabled, "Pause");
                }
            }

            //Time-Step Frames
            GUILayout.Label("Increment-Snap From: " + ((animState == null)? "" : (animState.time % animState.length).ToString()));
            GUILayout.BeginHorizontal();
            {
                //Setup
                timeStepText = (timeStepText == null) ? "0.00" : timeStepText;

                //Text Edit
                GUILayout.Label("Increment-Snap Every: ");
                timeStepText = GUILayout.TextField(timeStepText);
                GUILayout.Label(" Seconds");

                //Apply button
                if (GUILayout.Button("Auto-Step"))
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(timeStepText, @"[\+]?\d*\.?\d*") && float.Parse(timeStepText) > 0)
                    {
                        timeStepPressed = true;
                    }
                    else
                    {
                        infoMessage = "Invalid Time-Step float";
                    }
                }
            }
            GUILayout.EndHorizontal();

            //Auto-Frame
            if (maidMan.boMAN)
            {
                //Auto build frames from man's anm file
                if (GUILayout.Button("Auto-Snap Using Man .anm Frames"))
                {
                    newAnm.frames.Clear();
                    autoFrames = AnmMaker.readBoneFromBinary(anmAnimationSelectedName);
                    autoPoseNext = true;
                }
            }
            GUI.enabled = true;

            //Old
            {
                //Animation to pick

                //AnimationState animState = null;
                //bool temp;

                //#region
                //if (maidMan != null)
                //{
                //    anim = maidMan.GetAnimation();

                //    //First check if animation is playing
                //    if (anim != null)
                //    {
                //        foreach (AnimationState anmState in anim)
                //        {
                //            //Take the first enabled animation as the one we want
                //            if (anmState.enabled)
                //            {
                //                animState = anmState;
                //                anmStateName = anmState.name;
                //                anmPause = false;
                //                break;
                //            }
                //        }
                //    }

                //    //If no animation is playing, check if we paused an animation
                //    if (animState == null && anmStateName != null)
                //    {
                //        if (anim != null)
                //        {
                //            foreach (AnimationState anmState in anim)
                //            {
                //                if (anmState.name.Equals(anmStateName))
                //                {
                //                    animState = anmState;
                //                    break;
                //                }
                //            }
                //        }
                //    }
                //}

                //anmTimeText = (anmTimeText == null)? "0.00" : anmTimeText;
                //float maxTime = (animState != null)? animState.length : 0f;
                //GUI.enabled = (anim != null && animState != null);

                //#endregion

                //Pause
                //{
                //    temp = GUI.enabled;
                //    if (!anmPause)
                //    {
                //        GUI.enabled = (anim != null && animState != null);
                //    }

                //    //Toggle
                //    anmPause = GUILayout.Toggle(anmPause, "PAUSE");
                //    {
                //        //If pause was just pressed
                //        if (anmPause)
                //        {
                //            //Check the animation cache
                //            foreach (AnimationState anmState in anim)
                //            {
                //                //Disable every animation as double check
                //                if (anmState.enabled)
                //                {
                //                    anmState.enabled = false;
                //                    anmPause = true;
                //                    anmStateName = anmState.name;
                //                }
                //            }
                //        }
                //        else
                //        {
                //            //If resume was just pressed, this is harder
                //            //you dont actually know which animation in the cache is playing if its already paused
                //            animState.enabled = true;
                //            anmPause = false;
                //            anmStateName = null;
                //        }
                //    }

                //    GUI.enabled = temp;
                //}

                ////Time-Step Frames
                //{
                //    temp = GUI.enabled;
                //    GUI.enabled = (anim != null && animState != null);

                //    //Text Edit
                //    timeStepText = GUILayout.TextField(timeStepText);

                //    //Apply button
                //    if (GUILayout.Button("Time-Step"))
                //    {
                //        if (System.Text.RegularExpressions.Regex.IsMatch(timeStepText, @"[\+]?\d*\.?\d*"))
                //        {
                //            timeStepPressed = true;
                //        }
                //        else
                //        {
                //            infoMessage = "Invalid Time-Step float";
                //        }
                //    }

                //    GUI.enabled = temp;
                //}

                //GUI.enabled = true;
            }
        }
    }


    public class AnmShapeKeyPlayer : MonoBehaviour
    {
        public static string lastAnmLoadMaidGUID { get; set; }

        public static Dictionary<string, Dictionary<int, string>> channelToSK = new Dictionary<string, Dictionary<int, string>>() 
        { 
            { 
                "default", new Dictionary<int, string>() { {21, "munel" } }
            } 
        };

        public void UpdateShapeKey(AnmShapeKeyEvent skEvt)
        {
            if(skEvt == null)
            {
                UnityEngine.Debug.Log("Null skEvt");
            }

            if(GameMain.Instance == null)
            {
                UnityEngine.Debug.Log("Null inst");
            }

            if(GameMain.Instance.CharacterMgr == null)
            {
                UnityEngine.Debug.Log("Null charmgr");
            }
            //Check every maid
            int maidCount = GameMain.Instance.CharacterMgr.GetMaidCount();
            for(int i=0; i<maidCount; i++)
            {
                //Ensure active
                Maid maid = GameMain.Instance.CharacterMgr.GetMaid(i);
                if(maid!=null && maid.isActiveAndEnabled)
                {
                    if(maid.status.guid.Equals(skEvt.maidGuid))
                    {
                        UnityEngine.Debug.Log("Maid Found");

                        //Convert channel number to shapekey name
                        foreach (Dictionary<int, string> convertDict in channelToSK.Values)
                        {
                            if (skEvt.skName != null)
                            {
                                //for now use 0 which is always body -- need to update in future
                                TMorph morph = maid.body0.goSlot[0].morph;
                                if (morph != null)
                                {
                                    foreach (string key in morph.hash.Keys)
                                    {
                                        if (key.Equals(skEvt.skName))
                                        {
                                            UnityEngine.Debug.Log("SK Found");
                                            int f_nIdx = (int)morph.hash[(object)key];
                                            morph.SetBlendValues(f_nIdx, skEvt.value);
                                            break;
                                        }
                                    }
                                    morph.FixBlendValues();
                                }
                                break;
                            }
                            else
                            {
                                if (convertDict.ContainsKey(skEvt.channel))
                                {
                                    string skName = convertDict[skEvt.channel];

                                    UnityEngine.Debug.Log("Conversion Found");

                                    //for now use 0 which is always body -- need to update in future
                                    TMorph morph = maid.body0.goSlot[0].morph;
                                    if (morph != null)
                                    {
                                        foreach (string key in morph.hash.Keys)
                                        {
                                            if (key.Equals(skName))
                                            {
                                                UnityEngine.Debug.Log("SK Found");
                                                int f_nIdx = (int)morph.hash[(object)key];
                                                morph.SetBlendValues(f_nIdx, skEvt.value);
                                                break;
                                            }
                                        }
                                        morph.FixBlendValues();
                                    }
                                    break;
                                }
                            }
                        }

                        break;
                    }
                    ////Get the body's animation
                    //Animation anim = maid.GetAnimation();
                    //if(anim != null)
                    //{
                    //    //Get animation states
                    //    bool directMatch = false;
                    //    foreach (AnimationState anmState in anim)
                    //    {
                    //        if(skEvt.evt == null)
                    //        {
                    //            UnityEngine.Debug.Log("null evt");
                    //        }
                    //        if(skEvt.evt.animationState == null)
                    //        {
                    //            UnityEngine.Debug.Log("null evt st");
                    //        }
                    //        //If it matches the event's animation state -- may have to tweak to be name if reference doesnt work
                    //        if (anmState == skEvt.evt.animationState || anmState.name.Equals(skEvt.evt.animationState.name))
                    //        {
                    //            if(anmState == skEvt.evt.animationState)
                    //            {
                    //                UnityEngine.Debug.Log("Animation State Matched");
                    //            }
                    //            else
                    //            {
                    //                UnityEngine.Debug.Log("Animation State Name Matched....TODO need another way to narrow");
                    //            }
                                
                    //            //Convert channel number to shapekey name
                    //            foreach(Dictionary<int, string> convertDict in channelToSK.Values)
                    //            {
                    //                if(convertDict.ContainsKey(skEvt.channel))
                    //                {
                    //                    string skName = convertDict[skEvt.channel];

                    //                    //for now use 0 which is always body -- need to update in future
                    //                    TMorph morph = maid.body0.goSlot[0].morph;
                    //                    if (morph != null)
                    //                    {
                    //                        foreach (string key in morph.hash.Keys)
                    //                        {
                    //                            if (key.Equals(skName))
                    //                            {
                    //                                int f_nIdx = (int)morph.hash[(object)key];
                    //                                morph.SetBlendValues(f_nIdx, skEvt.value);
                    //                                break;
                    //                            }
                    //                        }
                    //                        morph.FixBlendValues();
                    //                    }
                    //                    break;
                    //                }
                    //            }
                                
                    //            directMatch = true;
                    //            break;
                    //        }
                    //    }

                    //    //If we got the actual state reference no need to check every maid
                    //    if(directMatch)
                    //    {
                    //        break;
                    //    }
                    //}
                    //else
                    //{
                    //    UnityEngine.Debug.Log("null anim");
                    //}
                }
            }
        }

        public class AnmShapeKeyEvent : ScriptableObject
        {
            public int channel { get; set; }
            public float value { get; set; }
            public AnimationEvent evt { get; set; }
            public string maidGuid { get; set; }
            public string skName { get; set; }

            public AnmShapeKeyEvent()
            {
                evt = null;
                maidGuid = null;
                skName = null;
            }
        }
    }
}