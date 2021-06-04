using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //TODO: Inicializar matriz a 0's
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                matriu[i, j] = 0;
            }
        }


        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)

        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int o = 0; o < Constants.NumTiles; o++)
            {
                if (o == i + 1 && o % Constants.TilesPerRow != 0)
                {
                    matriu[i, o] = 1;
                }
                if (o == i - 1 && (o + 1) % Constants.TilesPerRow != 0)
                {
                    matriu[i, o] = 1;
                }
                if (o == i + Constants.TilesPerRow)
                {
                    matriu[i, o] = 1;
                }
                if (o == i - Constants.TilesPerRow)
                {
                    matriu[i, o] = 1;
                }
            }
        }

        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes

        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int o = 0; o < Constants.NumTiles; o++)
            {
                if (matriu[i, o] == 1)
                {
                    tiles[i].adjacency.Add(o);
                }
            }
        }
    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco*/

        Tile casillaFinal = null;

        int posicionPoli1 = cops[0].GetComponent<CopMove>().currentTile;
        int posicionPoli2 = cops[1].GetComponent<CopMove>().currentTile;

        List<Tile> listVisitedTile = new List<Tile>();
        foreach (var tile in tiles)
        {
            if (tile.selectable)
            {
                listVisitedTile.Add(tile);
            }
        }

        List<Tile> listVisitedTile2 = new List<Tile>();
        foreach (var tile in tiles)
        {
            if (tile.selectable)
            {
                listVisitedTile2.Add(tile);
            }
        }

        algoritmoBFS(posicionPoli1);

        foreach (Tile visitedTile in listVisitedTile)
        {
            casillaFinal = visitedTile;

            if (visitedTile.distance >= casillaFinal.distance)
            {
                casillaFinal = visitedTile;

            }
        }

        algoritmoBFS(posicionPoli2);

        foreach (Tile visitedTile in listVisitedTile2)
        {
            if (visitedTile.distance >= casillaFinal.distance)
            {
                casillaFinal = visitedTile;

            }
        }

        //- Movemos al caco a esa casilla
        robber.GetComponent<RobberMove>().MoveToTile(casillaFinal);


        /*- Actualizamos la variable currentTile del caco a la nueva casilla
        */
        robber.GetComponent<RobberMove>().currentTile = casillaFinal.numTile;

    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win! :)";
        else
            finalMessage.text = "You Lose! :(";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
                 
        int indexcurrentTile;


        int posPoli1 = cops[0].GetComponent<CopMove>().currentTile;
        int posPoli2 = cops[1].GetComponent<CopMove>().currentTile;
        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;
        tiles[indexcurrentTile].visited = true;
        tiles[indexcurrentTile].parent = null;
        tiles[indexcurrentTile].distance = 0;
        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
        Tile casillaActual = null;

        nodes.Enqueue(tiles[indexcurrentTile]);

        while (nodes.Count != 0)
        {
            casillaActual = nodes.Dequeue();
            casillaActual.selectable = true;
            if ((casillaActual.distance + 1) < 3)
            {
                foreach (int i in casillaActual.adjacency)
                {
                    if (tiles[i].visited == false && tiles[i].numTile != posPoli1 && tiles[i].numTile != posPoli2)
                    {
                        tiles[i].visited = true;
                        tiles[i].parent = casillaActual;
                        tiles[i].distance = casillaActual.distance + 1;

                        nodes.Enqueue(tiles[i]);
                    }
                }
            }
        }

    }

    public void algoritmoBFS(int tile)
    {
        Queue<Tile> nodes = new Queue<Tile>();
        Tile casillaActual;

        nodes.Enqueue(tiles[tile]);

        while (nodes.Count != 0)
        {
            casillaActual = nodes.Dequeue();

            foreach (int i in casillaActual.adjacency)
            {
                if (tiles[i].visited == false)
                {

                    tiles[i].visited = true;
                    tiles[i].parent = casillaActual;
                    tiles[i].distance = casillaActual.distance + 1;

                    nodes.Enqueue(tiles[i]);
                }
            }

        }
    }







}
