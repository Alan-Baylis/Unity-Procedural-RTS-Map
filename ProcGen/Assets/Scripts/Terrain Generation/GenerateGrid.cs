﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour {

    public static GridCell[,] mapGrid = new GridCell[300,300]; 
    const float y = 35;
    public bool gridGenerated;
    public static int currentlyGeneratedCells;
    public static int spreadAmount;
    public static int podID;
    public GameObject startingBuilding;
    public GameObject worker;
    public Camera mainCamera;

    public List<GameObject> lumberjacks;
    public List<GameObject> gatherers;
    public List<GameObject> miners;

    Vector3 lineTarget;
    float newY;
    int arrayX;
    int arrayY;
    public Pathfinding pathfinder;

    public List<GridCell> GetNeighbours(GridCell cell)
    {
        List<GridCell> neightbours = new List<GridCell>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = cell.gridX + x;
                int checkY = cell.gridY + y;
                
                if (checkX >= 0 && checkX < mapGrid.GetLength(0) && checkY >= 0 && checkY < mapGrid.GetLength(1))
                {
                    neightbours.Add(mapGrid[checkX, checkY]);
                }
            }
        }

        return neightbours;

    }

    public void Generate()
    {
        gridGenerated = false;
        arrayX = 0;
        arrayY = 0;
        for (int z = -150; z != 150; z++)
        {
            for (int x = -150; x != 150; x++)
            {
                mapGrid[arrayX, arrayY] = new GridCell(new Vector3(x, y, z), GridCell.CellType.Grass, false, 0, arrayX, arrayY);
                arrayX++;
            }
            arrayY++;
            arrayX = 0;
        }
        RaycastHit hit;
        foreach (GridCell cell in mapGrid)
        {
            Physics.Raycast(cell.position, Vector3.down, out hit, 200.0f);
            cell.position = hit.point;
			if (hit.collider.tag.Equals ("Water")) {
				cell.myCell = GridCell.CellType.Water;
			} 
			else if (hit.collider.tag.Equals ("Island") && cell.position.y > 10) 
			{
				cell.myCell = GridCell.CellType.RareZone;
			}
            else if (cell.position.y > 19.8)
            {
                cell.myCell = GridCell.CellType.RareZone;
            }
            else if (cell.position.y > 10.5)
            {
                cell.myCell = GridCell.CellType.ElevatedLand;
            }
            else if (hit.collider.tag.Equals ("Island") && cell.position.y < 10)
            {
                cell.myCell = GridCell.CellType.ElevatedLand;
            }
            else
            {
                cell.myCell = GridCell.CellType.Grass;
            }
        }
        gridGenerated = true;
    }

    public void ChooseStartingLocation()
    {
        int gridX;
        int gridY;

        bool startingAreaGenerated = false;
        bool checkAreaFailed = false;


        int counter = 0;

        while (startingAreaGenerated == false)
        {
            checkAreaFailed = false;

            counter++;
            
            if (counter == 1000)
            {
                Debug.Log("Starting area gen failed");
                break;
            }

            gridX = Random.Range(0, mapGrid.GetLength(0));
            gridY = Random.Range(0, mapGrid.GetLength(1));

            while (mapGrid[gridX, gridY].myCell != GridCell.CellType.Grass)
            {
                gridX = Random.Range(0, mapGrid.GetLength(0));
                gridY = Random.Range(0, mapGrid.GetLength(1));
            }
          
            for (int y = gridY - 12; y != gridY + 12; y++)
            {
                for (int x = gridX - 12; x != gridX + 12; x++)
                {
                    if (mapGrid[x, y].myCell == GridCell.CellType.Grass || mapGrid[x, y].myCell == GridCell.CellType.StartingCell)
                    {
                        //Gucci Mane
                    }
                    else
                    {
                        checkAreaFailed = true;
                        break;
                    }                  
                }
                if (checkAreaFailed == true)
                {
                    break;
                }
            }
            if (checkAreaFailed == false)
            {
                startingAreaGenerated = true;

                for (int y = gridY - 12; y != gridY + 12; y++)
                {
                    for (int x = gridX - 12; x != gridX + 12; x++)
                    {
                        mapGrid[x, y].myCell = GridCell.CellType.StartingCell;
                        
                    }
                }
                Instantiate((Object)startingBuilding, mapGrid[gridX, gridY].position, Quaternion.identity);
                
                //mainCamera.transform.position = new Vector3(mapGrid[gridX, gridY].position.x - 28, mapGrid[gridX, gridY].position.y + 28, mapGrid[gridX, gridY].position.z + 44);
            }
        }
    }

    public void GetRandomSelection(int cellAmount, int resourcePodCount, bool rareResource)
    {
        spreadAmount = cellAmount;
        int startingCells;
        int currentStartingCells = 0;
        int newX;
        int newY;
        int gridX;
        int gridY;
        int counter = 0;
        int counter1 = 0;
        int counter2 = 0;
        GridCell.CellType targetArea;

        if (rareResource == true)
        {
            targetArea = GridCell.CellType.RareZone;
            startingCells = 3;
        }
        else
        {
            targetArea = GridCell.CellType.Grass;
            startingCells = 15;
        }

        gridX = Random.Range(0, mapGrid.GetLength(0));
        gridY = Random.Range(0, mapGrid.GetLength(1));

        while (mapGrid[gridX, gridY].myCell != targetArea)
        {
            gridX = Random.Range(0, mapGrid.GetLength(0));
            gridY = Random.Range(0, mapGrid.GetLength(1));
        }
        if (rareResource == true)
        {
            mapGrid[gridX, gridY].myCell = GridCell.CellType.RareResourceStartingPoint;
        }
        else
        {
            mapGrid[gridX, gridY].myCell = GridCell.CellType.ResourceStartingPoint;
        }
       
        mapGrid[gridX, gridY].resourcePodID = podID;
        currentStartingCells++;

        while (currentStartingCells < startingCells)
        {
            counter2++;
            newX = FindNearX(gridX);
            newY = FindNearY(gridY);

            while (gridX == newX && gridY == newY)
            {
                
                newX = FindNearX(gridX);
                newY = FindNearY(gridY);
                
                counter++;

                if (counter == 100)
                {
                    counter = 0;
                    break;                    
                }
            }

            while (mapGrid[newX, newY].myCell != targetArea)
            {
                newX = FindNearX(gridX);
                newY = FindNearY(gridY);
                counter1++;
                if (counter1 == 100)
                {
                    counter1 = 0;
                    break;                    
                }
            }
            if (mapGrid[newX, newY].myCell == targetArea)
            {
                mapGrid[newX, newY].resourcePodID = podID;
                if (rareResource == true)
                {
                    mapGrid[newX, newY].myCell = GridCell.CellType.RareResourceStartingPoint;
                    GenerateResourceCells(newX, newY, true);
                }
                else
                {
                    mapGrid[newX, newY].myCell = GridCell.CellType.ResourceStartingPoint;
                    GenerateResourceCells(newX, newY, false);
                }                              
                currentStartingCells++;
            }
            
            if (counter2 == 100)
            {
                counter2 = 0;
                break;
            }           

        }

    }

    public void spawnStartingWorkers(int amountOfWorkers)
    {
        int gridX;
        int gridY;

        for (int i = 0; i != amountOfWorkers; i++)
        {
            gridX = Random.Range(0, mapGrid.GetLength(0));
            gridY = Random.Range(0, mapGrid.GetLength(1));

            while (mapGrid[gridX, gridY].myCell != GridCell.CellType.StartingCell)
            {
                gridX = Random.Range(0, mapGrid.GetLength(0));
                gridY = Random.Range(0, mapGrid.GetLength(1));
            }

            lumberjacks.Add(Instantiate(worker, mapGrid[gridX, gridY].position, Quaternion.identity));
            UserResources.populationAmount++;
            lumberjacks[lumberjacks.Count -1].GetComponent<Character>().myCell = mapGrid[gridX, gridY];

            gridX = Random.Range(0, mapGrid.GetLength(0));
            gridY = Random.Range(0, mapGrid.GetLength(1));

            while (mapGrid[gridX, gridY].myCell != GridCell.CellType.StartingCell)
            {
                gridX = Random.Range(0, mapGrid.GetLength(0));
                gridY = Random.Range(0, mapGrid.GetLength(1));
            }

            gatherers.Add(Instantiate(worker, mapGrid[gridX, gridY].position, Quaternion.identity));
            UserResources.populationAmount++;
            gatherers[gatherers.Count - 1].GetComponent<Character>().myCell = mapGrid[gridX, gridY];

            gridX = Random.Range(0, mapGrid.GetLength(0));
            gridY = Random.Range(0, mapGrid.GetLength(1));

            while (mapGrid[gridX, gridY].myCell != GridCell.CellType.StartingCell)
            {
                gridX = Random.Range(0, mapGrid.GetLength(0));
                gridY = Random.Range(0, mapGrid.GetLength(1));
            }

            miners.Add(Instantiate(worker, mapGrid[gridX, gridY].position, Quaternion.identity));
            UserResources.populationAmount++;
            miners[miners.Count - 1].GetComponent<Character>().myCell = mapGrid[gridX, gridY];

        }
        //while (InstantiateModels.resourcesInstantiated == false)
        //{
        //    //Wait for models to be instantiated before calculating paths
        //}
        foreach (GameObject character in lumberjacks)
        {
            character.GetComponent<Character>().pathToResource = pathfinder.FindPath(GridCell.CellType.Wood, character.GetComponent<Character>().myCell, true);
            character.GetComponent<Character>().pathToHome = pathfinder.FindPath(GridCell.CellType.Wood, character.GetComponent<Character>().myCell, false);
            character.GetComponent<Character>().pathFound = true;
            character.GetComponent<Character>().myType = Character.CharacterType.Lumberjack;
        }

        foreach (GameObject character in gatherers)
        {
            character.GetComponent<Character>().pathToResource = pathfinder.FindPath(GridCell.CellType.Berry, character.GetComponent<Character>().myCell, true);
            character.GetComponent<Character>().pathToHome = pathfinder.FindPath(GridCell.CellType.Berry, character.GetComponent<Character>().myCell, false);
            character.GetComponent<Character>().pathFound = true;
            character.GetComponent<Character>().myType = Character.CharacterType.Gatherer;
        }

        foreach (GameObject character in miners)
        {
            character.GetComponent<Character>().pathToResource = pathfinder.FindPath(GridCell.CellType.Rock, character.GetComponent<Character>().myCell, true);
            character.GetComponent<Character>().pathToHome = pathfinder.FindPath(GridCell.CellType.Rock, character.GetComponent<Character>().myCell, false);
            character.GetComponent<Character>().pathFound = true;
            character.GetComponent<Character>().myType = Character.CharacterType.Miner;
        }
    }

    public void GenerateResourceCells(int gridX, int gridY, bool rareResource)
    {
        int counter = 0;
        int counter1 = 0;
        int newX;
        int newY;
        int generatedCells = 0;
        GridCell.CellType targetArea;

        if (rareResource == true)
        {
            targetArea = GridCell.CellType.RareZone;
        }
        else
        {
            targetArea = GridCell.CellType.Grass;
        }

        while (generatedCells < spreadAmount)
        {
            counter1++;
            newX = FindNearX(gridX);
            newY = FindNearY(gridY);
            
            while(mapGrid[newX, newY].occupiedCell == true)
            {
                //Debug.Log(newX + " " + newY);
                counter++;
                newX = FindNearX(gridX);
                newY = FindNearY(gridY);
                if (counter == 100)
                {                    
                    counter = 0;
                    break;
                }
            }
            mapGrid[newX, newY].occupiedCell = true;
            if (mapGrid[newX, newY].myCell == targetArea)
            {
                if (rareResource == true)
                {
                    mapGrid[newX, newY].myCell = GridCell.CellType.RareResource;
                }
                else
                {
                    mapGrid[newX, newY].myCell = GridCell.CellType.Resource;
                }
                mapGrid[newX, newY].resourcePodID = podID;
                generatedCells++;
            }

            if (counter1 == 100)
            {
                counter1 = 0;
                break;
            }
        }
    }

    public int FindNearX(int gridX)
    {
        int x = gridX;
        int randomChange = Random.Range(0, 6);

        switch (randomChange)
        {
            case 0:
                x = x + 3;
                break;
            case 1:
                //X remains the same
                break;
            case 2:
                x = x - 3;
                break;
            case 3:
                x = x + 5;
                break;
            case 4:
                x = x - 5;
                break;
            case 5:
                x = x + 7;
                break;
            case 6:
                x = x - 7;
                break;
        }
        if (x > 230)
        {
            x = 230;
        }
        else if (x < 0)
        {
            x = 0;
        }
        return x;
    }

    public int FindNearY(int gridY)
    {
        int y = gridY;
        int randomChange = Random.Range(0, 6);

        switch (randomChange)
        {
            case 0:
                y = y + 3;
                break;
            case 1:
                //Y remains the same
                break;
            case 2:
                y = y - 3;
                break;
            case 3:
                y = y + 5;
                break;
            case 4:
                y = y - 5;
                break;
            case 5:
                y = y + 7;
                break;
            case 6:
                y = y - 7;
                break;
        }
        if (y > 230)
        {
            y = 230;
        }
        else if (y < 0)
        {
            y = 0;
        }
        return y;
    }

    public void DrawGridLines()
    {
        Vector3 drawTarget;
        Vector3 drawResourceTarget;
        foreach (GridCell cell in mapGrid)
        {
            drawTarget = new Vector3(cell.position.x, cell.position.y + 1, cell.position.z);
            drawResourceTarget = new Vector3(cell.position.x, cell.position.y + 3, cell.position.z);
            switch (cell.myCell)
            {
                case GridCell.CellType.ElevatedLand:
                    Debug.DrawLine(cell.position, drawTarget, Color.cyan, 100000);
                    break;
                case GridCell.CellType.Grass:
                    Debug.DrawLine(cell.position, drawTarget, Color.green, 100000);
                    break;

                case GridCell.CellType.RareZone:
                    Debug.DrawLine(cell.position, drawTarget, Color.white, 100000);
                    break;

                case GridCell.CellType.Resource:
                    Debug.DrawLine(cell.position, drawResourceTarget, Color.yellow, 100000);
                    break;

                case GridCell.CellType.RareResource:
                    Debug.DrawLine(cell.position, drawResourceTarget, Color.magenta, 100000);
                    break;

                case GridCell.CellType.ResourceStartingPoint:
                    Debug.DrawLine(cell.position, drawResourceTarget, Color.red, 100000);
                    break;

                case GridCell.CellType.RareResourceStartingPoint:
                    Debug.DrawLine(cell.position, drawResourceTarget, Color.black, 100000);
                    break;

                case GridCell.CellType.StartingCell:
                    Debug.DrawLine(cell.position, drawResourceTarget, Color.grey, 100000);
                    break;
                case GridCell.CellType.Water:
                    Debug.DrawLine(cell.position, drawTarget, Color.blue, 100000);
                    break;
			    case GridCell.CellType.IslandCell:
				    Debug.DrawLine(cell.position, drawTarget, Color.black, 100000);
				    break;
            }
        }
        
    }

    
}
