using System;
using Mogre;

namespace Mogre.Helpers
{
    public class MeshBuilderHelper
    {
        protected MeshPtr mMeshPtr;
        protected SubMesh mSubMesh;
        protected string mName, mResourceGroup;
        protected uint mVertextStart, mVertexCount;
        protected HardwareVertexBufferSharedPtr mvbuf;
        protected uint offset;
        protected uint mVertexSize;
        protected uint mNumVerts;
        protected unsafe void* pVBuffStart = (void*)0;
        protected uint mTriagleCount;
        protected HardwareIndexBuffer.IndexType mIndexType;
        protected unsafe void* pIBuffStart = (void*)0;

        public MeshBuilderHelper(string name, string resourcegroup,
            bool usesharedvertices, uint vertexstart, uint vertexcount)
        {
            mName = name;
            mResourceGroup = resourcegroup;
            mVertextStart = vertexstart;
            mVertexCount = vertexcount;

            // Return now if already exists
            if (MeshManager.Singleton.ResourceExists(name))
                return;

            mMeshPtr = MeshManager.Singleton.CreateManual(mName, mResourceGroup);
            mSubMesh = mMeshPtr.CreateSubMesh();
            mSubMesh.useSharedVertices = usesharedvertices;
            mSubMesh.vertexData = new VertexData();
            mSubMesh.vertexData.vertexStart = mVertextStart;
            mSubMesh.vertexData.vertexCount = mVertexCount;
            offset = 0;
            mIndexType = HardwareIndexBuffer.IndexType.IT_16BIT;
        }

        public virtual VertexElement AddElement(VertexElementType theType,
                                                VertexElementSemantic semantic)
        {
            VertexElement ve = mSubMesh.vertexData.vertexDeclaration.AddElement(0, offset,
                                                                         theType, semantic);
            offset += VertexElement.GetTypeSize(theType);
            return ve;
        }

        public virtual void CreateVertexBuffer(uint numVerts, HardwareBuffer.Usage usage)
        {
            CreateVertexBuffer(numVerts, usage, false);
        }

        public virtual void CreateVertexBuffer(uint numVerts,
                                               HardwareBuffer.Usage usage,
                                               bool useShadowBuffer)
        {
            mVertexSize = offset;
            mNumVerts = numVerts;
            mvbuf = HardwareBufferManager.Singleton.CreateVertexBuffer(
                    mVertexSize, mNumVerts, usage, useShadowBuffer);
            unsafe
            {
                pVBuffStart = mvbuf.Lock(HardwareBuffer.LockOptions.HBL_DISCARD);
            }
        }

        public virtual void SetVertFloat(uint vertexindex, uint byteoffset, float val1)
        {
            if (vertexindex >= mNumVerts)
                throw new IndexOutOfRangeException(
                             "'vertexIndex' cannot be greater than the number of vertices.");
            unsafe
            {
                byte* pp = (byte*)pVBuffStart;
                pp += (mVertexSize * vertexindex) + byteoffset;
                float* p = (float*)pp;
                *p = val1;
            }
        }

        public virtual void SetVertFloat(uint vertexindex,
                                         uint byteoffset,
                                         float val1,
                                         float val2)
        {
            if (vertexindex >= mNumVerts)
                throw new IndexOutOfRangeException(
                                "'vertexIndex' cannot be greater than the number of vertices.");
            unsafe
            {
                byte* pp = (byte*)pVBuffStart;
                pp += (mVertexSize * vertexindex) + byteoffset;
                float* p = (float*)pp;
                *p++ = val1;
                *p = val2;
            }
        }

        public virtual void SetVertFloat(uint vertexindex,
                                         uint byteoffset,
                                         float val1,
                                         float val2,
                                         float val3)
        {
            if (vertexindex >= mNumVerts)
                throw new IndexOutOfRangeException(
                                "'vertexIndex' cannot be greater than the number of vertices.");
            unsafe
            {
                byte* pp = (byte*)pVBuffStart;
                pp += (mVertexSize * vertexindex) + byteoffset;
                float* p = (float*)pp;
                *p++ = val1;
                *p++ = val2;
                *p = val3;
            }
        }

        public virtual void SetVertFloat(uint vertexindex,
                                         uint byteoffset,
                                         float val1, float val2, float val3, float val4)
        {
            if (vertexindex >= mNumVerts)
                throw new IndexOutOfRangeException(
                             "'vertexIndex' cannot be greater than the number of vertices.");
            unsafe
            {
                byte* pp = (byte*)pVBuffStart;
                pp += (mVertexSize * vertexindex) + byteoffset;
                float* p = (float*)pp;
                *p++ = val1;
                *p++ = val2;
                *p++ = val3;
                *p = val4;
            }
        }

        public virtual void CreateIndexBuffer(uint triaglecount,
                                                HardwareIndexBuffer.IndexType itype,
                                                HardwareBuffer.Usage usage)
        {
            CreateIndexBuffer(triaglecount, itype, usage, false);
        }

        public virtual void CreateIndexBuffer(uint triaglecount,
                                               HardwareIndexBuffer.IndexType itype,
                                                HardwareBuffer.Usage usage,
                                                bool useShadowBuffer)
        {
            mvbuf.Unlock();
            mTriagleCount = triaglecount;
            mIndexType = itype;
            mSubMesh.vertexData.vertexBufferBinding.SetBinding(0, mvbuf);
            mSubMesh.indexData.indexCount = mTriagleCount * 3;
            HardwareIndexBufferSharedPtr ibuf = HardwareBufferManager.Singleton
                .CreateIndexBuffer(mIndexType, mTriagleCount * 3, usage, useShadowBuffer);
            mSubMesh.indexData.indexBuffer = ibuf;
            unsafe
            {
                pIBuffStart = ibuf.Lock(HardwareBuffer.LockOptions.HBL_DISCARD);
            }
        }

        public virtual void SetIndex16bit(uint triagleIdx,
                                          ushort vidx1, ushort vidx2, ushort vidx3)
        {
            if (triagleIdx >= mTriagleCount)
                throw new IndexOutOfRangeException(
                              "'triagleIdx' cannot be greater than the number of triangles.");
            if (mIndexType != HardwareIndexBuffer.IndexType.IT_16BIT)
                throw new NotSupportedException(
                              "HardwareIndexBuffer.IndexType other than 'IT_16BIT' is not supported.");
            unsafe
            {
                ushort* p = (ushort*)pIBuffStart;
                p += (triagleIdx * 3);
                *p++ = vidx1;
                *p++ = vidx2;
                *p = vidx3;
            }
        }

        public virtual void SetIndex32bit(uint triagleIdx,
                                          uint vidx1, uint vidx2, uint vidx3)
        {
            if (triagleIdx >= mTriagleCount)
                throw new IndexOutOfRangeException(
                            "'triagleIdx' cannot be greater than the number of triangles.");
            if (mIndexType != HardwareIndexBuffer.IndexType.IT_16BIT)
                throw new NotSupportedException(
                            "HardwareIndexBuffer.IndexType other than 'IT_16BIT' is not supported.");
            unsafe
            {

                uint* p = (uint*)pIBuffStart;
                p += (triagleIdx * 3);
                *p++ = vidx1;
                *p++ = vidx2;
                *p = vidx3;
            }
        }
        public virtual MeshPtr Load(string materialname)
        {
            mSubMesh.indexData.indexBuffer.Unlock();
            mSubMesh.SetMaterialName(materialname);
            mMeshPtr.Load();
            return mMeshPtr;
        }

        #region Protected and Private fields

        #endregion

    }
}