using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Generates a mesh for an <see cref="ARFace"/>.
    /// </summary>
    /// <remarks>
    /// If this <c>GameObject</c> has a <c>MeshFilter</c> and/or <c>MeshCollider</c>,
    /// this component will generate a mesh from the underlying <c>XRFace</c>.
    /// </remarks>
    [RequireComponent(typeof(ARFace))]
    public sealed class ARFaceMeshVisualizerCustom : MonoBehaviour
    {
        /// <summary>
        /// Get the <c>Mesh</c> that this visualizer creates and manages.
        /// </summary>
        public Mesh mesh { get; private set; }

        [SerializeField]
        GameObject m_EyePrefab;


        public GameObject eyePrefab
        {
            get => m_EyePrefab;
            set => m_EyePrefab = value;
        }

        GameObject m_LeftEyeGameObject;
        GameObject m_RightEyeGameObject;

        // [SerializeField]
        // GameObject m_CamFPVPrefab;

        // public GameObject camFPVPrefab
        // {
        //     get => m_CamFPVPrefab;
        //     set => m_CamFPVPrefab = value;
        // }

        // GameObject m_camFPVGameObject;

        public GameObject FPVCam;



        void CreateEyeGameObjectsIfNecessary()
        {
            if (m_Face.leftEye != null && m_LeftEyeGameObject == null )
            {
                m_LeftEyeGameObject = Instantiate(m_EyePrefab, m_Face.leftEye);
                m_LeftEyeGameObject.SetActive(false);
            }
            if (m_Face.rightEye != null && m_RightEyeGameObject == null)
            {
                m_RightEyeGameObject = Instantiate(m_EyePrefab, m_Face.rightEye);
                m_RightEyeGameObject.SetActive(false);
            }
            // if(m_Face.rightEye != null && m_Face.leftEye != null && m_camFPVGameObject == null)
            // {

            //     m_camFPVGameObject = Instantiate(m_CamFPVPrefab);

                



            //     m_camFPVGameObject.SetActive(false);

                

            // }
        }

        void SetVisible(bool visible)
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
            if (m_MeshRenderer == null)
            {
                return;
            }

            
            if (m_LeftEyeGameObject != null && m_RightEyeGameObject != null)
            {
                m_LeftEyeGameObject.SetActive(visible);
                m_RightEyeGameObject.SetActive(visible);
                //m_camFPVGameObject.SetActive(visible);
            }

            if(visible && (m_LeftEyeGameObject == null && m_RightEyeGameObject == null))
                CreateEyeGameObjectsIfNecessary();
            m_MeshRenderer.enabled = visible;
        }

        void SetMeshTopology()
        {
            if (mesh == null)
            {
                return;
            }

            using (new ScopedProfiler("SetMeshTopology"))
            {
                using (new ScopedProfiler("ClearMesh"))
                mesh.Clear();

                if (m_Face.vertices.Length > 0 && m_Face.indices.Length > 0)
                {
                    using (new ScopedProfiler("SetVertices"))
                    mesh.SetVertices(m_Face.vertices);

                    using (new ScopedProfiler("SetIndices"))
                    mesh.SetIndices(m_Face.indices, MeshTopology.Triangles, 0, false);

                    using (new ScopedProfiler("RecalculateBounds"))
                    mesh.RecalculateBounds();

                    if (m_Face.normals.Length == m_Face.vertices.Length)
                    {
                        using (new ScopedProfiler("SetNormals"))
                        mesh.SetNormals(m_Face.normals);
                    }
                    else
                    {
                        using (new ScopedProfiler("RecalculateNormals"))
                        mesh.RecalculateNormals();
                    }
                }

                if (m_Face.uvs.Length > 0)
                {
                    using (new ScopedProfiler("SetUVs"))
                    mesh.SetUVs(0, m_Face.uvs);
                }

                var meshFilter = GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    meshFilter.sharedMesh = mesh;
                }

                var meshCollider = GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    meshCollider.sharedMesh = mesh;
                }

                m_TopologyUpdatedThisFrame = true;
            }
        }

        void UpdateVisibility()
        {
            var visible = enabled &&
                (m_Face.trackingState != TrackingState.None) &&
                (ARSession.state > ARSessionState.Ready);

            SetVisible(visible);
        }

        // void SetCamFPVPosition()
        // {
        //     m_camFPVGameObject.transform.position = Vector3.Lerp(m_Face.leftEye.transform.position, m_Face.rightEye.transform.position, 0.5f);
        //     //m_camFPVGameObject.transform.rotation = Quaternion.Slerp(m_Face.leftEye.rotation, m_Face.rightEye.rotation, 0.5f);

        //     m_camFPVGameObject.transform.rotation = m_Face.transform.rotation;
        // }

        void SetCamFPVPosition()
        {
            FPVCam.transform.position = Vector3.Lerp(m_Face.leftEye.transform.position, m_Face.rightEye.transform.position, 0.5f);
            //m_camFPVGameObject.transform.rotation = Quaternion.Slerp(m_Face.leftEye.rotation, m_Face.rightEye.rotation, 0.5f);

            FPVCam.transform.rotation = m_Face.transform.rotation;
        }

        void OnUpdated(ARFaceUpdatedEventArgs eventArgs)
        {
            UpdateVisibility();
            CreateEyeGameObjectsIfNecessary();

            SetCamFPVPosition();

            if (!m_TopologyUpdatedThisFrame)
            {
                SetMeshTopology();
            }
            m_TopologyUpdatedThisFrame = false;
        }

        void OnSessionStateChanged(ARSessionStateChangedEventArgs eventArgs)
        {
            UpdateVisibility();
        }

        void Awake()
        {
            mesh = new Mesh();
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_Face = GetComponent<ARFace>();
        }

        void OnEnable()
        {
            m_Face.updated += OnUpdated;
            ARSession.stateChanged += OnSessionStateChanged;
            UpdateVisibility();
        }

        void OnDisable()
        {
            m_Face.updated -= OnUpdated;
            ARSession.stateChanged -= OnSessionStateChanged;
        }

        ARFace m_Face;
        MeshRenderer m_MeshRenderer;
        bool m_TopologyUpdatedThisFrame;
    }
}
