using Mogre_Procedural.MogreBites;
using org.ogre.framework;
using SampleInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleBrowser
{
    public class SampleState : AppState
    {
        private bool isQuit;
        private ISample sample;

        private Button btnResumeSample;
        private Button btnStopSample;
        private Button btnQuitApp;

        public SampleState()
        {
            isQuit = false;
        }

        public override void Enter()
        {
            sample = UserData as ISample;
            sample.SampleStopped += SampleStopped;
            sample.Start();

            OgreFramework.Instance.keyboard.KeyPressed += Keyboard_KeyPressed;
        }

        public override void Resume()
        {
        }

        public override void Exit()
        {
            sample.Stop();
        }

        private bool Keyboard_KeyPressed(MOIS.KeyEvent arg)
        {
            if (arg.key == MOIS.KeyCode.KC_ESCAPE)
            {
                isQuit = true;
            }

            return true;
        }

        private void SampleStopped()
        {
            changeAppState(findByName("MainMenu"));
        }

        public override void Update(double timeSinceLastFrame)
        {
            sample.Update((float)timeSinceLastFrame);

            if (isQuit)
            {
                shutdown();
                return;
            }
        }
    }
}
