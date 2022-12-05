using System;
using System.Collections.Generic;
using System.Text;
using Mogre;
using MOIS;
using Mogre_Procedural.MogreBites;
using org.ogre.framework;
using Vector3 = Mogre.Vector3;
using SampleInterface;
using System.Linq;
using System.IO;
using System.Reflection;
using Type = System.Type;

namespace SampleBrowser
{
    public class MenuState : AppState
    {
        protected bool m_bQuit;

        private SelectMenu sampleChooseMenu;
        private Label lbSampleTitle;
        private TextBox txtSampleDesc;
        private Slider sliderSample;
        private List<ISample> samples;
        private List<OverlayContainer> sampleThumbOverlyContainers;
        private float carouselPlace;
        private string selectedSampleName;

        public MenuState()
        {
            m_bQuit = false;
            frameEvent = new FrameEvent();
        }
        public override void Enter()
        {
            FontManager.Singleton.GetByName("SdkTrays/Caption").Load();

            OgreFramework.Instance.log.LogMessage("Entering MainMenu...");
            m_bQuit = false;

            samples = new List<ISample>();
            string fullPath = Environment.CurrentDirectory;
            DirectoryInfo di = new DirectoryInfo(fullPath);
            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Extension == ".dll")
                {
                    try
                    {
                        var assembly = Assembly.LoadFile(file.FullName);
                        Type[] types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            if (type.GetInterface("ISample") != null && type.Name != "Sample")
                            {
                                ISample sample = assembly.CreateInstance(type.FullName) as ISample;
                                sample.Setup(new object[] { 
                                    OgreFramework.Instance.root,
                                    OgreFramework.Instance.viewport,
                                    OgreFramework.Instance.trayMgr,
                                    OgreFramework.Instance.mouse,
                                    OgreFramework.Instance.keyboard,
                                });
                                samples.Add(sample);
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            sampleThumbOverlyContainers = new List<OverlayContainer>();

            sceneMgr = OgreFramework.Instance.root.CreateSceneManager(Mogre.SceneType.ST_GENERIC, "MainMenuSceneMgr");
            ColourValue cvAmbineLight = new ColourValue(0.7f, 0.7f, 0.7f);
            sceneMgr.AmbientLight = cvAmbineLight;
 
            camera = sceneMgr.CreateCamera("MainMenuCamera");
            camera.SetPosition(0, 0, 800);
            camera.LookAt(new Vector3(0, 0, 0));
            camera.NearClipDistance = 1;

            camera.AspectRatio = OgreFramework.Instance.viewport.ActualWidth / OgreFramework.Instance.viewport.ActualHeight;

            OgreFramework.Instance.viewport.Camera = camera;

            OgreFramework.Instance.trayMgr.showFrameStats(TrayLocation.TL_BOTTOMLEFT);
            OgreFramework.Instance.trayMgr.showLogo(TrayLocation.TL_BOTTOMRIGHT);
            OgreFramework.Instance.trayMgr.showCursor();

            OgreFramework.Instance.trayMgr.destroyAllWidgets();

            createScene();

            setupUI();

            setupModMenu();

            OgreFramework.Instance.mouse.MouseMoved += mouseMoved;
            OgreFramework.Instance.mouse.MousePressed += mousePressed;
            OgreFramework.Instance.mouse.MouseReleased += mouseReleased;
            OgreFramework.Instance.keyboard.KeyPressed += keyPressed;
            OgreFramework.Instance.keyboard.KeyReleased += keyReleased;

            OgreFramework.Instance.root.FrameRenderingQueued += FrameRenderingQueued;
        }

        private bool FrameRenderingQueued(FrameEvent evt)
        {
            selectedSampleName = sampleChooseMenu.getSelectedItem();
            float carouselOffset = sampleChooseMenu.getSelectionIndex() - carouselPlace;
            if ((carouselOffset <= 0.001) && (carouselOffset >= -0.001)) carouselPlace = sampleChooseMenu.getSelectionIndex();
            else carouselPlace += carouselOffset * SampleBrowser.Helper.Clamp<float>(evt.timeSinceLastFrame * 15.0f, -1.0f, 1.0f);

            for (int i = 0; i < sampleThumbOverlyContainers.Count; i++)
            {
                float thumbOffset = carouselPlace - i;
                float phase = (thumbOffset / 2.0f) - 2.8f;

                if (thumbOffset < -5 || thumbOffset > 4)    // prevent thumbnails from wrapping around in a circle
                {
                    sampleThumbOverlyContainers[i].Hide();
                    continue;
                }
                else
                {
                    sampleThumbOverlyContainers[i].Show();
                }

                float left = Mogre.Math.Cos(phase) * 200.0f;
                float top = Mogre.Math.Sin(phase) * 200.0f;
                float scale = 1.0f / Convert.ToSingle(System.Math.Pow((Mogre.Math.Abs(thumbOffset) + 1.0f), 0.75f));

                OverlayContainer.ChildContainerIterator iter = sampleThumbOverlyContainers[i].GetChildContainerIterator();
                BorderPanelOverlayElement frame = (BorderPanelOverlayElement)iter.ElementAt(0);

                sampleThumbOverlyContainers[i].SetDimensions(128.0f * scale, 96.0f * scale);
                frame.SetDimensions(sampleThumbOverlyContainers[i].Width + 16.0f, sampleThumbOverlyContainers[i].Height + 16.0f);
                sampleThumbOverlyContainers[i].SetPosition((left - 80.0f - (sampleThumbOverlyContainers[i].Width / 2.0f)),
                    (top - 5.0f - (sampleThumbOverlyContainers[i].Height / 2.0f)));

                if (i == sampleChooseMenu.getSelectionIndex())
                {
                    frame.BorderMaterialName = "SdkTrays/Frame/Over";
                }
                else
                {
                    frame.BorderMaterialName = "SdkTrays/Frame";
                }
            }

            OgreFramework.Instance.trayMgr.frameRenderingQueued(evt);

            return true;
        }

        private void setupUI()
        {
            OgreFramework.Instance.trayMgr.destroyAllWidgets();

            OgreFramework.Instance.trayMgr.createLabel(TrayLocation.TL_TOP, "lbSampleBrowser", "Sample Browser", 200);
            
            lbSampleTitle = OgreFramework.Instance.trayMgr.createLabel(TrayLocation.TL_LEFT, "lbSampleInfo", "Sample Info");
            lbSampleTitle.setCaption("Sample Info");
            
            txtSampleDesc = OgreFramework.Instance.trayMgr.createTextBox(TrayLocation.TL_LEFT, "txtSampleInfo", "Sample Info", 250, 208);
            txtSampleDesc.setCaption("Sample Info");
            
            sampleChooseMenu = OgreFramework.Instance.trayMgr.createThickSelectMenu(TrayLocation.TL_LEFT, "smSample", "Select Sample", 250, 10);
            sampleChooseMenu.setCaption("Select Sample");
            sampleChooseMenu.setItems(samples.Select(o => o.Name).ToStringVector());
            
            sliderSample = OgreFramework.Instance.trayMgr.createThickSlider(TrayLocation.TL_LEFT, "slderSamples", "Slider Samples", 250, 80, 0, 0, 0);
            sliderSample.setCaption("Slider Samples");
            if (samples.Count > 0)
            {
                lbSampleTitle.setCaption(sampleChooseMenu.getSelectedItem());
            }

            OgreFramework.Instance.trayMgr.createSeparator(TrayLocation.TL_RIGHT, "LogoSep");
            OgreFramework.Instance.trayMgr.createButton(TrayLocation.TL_RIGHT, "btnStartSample", "Start", 160);
            OgreFramework.Instance.trayMgr.createButton(TrayLocation.TL_RIGHT, "btnQuit", "Quit", 160);
        }

        public void createScene()
        {
            sceneMgr.SetSkyBox(true, "Examples/SpaceSkyBox", 5000);

            Light light = sceneMgr.CreateLight();
            light.Type = Light.LightTypes.LT_POINT;
            light.Position = new Vector3(-250, 200, 0);
            light.SetSpecularColour(255, 255, 255);
        }

        public override void Exit()
        {
            OgreFramework.Instance.log.LogMessage("Leaving MainMenu...");

            OgreFramework.Instance.mouse.MouseMoved -= mouseMoved;
            OgreFramework.Instance.mouse.MousePressed -= mousePressed;
            OgreFramework.Instance.mouse.MouseReleased -= mouseReleased;
            OgreFramework.Instance.keyboard.KeyPressed -= keyPressed;
            OgreFramework.Instance.keyboard.KeyReleased -= keyReleased;

            sceneMgr.DestroyCamera(camera);
            if (sceneMgr != null)
                OgreFramework.Instance.root.DestroySceneManager(sceneMgr);

            foreach(var thumbSampleContainer in sampleThumbOverlyContainers)
            {
                OgreFramework.Instance.trayMgr.getTraysLayer().Remove2D(thumbSampleContainer);
            }

            OgreFramework.Instance.trayMgr.clearAllTrays();
            OgreFramework.Instance.trayMgr.destroyAllWidgets();
            OgreFramework.Instance.trayMgr.setListener(null);
        }

        public bool keyPressed(KeyEvent keyEventRef)
        {
            if(OgreFramework.Instance.keyboard.IsKeyDown(MOIS.KeyCode.KC_ESCAPE))
            {
                m_bQuit = true;
                return true;
            }

            OgreFramework.Instance.KeyPressed(keyEventRef);
            return true;
        }
        public bool keyReleased(KeyEvent keyEventRef)
        {
            OgreFramework.Instance.KeyReleased(keyEventRef);
            return true;
        }

        public bool mouseMoved(MouseEvent arg)
        {
            if (arg.state.Z.rel != 0 && sampleChooseMenu.getNumItems() != 0)
            {
                float newIndex = sampleChooseMenu.getSelectionIndex() - arg.state.Z.rel / Mogre.Math.Abs((float)arg.state.Z.rel);
                float finalIndex = SampleBrowser.Helper.Clamp<float>(newIndex, 0.0f, (float)(sampleChooseMenu.getNumItems() - 1));

                sampleChooseMenu.selectItem((uint)finalIndex);
                lbSampleTitle.setCaption(samples[(int)finalIndex].Name);

                txtSampleDesc.setText(samples[(int)finalIndex].Desc);

                selectedSampleName = sampleChooseMenu.getSelectedItem();
            }

            if (OgreFramework.Instance.trayMgr.injectMouseMove(arg)) return true;
            return true;
        }
        public bool mousePressed(MouseEvent evt, MouseButtonID id)
        {
            if (OgreFramework.Instance.trayMgr.injectMouseDown(evt, id)) return true;
            return true;
        }
        public bool mouseReleased(MouseEvent evt, MouseButtonID id)
        {
            if (OgreFramework.Instance.trayMgr.injectMouseUp(evt, id)) return true;
            return true;
        }

        public override void buttonHit(Button button)
        {
            if (button.getName() == "btnQuit")
            {
                m_bQuit = true;
            }
            else if (button.getName() == "btnStartSample")
            {
                AppState sampleState = findByName("SampleState");
                sampleState.UserData = samples.Where(o => o.Name == selectedSampleName).FirstOrDefault();
                changeAppState(sampleState);
            }
        }

        public override void Update(double timeSinceLastFrame)
        {
            frameEvent.timeSinceLastFrame = (float)timeSinceLastFrame;
            OgreFramework.Instance.trayMgr.frameRenderingQueued(frameEvent);

            if (m_bQuit == true)
            {
                shutdown();
                return;
            }
        }


        private void setupModMenu()
        {
            MaterialPtr thumbMat = MaterialManager.Singleton.Create("ModThumbnail", "General");
            thumbMat.GetTechnique(0).GetPass(0).CreateTextureUnitState();
            MaterialPtr templateMat = MaterialManager.Singleton.GetByName("ModThumbnail");

            foreach (var sample in samples)
            {
                string name = "ThumbSample" + (sampleThumbOverlyContainers.Count + 1).ToString();

                MaterialPtr newMat = templateMat.Clone(name);

                TextureUnitState tus = newMat.GetTechnique(0).GetPass(0).GetTextureUnitState(0);
                if (ResourceGroupManager.Singleton.ResourceExists("General", sample.Thumb))
                {
                    tus.SetTextureName(sample.Thumb);
                }
                else
                {
                    tus.SetTextureName("thumb_error.png");
                }

                BorderPanelOverlayElement bp = (BorderPanelOverlayElement)
                        OverlayManager.Singleton.CreateOverlayElementFromTemplate("SdkTrays/Picture", "BorderPanel", (name));


                bp.HorizontalAlignment = GuiHorizontalAlignment.GHA_RIGHT;
                bp.VerticalAlignment = GuiVerticalAlignment.GVA_CENTER;
                bp.MaterialName = name;
                OgreFramework.Instance.trayMgr.getTraysLayer().Add2D(bp);

                sampleThumbOverlyContainers.Add(bp);
            }
        }
    }
}
