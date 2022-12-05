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
        ISample sample;
        public override void Enter()
        {
            sample = UserData as ISample;
            sample.Start();
        }

        public override void Exit()
        {
            sample.Stop();
        }
    }
}
