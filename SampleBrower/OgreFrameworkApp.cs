using System;
using System.Collections.Generic;
using System.Text;
using org.ogre.framework;

namespace SampleBrowser
{
    public class OgreFrameworkApp : IDisposable
    {
        public OgreFrameworkApp()
        {
            appStateManager = null;
        }

        public void Start()
        {
            if (!OgreFramework.Instance.InitOgre("MOGRE Sample Browser", null, "SampleBrowser", "resources.cfg"))
		        return;

            OgreFramework.Instance.log.LogMessage("SampleBrowser initialized!");

            appStateManager = new AppStateManager();

            AppState.Create<MenuState>(appStateManager, "MainMenu");
            AppState.Create<SampleState>(appStateManager, "SampleState");

            appStateManager.Start(appStateManager.FindByName("MainMenu"));
        }

        public void Dispose()
        {
            appStateManager = null;
        }

        private AppStateManager appStateManager;
    }
}
