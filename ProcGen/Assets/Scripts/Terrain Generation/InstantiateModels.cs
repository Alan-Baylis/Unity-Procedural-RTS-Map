﻿//Class responsible for distributing resource prefabs around the grid 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateModels : MonoBehaviour {

    public GameObject[] resources = new GameObject[4];
    public GameObject[] rareResources = new GameObject[2];

    GameObject targetResource;
    GameObject targetRareResource;

    GridCell.CellType resourceType;

    Vector3 offset;

    int currentPodsSpawned = 1;

    public GameObject townCentre;

    int podCount;

    public static bool resourcesInstantiated;


	public void InstantiateResources(GridCell[,] grid, System.Random myPrng)
    {
        resourcesInstantiated = false;
        podCount = GenerateGrid.podID;

        while (currentPodsSpawned < podCount)
        {
            targetResource = resources[myPrng.Next(0, 4)];
            targetRareResource = rareResources[myPrng.Next(0, 2)];

            switch(targetResource.tag)
            {
                case "WoodSource":
                    resourceType = GridCell.CellType.Wood;
                    break;
                case "RockSource":
                    resourceType = GridCell.CellType.Rock;
                    break;
                case "BerrySource":
                    resourceType = GridCell.CellType.Berry;
                    break;
            }
               
            foreach (GridCell cell in grid)
            {
                if (cell.resourcePodID == currentPodsSpawned)
                {
                    if (cell.myCell == GridCell.CellType.Resource || cell.myCell == GridCell.CellType.ResourceStartingPoint)
                    {
                        cell.cellObject = Instantiate(targetResource, cell.position, Quaternion.identity);
                        cell.myCell = resourceType;
                    }
                    else if (cell.myCell == GridCell.CellType.RareResource || cell.myCell == GridCell.CellType.RareResourceStartingPoint)
                    {
                        offset = new Vector3(cell.position.x, cell.position.y - 0.6f, cell.position.z);
                        cell.cellObject = Instantiate(targetRareResource, cell.position, Quaternion.identity);
                        cell.myCell = GridCell.CellType.Gem;
                    }
                }
            }
            currentPodsSpawned++;
        }
        resourcesInstantiated = true;
    }
    
}
