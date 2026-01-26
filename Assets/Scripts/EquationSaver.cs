using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;

[System.Serializable]
public class EquationData
{
    public string equation;
    public int number1;
    public int number2;
    public string operatorSymbol;
    public int result;

    public EquationData(int num1, int num2, string op, int res)
    {
        number1 = num1;
        number2 = num2;
        operatorSymbol = op;
        result = res;
        equation = $"{num1} {op} {num2} = {res}";
    }
}

[System.Serializable]
public class EquationList
{
    public List<EquationData> equations = new List<EquationData>();
}

public class EquationSaver : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField number1Input;
    public TMP_InputField number2Input;
    public TMP_Dropdown operatorDropdown;

    public TextMeshProUGUI equationsTxt;

    private List<EquationData> equations = new List<EquationData>();

    public GameObject warningTxt;

    private void Start()
    {
        // Initialize dropdown with operators
        if (operatorDropdown != null)
        {
            operatorDropdown.ClearOptions();
            operatorDropdown.AddOptions(new List<string> { "+", "-" });
            operatorDropdown.value = 0; // Set default to "+"
        }

        // Clear existing equations if file exists
        ClearEquations();
    }

    public void CreateAndSaveEquation()
    {
        // Get values from input fields
        int num1 = 0;
        int num2 = 0;

        // Validate first number
        if (number1Input == null || string.IsNullOrEmpty(number1Input.text))
        {
            Debug.LogWarning("First number is empty!");
            return;
        }

        if (!int.TryParse(number1Input.text, out num1))
        {
            Debug.LogWarning("Invalid first number!");
            return;
        }

        // Validate second number
        if (number2Input == null || string.IsNullOrEmpty(number2Input.text))
        {
            Debug.LogWarning("Second number is empty!");
            return;
        }

        if (!int.TryParse(number2Input.text, out num2))
        {
            Debug.LogWarning("Invalid second number!");
            return;
        }

        // Get selected operator
        string selectedOperator = "+";
        if (operatorDropdown != null && operatorDropdown.options.Count > 0)
        {
            selectedOperator = operatorDropdown.options[operatorDropdown.value].text;
        }

        // Perform calculation
        int result = 0;
        string equation = "";

        if (selectedOperator == "+")
        {
            result = num1 + num2;
            equation = $"{num1} + {num2} = {result}";
        }
        else if (selectedOperator == "-")
        {
            result = num1 - num2;
            equation = $"{num1} - {num2} = {result}";
        }
        else
        {
            Debug.LogWarning("Invalid operator!");
            return;
        }

        // Create equation data
        EquationData equationData = new EquationData(num1, num2, selectedOperator, result);

        if (result <= 100)
        {
            // Add to list
            equations.Add(equationData);
        }
        else
        {
            warningTxt.SetActive(true);

            Invoke(nameof(warningTxtClose), 3.0f);
        }

        // Save to JSON file
        SaveEquationsToJSON();

        Debug.Log($"Equation saved: {equation}");
    }

    public void warningTxtClose()
    {
        warningTxt.SetActive(false);
    }

    private void SaveEquationsToJSON()
    {
        try
        {
            // Create wrapper object for JSON serialization
            EquationList equationList = new EquationList();
            equationList.equations = equations;

            // Convert to JSON
            string json = JsonUtility.ToJson(equationList, true);

            PlayerPrefs.SetString("Equations", json);
            number1Input.text = "";
            number2Input.text = "";
            LoadEquations();

            Debug.Log($"Equations saved to: {PlayerPrefs.GetString("Equations")}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving equations to JSON: {e.Message}");
        }
    }

    private void LoadEquations()
    {
        try
        {
            // Read JSON file
            string json = PlayerPrefs.GetString("Equations");

            // Parse JSON
            EquationList equationList = JsonUtility.FromJson<EquationList>(json);

            if (equationList != null && equationList.equations != null)
            {
                equations = equationList.equations;
                Debug.Log($"Loaded {equations.Count} equations from JSON file.");
                for (int i = 0; i < equations.Count; i++)
                {
                    equationsTxt.text = equationsTxt.text + "\n" + equations[i].equation.ToString();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading equations from JSON: {e.Message}");
        }
    }

    public void ClearEquations()
    {
        equations.Clear();
        SaveEquationsToJSON();
        Debug.Log("All equations cleared!");
    }

    public int GetEquationCount()
    {
        return equations.Count;
    }
}

