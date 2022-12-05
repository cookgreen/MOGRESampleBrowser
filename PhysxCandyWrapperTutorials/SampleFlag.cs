using Mogre;
using Mogre.PhysX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhysxCandyWrapperTutorials
{
    public class SampleFlag : Sample
    {
        private SceneManager sceneManager;
        private Camera camera;

        private Scene scene;
        private Physics physics;

        public override string Name { get { return "PhysxSample_Cloth"; } }
        public override string Thumb { get { return "Thumb_Cloth.png"; } }
        public override string Desc { get { return "PhysxSample_Cloth"; } }

        public override void Start()
        {
            base.Start();

            trayManager.destroyAllWidgets();

            trayManager.setListener(this);

            sceneManager = root.CreateSceneManager(SceneType.ST_GENERIC);
            ColourValue cvAmbineLight = new ColourValue(0.7f, 0.7f, 0.7f);
            sceneManager.AmbientLight = cvAmbineLight;

            camera = sceneManager.CreateCamera("TutorialCam");

            camera.SetPosition(0, 0, 800);
            camera.LookAt(new Vector3(0, 0, 0));
            camera.NearClipDistance = 1;

            camera.AspectRatio = viewport.ActualWidth / viewport.ActualHeight;

            viewport.Camera = camera;

            mouse.MouseMoved += Mouse_MouseMoved;
            mouse.MousePressed += Mouse_MousePressed;
            mouse.MouseReleased += Mouse_MouseReleased;
            keyboard.KeyPressed += Keyboard_KeyPressed;
            keyboard.KeyReleased += Keyboard_KeyReleased;

            setupPhysics();
        }

        private void setupPhysics()
        {
            physics = Physics.Create();

            SceneDesc sceneDesc = new SceneDesc();
            sceneDesc.Gravity = new Vector3(0, -9.8f, 0);
            scene = physics.CreateScene(sceneDesc);

            Mogre.PhysX.Material sceneMat = scene.Materials[0];
            sceneMat.Restitution = 0.1f;
            sceneMat.StaticFriction = 0.9f;
            sceneMat.DynamicFriction = 0.5f;


        }

        public override void Stop()
        {
            mouse.MouseMoved -= Mouse_MouseMoved;
            mouse.MousePressed -= Mouse_MousePressed;
            mouse.MouseReleased -= Mouse_MouseReleased;
            keyboard.KeyPressed -= Keyboard_KeyPressed;
            keyboard.KeyReleased -= Keyboard_KeyReleased;
        }

        protected bool Keyboard_KeyReleased(MOIS.KeyEvent arg)
        {
            return true;
        }

        protected bool Keyboard_KeyPressed(MOIS.KeyEvent arg)
        {
            return true;
        }

        protected bool Mouse_MouseReleased(MOIS.MouseEvent arg, MOIS.MouseButtonID id)
        {
            trayManager.injectMouseUp(arg, id);

            return true;
        }

        protected bool Mouse_MousePressed(MOIS.MouseEvent arg, MOIS.MouseButtonID id)
        {
            trayManager.injectMouseDown(arg, id);

            return true;
        }

        protected bool Mouse_MouseMoved(MOIS.MouseEvent arg)
        {
            trayManager.injectMouseMove(arg);

            return true;
        }
    }
}
