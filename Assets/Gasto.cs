// Gasto.cs
using UnityEngine;
using System;

[System.Serializable]
public class Gasto
{
    public string nombre;
    public float valor;
    public TipoGasto tipo;
    
    public Gasto(string nombre, float valor, TipoGasto tipo)
    {
        this.nombre = nombre;
        this.valor = valor;
        this.tipo = tipo;
    }
}

public enum TipoGasto
{
    Mensual,
    Semanal,
    Diario,
    Unico
}