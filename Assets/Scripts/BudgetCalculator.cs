using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BudgetCalculator : MonoBehaviour
{
    [Header("Ingresos")]
    [SerializeField] private TMP_InputField salaryInput;
    [SerializeField] private TMP_InputField savingsPercentageInput;
    [SerializeField] private TMP_InputField debtInput;

    [Header("Gastos Mensuales")]
    [SerializeField] private TMP_InputField monthlyExpenseNameInput;
    [SerializeField] private TMP_InputField monthlyExpenseAmountInput;
    [SerializeField] private Transform monthlyExpensesContainer;    
    [SerializeField] private TextMeshProUGUI monthlyExpensesTotalText;
    [SerializeField] private TextMeshProUGUI monthlyExpensesCountText;
    [SerializeField] private Button AddMonthlyExpenseButton;

    [Header("Gastos Semanales")]
    [SerializeField] private TMP_InputField weeklyExpenseNameInput;
    [SerializeField] private TMP_InputField weeklyExpenseAmountInput;
    [SerializeField] private Transform weeklyExpensesContainer;
    [SerializeField] private TextMeshProUGUI weeklyExpensesTotalText;
    [SerializeField] private TextMeshProUGUI weeklyExpensesCountText;
    [SerializeField] private Button AddWeeklyExpenseButton;

    [Header("Gastos Diarios")]
    [SerializeField] private TMP_InputField dailyExpenseNameInput;
    [SerializeField] private TMP_InputField dailyExpenseAmountInput;
    [SerializeField] private Transform dailyExpensesContainer;
    [SerializeField] private TextMeshProUGUI dailyExpensesTotalText;
    [SerializeField] private TextMeshProUGUI dailyExpensesCountText;
    [SerializeField] private Button AddDailyExpenseButton;

    [Header("Gastos Únicos")]
    [SerializeField] private TMP_InputField onetimeExpenseNameInput;
    [SerializeField] private TMP_InputField onetimeExpenseAmountInput;
    [SerializeField] private Transform onetimeExpensesContainer;
    [SerializeField] private TextMeshProUGUI onetimeExpensesTotalText;
    [SerializeField] private TextMeshProUGUI onetimeExpensesCountText;
    [SerializeField] private Button AddOnetimeExpenseButton;

    [Header("Resultados")]
    [SerializeField] private TextMeshProUGUI savingsAmountText;
    [SerializeField] private TextMeshProUGUI totalExpensesText;
    [SerializeField] private TextMeshProUGUI availableMonthlyText;
    [SerializeField] private TextMeshProUGUI availableWeeklyText;
    [SerializeField] private TextMeshProUGUI availableDailyText;
    [SerializeField] private TextMeshProUGUI debtText;

    [Header("Prefabs")]
    [SerializeField] private GameObject expensePrefab;

    private float totalMonthlyExpenses = 0;
    private float totalWeeklyExpenses = 0;
    private float totalDailyExpenses = 0;
    private float totalOnetimeExpenses = 0;
    private int monthlyExpensesCount = 0;
    private int weeklyExpensesCount = 0;
    private int dailyExpensesCount = 0;
    private int onetimeExpensesCount = 0;

    private const string SALARY_KEY = "salary";
    private const string SAVINGS_PERCENTAGE_KEY = "savings_percentage";
    private const string DEBT_KEY = "debt";
    private const string MONTHLY_EXPENSES_COUNT_KEY = "monthly_expenses_count";
    private const string WEEKLY_EXPENSES_COUNT_KEY = "weekly_expenses_count";
    private const string DAILY_EXPENSES_COUNT_KEY = "daily_expenses_count";
    private const string ONETIME_EXPENSES_COUNT_KEY = "onetime_expenses_count";
    private const string EXPENSE_NAME_KEY = "expense_name_";
    private const string EXPENSE_AMOUNT_KEY = "expense_amount_";
    private const string EXPENSE_TYPE_KEY = "expense_type_";

    private void Start()
    {
        // Configurar validación de inputs
        SetupInputValidation();
        
        // Configurar cálculos automáticos y guardado
        SetupAutoCalculations();

        // Cargar datos guardados
        LoadSavedData();

        // Inicializar textos de totales
        UpdateExpenseTotals();
        UpdateCalculations();
    }

    private void SetupInputValidation()
    {
        salaryInput.onValueChanged.AddListener(ValidateNumberInput);
        savingsPercentageInput.onValueChanged.AddListener(ValidatePercentageInput);
        debtInput.onValueChanged.AddListener(ValidateNumberInput);
        monthlyExpenseAmountInput.onValueChanged.AddListener(ValidateNumberInput);
        weeklyExpenseAmountInput.onValueChanged.AddListener(ValidateNumberInput);
        dailyExpenseAmountInput.onValueChanged.AddListener(ValidateNumberInput);
        onetimeExpenseAmountInput.onValueChanged.AddListener(ValidateNumberInput);
    }

    private void SetupAutoCalculations()
    {
        salaryInput.onValueChanged.AddListener(value => {
            UpdateCalculations();
            SaveSalary();
        });
        
        savingsPercentageInput.onValueChanged.AddListener(value => {
            UpdateCalculations();
            SaveSavingsPercentage();
        });

        debtInput.onValueChanged.AddListener(value => {
            UpdateCalculations();
            SaveDebt();
        });
    }

    private void SaveSalary()
    {
        if (int.TryParse(salaryInput.text, out int salary))
        {
            PlayerPrefs.SetInt(SALARY_KEY, salary);
            PlayerPrefs.Save();
        }
    }

    private void SaveSavingsPercentage()
    {
        if (int.TryParse(savingsPercentageInput.text, out int percentage))
        {
            PlayerPrefs.SetInt(SAVINGS_PERCENTAGE_KEY, percentage);
            PlayerPrefs.Save();
        }
    }

    private void SaveDebt()
    {
        if (int.TryParse(debtInput.text, out int debt))
        {
            PlayerPrefs.SetInt(DEBT_KEY, debt);
            PlayerPrefs.Save();
        }
    }

    private void SaveExpense(string name, int amount, string expenseType, int index)
    {
        string typePrefix = $"{expenseType}_";
        PlayerPrefs.SetString(EXPENSE_NAME_KEY + typePrefix + index, name);
        PlayerPrefs.SetInt(EXPENSE_AMOUNT_KEY + typePrefix + index, amount);
        PlayerPrefs.SetString(EXPENSE_TYPE_KEY + typePrefix + index, expenseType);
        PlayerPrefs.Save();
    }

    private void LoadSavedData()
    {
        // Cargar datos básicos
        if (PlayerPrefs.HasKey(SALARY_KEY))
            salaryInput.text = PlayerPrefs.GetInt(SALARY_KEY).ToString();
        
        if (PlayerPrefs.HasKey(SAVINGS_PERCENTAGE_KEY))
            savingsPercentageInput.text = PlayerPrefs.GetInt(SAVINGS_PERCENTAGE_KEY).ToString();

        if (PlayerPrefs.HasKey(DEBT_KEY))
            debtInput.text = PlayerPrefs.GetInt(DEBT_KEY).ToString();

        // Cargar gastos por tipo
        LoadExpensesByType("monthly", MONTHLY_EXPENSES_COUNT_KEY, monthlyExpensesContainer, ref monthlyExpensesCount);
        LoadExpensesByType("weekly", WEEKLY_EXPENSES_COUNT_KEY, weeklyExpensesContainer, ref weeklyExpensesCount);
        LoadExpensesByType("daily", DAILY_EXPENSES_COUNT_KEY, dailyExpensesContainer, ref dailyExpensesCount);
        LoadExpensesByType("onetime", ONETIME_EXPENSES_COUNT_KEY, onetimeExpensesContainer, ref onetimeExpensesCount);
    }

    private void LoadExpensesByType(string type, string countKey, Transform container, ref int count)
    {
        int savedCount = PlayerPrefs.GetInt(countKey, 0);
        for (int i = 0; i < savedCount; i++)
        {
            string typePrefix = $"{type}_";
            string name = PlayerPrefs.GetString(EXPENSE_NAME_KEY + typePrefix + i, "");
            int amount = PlayerPrefs.GetInt(EXPENSE_AMOUNT_KEY + typePrefix + i, 0);
            
            if (!string.IsNullOrEmpty(name) && amount > 0)
            {
                CreateExpenseObject(name, amount, container, type);
                count++;
            }
        }
    }

    public void AddExpense(string type)
    {
        TMP_InputField nameInput;
        TMP_InputField amountInput;
        Transform container;
        ref int count = ref monthlyExpensesCount; // Default reference

        switch (type)
        {
            case "monthly":
                nameInput = monthlyExpenseNameInput;
                amountInput = monthlyExpenseAmountInput;
                container = monthlyExpensesContainer;
                count = ref monthlyExpensesCount;
                break;
            case "weekly":
                nameInput = weeklyExpenseNameInput;
                amountInput = weeklyExpenseAmountInput;
                container = weeklyExpensesContainer;
                count = ref weeklyExpensesCount;
                break;
            case "daily":
                nameInput = dailyExpenseNameInput;
                amountInput = dailyExpenseAmountInput;
                container = dailyExpensesContainer;
                count = ref dailyExpensesCount;
                break;
            case "onetime":
                nameInput = onetimeExpenseNameInput;
                amountInput = onetimeExpenseAmountInput;
                container = onetimeExpensesContainer;
                count = ref onetimeExpensesCount;
                break;
            default:
                return;
        }

        if (string.IsNullOrEmpty(nameInput.text) || 
            !int.TryParse(amountInput.text, out int amount))
            return;

        CreateExpenseObject(nameInput.text, amount, container, type);

        // Guardar el nuevo gasto
        SaveExpense(nameInput.text, amount, type, count);
        
        count++;
        PlayerPrefs.SetInt($"{type}_expenses_count_key", count);
        PlayerPrefs.Save();
        
        UpdateExpenseTotals();

        // Limpiar inputs
        nameInput.text = "";
        amountInput.text = "";
    }

    private void CreateExpenseObject(string name, int amount, Transform container, string type)
    {
        GameObject expenseObj = Instantiate(expensePrefab, container);
        
        // Configurar textos
        expenseObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = name;
        expenseObj.transform.Find("AmountText").GetComponent<TextMeshProUGUI>().text = $"{amount} MXN";
        
        // Configurar botón de eliminar
        Button deleteButton = expenseObj.transform.Find("DeleteButton").GetComponent<Button>();
        deleteButton.onClick.AddListener(() => 
        {
            switch (type)
            {
                case "monthly":
                    totalMonthlyExpenses -= amount;
                    monthlyExpensesCount--;
                    PlayerPrefs.SetInt(MONTHLY_EXPENSES_COUNT_KEY, monthlyExpensesCount);
                    break;
                case "weekly":
                    totalWeeklyExpenses -= amount;
                    weeklyExpensesCount--;
                    PlayerPrefs.SetInt(WEEKLY_EXPENSES_COUNT_KEY, weeklyExpensesCount);
                    break;
                case "daily":
                    totalDailyExpenses -= amount;
                    dailyExpensesCount--;
                    PlayerPrefs.SetInt(DAILY_EXPENSES_COUNT_KEY, dailyExpensesCount);
                    break;
                case "onetime":
                    totalOnetimeExpenses -= amount;
                    onetimeExpensesCount--;
                    PlayerPrefs.SetInt(ONETIME_EXPENSES_COUNT_KEY, onetimeExpensesCount);
                    break;
            }
            
            ReorganizeExpenses(type);
            
            Destroy(expenseObj);
            UpdateCalculations();
            UpdateExpenseTotals();
            PlayerPrefs.Save();
        });

        // Actualizar total
        switch (type)
        {
            case "monthly":
                totalMonthlyExpenses += amount;
                break;
            case "weekly":
                totalWeeklyExpenses += amount;
                break;
            case "daily":
                totalDailyExpenses += amount;
                break;
            case "onetime":
                totalOnetimeExpenses += amount;
                break;
        }

        UpdateCalculations();
    }

    private void ReorganizeExpenses(string type)
    {
        Transform container = type switch
        {
            "monthly" => monthlyExpensesContainer,
            "weekly" => weeklyExpensesContainer,
            "daily" => dailyExpensesContainer,
            "onetime" => onetimeExpensesContainer,
            _ => null
        };

        if (container == null) return;
        
        // Obtener todos los gastos actuales
        List<(string name, int amount)> expenses = new List<(string name, int amount)>();
        foreach (Transform child in container)
        {
            string name = child.Find("NameText").GetComponent<TextMeshProUGUI>().text;
            string amountText = child.Find("AmountText").GetComponent<TextMeshProUGUI>().text;
            int amount = int.Parse(amountText.Replace(" MXN", ""));
            expenses.Add((name, amount));
        }

        // Limpiar PlayerPrefs para este tipo de gastos
        for (int i = 0; i < 100; i++)
        {
            string typePrefix = $"{type}_";
            PlayerPrefs.DeleteKey(EXPENSE_NAME_KEY + typePrefix + i);
            PlayerPrefs.DeleteKey(EXPENSE_AMOUNT_KEY + typePrefix + i);
            PlayerPrefs.DeleteKey(EXPENSE_TYPE_KEY + typePrefix + i);
        }

        // Guardar los gastos reorganizados
        for (int i = 0; i < expenses.Count; i++)
        {
            SaveExpense(expenses[i].name, expenses[i].amount, type, i);
        }
    }

    private void UpdateExpenseTotals()
    {
        monthlyExpensesTotalText.text = $"{totalMonthlyExpenses:F0} MXN";
        monthlyExpensesCountText.text = monthlyExpensesCount.ToString();

        weeklyExpensesTotalText.text = $"{totalWeeklyExpenses:F0} MXN";
        weeklyExpensesCountText.text = weeklyExpensesCount.ToString();

        dailyExpensesTotalText.text = $"{totalDailyExpenses:F0} MXN";
        dailyExpensesCountText.text = dailyExpensesCount.ToString();

        onetimeExpensesTotalText.text = $"{totalOnetimeExpenses:F0} MXN";
        onetimeExpensesCountText.text = onetimeExpensesCount.ToString();
    }

    private void ValidateNumberInput(string value)
    {
        if (!string.IsNullOrEmpty(value) && !int.TryParse(value, out _))
        {
            string newValue = "";
            foreach (char c in value)
            {
                if (char.IsDigit(c))
                    newValue += c;
            }
            TMP_InputField input = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
            input.text = newValue;
        }
    }

    private void ValidatePercentageInput(string value)
    {
        if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int percentage))
        {
            if (percentage < 0) savingsPercentageInput.text = "0";
            if (percentage > 100) savingsPercentageInput.text = "100";
        }
    }

    private void UpdateCalculations()
    {
        if (!int.TryParse(salaryInput.text, out int salary) || 
            !int.TryParse(savingsPercentageInput.text, out int savingsPercentage) ||
            !int.TryParse(debtInput.text, out int debt))
            return;

        const int daysInMonth = 30;
        const int weeksInMonth = 4;

        // Calcular ahorro mensual
        float monthlySavings = salary * (savingsPercentage / 100f);
        
        // Convertir todos los gastos a mensual
        float weeklyExpensesMonthly = totalWeeklyExpenses * weeksInMonth;
        float dailyExpensesMonthly = totalDailyExpenses * daysInMonth;
        
        // Calcular total de gastos mensual
        float totalMonthlyExpenses = this.totalMonthlyExpenses + weeklyExpensesMonthly + dailyExpensesMonthly + totalOnetimeExpenses;
        
        // Calcular disponible mensual (después de ahorro y gastos)
        float availableMonthly = salary - monthlySavings - totalMonthlyExpenses - debt;
        
        // Calcular disponibles por periodo
        float availableWeekly = availableMonthly / weeksInMonth;
        float availableDaily = availableMonthly / daysInMonth;

        // Actualizar UI
        savingsAmountText.text = $"{monthlySavings:F0} MXN";
        totalExpensesText.text = $"{totalMonthlyExpenses:F0} MXN";
        availableMonthlyText.text = $"{availableMonthly:F0} MXN";
        availableWeeklyText.text = $"{availableWeekly:F0} MXN";
        availableDailyText.text = $"{availableDaily:F0} MXN";
        debtText.text = $"{debt:F0} MXN";
    }

    // Métodos públicos para los botones
    public void AddMonthlyExpense() => AddExpense("monthly");
    public void AddWeeklyExpense() => AddExpense("weekly");
    public void AddDailyExpense() => AddExpense("daily");
    public void AddOnetimeExpense() => AddExpense("onetime");
}
