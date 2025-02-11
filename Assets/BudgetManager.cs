using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class ExpenseData
{
    public string name;
    public float value;
    public int type;
}

public class BudgetManager : MonoBehaviour
{   

    [Header("Inputs")]
    public TMP_InputField salaryInput;
    public TMP_InputField savingsPercentageInput;
    public TMP_InputField expenseNameInput;
    public TMP_InputField expenseValueInput;
    public TMP_Dropdown expenseTypeDropdown;

    [Header("Outputs")]
    public TMP_Text salaryText;
    public TMP_Text percentText;
    public TMP_Text savingsText;
    public TMP_Text availableText;
    public TMP_Text totalExpensesText;
    public TMP_Text monthlyExpensesText;
    public TMP_Text weeklyExpensesText;
    public TMP_Text dailyExpensesText;
    public TMP_Text uniqueExpensesText;
    public TMP_Text monthlyAvailableText;    
    public TMP_Text weeklyAvailableText;
    public TMP_Text dailyAvailableText;    
    public TMP_Text debtText;

    [Header("OtherOutputs")]
    public TMP_Text savingsTextOther;
    public TMP_Text totalExpensesTextOther;
    public TMP_Text monthlyAvailableTextOther; 
    public TMP_Text debtTextOther;

    [Header("Prefab & Container")]
    public GameObject expensePrefab;
    public Transform monthlyContainer;
    public Transform weeklyContainer;
    public Transform dailyContainer;
    public Transform uniqueContainer;

    [Header("UI")]
    public Image savingSlider;
    
    [SerializeField] private float salary;
    [SerializeField] private float savingsPercentage;
    [SerializeField] private float available;
    [SerializeField] private float totalExpenses = 0;
    [SerializeField] private float monthlyExpenses = 0;
    [SerializeField] private float weeklyExpenses = 0;
    [SerializeField] private float dailyExpenses = 0;
    [SerializeField] private float uniqueExpenses = 0;
    [SerializeField] private float savings;
    [SerializeField] private float debt;

    private List<GameObject> expenseInstances = new List<GameObject>();
    private List<ExpenseData> savedExpenses = new List<ExpenseData>();

    void Start()
    {        
        InicialSetup();
    }

    void InicialSetup()
    {
        LoadData();
        SetupAutoCalculations();
        InstantiateSavedExpenses();
        CalculateAllValues();        
        UpdateInputFields();
    }

    public void ResetDataApp()
    {
        salary = 0;
        savingsPercentage = 0;
        available = 0;
        totalExpenses = 0;
        monthlyExpenses = 0;
        weeklyExpenses = 0;
        dailyExpenses = 0;
        uniqueExpenses = 0;
        savings = 0;
        debt = 0;

        PlayerPrefs.DeleteAll();
        DeleteAllExpenses();
        InicialSetup();
        UpdateUI();
    }

    public void OnSalaryChange()
    {
        salaryInput.text = salaryInput.text.Replace("MXN", "").Trim();
        if (float.TryParse(salaryInput.text, out salary))
        {
            CalculateAllValues();
            SaveData();
            
            // Add symbols to the input fields
            salaryInput.text = $"{salary} MXN";            
        }
    }
    public void OnSaveChange()
    {
        savingsPercentageInput.text = savingsPercentageInput.text.Replace("%", "").Trim();
        if (float.TryParse(savingsPercentageInput.text, out savingsPercentage))
        {
            CalculateAllValues();
            SaveData();
            
            savingSlider.fillAmount = savingsPercentage / 100f;
            savingsPercentageInput.text = $"{savingsPercentage}%";
        }
    }

    private void SetupAutoCalculations()
    {
        salaryInput.onSubmit.AddListener(value => {
            OnSalaryChange();
        });
        
        savingsPercentageInput.onSubmit.AddListener(value => {
            OnSaveChange();
        });        
    }    

    private void CalculateAllValues()
    {
        if (salary <= 0 || savingsPercentage <= 0)
            return;

        CalculateSavings();
        CalculateAvailable();
        CalculateTotalExpenses();
        CalculateDebt();
        UpdateUI();
        SaveData();
    }

    private void CalculateSavings()
    {
        savings = salary * savingsPercentage / 100f;
        Debug.Log(savings);
    }

    private void CalculateAvailable()
    {
        available = salary - savings;
        Debug.Log(available);
    }

    private void CalculateTotalExpenses()
    {
        totalExpenses = monthlyExpenses + weeklyExpenses + dailyExpenses + uniqueExpenses;
        Debug.Log(totalExpenses);
    }

    private void CalculateDebt()
    {
        debt = totalExpenses > available ? totalExpenses - available : 0;
        Debug.Log(debt);
    }

    private float CalculateMonthlyAvailable()
    {
        //Debug.Log(available - monthlyExpenses);
        CalculateDebt();
        return available - totalExpenses;        
    }

    private float CalculateWeeklyAvailable()
    {
        //Debug.Log(available - weeklyExpenses);
        CalculateDebt();
        return Mathf.RoundToInt((available - totalExpenses) / 4);   
    }

    private float CalculateDailyAvailable()
    {
        //Debug.Log(available - dailyExpenses);
        CalculateDebt();
        return Mathf.RoundToInt((available - totalExpenses) / 30); 
    }

    public void AddExpense()
    {
        if (!float.TryParse(expenseValueInput.text, out float value) || string.IsNullOrEmpty(expenseNameInput.text))
            return;

        string expenseName = expenseNameInput.text;
        int expenseType = expenseTypeDropdown.value;
        Transform container = GetContainer(expenseType);
        
        GameObject newExpense = Instantiate(expensePrefab, container);
        TMP_Text[] textComponents = newExpense.GetComponentsInChildren<TMP_Text>();

        foreach (var textComponent in textComponents)
        {
            if (textComponent.name == "GastoName")
            {
                textComponent.text = expenseName;
            }
            else if (textComponent.name == "GastoValue")
            {
                textComponent.text = $"{value} MXN";
            }
        }

        Button deleteButton = newExpense.GetComponentInChildren<Button>();
        if (deleteButton != null)
        {
            Debug.Log("Delete Button");
            deleteButton.onClick.AddListener(() => DeleteExpense(newExpense, expenseType, value));
        }

        expenseInstances.Add(newExpense);

        UpdateExpenses(expenseType, value);
        expenseValueInput.text = "";
        expenseNameInput.text = "";
        SaveExpense(expenseName, value, expenseType);
        SaveData();
    }

    public void DeleteExpense(GameObject expenseInstance, int type, float value)
    {
        Debug.Log("Delete void initiated");
        Destroy(expenseInstance);
        expenseInstances.Remove(expenseInstance);
        savedExpenses.RemoveAll(expense => expense.name == expenseInstance.GetComponentInChildren<TMP_Text>().text && expense.value == value && expense.type == type);
        ReduceExpenses(type, value);
        CalculateAllValues();
        SaveData();
        UpdateUI();
        Debug.Log("Delete void finish");
    }

    public void DeleteAllExpenses()
    {
        foreach (var expenseInstance in expenseInstances)
        {
            Destroy(expenseInstance);
        }
        expenseInstances.Clear();
        savedExpenses.Clear();
        monthlyExpenses = 0;
        weeklyExpenses = 0;
        dailyExpenses = 0;
        uniqueExpenses = 0;
        CalculateAllValues();
        SaveData();
        UpdateUI();
    }

    private void ReduceExpenses(int type, float value)
    {
        switch (type)
        {
            case 0: monthlyExpenses -= value; break;
            case 1: weeklyExpenses -= value; break;
            case 2: dailyExpenses -= value; break;
            case 3: uniqueExpenses -= value; break;
        }
    }

    private Transform GetContainer(int type)
    {
        switch (type)
        {
            case 0: return monthlyContainer;
            case 1: return weeklyContainer;
            case 2: return dailyContainer;
            case 3: return uniqueContainer;
            default: return monthlyContainer;
        }
    }

    private void UpdateExpenses(int type, float value)
    {
        switch (type)
        {
            case 0: monthlyExpenses += value; break;
            case 1: weeklyExpenses += value; break;
            case 2: dailyExpenses += value; break;
            case 3: uniqueExpenses += value; break;
        }

        CalculateAllValues();
        UpdateUI();
    }

    private void UpdateUI()
    {
        salaryText.text = $"{salary} MXN";
        percentText.text = $"{salary - savings} MXN";
        savingsText.text = $"{savings} MXN";
        availableText.text = $"{available} MXN";
        totalExpensesText.text = $"{totalExpenses} MXN";
        monthlyExpensesText.text = $"{monthlyExpenses} MXN";
        weeklyExpensesText.text = $"{weeklyExpenses} MXN";
        dailyExpensesText.text = $"{dailyExpenses} MXN";
        uniqueExpensesText.text = $"{uniqueExpenses} MXN";
        monthlyAvailableText.text = $"{CalculateMonthlyAvailable()} MXN";
        weeklyAvailableText.text = $"{CalculateWeeklyAvailable()} MXN";
        dailyAvailableText.text = $"{CalculateDailyAvailable()} MXN";
        debtText.text = $"{debt} MXN";

        savingsTextOther.text = $"{savings} MXN";
        totalExpensesTextOther.text = $"{totalExpenses} MXN";
        monthlyAvailableTextOther.text = $"{CalculateMonthlyAvailable()} MXN";
        debtTextOther.text = $"{debt} MXN";
    }

    private void SaveExpense(string name, float value, int type)
    {
        ExpenseData expenseData = new ExpenseData { name = name, value = value, type = type };
        savedExpenses.Add(expenseData);
        SaveData();
    }

    private void InstantiateSavedExpenses()
    {
        foreach (var expense in savedExpenses)
        {
            Transform container = GetContainer(expense.type);
            GameObject newExpense = Instantiate(expensePrefab, container);
            TMP_Text[] textComponents = newExpense.GetComponentsInChildren<TMP_Text>();

            foreach (var textComponent in textComponents)
            {
                if (textComponent.name == "GastoName")
                {
                    textComponent.text = expense.name;
                }
                else if (textComponent.name == "GastoValue")
                {
                    textComponent.text = $"{expense.value} MXN";
                }
            }

            expenseInstances.Add(newExpense);
            UpdateExpenses(expense.type, expense.value);
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetFloat("Salary", salary);
        PlayerPrefs.SetFloat("SavingsPercentage", savingsPercentage);
        PlayerPrefs.SetFloat("Available", available);
        PlayerPrefs.SetFloat("TotalExpenses", totalExpenses);
        //PlayerPrefs.SetFloat("MonthlyExpenses", monthlyExpenses);
        //PlayerPrefs.SetFloat("WeeklyExpenses", weeklyExpenses);
        //PlayerPrefs.SetFloat("DailyExpenses", dailyExpenses);
        //PlayerPrefs.SetFloat("UniqueExpenses", uniqueExpenses);
        PlayerPrefs.SetString("SavedExpenses", JsonUtility.ToJson(new Serialization<ExpenseData>(savedExpenses)));
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        salary = PlayerPrefs.GetFloat("Salary", 0);
        savingsPercentage = PlayerPrefs.GetFloat("SavingsPercentage", 0);
        available = PlayerPrefs.GetFloat("Available", 0);
        totalExpenses = PlayerPrefs.GetFloat("TotalExpenses", 0);
        //monthlyExpenses = PlayerPrefs.GetFloat("MonthlyExpenses", 0);
        //weeklyExpenses = PlayerPrefs.GetFloat("WeeklyExpenses", 0);
        //dailyExpenses = PlayerPrefs.GetFloat("DailyExpenses", 0);
        //uniqueExpenses = PlayerPrefs.GetFloat("UniqueExpenses", 0);
        string savedExpensesJson = PlayerPrefs.GetString("SavedExpenses", JsonUtility.ToJson(new Serialization<ExpenseData>(new List<ExpenseData>())));
        savedExpenses = JsonUtility.FromJson<Serialization<ExpenseData>>(savedExpensesJson).ToList();
    }

    private void UpdateInputFields()
    {
        salaryInput.text = salary > 0 ? $"{salary} MXN" : "";
        savingsPercentageInput.text = savingsPercentage > 0 ? $"{savingsPercentage}%" : "";
        savingSlider.fillAmount = savingsPercentage / 100f;
    }
}

[System.Serializable]
public class Serialization<T>
{
    public List<T> target;

    public Serialization(List<T> target)
    {
        this.target = target;
    }

    public List<T> ToList()
    {
        return target;
    }
}