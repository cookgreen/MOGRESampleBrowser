using Mogre;
using Mogre.PhysX;
using MOIS;
using org.ogre.framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samples
{
    public class SampleNewtonsCradle : Sample
    {
        private Scene scene;
        private Physics physx;

        private const float centery = 2;
        private const float stringlength = 4.08f;
        private const float radius = 1.0f;
        private const float planesize = 4.5f;

        public override string Name { get { return "SamplePhysx_NewtonsCradle"; } }
        public override string Thumb { get { return "Thumb_NewtonsCradle.png"; } }
        public override string Desc { get { return "Sample for the Newton's Cradle"; } }

        public SampleNewtonsCradle()
        {
            physx = Physics.Create();
            SceneDesc desc = new SceneDesc();
            scene = physx.CreateScene(desc);
            scene.Gravity = new Mogre.Vector3(0, -9.81f, 0);

            scene.Gravity = new Mogre.Vector3(0, -9.81f, 0);
            var defm = scene.Materials[0];
            defm.Restitution = 0.5f;
            defm.DynamicFriction = defm.StaticFriction = 0.6f;

            scene.CreateActor(new ActorDesc(new PlaneShapeDesc()));
        }

        public override void Start()
        {
            base.Start();

            trayManager.destroyAllWidgets();
            trayManager.setListener(this);

            sceneManager = root.CreateSceneManager(SceneType.ST_GENERIC, "PhysxNewtonCradleMgr");
            ColourValue cvAmbineLight = new ColourValue(0.7f, 0.7f, 0.7f);
            sceneManager.AmbientLight = cvAmbineLight;

            camera = sceneManager.CreateCamera("PhysxNewtonCradleCamera");
            camera.NearClipDistance = 5;
            camera.FarClipDistance = 999;
            camera.AspectRatio = viewport.ActualWidth / viewport.ActualHeight;

            viewport.Camera = camera;

            string[] mats = new string[]
            {
                "BaseRed",
                "BaseGreen",
                "BaseBlue",
                "BaseBlack",
                "BasePurple"
            };

            var physicsMat = scene.CreateMaterial(new MaterialDesc(0.0f, 0.0f, 1.0f));

            var sphereShapeDesc = new SphereShapeDesc(radius);
            sphereShapeDesc.SkinWidth = 0.0f;
            sphereShapeDesc.MaterialIndex = physicsMat.Index;

            var sphereActorDesc = new ActorDesc(new BodyDesc(0.124f, 1.0f), 150.1f, sphereShapeDesc);

            for (int i = -2; i <= 2; i++)
            {
                sphereActorDesc.GlobalPosition = new Mogre.Vector3(2 * radius * i, centery, 0);
                var sphereActor = scene.CreateActor(sphereActorDesc);

                var revoluteJointDesc = new RevoluteJointDesc();
                revoluteJointDesc.JointFlags = JointFlags.CollisionEnabled | JointFlags.Visualization;
                revoluteJointDesc.SetActors(null, sphereActor);
                revoluteJointDesc.GlobalAnchor = sphereActorDesc.GlobalPosition + new Mogre.Vector3(0, stringlength, 0);
                revoluteJointDesc.GlobalAxis = new Mogre.Vector3(0, 0, 1);

                scene.CreateJoint(revoluteJointDesc);
            }

            PhysxExpansion.CreateSphere("SPHERE", radius, sceneManager, 32, 32);

            foreach (var actor in scene.Actors)
            {
                if (!actor.IsDynamic)
                    continue;

                var sphereEnt = sceneManager.CreateEntity("Sphere_" + Guid.NewGuid(), "SPHERE");
                var sphereSceneNode = sceneManager.RootSceneNode.CreateChildSceneNode();
                sphereSceneNode.AttachObject(sphereEnt);
                sphereSceneNode.Position = actor.GlobalPosition;
                actorSceneNodes.Add(new ActorSceneNode(actor, sphereSceneNode));
            }

            for (int i = -2; i <= 2; i++)
            {
                var actor = scene.Actors[i + 3];
                var pos = actor.GlobalPosition;

                SceneNode actorSceneNode = actorSceneNodes.Where(o => o.Actor.Name == actor.Name).FirstOrDefault().SceneNode;

                ManualObject lineObject = sceneManager.CreateManualObject();
                SceneNode lineObjectNode = actorSceneNode.CreateChildSceneNode();
                lineObject.Begin(mats[i + 2], RenderOperation.OperationTypes.OT_LINE_STRIP);

                lineObject.Position(pos.x, pos.y, pos.z);
                lineObject.Position(2 * radius * i, stringlength + centery, -3);
                lineObject.Position(pos.x, pos.y, pos.z);
                lineObject.Position(2 * radius * i, stringlength + centery, 3);

                lineObject.End();

                lineObjectNode.Position = new Mogre.Vector3(
                    pos.x - 4 * radius * i,
                    pos.y - stringlength,
                    pos.z
                );
                lineObjectNode.AttachObject(lineObject);
            }

            ManualObject panelObject = sceneManager.CreateManualObject();
            SceneNode panelObjectNode = sceneManager.RootSceneNode.CreateChildSceneNode();
            panelObject.Begin("BaseWhiteNoLighting", RenderOperation.OperationTypes.OT_TRIANGLE_LIST);
            panelObject.Position(planesize, stringlength + centery, planesize);
            panelObject.Position(-planesize, stringlength + centery, planesize);
            panelObject.Position(-planesize, stringlength + centery, -planesize);
            panelObject.Position(planesize, stringlength + centery, -planesize);
            panelObject.Quad(0, 1, 2, 3);
            panelObject.End();
            panelObjectNode.AttachObject(panelObject);
            actorSceneNodes.Add(new ActorSceneNode(scene.Actors[0], panelObjectNode));

            var actorPos = scene.Actors[0].GlobalPosition;

            //Look At the Cradles
            camera.Position = new Mogre.Vector3(
                actorPos.x,
                actorPos.y - 2,
                actorPos.z + 15
                );
            camera.Pitch(new Radian(new Degree(20)));

            scene.Simulate(0);
        }

        public override void Stop()
        {
            base.Stop();
        }

        protected override bool Keyboard_KeyPressed(KeyEvent arg)
        {
            if (arg.key == KeyCode.KC_B)
            {
                scene.Actors[1].AddForce(new Mogre.Vector3(-7, 0, 0), ForceModes.VelocityChange);
            }

            return base.Keyboard_KeyPressed(arg);
        }

        public override void Update(float timeSinceLastFrame)
        {
            scene.FlushStream();
            scene.FetchResults(SimulationStatuses.AllFinished, true);
            scene.Simulate(timeSinceLastFrame);

            base.Update(timeSinceLastFrame);
        }
    }
}
