using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

public class IA : MonoBehaviour 
{
    private MazeGenerator.Casilla[,] tablero;
    private int width;
    private int height;
    private int posX;
    private int posY;
    private float distanciaMediosCasillas;
    private Vector3 directorVelocidad;
    private int orientacionGiro;
    int orientacionActual = 0;//0N,1E,2S,3W

    private float timeParadoAcum = 0;
    private float timeGiro = 0;
    private MazeGenerator generator;
    private AnimationController m_animationController;

	private myThread m_thread;

	/*
    public float alphaMovimiento = 0.01f;
    public float alphaGiro = 5;
    public enum Giro { Derecha, Izquierda,Aleatorio }
    public Giro como_gira = Giro.Derecha;
    public bool haceTrampas = false;
    public float velocidad = 10;
    public float velocidadGiro = 0.55f;
    public float tiempoParado = 5;
    public float distanciaMaxima = 1.1f;
    public float timeOutGiro = 3;
    public string groundTag = "Ground";


    public enum estadoActual { Avanzado, Girando, Llegando, GirandoYAvanzando,Parado,DoNothing }
    public estadoActual m_estado = estadoActual.DoNothing;
	*/
	// Use this for initialization
	void Start () 
    {
        GameObject go = GameObject.FindGameObjectWithTag("Generator");
        generator=GameObject.FindGameObjectWithTag("Generator").GetComponent<MazeGenerator>();
        tablero = generator.Tablero;
        width = generator.Width;
        height = generator.Heigh;

        posX = (int)generator.casillaComienzo.x;
        posY = (int)generator.casillaComienzo.y;

        m_animationController = GetComponent<AnimationController>();

		m_thread = new myThread(posX,posY,tablero,width,height);

		Thread t = new Thread(new ThreadStart(m_thread.aStar));
		t.Start();
	}

	void Update() {
		if(!m_thread.ended) {
			Debug.Log("not ended");
		}
	}

	private class Node : IComparable<Node>{
		public List<Vector2> lista = new List<Vector2>();
		public int costeActual;
		public int heuristica;

		public Vector2 position;

		public int getCoste() {
			return costeActual + heuristica;
		}
		private void setHeuristica(int value) {
			heuristica = value;
		}

		public int CompareTo(Node y) {
			Node x = this;
			if(x.getCoste() > y.getCoste()) {
				return -1;
			}
			if(x.getCoste() < y.getCoste()) {
				return 1;
			}
			return 0;
		}

	}

	private class myThread {

		private List<Node> listaOrdenada;
		public Node result;
		public bool ended;
		private MazeGenerator.Casilla[,] casillas;
		private List<Vector2> visitadas;
		private int width;
		private int height;

		public myThread(int posX, int posY, MazeGenerator.Casilla[,] casillas, int width, int height) {
			result = null;
			ended = false;
			this.casillas = casillas;
			this.width = width;
			this.height = height;
			visitadas = new List<Vector2>();

			Node n = new Node();
			n.lista = new List<Vector2>();
			n.position = new Vector2(posX, posY);
			n.costeActual = 0;

			listaOrdenada = new List<Node>();//new SortedList(new myOrdered());
			listaOrdenada.Add(n);
		}

		public void aStar() {
			Debug.Log("start aStart" );
			while(listaOrdenada.Count != 0 && !ended) {
				Node n = (Node) listaOrdenada[0];
				listaOrdenada.RemoveAt(0);

				visitadas.Add(n.position);

				MazeGenerator.Casilla c = casillas[(int)n.position.x,(int)n.position.y];

				if(n.position == new Vector2(width - 1, height - 1)) {
					ended = true;
					n.lista.Add(n.position);
					result = n;
				}

				if(!c.wallUp) {
					Vector2 newPos = new Vector2(n.position.x + 1, n.position.y);
					addToList(newPos,n.costeActual,n.lista,n.position);
				}
				if(!c.wallDown) {
					Vector2 newPos = new Vector2(n.position.x - 1, n.position.y);
					addToList(newPos,n.costeActual,n.lista,n.position);
				}

				if(!c.wallRight) {
					Vector2 newPos = new Vector2(n.position.x, n.position.y + 1);
					addToList(newPos,n.costeActual,n.lista,n.position);
				}

				if(!c.wallLeft) {
					Vector2 newPos = new Vector2(n.position.x, n.position.y - 1);
					addToList(newPos,n.costeActual,n.lista,n.position);
				}
			}	
			Debug.Log("end thread");
		}

		private void addToList(Vector2 v, int coste, List<Vector2> listaVisitados, Vector2 anterior) {
			if(!visitadas.Contains(v)) {
				Node n = new Node();
				n.lista = new List<Vector2>();
				n.lista.AddRange(listaVisitados);
				n.lista.Add(anterior);
				n.position = v;
				n.costeActual = coste + 1;
				//calcular euristica
				int diffAncho = width - (int)v.x;
				int diffAlto = height - (int)v.y;
				n.heuristica = diffAlto + diffAncho;

				listaOrdenada.Add(n);
				listaOrdenada.Sort();
			}
		}
	}
}
