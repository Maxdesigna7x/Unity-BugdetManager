using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Globalization;

public class GestorFinanzas : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_InputField inputSalario;
    public TMP_InputField inputPorcentajeAhorro;
    public TMP_InputField inputNombreGasto;
    public TMP_InputField inputValorGasto;
    public TMP_Dropdown dropdownTipoGasto;
    public TextMeshProUGUI textoAhorroMensual;
    public TextMeshProUGUI textoDisponibleMensual;
    public TextMeshProUGUI textoTotalGastos;
    public TextMeshProUGUI textoGastosMensuales;
    public TextMeshProUGUI textoGastosSemanales;
    public TextMeshProUGUI textoGastosDiarios;
    public TextMeshProUGUI textoGastosUnicos;
    public TextMeshProUGUI textoDeuda;

    private float salario;
    private float porcentajeAhorro;
    private List<Gasto> gastos = new List<Gasto>();
    
    // Cultura específica para México
    private CultureInfo culturaMX = new CultureInfo("es-MX");

    private void Start()
    {
        InicializarDropdown();
        CargarDatos();
        ActualizarUI();
    }

    private string FormatearMoneda(float valor)
    {
        return string.Format(culturaMX, "MXN {0:N2}", valor);
    }

    private void InicializarDropdown()
    {
        dropdownTipoGasto.ClearOptions();
        
        var opciones = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("Mensual"),
            new TMP_Dropdown.OptionData("Semanal"),
            new TMP_Dropdown.OptionData("Diario"),
            new TMP_Dropdown.OptionData("Único")
        };
        
        dropdownTipoGasto.AddOptions(opciones);
    }

    public void ActualizarSalario(string nuevoSalario)
    {
        // Remover cualquier símbolo de moneda o espacios que el usuario pudiera haber ingresado
        nuevoSalario = nuevoSalario.Replace("MXN", "").Trim();
        
        if (float.TryParse(nuevoSalario, NumberStyles.Any, culturaMX, out salario))
        {
            ActualizarCalculos();
            GuardarDatos();
        }
    }

    public void ActualizarPorcentajeAhorro(string nuevoPorcentaje)
    {
        if (float.TryParse(nuevoPorcentaje, NumberStyles.Any, culturaMX, out porcentajeAhorro))
        {
            ActualizarCalculos();
            GuardarDatos();
        }
    }

    public void AgregarGasto()
    {
        if (string.IsNullOrEmpty(inputNombreGasto.text))
        {
            Debug.LogWarning("El nombre del gasto no puede estar vacío");
            return;
        }

        // Remover cualquier símbolo de moneda o espacios
        string valorLimpio = inputValorGasto.text.Replace("MXN", "").Trim();
        
        if (float.TryParse(valorLimpio, NumberStyles.Any, culturaMX, out float valorGasto))
        {
            var nuevoGasto = new Gasto(
                inputNombreGasto.text,
                valorGasto,
                (TipoGasto)dropdownTipoGasto.value
            );
            
            gastos.Add(nuevoGasto);
            ActualizarCalculos();
            GuardarDatos();
            
            // Limpiar campos
            inputNombreGasto.text = "";
            inputValorGasto.text = "";
            dropdownTipoGasto.value = 0;
        }
        else
        {
            Debug.LogWarning("Por favor ingrese un valor válido para el gasto");
        }
    }

    private void ActualizarCalculos()
    {
        float ahorroMensual = salario * (porcentajeAhorro / 100f);
        float disponibleMensual = salario - ahorroMensual;

        float gastosMensuales = gastos.Where(g => g.tipo == TipoGasto.Mensual).Sum(g => g.valor);
        float gastosSemanales = gastos.Where(g => g.tipo == TipoGasto.Semanal).Sum(g => g.valor) * 4;
        float gastosDiarios = gastos.Where(g => g.tipo == TipoGasto.Diario).Sum(g => g.valor) * 30;
        float gastosUnicos = gastos.Where(g => g.tipo == TipoGasto.Unico).Sum(g => g.valor);
        
        float totalGastos = gastosMensuales + gastosSemanales + gastosDiarios + gastosUnicos;
        float deuda = totalGastos > disponibleMensual ? totalGastos - disponibleMensual : 0;

        ActualizarUI(
            ahorroMensual,
            disponibleMensual,
            totalGastos,
            gastosMensuales,
            gastosSemanales,
            gastosDiarios,
            gastosUnicos,
            deuda
        );
    }

    private void ActualizarUI(
        float ahorroMensual = 0,
        float disponibleMensual = 0,
        float totalGastos = 0,
        float gastosMensuales = 0,
        float gastosSemanales = 0,
        float gastosDiarios = 0,
        float gastosUnicos = 0,
        float deuda = 0)
    {
        textoAhorroMensual.text = $"Ahorro Mensual: {FormatearMoneda(ahorroMensual)}";
        textoDisponibleMensual.text = $"Disponible Mensual: {FormatearMoneda(disponibleMensual)}";
        textoTotalGastos.text = $"Total Gastos: {FormatearMoneda(totalGastos)}";
        textoGastosMensuales.text = $"Gastos Mensuales: {FormatearMoneda(gastosMensuales)}";
        textoGastosSemanales.text = $"Gastos Semanales: {FormatearMoneda(gastosSemanales)}";
        textoGastosDiarios.text = $"Gastos Diarios: {FormatearMoneda(gastosDiarios)}";
        textoGastosUnicos.text = $"Gastos Únicos: {FormatearMoneda(gastosUnicos)}";
        textoDeuda.text = $"Deuda: {FormatearMoneda(deuda)}";
    }

    private void GuardarDatos()
    {
        var datosGuardados = new DatosFinancieros
        {
            salario = this.salario,
            porcentajeAhorro = this.porcentajeAhorro,
            gastos = this.gastos
        };

        string json = JsonUtility.ToJson(datosGuardados);
        PlayerPrefs.SetString("DatosFinancieros", json);
        PlayerPrefs.Save();
    }

    private void CargarDatos()
    {
        if (PlayerPrefs.HasKey("DatosFinancieros"))
        {
            string json = PlayerPrefs.GetString("DatosFinancieros");
            var datos = JsonUtility.FromJson<DatosFinancieros>(json);
            
            salario = datos.salario;
            porcentajeAhorro = datos.porcentajeAhorro;
            gastos = datos.gastos;

            inputSalario.text = salario.ToString("N2", culturaMX);
            inputPorcentajeAhorro.text = porcentajeAhorro.ToString("N2", culturaMX);
        }
    }
}

[System.Serializable]
public class DatosFinancieros
{
    public float salario;
    public float porcentajeAhorro;
    public List<Gasto> gastos;
}