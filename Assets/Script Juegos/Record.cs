using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class Puntaje
{
    public string NombreJugador;
    public int Puntos;

    public Puntaje(string Nombre, int puntos)
    {
        NombreJugador = Nombre;
        Puntos = puntos;
    }
}

public class Record : MonoBehaviour
{
    private const string PuntajeJuego = "Puntaje";
    public List<Puntaje> ListaRecord = new List<Puntaje>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CargarPuntajes();
    }

    public void GuardarPuntajeJuego(string nombre, int puntos)
    {
        // Puntaje actual de juego
        CargarPuntajes();

        // Tener un nuevo Puntaje
        ListaRecord.Add(new Puntaje(nombre, puntos));

        //Del puntaje mas alto al mas bajo
        ListaRecord.Sort((a, b) => b.Puntos.CompareTo(a.Puntos));

        // Top 10 Puntajes
        if (ListaRecord.Count > 10)
            ListaRecord.RemoveRange(10, ListaRecord.Count - 10);

        // Se convierte la lista a Json y se guarda
        string json = JsonUtility.ToJson(new ListaWrapper(ListaRecord));
        PlayerPrefs.SetString(PuntajeJuego, json);  // ? corregido
        PlayerPrefs.Save();
    }

    //se guardan los puntajes si entra entre los primeros 10
    public void CargarPuntajes()
    {
        if (PlayerPrefs.HasKey(PuntajeJuego))
        {
            string json = PlayerPrefs.GetString(PuntajeJuego);
            ListaWrapper wrapper = JsonUtility.FromJson<ListaWrapper>(json);
            ListaRecord = wrapper.Lista;
        }
        else
        {
            ListaRecord = new List<Puntaje>();
        }
    }

    public void BorrarPuntos()
    {
        PlayerPrefs.DeleteKey(PuntajeJuego);
        ListaRecord.Clear();
    }

    [System.Serializable]
    private class ListaWrapper
    {
        public List<Puntaje> Lista;
        public ListaWrapper(List<Puntaje> lista)
        {
            Lista = lista;
        }
    }
}