using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeGen : MonoBehaviour
{
    private class Node
    {
        private GameObject node_object;
        public bool is_active => node_object;
        public Transform transform => node_object.transform;
        public Vector2 position => node_object.transform.position;
        private List<Node> connections;
        private List<LineRenderer> edges;
        public int node_strength = 1;
        public Node(GameObject game_obj)
        {
            node_object = game_obj;
            connections = new List<Node>();
            edges = new List<LineRenderer>();
        }

        public void AddConnection(Node new_node)
        {
            connections.Add(new_node);
        }

        public void GiveConnections(Node inheritor) // give all connections to inheriting node when merging
        {
            for (int i = 0; i < connections.Count; i++)
            {
                inheritor.AddConnection(connections[i]);
            }
        }

        public void BuildEdges()
        {
            foreach(Node connect in connections)
            {
                if (!connect.is_active)
                {
                    continue;
                }
                Debug.DrawLine(connect.position, connect.position + (position - connect.position)*0.5f, Color.green, 10000f);
                LineRenderer new_edge = Instantiate(node_object.transform.GetChild(0).gameObject, node_object.transform).GetComponent<LineRenderer>();
                new_edge.SetPosition(0, position);
                new_edge.SetPosition(1, connect.position);
                edges.Add(new_edge);
            }
        }

        public void DrawEdges()
        {
            foreach(Node connect in connections)
            {
                if (!connect.is_active)
                {
                    continue;
                }
                Debug.DrawLine(position, connect.position, Color.red);
            }
        }

        public void DestroyNode()
        {
            node_object.transform.position = new Vector3(0, 30, 0);
            Destroy(node_object);
            node_object = null;
        }

        public void SetPos(Vector2 new_pos)
        {
            node_object.transform.position = new_pos;
        }

        public void ApplyStrength() // increase size based on strength
        {
            node_object.transform.localScale *= node_strength;
        }
    }
    public GameObject NodeInstance;
    public GameObject NodeLabel;
    private HashSet<Node> all_nodes = new HashSet<Node>();

    [Header("Node Count")] // total nodes in this map
    public int min_nodes;
    public int max_nodes;
    [Header("Node Branching")] // total branching per node
    public int min_branching;
    public int max_branching;
    public float min_branch_range;
    public float max_branch_range;

    [Header("Node Merging")] // total branching per node
    public float max_merge_dist;

    [Header("Node Naming")]
    public string[] town_sizes = new string[] {"Camp", "Village", "Town", "City", "Metropolis", "Hive World"};

    // Start is called before the first frame update
    void Start()
    {
        town_sizes = new string[] {"Camp", "Village", "Town", "City", "Metropolis", "Hive World"};
        GenerateGraph();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Node node in all_nodes)
        {
            //node.BuildEdges();
            node.DrawEdges();
        }
    }

    public void GenerateGraph()
    {
        // clear previous batch
        NodeInstance.SetActive(true);
        NodeLabel.SetActive(true);
        foreach(Node node in all_nodes)
        {
            node.DestroyNode();
        }
        
        // initialize node with base value
        all_nodes.Clear();
        Node curr_node = GenerateNode(Vector2.zero);
        int node_count = Random.Range(min_nodes, max_nodes+1);

        Queue<Node> node_queue = new Queue<Node> {};
        node_queue.Enqueue(curr_node);

        // initiate BFS Propagation
        for (int i = 0; i < node_count; i++)
        {
            // get propagation point
            curr_node = node_queue.Dequeue();
            all_nodes.Add(curr_node);

            // merge propagation point
            List<Node> node_destroyer = new List<Node>();
            foreach(Node other_node in all_nodes)
            {
                if (other_node == curr_node)
                {
                    continue;
                }

                // check if node is within merge threshold
                if ((other_node.position - curr_node.position).magnitude <= max_merge_dist)
                {
                    // "remove" the other node, but average out the new positions
                    node_destroyer.Add(other_node);
                }
            }

            // merge everything in merge range under this node!
            if (node_destroyer.Count > 0)
            {
                Vector2 merge_pos = curr_node.position;
                foreach(Node destroy in node_destroyer)
                {
                    merge_pos += destroy.position;
                    destroy.GiveConnections(curr_node);
                    destroy.DestroyNode();
                    curr_node.node_strength += destroy.node_strength;
                    all_nodes.Remove(destroy);
                }
                merge_pos /= (1+node_destroyer.Count);
                curr_node.SetPos(merge_pos);
            }

            // branch out
            int branches = Random.Range(min_branching, max_branching + 1);
            for (int b = 0; b < branches; b++)
            {
                float branch_range = Random.Range(min_branch_range, max_branch_range);
                Node branch_node = GenerateNode(curr_node.position + Random.insideUnitCircle*branch_range);
                node_queue.Enqueue(branch_node);
                branch_node.AddConnection(curr_node);
                curr_node.AddConnection(branch_node);
            }
        }

        foreach(Node unmade in node_queue)
        {
            unmade.DestroyNode();
        }

        // build edges
        Node start = null;
        int start_strength = max_nodes;
        Node end = null;
        int end_strength = -1;
        foreach(Node node in all_nodes)
        {
            node.BuildEdges();
            node.ApplyStrength();
            GameObject new_label = Instantiate(NodeLabel, node.position, Quaternion.identity);
            new_label.transform.SetParent(node.transform, true);
            new_label.transform.localPosition += new Vector3(0,1f,0);
            new_label.GetComponent<TextMeshPro>().text = town_sizes[Mathf.Min(town_sizes.Length-1, node.node_strength-1)];

            // find start and end nodes
            if (node.node_strength < start_strength)
            {
                start = node;
                start_strength = node.node_strength;
            }
            if (node.node_strength > end_strength)
            {
                end = node;
                end_strength = node.node_strength;
            }
        }
        start.transform.GetChild(start.transform.childCount-1).gameObject.GetComponent<TextMeshPro>().text += " (START)";
        end.transform.GetChild(end.transform.childCount-1).gameObject.GetComponent<TextMeshPro>().text += " (END)";

        NodeInstance.SetActive(false);
        NodeLabel.SetActive(false);

    }
    private Node GenerateNode(Vector2 position)
    {
        GameObject new_node_obj = Instantiate(NodeInstance, position, Quaternion.identity);
        Node new_node = new Node(new_node_obj);
        return new_node;

    }
}
