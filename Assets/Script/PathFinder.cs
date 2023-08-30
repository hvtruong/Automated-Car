using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// Create Node of size 4x4
class Node
{
    public Vector3 position;
    public Node parent;
    public float f, g, h;
    private float distanceToEdge = 2f;
    public Vector3 leftFrontCornerPosition, rightFrontCornerPosition, leftBackCornerPosition, rightBackCornerPosition;

    public Node(Vector3 position, Node parent)
    {
        this.position = position;
        this.parent = parent;
        this.f = 0;
        this.g = 0;
        this.h = 0;

        leftFrontCornerPosition = position + new Vector3(-distanceToEdge, 0, distanceToEdge);
        rightFrontCornerPosition = position + new Vector3(distanceToEdge, 0, distanceToEdge);
        leftBackCornerPosition = position + new Vector3(-distanceToEdge, 0, -distanceToEdge);
        rightBackCornerPosition = position + new Vector3(distanceToEdge, 0, -distanceToEdge);
    }

    public bool isBlockerNode()
    {
        RaycastHit hit;

        if (Physics.Raycast(leftFrontCornerPosition, rightFrontCornerPosition - leftFrontCornerPosition, out hit, 4f) ||
            Physics.Raycast(rightFrontCornerPosition, rightBackCornerPosition - rightFrontCornerPosition, out hit, 4f) ||
            Physics.Raycast(rightBackCornerPosition, leftBackCornerPosition - rightBackCornerPosition, out hit, 4f) ||
            Physics.Raycast(leftBackCornerPosition, leftFrontCornerPosition - leftBackCornerPosition, out hit, 4f) ||
            Physics.Raycast(leftFrontCornerPosition + Vector3.up, Vector3.down, out hit, 5f) ||
            Physics.Raycast(leftBackCornerPosition + Vector3.up, Vector3.down, out hit, 5f) ||
            Physics.Raycast(rightFrontCornerPosition + Vector3.up, Vector3.down, out hit, 5f) ||
            Physics.Raycast(rightBackCornerPosition + Vector3.up, Vector3.down, out hit, 5f))
        {
            string tag = hit.transform.tag;
            if (tag == "blocker" || tag == "terrain")
                return true;
        }

        return false;
    }

    public override bool Equals(object obj)
    {
        return obj is Node node &&
               position.Equals(node.position) &&
               EqualityComparer<Node>.Default.Equals(parent, node.parent) &&
               f == node.f &&
               g == node.g &&
               h == node.h;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(position, parent, f, g, h);
    }

    public static bool operator ==(Node left, Node right)
    {
        if (left is null || right is null)
            return false;
        return left.position == right.position;
    }

    public static bool operator !=(Node left, Node right)
    {
        return !(left == right);
    }
}

class PathFinder : MonoBehaviour
{
    private Vector3 originalPosition;
    private Vector3 destination;
    private Node currentNode;
    private List<Node> openList, closedList;
    public List<Node> pathList;
    static float diagonalDistance = 4f;
    Vector3[] directions = {new Vector3(4f, 0, 0), new Vector3(-4f, 0, 0), new Vector3(0, 0, 4f), new Vector3(0, 0, -4f),
        new Vector3(diagonalDistance, 0, diagonalDistance), new Vector3(diagonalDistance, 0, -diagonalDistance), new Vector3(
            -diagonalDistance, 0, diagonalDistance), new Vector3(-diagonalDistance, 0, -diagonalDistance)};

    public PathFinder(Vector3 position, Vector3 destination, Vector3 forward)
    {
        originalPosition = position + 3f * forward;
        this.destination = destination;
        
        openList = new List<Node>();
        closedList = new List<Node>();
        pathList = new List<Node>();
    }

    private bool arrivedAtDestination(Vector3 position)
    {
        return (Math.Abs(position.x - destination.x) < 2f && Math.Abs(position.z - destination.z) < 2f) ? true : false;
    }

    public List<Node> GetPath()
    {
        Node startNode = new Node(originalPosition, null),
            endNode = new Node(destination, null);

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            currentNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].f < currentNode.f)
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
            List<Node> neighbors = new List<Node>();

            foreach (Vector3 direction in directions)
            {
                Node node = new Node(currentNode.position + direction, currentNode);
                if (!node.isBlockerNode())
                    neighbors.Add(node);
            }

            foreach (Node node in neighbors)
            {
                if (arrivedAtDestination(node.position))
                {
                    pathList.Insert(0, endNode);

                    Node reversedCurrentNode = node.parent;
                    while (reversedCurrentNode is Node)
                    {
                        Vector3 pos = reversedCurrentNode.position;
                        pathList.Insert(0, reversedCurrentNode);
                        
                        reversedCurrentNode = reversedCurrentNode.parent;
                    }

                    return pathList;
                }

                node.g = currentNode.g + (node.position - currentNode.position).magnitude;
                node.h = 1.5f * (float)Math.Sqrt((Math.Pow(node.position.x - endNode.position.x, 2) + Math.Pow(node.position.z - endNode.position.z, 2)));
                node.f = node.g + node.h;

                bool skip = false;
                foreach (Node openNode in openList)
                {
                    if (openNode == node && openNode.f <= node.f)
                    {
                        skip = true;
                        break;
                    }
                }

                foreach (Node closedNode in closedList)
                {
                    if (closedNode == node && closedNode.f <= node.f)
                    {
                        skip = true;
                        break;
                    }
                }

                if (skip)
                    continue;
                else
                    openList.Add(node);
            }
        }

        return pathList;
    }
}