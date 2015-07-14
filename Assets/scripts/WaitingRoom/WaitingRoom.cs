using UnityEngine;
using System.Collections;

public class WaitingRoom : MonoBehaviour {

    private NetworkManager m_network;
    public GameObject[] players;


    // Use this for initialization
    void Start()
    {
        m_network = GameObject.FindGameObjectWithTag("Network").GetComponent<NetworkManager>();
  
    }

    void Update()
    {
        if (m_network.m_playersName.Count >= 1)
        {
            Renderer render = players[0].GetComponentInChildren<Renderer>();
            render.material.SetVector("_ColorPlayer", new Vector4(m_network.m_playersColours[0].x, m_network.m_playersColours[0].y,
               m_network.m_playersColours[0].z, 1.0f));
            render.material.SetFloat("_ByN", 1.0f);
        }
        else
        {
            Renderer render = players[0].GetComponentInChildren<Renderer>();
            render.material.SetFloat("_ByN", 0.0f);
        }

        if (m_network.m_playersName.Count >= 2)
        {
            Renderer render = players[1].GetComponentInChildren<Renderer>();
            render.material.SetVector("_ColorPlayer", new Vector4(m_network.m_playersColours[1].x, m_network.m_playersColours[1].y,
               m_network.m_playersColours[1].z, 1.0f));
            render.material.SetFloat("_ByN", 1.0f);
        }
        else
        {
            Renderer render = players[1].GetComponentInChildren<Renderer>();
            render.material.SetFloat("_ByN", 0.0f);
        }
		/*
        if (m_network.m_playersName.Count >= 3)
        {
            Renderer render = players[2].GetComponentInChildren<Renderer>();
            render.material.SetVector("_ColorPlayer", new Vector4(m_network.m_playersColours[2].x, m_network.m_playersColours[2].y,
               m_network.m_playersColours[2].z, 1.0f));
            render.material.SetFloat("_ByN", 1.0f);
        }
        else
        {
            Renderer render = players[2].GetComponentInChildren<Renderer>();
            render.material.SetFloat("_ByN", 0.0f);
        }
        if (m_network.m_playersName.Count >= 4)
        {
            Renderer render = players[3].GetComponentInChildren<Renderer>();
            render.material.SetVector("_ColorPlayer", new Vector4(m_network.m_playersColours[3].x, m_network.m_playersColours[3].y,
               m_network.m_playersColours[3].z, 1.0f));
            render.material.SetFloat("_ByN", 1.0f);
        }
        else
        {
            Renderer render = players[3].GetComponentInChildren<Renderer>();
            render.material.SetFloat("_ByN", 0.0f);
        }
        */
    }

    void OnGUI()
    {
        float origenY = 0.65f * Screen.height;
        float tamLabelX = 0.2f * Screen.width;
        float tamLabelY = 0.1f * Screen.height;
		/*
        if (m_network.m_playersName.Count >= 1)
        {
            //label 1
            m_network.construyeLabel(new Rect(0.12f * Screen.width, origenY, tamLabelX, tamLabelY), m_network.m_playersName[0], getColor(0));
        }
        if (m_network.m_playersName.Count >= 2)
        {
            //label 2
            m_network.construyeLabel(new Rect(0.32f * Screen.width, origenY, tamLabelX, tamLabelY), m_network.m_playersName[1], getColor(1));
        }
        if (m_network.m_playersName.Count >= 3)
        {
            //label 3
            m_network.construyeLabel(new Rect(0.55f * Screen.width, origenY, tamLabelX, tamLabelY), m_network.m_playersName[2], getColor(2));
        }
        if (m_network.m_playersName.Count >= 4)
        {
            //label 4
            m_network.construyeLabel(new Rect(0.75f * Screen.width, origenY, tamLabelX, tamLabelY), m_network.m_playersName[3], getColor(3));
        }
		*/
        switch (m_network.ActualState)
        {
            case NetworkManager.GameState.Results:
                if (m_network.construyeButton(new Rect(0.4f * Screen.width, 0.75f * Screen.height, 0.2f * Screen.width, 0.2f * Screen.height),
                 "Back"))
                {
                    Network.Disconnect();
                    m_network.restartGame();
                    Application.LoadLevel("lobby");
                }
                Vector2 tam = m_network.getLabelSize("Game Over");
                m_network.construyeLabel(new Rect(Screen.width * 0.45f - tam.x / 2, Screen.height * 0.05f, tam.x, tam.y), "Game Over", Color.red, 40);
                tam = m_network.getLabelSize("Total Time: " + Mathf.RoundToInt(m_network.TimeAcumulado) + " s");
                m_network.construyeLabel(new Rect(Screen.width * 0.45f - tam.x / 2, Screen.height * 0.15f, tam.x, tam.y), "Total Time: " + Mathf.RoundToInt(m_network.TimeAcumulado) + " s", Color.red, 40);
                float origenY2 = 0.7f * Screen.height;
                if (m_network.m_playersName.Count >= 1)
                {
                    //label 1
                    m_network.construyeLabel(new Rect(0.32f * Screen.width, origenY2, tamLabelX, tamLabelY), m_network.PlayerPositions[0] + " m", getColor(0));
                }
                if (m_network.m_playersName.Count >= 2)
                {
                    //label 2
                    m_network.construyeLabel(new Rect(0.55f * Screen.width, origenY2, tamLabelX, tamLabelY), m_network.PlayerPositions[1] + " m", getColor(1));
                }
                break;
        }
        
    }

    private Color getColor(int id)
    {
        return new Color(m_network.m_playersColours[id].x, m_network.m_playersColours[id].y, m_network.m_playersColours[id].z);
    }

}
