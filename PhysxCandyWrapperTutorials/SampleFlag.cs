using Mogre;
using Mogre.PhysX;
using Mogre_Procedural.MogreBites;
using org.ogre.framework;
using SampleInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PhysxCandyWrapperTutorials
{
    public class SampleFlag : Sample
    {
        private Scene scene;
        private Physics physics;
        private SelectMenu curtainSelectMenu;

        private ClothEntityRenderable clothEntRenderable;

        public override string Name { get { return "PhysxSample_Cloth"; } }
        public override string Thumb { get { return "Thumb_Cloth.png"; } }
        public override string Desc { get { return "PhysxSample_Cloth"; } }

        public override void Start()
        {
            base.Start();

            trayManager.destroyAllWidgets();

            trayManager.setListener(this);

            ColourValue cvAmbineLight = new ColourValue(16.0f / 255.0f, 16.0f / 255.0f, 16.0f / 255.0f);
            viewport.BackgroundColour = cvAmbineLight;
            sceneManager.SetFog(FogMode.FOG_EXP, cvAmbineLight, 0.001f, 800, 1000);

            sceneManager.ShadowTechnique = ShadowTechnique.SHADOWTYPE_TEXTURE_MODULATIVE;
            sceneManager.ShadowColour = new ColourValue(0.5f, 0.5f, 0.5f);
            sceneManager.SetShadowTextureSize(1024);
            sceneManager.ShadowTextureCount = 1;

            MeshManager.Singleton.CreatePlane("floor", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, 
                new Plane(Vector3.UNIT_Y, 0), 1000, 1000, 1, 1, true, 1, 1, 1, Vector3.UNIT_Z);

            Entity floor = sceneManager.CreateEntity("floor", "floor");
            floor.SetMaterialName("ground-from-nxogre.org");
            floor.CastShadows = false;
            sceneManager.RootSceneNode.AttachObject(floor);

            sceneManager.AmbientLight = new ColourValue(0.3f, 0.3f, 0.3f);

            Light light = sceneManager.CreateLight();
            light.Type = Light.LightTypes.LT_POINT;
            light.Position = new Vector3(-10, 40, 20);
            light.SpecularColour = ColourValue.White;

            camera.SetPosition(10, 10, 10);
            camera.LookAt(new Vector3(0, 0, 0));
            camera.NearClipDistance = 0.02f;
            camera.FarClipDistance = 1000.0f;

            curtainSelectMenu = trayManager.createLongSelectMenu(TrayLocation.TL_RIGHT, "MoveMethod", "Method", 300, 200, 5);
            curtainSelectMenu.addItem("setPosition");
            curtainSelectMenu.addItem("attachVertexToGlobalPosition");
            curtainSelectMenu.addItem("addForceAtVertex");

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

            scene.CreateActor(new ActorDesc(new PlaneShapeDesc()));

            makeBox(new Vector3(0, 0.5f, 0.5f));

            makeCloth(new Vector3(0, 3, 0));
        }

        private void makeBox(Vector3 globalPose)
        {
            BodyDesc bodyDesc = new BodyDesc();
            bodyDesc.Mass = 40.0f;
            bodyDesc.LinearVelocity = Vector3.ZERO;

            BoxShapeDesc boxShapeDesc = new BoxShapeDesc();
            boxShapeDesc.Dimensions = new Vector3(0.5f * 1, 0.5f * 1, 0.5f * 1);

            ActorDesc actorDesc = new ActorDesc();
            actorDesc.Body = bodyDesc;
            actorDesc.Shapes.Add(boxShapeDesc);
            Actor actor = scene.CreateActor(actorDesc);
            actor.GlobalPosition = globalPose;

            Entity entity = sceneManager.CreateEntity("cube.1m.mesh");
            SceneNode node = sceneManager.RootSceneNode.CreateChildSceneNode();
            node.AttachObject(entity);

            ActorSceneNode actorSceneNode = new ActorSceneNode(actor, node);
            actorSceneNodes.Add(actorSceneNode);
        }

        private void makeCloth(Vector3 barPosition)
        {
            Vector3 clothPos = barPosition;
            Vector2 clothSize = new Vector2(8, 4);

            clothPos.x -= clothSize.x * 0.5f;

            Vector3 holderPos = clothPos;
            holderPos.x += clothSize.x * 0.5f; ;
            holderPos.y += 0.05f;
            holderPos.z -= 0.05f;

            BodyDesc holsterBodyDesc = new BodyDesc();
            holsterBodyDesc.Mass = 10;
            BoxShapeDesc holderShapeDesc = new BoxShapeDesc();
            holderShapeDesc.Dimensions = new Vector3(0.5f * 10, 0.5f * 0.1f, 0.5f * 10.1f);

            ActorDesc holsterActorDesc = new ActorDesc();
            holsterActorDesc.Body = holsterBodyDesc;
            holsterActorDesc.Shapes.Add(holderShapeDesc);
            Actor holsterActor = scene.CreateActor(holsterActorDesc);
            holsterActor.GlobalPosition = holderPos;

            Entity holsterEnt = sceneManager.CreateEntity("cube.mesh");
            SceneNode holsterNode = sceneManager.RootSceneNode.CreateChildSceneNode();
            holsterNode.AttachObject(holsterEnt);
            //holsterNode.Scale(0.1f, 0.001f, 0.001f);

            ActorSceneNode holsterActorSceneNode = new ActorSceneNode(holsterActor, holsterNode);
            actorSceneNodes.Add(holsterActorSceneNode);


            D6JointDesc d6Desc = new D6JointDesc();
            d6Desc.SetActors(null, holsterActor);
            d6Desc.GlobalAnchor = holderPos;
            d6Desc.GlobalAxis = new Vector3(1, 0, 0);
            d6Desc.TwistMotion = D6JointMotions.Locked;
            d6Desc.Swing1Motion = D6JointMotions.Locked;
            d6Desc.Swing2Motion = D6JointMotions.Locked;
            d6Desc.XMotion = D6JointMotions.Free;
            d6Desc.YMotion = D6JointMotions.Locked;
            d6Desc.ZMotion = D6JointMotions.Locked;
            scene.CreateJoint(d6Desc);

            ClothDesc cd = new ClothDesc();
            MeshManager.Singleton.CreatePlane("curtain",
            ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, new Plane(Vector3.UNIT_Z, 0), clothSize.x, clothSize.y, 50, 50, true, 1, 1, 1, Vector3.NEGATIVE_UNIT_X);
            
            Entity clothEnt = sceneManager.CreateEntity("Curtain", "curtain");
            clothEnt.SetMaterialName("wales");
            SceneNode clothSceneNode = holsterNode.CreateChildSceneNode();
            clothSceneNode.AttachObject(clothEnt);

            MeshPtr mp = clothEnt.GetMesh();
            StaticMeshData meshdata = new StaticMeshData(mp);

            cd.ClothMesh = CookClothMesh(meshdata, "nug.xcl");
            cd.Thickness = 0.2f;
            cd.Friction = 0.5f;
            //cd.GlobalPose.SetTrans(clothPos);

            Cloth cloth = scene.CreateCloth(cd);
            cloth.AttachToShape(holsterActor.Shapes[0], ClothAttachmentFlags.Twoway);

            clothEntRenderable = new ClothEntityRenderable(cloth, clothEnt);
        }

        private ClothMesh CookClothMesh(StaticMeshData meshdata, string cookingTarget)
        {
            CookingInterface.InitCooking();
            using (var cd = new ClothMeshDesc())
            {
                cd.PinPoints<float>(meshdata.Points, 0, 12);
                cd.PinTriangles<uint>(meshdata.Indices, 0, 12);

                cd.VertexCount = (uint)meshdata.Vertices.Length;
                cd.TriangleCount = (uint)meshdata.Indices.Length / 3;

                string xclFullPath = Path.Combine(Environment.CurrentDirectory, "media", cookingTarget);

                if (File.Exists(xclFullPath)) File.Delete(xclFullPath);

                FileStream xclStream = new FileStream(xclFullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                ClothMesh clothMesh = null;
                
                if (CookingInterface.CookClothMesh(cd, xclStream))
                {
                    xclStream.Close();
                    DataStreamPtr dsp = ResourceGroupManager.Singleton.OpenResource(cookingTarget);
                    clothMesh = physics.CreateClothMesh(Helper.DataPtrToStream(dsp));
                }

                CookingInterface.CloseCooking();

                xclStream.Close();
                return clothMesh;
            }
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void Update(float timeSinceLastFrame)
        {
            clothEntRenderable.Update(timeSinceLastFrame);

            scene.FlushStream();
            scene.FetchResults(SimulationStatuses.AllFinished, true);
            scene.Simulate(timeSinceLastFrame);

            base.Update(timeSinceLastFrame);
        }
    }
}
