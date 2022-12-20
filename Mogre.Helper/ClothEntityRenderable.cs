using Mogre;
using Mogre.PhysX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mogre.PhysX.Helper
{
    public class ClothEntityRenderable
    {
        private Cloth cloth;
        private Entity entity;

        public Cloth Cloth
        {
            get { return cloth; }
        }

        public ClothEntityRenderable(Cloth cloth, Entity entity)
        {
            this.cloth = cloth;
            this.entity = entity;
        }

        public unsafe void Update(float deltaTime)
        {
            Mesh mesh = entity.GetMesh();
            for (ushort i = 0; i < mesh.NumSubMeshes; i++)
            {
                SubMesh subMesh = mesh.GetSubMesh(i);

                if (!subMesh.useSharedVertices)
                {
                    VertexData vd = subMesh.vertexData;
                    VertexElement posElement = vd.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.VES_POSITION);
                    HardwareVertexBufferSharedPtr buffer = vd.vertexBufferBinding.GetBuffer(posElement.Source);
                    byte* ptr = (byte*)buffer.Lock(HardwareBuffer.LockOptions.HBL_NORMAL);
                    float* arr = default(float*);
                    for (uint j = 0; j < vd.vertexCount; j++)
                    {
                        Vector3 vect = cloth.GetPosition(j);

                        posElement.BaseVertexPointerToElement(ptr, &arr);
                        *arr = vect.x;
                        arr[1] = vect.y;
                        arr[2] = vect.z;

                        ptr += buffer.VertexSize;
                    }

                    buffer.Unlock();
                }

                IndexData indexData = subMesh.indexData;
                HardwareIndexBufferSharedPtr indexBuf = indexData.indexBuffer;
                HardwareIndexBuffer.IndexType indexType = indexBuf.Type;
                uint* pLong = (uint*)(indexBuf.Lock(HardwareBuffer.LockOptions.HBL_NORMAL));
                ushort* pShort = (ushort*)pLong;

                for (uint j = 0; j < indexData.indexCount; j++)
                {
                }

                indexBuf.Unlock();
            }

            if (mesh.sharedVertexData != null)
            {
                VertexData vd = mesh.sharedVertexData;
                VertexElement posElement = vd.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.VES_POSITION);
                HardwareVertexBufferSharedPtr buffer = vd.vertexBufferBinding.GetBuffer(posElement.Source);
                byte* ptr = (byte*)buffer.Lock(HardwareBuffer.LockOptions.HBL_NORMAL);
                float* arr = default(float*);
                for (uint j = 0; j < vd.vertexCount; j++)
                {
                    Vector3 vect = cloth.GetPosition(j);

                    posElement.BaseVertexPointerToElement(ptr, &arr);
                    *arr = vect.x;
                    arr[1] = vect.y;
                    arr[2] = vect.z;

                    ptr += buffer.VertexSize;
                }

                buffer.Unlock();
            }

            mesh._setBounds(cloth.ValidBounds);
        }
    }
}
