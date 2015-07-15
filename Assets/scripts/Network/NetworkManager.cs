using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager:MonoBehaviour
{
    public int numPlayers = 4;
    private string _porcentajeTraps = "0";
    public string m_player1Name = "Player";
    public Vector3 m_player1Color = new Vector3(0f, 1f, 0.5f);
       
    public GameObject jugadorGameObject;

    private int m_seed;    
    private int m_idTemporal=-1;
    private float timeAcum = 0;
   
    float anchoCampo;
    float altoCampo;
    public float anchoCampoBase = 5;
    public float altoCampoBase = 5;
    public float maximoAnchoAlto = 20.0f;
    public float minimoAnchoAlto = 5.0f;
    private bool error = false;
    private bool tabPulsado = false;

    public List<string> m_playersName= new List<string>();
    public List<Vector3> m_playersColours = new List<Vector3>();
    private int[] m_playersPosition;
    private List<GameObject> jugadoresInstanciados;
    private Vector3 prizeInstanciado;
    int playersReady = 0;
    public Color m_colorIA;

    #region GuiConfiguration
    public Texture2D texturaFondo;
    public Texture2D[] texturasPlayers;
    public int maximoCaracteres=10;
    public Texture2D m_texturaBoton;
    public Texture2D[] m_texturaFlecha;
    public Texture2D texturaTextField;
    public Font fuentBoton;
    public Texture2D[] m_checkBox;
    public Texture2D m_sepradorPantallas;
    #endregion

    int m_porcentajeQuitaVidas = 0;

    public bool playerInstanciado = false;
    #region Accesores
    public int Ancho
    {
        get { return Mathf.RoundToInt(anchoCampo); }
    }
    public int Alto
    {
        get { return Mathf.RoundToInt(altoCampo); }
    }
    public int Seed
    {
        get { return m_seed; }
        set { m_seed = value; }
    }
    public int PorcentajeQuitaVidas
    {
        get { return m_porcentajeQuitaVidas; }
        set { m_porcentajeQuitaVidas = value; }
    }
    public string Player1Name
    {
        get { return m_player1Name; }
        set { m_player1Name = value; }
    }
    public Vector3 Player1Color
    {
        get { return m_player1Color; }
        set { m_player1Color = value; }
    }
    public GameState ActualState
    {
        get { return actualState; }
        set { actualState = value; }
    }
    public float TimeAcumulado
    {
        get { return timeAcum; }
    }
    public int[] PlayerPositions
    {
        get { return m_playersPosition; }
    }
    #endregion

    /// <summary>
    /// None
    /// Game-> in server and client-> in game
	/// Configuring -> to set the size of the maze
	/// Results -> The final results
    /// </summary>

	public enum GameState{None, Game, Configuring, Results};

    private GameState actualState = GameState.None;

    void Start()
    {
        GameObject[] aux = GameObject.FindGameObjectsWithTag("Network");
        if (aux.Length != 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (actualState == GameState.Game)
        {
            timeAcum += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                tabPulsado = true;
            }
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                tabPulsado = false;
            }
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                exitGame();
            }


        }

        if (actualState == GameState.None)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

    }
    
    public void habilitarComponentes(bool val)
    {
        for (int i = 0; i < jugadoresInstanciados.Count; i++)
        {
            Info info = jugadoresInstanciados[i].GetComponent<Info>();

            if (info != null && info.MINE)
            {
                jugadoresInstanciados[i].GetComponent<MovementController>().enabled = val;
                jugadoresInstanciados[i].GetComponent<AnimationController>().enabled = val;
            } else {
				Debug.LogWarning(i + " no tineen info");
			}
        }
    }
    public void habilitarComponentes(bool val,GameObject go)
    {
        go.GetComponent<MovementController>().enabled = val;
        go.GetComponent<AnimationController>().enabled = val;
    }
    public void restartGame()
    {
        m_playersName.Clear();
        m_playersPosition = new int[numPlayers];
        if(jugadoresInstanciados!= null && jugadoresInstanciados.Count !=0) {
            jugadoresInstanciados.Clear();
		}
        tabPulsado = false;
        tabCalculado = false;
        anchoCampo = 10;
        altoCampo = 10;
        playersReady = 0;
        timeAcum = 0;
        actualState = GameState.None;
    }

    
    #region GUI
    void OnGUI()
    {
        switch (actualState)
        {
            case GameState.None: OnGuiNone(); break;
            case GameState.Game: OnGuiGame(); break;
            case GameState.Configuring: OnGuiConfiguring(); break;
        }
    }

    void OnGuiNone()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturaFondo);


        if (construyeButton(new Rect(0.35f * Screen.width, 0.35f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height), 
            "Start Game"))
        {
            anchoCampo = anchoCampoBase;
            altoCampo = altoCampoBase;
            actualState = GameState.Configuring;
        }
    }
    
    bool tabCalculado = false;
    float anchoTab;
    float altoTab;
    void OnGuiGame() 
    {
        float origenX = 0.05f * Screen.width;
        float origenY = 0.05f * Screen.height;
        float separacion = 0.05f * Screen.height;
        float widthTexto = 0.1f * Screen.width;
        float heighTexto = 0.1f * Screen.height;

        if (!tabCalculado && actualState == GameState.Game)
        {
            tabCalculado = true;            
            float yActual;
            float xActual;
            
            //yActual = origenY + aumento;
            yActual = origenY;
            xActual = float.MinValue;
            Vector2 size;

            GameObject spawn=GameObject.Find("SpawnPoint2");
            
            float distancia = (prizeInstanciado - spawn.transform.position).sqrMagnitude;
            int value = Mathf.RoundToInt(distancia);


            for (int i = 0; i < m_playersName.Count; i++)
            {
                size = getLabelSize(m_playersName[i]);
                xActual = Mathf.Max(xActual,size.x);
                yActual += size.y;

                string texto = value.ToString() + " m";
                size = getLabelSize(texto);
                xActual = Mathf.Max(xActual, size.x);
                yActual += size.y;

            }

            anchoTab = xActual + separacion * 2;
            altoTab = yActual + separacion;

        }

        if (tabPulsado)
        {
            calcularTodasLasDistancias();

            float yActual;
            float xActual;
            yActual = origenY;
            xActual = origenX;
           
            //fondo
            GUI.DrawTexture(new Rect(origenX,origenY,anchoTab,altoTab), texturaFondo);

            xActual += separacion;
            yActual += separacion;

            Vector2 size;
            
            for (int i = 0; i < m_playersName.Count; i++)
            {
                construyeLabel(new Rect(xActual, yActual, widthTexto, heighTexto), m_playersName[i], getColor(i));
                size = getLabelSize(m_playersName[i]);
                yActual += size.y;
                construyeLabel(new Rect(xActual, yActual, widthTexto, heighTexto), m_playersPosition[i].ToString() + " m", getColor(i));
                size = getLabelSize(m_playersPosition[i].ToString());
                yActual += size.y;
            }
            

        }
    }
    private Color getColor(int id)
    {
        return new Color(m_playersColours[id].x, m_playersColours[id].y, m_playersColours[id].z);
    }
	void OnGuiConfiguring()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturaFondo);
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;
        style.fontSize = 20;

        construyeLabel(new Rect(0.2f * Screen.width, 0.2f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Width: " + transformaValue(anchoCampo), Color.black);
        anchoCampo = GUI.HorizontalSlider(new Rect(0.2f * Screen.width, 0.3f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), anchoCampo, minimoAnchoAlto, maximoAnchoAlto);

        construyeLabel(new Rect(0.2f * Screen.width, 0.35f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Heigh: " + transformaValue(altoCampo), Color.black);
        altoCampo = GUI.HorizontalSlider(new Rect(0.2f * Screen.width, 0.45f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), altoCampo, minimoAnchoAlto, maximoAnchoAlto);

        if (construyeButton(new Rect(0.15f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
             "Start Game"))
        {
            StartGame();
        }

        if (construyeButton(new Rect(0.55f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
            "Back"))
        {
            error = false;
            m_playersName.Clear();
            actualState = GameState.None;
        }

        if (error)
        {
            construyeLabel(new Rect(0.2f * Screen.width, 0.65f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "There was an error starting the server",Color.black);
        }
       
    }
  
    private int transformaValue(float f)
    {
        return Mathf.RoundToInt(f);
    }

    public bool construyeButton(Rect position, string texto,float margen=0.045f,float tam=0.1f)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = 25;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = Color.white;
        styleLabel.alignment = TextAnchor.MiddleCenter;
        styleLabel.font = fuentBoton;

        GUI.DrawTexture(position, m_texturaBoton);
        if (GUI.Button(new Rect(position.x + margen * Screen.width, 
            position.y + margen*Screen.height, 
            position.width - tam * Screen.width,
            position.height - tam * Screen.height), 
            texto, styleLabel))
        {
            return true;
        }
        return false;
    }
    public bool construyeButtonFlecha(Rect position,int indice)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = 25;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = Color.white;
        styleLabel.alignment = TextAnchor.MiddleCenter;
        styleLabel.font = fuentBoton;

        GUI.DrawTexture(position, m_texturaFlecha[indice]);
        if (GUI.Button(new Rect(position.x,
            position.y ,
            position.width ,
            position.height),"",styleLabel))
        {
            return true;
        }
        return false;
    }
    public void construyeLabel(Rect position, string texto,Color color,int tamFont=25)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = tamFont;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = color;
        styleLabel.font = fuentBoton;

        GUI.Label(position, texto,styleLabel);
    }
    public Vector2 getLabelSize(string texto)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = 25;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = Color.black;
        styleLabel.font = fuentBoton;

        return styleLabel.CalcSize(new GUIContent(texto));
    }
    public string construyeTextField(Rect position, string value,float separacion=0.05f)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = 25;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = Color.white;
        styleLabel.alignment = TextAnchor.MiddleCenter;
        styleLabel.font = fuentBoton;

        GUI.DrawTexture(position, texturaTextField);

        value = GUI.TextField(position, value,styleLabel);
        return value;
    }
    public bool checkBox(Rect position, bool valActual)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = 25;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = Color.white;
        styleLabel.alignment = TextAnchor.MiddleCenter;
        styleLabel.font = fuentBoton;
        if (valActual)
        {
            GUI.DrawTexture(position, m_checkBox[0]);
        }
        else
        {
            GUI.DrawTexture(position, m_checkBox[1]);
        }
        if (GUI.Button(new Rect(position.x,
            position.y,
            position.width,
            position.height), "", styleLabel))
        {
            return !valActual;
        }
        return valActual;

    }

	public void calcularTodasLasDistancias()
	{
		for (int i = 0; i < jugadoresInstanciados.Count; i++)
		{
			m_playersPosition[i] = calcularDistanciaPremio(i);
		}
	}
    #endregion
    #region Gestion Inicio partida
    public void instanciaAllPlayers()
    {
		playerInstanciado = true;
		
		instanciaPlayer();
        instanciaIA();

		findPlayersInstantiated();

		habilitarIA();

		Debug.Log("Start game en el client");
		actualState = GameState.Game;
		m_playersPosition = new int[m_playersName.Count];
		timeAcum = 0;
		
		//habilitarComponentes(true);
    }

	private void instanciaPlayer() {
		int i = 0;
		GameObject spawn = GameObject.Find("SpawnPoint" + i);
		m_playersColours.Add(m_player1Color);

		GameObject player = Instantiate(jugadorGameObject, spawn.transform.position, Quaternion.identity) as GameObject;
		Renderer render = player.GetComponentInChildren<Renderer>();
		render.material.SetVector("_ColorPlayer", new Vector4(m_playersColours[i].x, m_playersColours[i].y, m_playersColours[i].z,1.0f));
		Info info = player.GetComponent<Info>();
		info.ID = i;
		info.IA = false;

		GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");
		Camera camera = cameras[0].GetComponent<Camera>();
		camera.GetComponent<CameraController>().Target = player;
		playerInstanciado = true;
		habilitarComponentes(false, player);
		info.MINE = true;

		player.GetComponent<MovementController>().enabled = true;
		player.GetComponent<AnimationController>().enabled = true;

		m_playersName.Add(m_player1Name);

	}

    private void instanciaIA()
    {
        //creamos los extra
        int cuenta = 0;
        for (int i = m_playersName.Count; i < numPlayers; i++)
        {
            GameObject spawn = GameObject.Find("SpawnPoint" + i);
            if (spawn != null)
            {
                GameObject player = Instantiate(jugadorGameObject, spawn.transform.position, Quaternion.identity) as GameObject;
                Renderer render = player.GetComponentInChildren<Renderer>();
                render.material.SetVector("_ColorPlayer", new Vector4(m_colorIA.r,m_colorIA.g,m_colorIA.b,1.0f));
                string name = "IA" + "(" + cuenta + ")";
                while (m_playersName.Contains(name))
                {
                    cuenta++;
                    name = "IA" + "(" + cuenta + ")";
                }
				m_playersColours.Add(new Vector3(m_colorIA.r,m_colorIA.g,m_colorIA.b));
                m_playersName.Add(name);

				player.GetComponent<MovementController>().enabled = true;
				player.GetComponent<AnimationController>().enabled = true;

			}
		}
	}
	
	/// <summary>
	/// Called on client to begin the game
	/// </summary>
	public void StartGame()
	{
		Application.LoadLevel("maze");
    }

    private void findPlayersInstantiated()
    {
        jugadoresInstanciados = new List<GameObject>();

        bool encontrado = false;
        int cuenta = 0;
        while (!encontrado && cuenta <numPlayers)
        {
            string name = jugadorGameObject.name + "(Clone)";
            GameObject go=GameObject.Find(name);
            if (go != null)
            {
                jugadoresInstanciados.Add(go);
                go.name = m_playersName[cuenta];
                cuenta++;
            }
            else
            {
                encontrado = true;
            }
        }

        GameObject prize=GameObject.FindGameObjectWithTag("Prize");
        if (prize != null)
        {
            prizeInstanciado = prize.transform.position;
        }
		/*
        for (int i = 0; i < jugadoresInstanciados.Count; i++)
        {
            for(int j= 0; j< jugadoresInstanciados.Count; j++)
            {
                if (i != j)
                {
                    Physics.IgnoreCollision(jugadoresInstanciados[i].GetComponent<Collider>(), jugadoresInstanciados[j].GetComponent<Collider>());
                }
            }
        }
        */
    }

    private void habilitarIA()
    {
		/*
        IA.Giro[] giros = new IA.Giro[numPlayers - 1];
        for (int i = 0; i < giros.Length; i++)
        {
            int indice = i % 3;

            switch (indice)
            {
                case 0: giros[i] = IA.Giro.Derecha; break;
                case 1: giros[i] = IA.Giro.Izquierda; break;
                case 2: giros[i] = IA.Giro.Aleatorio; break;
            }
		} 
		*/
		for(int i = 1; i< jugadoresInstanciados.Count; i++) {
			IA componente = jugadoresInstanciados[i].GetComponent<IA>();
			componente.enabled = true;
			//componente.como_gira = giros[i-1];
			Info info = jugadoresInstanciados[i].GetComponent<Info>();
			info.IA = true;
			info.MINE = true;
			info.ID = i;
		}
	}
	
	#endregion
	#region fin juego
	public int calcularDistanciaAGanador(int id,int idGanador)
    {
        GameObject jugador = jugadoresInstanciados[id];
        GameObject ganador = jugadoresInstanciados[idGanador];
        float distancia = (ganador.transform.position - jugador.transform.position).sqrMagnitude;
        return Mathf.RoundToInt(distancia)/5;
    }
    public int calcularDistanciaPremio(int id)
    {
        if (jugadoresInstanciados != null && jugadoresInstanciados.Count>id)
        {
            GameObject jugador = jugadoresInstanciados[id];
            float distancia = (prizeInstanciado - jugador.transform.position).sqrMagnitude;
            return Mathf.RoundToInt(distancia)/5;
        }

        return 0;
    }

    #endregion
    #region exitGame
    public void exitGame()
    {
    	killGame();
	}
	public void endGame2(string nameWinner)
	{
		habilitarComponentes(false);
		
		int idGanador=0;
		//shearch for the winner
		bool encontrado = false;
		while (!encontrado && idGanador < m_playersName.Count)
		{
			if (m_playersName[idGanador] == nameWinner)
			{
				encontrado = true;
			}
			else
			{
				idGanador++;
			}
		}
		
		for (int i = 0; i < m_playersName.Count; i++)
		{
			m_playersPosition[i] = calcularDistanciaAGanador(i,idGanador);
		}
		actualState = GameState.Results;
		playerInstanciado = false;
		Application.LoadLevel("waitingRoom");
		//rebuscarGameObjects();
		
	}
    public void killGame()
    {
        //poner un mensaje...
        Application.LoadLevel("lobby");
        //rebuscarGameObjects();
        restartGame();   
    }
    #endregion

}
