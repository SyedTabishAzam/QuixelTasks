//Disclaimer - Help taken on certain functions from github
using MeshSplitting.MeshTools;
using MeshSplitting.SplitterMath;
using System;
using UnityEngine;

namespace MeshSplitting.Splitables
{
  
    public class Splitable : MonoBehaviour, ISplitable
    {
        //Speed of rotation on arrow click
        public float speed = 40f;


        private Transform _transform;

        private PlaneMath _splitPlane;
        private MeshContainer[] _meshContainerStatic;
        private IMeshSplitter[] _meshSplitterStatic;
        private MeshContainer[] _meshContainerSkinned;
        private IMeshSplitter[] _meshSplitterSkinned;

        private bool _isSplitting = false;
        private bool _splitMesh = false;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private void Update()
        {
            //rotate mesh
            if (Input.GetKey(KeyCode.LeftArrow))
                transform.RotateAround(transform.position, Vector3.up, speed * Time.deltaTime);
            if (Input.GetKey(KeyCode.RightArrow))
                transform.RotateAround(transform.position, Vector3.up, -speed * Time.deltaTime);

            if (_splitMesh)
            {
                //If its in split mode
                _splitMesh = false;

                bool anySplit = false;

                
                for (int i = 0; i < _meshContainerStatic.Length; i++)
                {
                    _meshContainerStatic[i].MeshInitialize();
                    _meshContainerStatic[i].CalculateWorldSpace();

                    // split mesh
                    _meshSplitterStatic[i].MeshSplit();

                    if (_meshContainerStatic[i].IsMeshSplit())
                    {
                        anySplit = true;
                        //Create a closed mesh
                        _meshSplitterStatic[i].MeshCreateCaps();
                    }
                }

                for (int i = 0; i < _meshContainerSkinned.Length; i++)
                {
                    _meshContainerSkinned[i].MeshInitialize();
                    _meshContainerSkinned[i].CalculateWorldSpace();

                    // split mesh
                    _meshSplitterSkinned[i].MeshSplit();

                    if (_meshContainerSkinned[i].IsMeshSplit())
                    {
                        anySplit = true;
                        //Create a closed mesh
                        _meshSplitterSkinned[i].MeshCreateCaps();
                    }
                }

                if (anySplit) CreateNewObjects();
                _isSplitting = false;
            }
        }

        public void Split(Transform splitTransform)
        {
            if (!_isSplitting)
            {
                //If a cut is made then get the cut plane
                _isSplitting = _splitMesh = true;
                _splitPlane = new PlaneMath(splitTransform);

                MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
                SkinnedMeshRenderer[] skinnedRenderes = GetComponentsInChildren<SkinnedMeshRenderer>();

                _meshContainerStatic = new MeshContainer[meshFilters.Length];
                _meshSplitterStatic = new IMeshSplitter[meshFilters.Length];

                //Loop through each mesh filter and intitialze a capped mesh and save it in container
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    _meshContainerStatic[i] = new MeshContainer(meshFilters[i]);

                    _meshSplitterStatic[i] = (IMeshSplitter)new MeshSplitterConcave(_meshContainerStatic[i], _splitPlane, splitTransform.rotation);

                }

                _meshSplitterSkinned = new IMeshSplitter[skinnedRenderes.Length];
                _meshContainerSkinned = new MeshContainer[skinnedRenderes.Length];
                //Although this function is rendundant here but it is important - it gets the bones and joints of the object incase the object is bendable
                for (int i = 0; i < skinnedRenderes.Length; i++)
                {
                    _meshContainerSkinned[i] = new MeshContainer(skinnedRenderes[i]);

                    _meshSplitterSkinned[i] =  (IMeshSplitter)new MeshSplitterConcave(_meshContainerSkinned[i], _splitPlane, splitTransform.rotation);

                }
            }
        }

        private void CreateNewObjects()
        {
            //For each side of sliced sphere create a new object

            
            Transform parent = _transform.parent;
            if (parent == null)
            {
                //Attach each created object to a single parent in the hierarchy
                GameObject go = new GameObject("Parent: " + gameObject.name);
                parent = go.transform;
                parent.position = Vector3.zero;
                parent.rotation = Quaternion.identity;
                parent.localScale = Vector3.one;
            }

            Mesh origMesh = GetMeshOnGameObject(gameObject);
            Rigidbody ownBody = null;
            float ownMass = 100f;
            float ownVolume = 1f;

            if (origMesh != null)
            {
                //For each created object apply same physics as on parent
                ownBody = GetComponent<Rigidbody>();
          
                if (ownBody != null) ownMass = ownBody.mass;
                Vector3 ownMeshSize = origMesh.bounds.size;
                ownVolume = ownMeshSize.x * ownMeshSize.y * ownMeshSize.z;
            }

            GameObject[] newGOs = new GameObject[2];
          
            //Create new game objects
            newGOs[0] = Instantiate(gameObject) as GameObject;
            newGOs[0].name = gameObject.name;
            newGOs[1] = gameObject;

            float higherY = 0;
            int selectedChild = 0;
            for (int i = 0; i < 2; i++)
            {
                //Each split will only create two objects 
                UpdateMeshesInChildren(i, newGOs[i]);

                Transform newTransform = newGOs[i].GetComponent<Transform>();
                if(transform.position.y > higherY)
                {
                    higherY = transform.position.y;
                    selectedChild = i;
                }
               
                newTransform.parent = parent;

                Mesh newMesh = GetMeshOnGameObject(newGOs[i]);
                if (newMesh != null)
                {
                    //For each game object add new mesh colider
                    MeshCollider newCollider = newGOs[i].GetComponent<MeshCollider>();
                    if (newCollider != null)
                    {
                        newCollider.sharedMesh = newMesh;
                        newCollider.convex = false;

                        // if hull has less than 255 polygons set convex, Unity limit!
                        if (newCollider.convex && newMesh.triangles.Length > 765)
                            newCollider.convex = false;
                    }

                    Rigidbody newBody = newGOs[i].GetComponent<Rigidbody>();
                    if (ownBody != null && newBody != null)
                    {
                        Vector3 newMeshSize = newMesh.bounds.size;
                        float meshVolume = newMeshSize.x * newMeshSize.y * newMeshSize.z;
                        float newMass = ownMass * (meshVolume / ownVolume);

                        newBody.useGravity = ownBody.useGravity;
                        newBody.mass = newMass;
                        newBody.velocity = ownBody.velocity;
                        newBody.angularVelocity = ownBody.angularVelocity;
                        

                    }
                }

                PostProcessObject(newGOs[i]);
            }

            //Increase Y for object which is higher
            newGOs[selectedChild].GetComponent<Transform>().position += Vector3.up;

        }

        private void UpdateMeshesInChildren(int i, GameObject go)
        {
            if (_meshContainerStatic.Length > 0)
            {
                MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
                for (int j = 0; j < _meshContainerStatic.Length; j++)
                {
                    Renderer renderer = meshFilters[j].GetComponent<Renderer>();
                   
                    if (i == 0)
                    {
                        if (_meshContainerStatic[j].HasMeshUpper() & _meshContainerStatic[j].HasMeshLower())
                        {
                            meshFilters[j].mesh = _meshContainerStatic[j].CreateMeshUpper();
                        }
                        else if (!_meshContainerStatic[j].HasMeshUpper())
                        {
                            if (renderer != null) Destroy(renderer);
                            Destroy(meshFilters[j]);
                        }
                    }
                    else
                    {
                        if (_meshContainerStatic[j].HasMeshUpper() & _meshContainerStatic[j].HasMeshLower())
                        {
                            meshFilters[j].mesh = _meshContainerStatic[j].CreateMeshLower();
                        }
                        else if (!_meshContainerStatic[j].HasMeshLower())
                        {
                            if (renderer != null) Destroy(renderer);
                            Destroy(meshFilters[j]);
                        }
                    }
                }
            }

            if (_meshContainerSkinned.Length > 0)
            {
                SkinnedMeshRenderer[] skinnedRenderer = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int j = 0; j < _meshContainerSkinned.Length; j++)
                {
                    if (i == 0)
                    {
                        if (_meshContainerSkinned[j].HasMeshUpper() & _meshContainerSkinned[j].HasMeshLower())
                        {
                            skinnedRenderer[j].sharedMesh = _meshContainerSkinned[j].CreateMeshUpper();
                        }
                        else if (!_meshContainerSkinned[j].HasMeshUpper())
                        {
                            Destroy(skinnedRenderer[j]);
                        }
                    }
                    else
                    {
                        if (_meshContainerSkinned[j].HasMeshUpper() & _meshContainerSkinned[j].HasMeshLower())
                        {
                            skinnedRenderer[j].sharedMesh = _meshContainerSkinned[j].CreateMeshLower();
                        }
                        else if (!_meshContainerSkinned[j].HasMeshLower())
                        {
                            Destroy(skinnedRenderer[j]);
                        }
                    }
                }
            }
        }

        private Material[] GetSharedMaterials(GameObject go)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                return skinnedMeshRenderer.sharedMaterials;
            }
            else
            {
                Renderer renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                {
                    return renderer.sharedMaterials;
                }
            }

            return null;
        }

        private void SetSharedMaterials(GameObject go, Material[] materials)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.sharedMaterials = materials;
            }
            else
            {
                Renderer renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }

        private void SetMeshOnGameObject(GameObject go, Mesh mesh)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.sharedMesh = mesh;
            }
            else
            {
                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    meshFilter.mesh = mesh;
                }
            }
        }

        private Mesh GetMeshOnGameObject(GameObject go)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                return skinnedMeshRenderer.sharedMesh;
            }
            else
            {
                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    return meshFilter.mesh;
                }
            }

            return null;
        }

        protected virtual void PostProcessObject(GameObject go) { }

        void OnGUI()
        {
            GUILayout.Label("Press and hold space to enter slice mode");
            GUILayout.Label("In slice mode - use mouse click to draw a line from A to B to cut the sphere (min distance = 5)");
            GUILayout.Label("W-S-A-D to move the camera");
            GUILayout.Label("Left/Right to rotate the sphere");
            GUILayout.Label("Middle mouse button to rotate the camera");
        }
    }
}
