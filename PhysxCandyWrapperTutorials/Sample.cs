using Mogre;
using Mogre_Procedural.MogreBites;
using MOIS;
using SampleInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhysxCandyWrapperTutorials
{
    public class Sample : SdkTrayListener, ISample
    {
        protected Root root;
        protected Viewport viewport;
        protected SdkTrayManager trayManager;
        protected Mouse mouse;
        protected Keyboard keyboard;

        public virtual string Name { get { return ""; } }
        public virtual string Desc { get { return ""; } }
        public virtual string Thumb { get { return ""; } }

        public Sample()
        {
        }

        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
        }

        public void Setup(object[] parameters)
        {
            root = parameters[0] as Root;
            viewport = parameters[1] as Viewport;
            trayManager = parameters[2] as SdkTrayManager; 
            mouse = parameters[3] as Mouse;
            keyboard = parameters[4] as Keyboard;
        }
    }
}
