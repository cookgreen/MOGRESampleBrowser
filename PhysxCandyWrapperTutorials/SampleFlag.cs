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
        private Actor planeActor;
        private Scene scene;
        private Physics physics;
        private SelectMenu clothSelectMenu;

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

            clothSelectMenu = trayManager.createLongSelectMenu(TrayLocation.TL_RIGHT, "MoveMethod", "Method", 300, 200, 5);
            clothSelectMenu.addItem("setPosition");
            clothSelectMenu.addItem("attachVertexToGlobalPosition");
            clothSelectMenu.addItem("addForceAtVertex");

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

            planeActor = scene.CreateActor(new ActorDesc(new PlaneShapeDesc()));

            makeBox(new Vector3(0, 0.5f, 0.5f));

            makeCloth(new Vector3(5, 4, 0));
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
            Vector3 pos = barPosition;
            Vector2 clothSize = new Vector2(8, 4);

            pos.x -= clothSize.x * 0.5f;

            Vector3 holderPos = pos;
            holderPos.x += clothSize.x * 0.5f; ;
            holderPos.y += 0.05f;
            holderPos.z -= 0.05f;

            BodyDesc holsterBodyDesc = new BodyDesc();
            holsterBodyDesc.Mass = 10;
            BoxShapeDesc holderShapeDesc = new BoxShapeDesc();
            holderShapeDesc.Dimensions = new Vector3(0.5f * 10, 0.5f * 0.1f, 0.5f * 10.1f);

            ActorDesc holsterActorDesc = new ActorDesc();
            holsterActorDesc.Body = null;
            holsterActorDesc.Shapes.Add(holderShapeDesc);
            Actor holsterActor = scene.CreateActor(holsterActorDesc);
            holsterActor.GlobalPosition = holderPos;

            Entity holsterEnt = sceneManager.CreateEntity("cube.mesh");
            SceneNode holsterNode = sceneManager.RootSceneNode.CreateChildSceneNode();
            holsterNode.AttachObject(holsterEnt);
            holsterNode.Scale(0.1f, 0.001f, 0.001f);

            ActorSceneNode holsterActorSceneNode = new ActorSceneNode(holsterActor, holsterNode);
            actorSceneNodes.Add(holsterActorSceneNode);


            BodyDesc clothBodyDesc = new BodyDesc();
            clothBodyDesc.Mass = 0;
            ActorDesc clothActorDesc = new ActorDesc();
            clothActorDesc.Body = holsterBodyDesc;
            BoxShapeDesc clothBoxShapeDesc = new BoxShapeDesc();
            clothActorDesc.Shapes.Add(clothBoxShapeDesc);
            Actor clothActor = scene.CreateActor(clothActorDesc);

            clothActor.GlobalPosition = pos;

            ClothDesc cd = new ClothDesc();
            MeshManager.Singleton.CreatePlane("flag",
            ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, new Plane(Vector3.UNIT_Z, 0), clothSize.x, clothSize.y, 50, 50, true, 1, 1, 1, Vector3.UNIT_Y);
            
            Entity clothEnt = sceneManager.CreateEntity("Flag", "flag");
            clothEnt.SetMaterialName("wales");

            SceneNode clothSceneNode = sceneManager.RootSceneNode.CreateChildSceneNode();
            clothSceneNode.AttachObject(clothEnt);

            MeshPtr mp = clothEnt.GetMesh();
            StaticMeshData meshdata = new StaticMeshData(mp);

            cd.ClothMesh = CookClothMesh(meshdata, "nug.xcl");
            cd.Thickness = 0.2f;
            cd.Friction = 0.5f;
            cd.GlobalPose.SetTrans(pos);

            Cloth cloth = scene.CreateCloth(cd);
            cloth.AttachToShape(planeActor.Shapes[0], ClothAttachmentFlags.Twoway);

            ActorSceneNode clothActorSceneNode = new ActorSceneNode(clothActor, clothSceneNode);
            actorSceneNodes.Add(clothActorSceneNode);

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
