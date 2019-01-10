using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VaalsTreeGenerator
{

    public class NodeTreeGenerator : MonoBehaviour
    {
        public int numIterations = 2;
        public Vector2Int minmaxBranches = new Vector2Int(4, 4);
        public Vector2 minmaxPos = new Vector2(0.5f, 1f);
        public float branchAngle;
        public Vector3 scaleVector = new Vector3(0.5f, 0.5f, 0.75f);
        public float visualScaleFactor = 6f; //This is used if the visual is parented to an empty object, set it to visual's local z
        public float leafScaleFactor = 2f;
        public float animatedAngleAmplitude = 40f;
        public float animatedFrequency = 1f;

        private GameObject TrunkPrefab, LeafPrefab;
        private List<Node> nodeList = new List<Node>();
        private Node rootNode;

        public LayerMask LeafLayer, TrunkLayer;
        // Use this for initialization
        void Start()
        {
            TrunkPrefab = Resources.Load<GameObject>("Tree/Trunk");
            LeafPrefab = Resources.Load<GameObject>("Tree/Leaf");
            StartCoroutine(GenerateTree());
        }

        public IEnumerator GenerateTree()
        {
            nodeList = new List<Node>();

            rootNode = new Node(null, transform.position, transform.localScale, transform.rotation);

            Node curNode = rootNode;
            List<Node> stack = new List<Node>();
            stack.Add(rootNode);
            nodeList.Add(rootNode);

            //Keep running until there are no more nodes in the list
            while (stack.Count > 0)
            {
                curNode = stack[0];
                //first spawn a trunk
                curNode.SpawnTrunk(TrunkPrefab);
                if (curNode.subID < numIterations)
                {
                    curNode.Subdivide(curNode, minmaxBranches, minmaxPos, scaleVector, branchAngle, visualScaleFactor);
                    foreach (Node nn in curNode.subNodes)
                    {
                        stack.Add(nn);
                        nodeList.Add(nn);
                    }
                }
                else
                {
                    //If we don't subdivide, we can spawn a leaf
                    curNode.SpawnLeaf(LeafPrefab, visualScaleFactor, leafScaleFactor);
                }
                stack.Remove(curNode);

            }
            yield return null;

        }


        private void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(r, out hit, 100, LeafLayer))
                {
                    foreach (Node n in nodeList)
                    {
                        if (n.leaf != null && n.leaf.gameObject == hit.collider.gameObject)
                        {
                            Destroy(n.leaf);
                            n.Subdivide(n, minmaxBranches, minmaxPos, scaleVector, branchAngle, visualScaleFactor);
                            foreach (Node subN in n.subNodes)
                            {
                                nodeList.Add(subN);
                                subN.SpawnTrunk(TrunkPrefab);
                                subN.SpawnLeaf(LeafPrefab, visualScaleFactor, leafScaleFactor);
                            }
                            break;
                        }

                    }


                }

            }

            if (Input.GetMouseButtonDown(1))
            {
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(r, out hit, 100, TrunkLayer))
                {
                    foreach (Node n in nodeList)
                    {
                        if (n != rootNode && n.trunk != null && n.trunk.gameObject == hit.collider.gameObject.transform.parent.gameObject)
                        {
                            nodeList = n.CullNode(nodeList);
                            break;

                        }

                    }

                }

            }

        }
        public void FixedUpdate()
        {
            Animate();
        }

        public void Animate()
        {

            foreach (Node n in nodeList)
            {
                if (n.trunk != null)
                {
                    float amp = animatedAngleAmplitude;
                    if (n.parent != null)
                    {
                        n.trunk.transform.position = n.parent.trunk.transform.position + n.parent.trunk.transform.forward * (n.position - n.parent.position).magnitude;
                    }

                    n.trunk.transform.localRotation = n.rotation * Quaternion.Euler(amp * Mathf.Sin(animatedFrequency * Time.time + n.trunk.transform.position.x), 0, 0);
                    if (n.IsLeaf())
                    {
                        n.UpdateLeafPosition();
                    }
                }

            }
        }
    }



    public class Node
    {
        public int subID;
        public Node parent;
        public List<Node> subNodes = new List<Node>();
        private bool isLeaf;

        public GameObject trunk;
        public GameObject leaf;

        public Vector3 position, endPos;
        public Vector3 size;
        public Quaternion rotation;

        public Node() { }
        public Node(Node parent, Vector3 position, Vector3 size, Quaternion rotation)
        {
            this.parent = parent;
            this.position = position;
            this.size = size;

            this.rotation = rotation;
            subNodes = new List<Node>();
            if (parent != null)
                this.subID = parent.subID + 1;
        }

        public void Subdivide(Node parent, Vector2Int minMaxBranches, Vector2 minMaxPos, Vector3 scaleVector, float branchAngle, float VisualScaleFactor)
        {
            //Dont subdivide if we already are subdivided
            if (subNodes.Count != 0) { return; }

            Transform LookTransform = parent == null ? trunk.transform : parent.trunk.transform;

            int numBranches = Random.Range(minMaxBranches.x, minMaxBranches.y);

            float step = 360f / numBranches;

            for (int i = 0; i < numBranches; i++)
            {
                Vector3 startPos = trunk.transform.position;
                endPos = trunk.transform.position + trunk.transform.forward * trunk.transform.localScale.z * VisualScaleFactor;//Scalefactor

                float stepPos = (minMaxPos.y - minMaxPos.x) / numBranches;

                Vector3 branchPos = Vector3.Lerp(startPos, endPos, minMaxPos.x + stepPos * i /*1f Random.Range(0.625f, 1f)*/);

                Vector3 v = new Vector3(Mathf.Cos(step * i * Mathf.Deg2Rad), Mathf.Sin(step * i * Mathf.Deg2Rad), Mathf.Tan(branchAngle * Mathf.Deg2Rad));
                Vector3 lookDir = LookTransform.TransformVector(v.normalized);

                subNodes.Add(new Node(this, branchPos, new Vector3(LookTransform.localScale.x * scaleVector.x, LookTransform.localScale.y * scaleVector.y, LookTransform.localScale.z * scaleVector.z), Quaternion.LookRotation(lookDir)));

            }
        }

        public bool IsLeaf()
        {
            return subNodes.Count == 0;
        }

        public void SpawnTrunk(GameObject prefab)
        {
            trunk = MonoBehaviour.Instantiate(prefab, position, rotation);
            trunk.transform.localScale = Vector3.Scale(trunk.transform.localScale, size);
        }

        public void SpawnLeaf(GameObject prefab, float VisualOffSetFactor = 6f, float visualScaleFactor = 2f)
        {
            leaf = MonoBehaviour.Instantiate(prefab, trunk.transform.position + trunk.transform.forward * trunk.transform.localScale.z * VisualOffSetFactor, rotation);
            leaf.transform.localScale = new Vector3(size.x, size.x, size.x) * visualScaleFactor;

        }
        public void UpdateLeafPosition(float VisualOffSetFactor = 6f, float visualScaleFactor = 2f)
        {
            if (leaf != null)
                leaf.transform.position = trunk.transform.position + trunk.transform.forward * trunk.transform.localScale.z * VisualOffSetFactor;
        }

        public List<Node> CullNode(List<Node> nodesList)
        {
            if (subNodes.Count > 0)
            {
                foreach (Node n in subNodes)
                {
                    nodesList = n.CullNode(nodesList);
                }
            }
            if (trunk != null) { MonoBehaviour.Destroy(trunk); }
            if (IsLeaf()) { MonoBehaviour.Destroy(leaf); }
            nodesList.Remove(this);
            return nodesList;

        }
    }
}