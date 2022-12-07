using Mogre;
using Mogre_Procedural.MogreBites;
using MOIS;
using SampleInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.ogre.framework;

namespace PhysxCandyWrapperTutorials
{
    public class Sample : SdkTrayListener, ISample
    {
        private Mogre.Vector3 camTranslateVector;
        private float moveScale;
        private bool isRightMouseDown;

        protected Root root;
        protected Viewport viewport;
        protected SdkTrayManager trayManager;
        protected Mouse mouse;
        protected Keyboard keyboard;
        protected SceneManager sceneManager;
        protected Camera camera;

        protected List<ActorSceneNode> actorSceneNodes;
        protected List<Overlay> hiddenOverlays;

        public event Action SampleStopped;

        public virtual string Name { get { return ""; } }
        public virtual string Desc { get { return ""; } }
        public virtual string Thumb { get { return ""; } }

        public Sample()
        {
            moveScale = 1;

            hiddenOverlays = new List<Overlay>();
            actorSceneNodes = new List<ActorSceneNode>();
        }

        public virtual void Start()
        {
            sceneManager = root.CreateSceneManager(SceneType.ST_GENERIC);
            camera = sceneManager.CreateCamera("TutorialCam");
            camera.AspectRatio = viewport.ActualWidth / viewport.ActualHeight;
            viewport.Camera = camera;

            mouse.MouseMoved += Mouse_MouseMoved;
            mouse.MousePressed += Mouse_MousePressed;
            mouse.MouseReleased += Mouse_MouseReleased;
            keyboard.KeyPressed += Keyboard_KeyPressed;
            keyboard.KeyReleased += Keyboard_KeyReleased;
        }

        public virtual void Pause()
        {
            trayManager.hideAll();
        }

        public virtual void Resume()
        {
            trayManager.showAll();
        }

        public virtual void Stop()
        {
            mouse.MouseMoved -= Mouse_MouseMoved;
            mouse.MousePressed -= Mouse_MousePressed;
            mouse.MouseReleased -= Mouse_MouseReleased;
            keyboard.KeyPressed -= Keyboard_KeyPressed;
            keyboard.KeyReleased -= Keyboard_KeyReleased;

            SampleStopped?.Invoke();
        }

        public void Setup(object[] parameters)
        {
            root = parameters[0] as Root;
            viewport = parameters[1] as Viewport;
            trayManager = parameters[2] as SdkTrayManager; 
            mouse = parameters[3] as Mouse;
            keyboard = parameters[4] as Keyboard;
        }

        protected virtual bool Keyboard_KeyReleased(MOIS.KeyEvent arg)
        {
            camTranslateVector = new Mogre.Vector3();

            return true;
        }

        protected virtual bool Keyboard_KeyPressed(MOIS.KeyEvent arg)
        {
            return true;
        }

        protected virtual bool Mouse_MouseReleased(MOIS.MouseEvent arg, MOIS.MouseButtonID id)
        {
            trayManager.injectMouseUp(arg, id);

            if (id == MouseButtonID.MB_Right)
            {
                isRightMouseDown = false;
            }

            return true;
        }

        protected virtual bool Mouse_MousePressed(MOIS.MouseEvent arg, MOIS.MouseButtonID id)
        {
            trayManager.injectMouseDown(arg, id);

            if (id == MouseButtonID.MB_Right)
            {
                isRightMouseDown = true;
            }

            return true;
        }

        protected virtual bool Mouse_MouseMoved(MOIS.MouseEvent arg)
        {
            trayManager.injectMouseMove(arg);

            if (isRightMouseDown)
            {
                Degree deCameraYaw = new Degree(arg.state.X.rel * -0.1f);
                camera.Yaw(deCameraYaw);
                Degree deCameraPitch = new Degree(arg.state.Y.rel * -0.1f);
                camera.Pitch(deCameraPitch);
            }


            return true;
        }

        public virtual void Update(float timeSinceLastFrame)
        {
            getInput();

            moveCamera();

            foreach(var actorNode in actorSceneNodes)
            {
                actorNode.Update(timeSinceLastFrame);
            }
        }

        public void moveCamera()
        {
            if (keyboard.IsKeyDown(KeyCode.KC_LSHIFT))
                camera.MoveRelative(camTranslateVector);
            camera.MoveRelative(camTranslateVector / 10);
        }
        public void getInput()
        {
            if (keyboard.IsKeyDown(KeyCode.KC_A))
                camTranslateVector.x = -moveScale;

            if (keyboard.IsKeyDown(KeyCode.KC_D))
                camTranslateVector.x = moveScale;

            if (keyboard.IsKeyDown(KeyCode.KC_W))
                camTranslateVector.z = -moveScale;

            if (keyboard.IsKeyDown(KeyCode.KC_S))
                camTranslateVector.z = moveScale;

            if (keyboard.IsKeyDown(KeyCode.KC_Q))
                camTranslateVector.y = -moveScale;

            if (keyboard.IsKeyDown(KeyCode.KC_E))
                camTranslateVector.y = moveScale;

            //camera roll
            if (keyboard.IsKeyDown(KeyCode.KC_Z))
            {
                camera.Roll(new Angle(-moveScale));
            }

            if (keyboard.IsKeyDown(KeyCode.KC_X))
            {
                camera.Roll(new Angle(moveScale));
            }

            //reset roll
            if (keyboard.IsKeyDown(KeyCode.KC_C))
                camera.Roll(-(camera.RealOrientation.Roll));
        }
    }
}
