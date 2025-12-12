using UnityEngine;
using TMPro;

public class Calculator : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField number1Input;
    public TMP_InputField number2Input;
    public TMP_Dropdown operatorDropdown;
    public TextMeshProUGUI resultInput;

    private void Start()
    {
        // Initialize dropdown with operators
        if (operatorDropdown != null)
        {
            operatorDropdown.ClearOptions();
            operatorDropdown.AddOptions(new System.Collections.Generic.List<string> { "+", "-" });
            operatorDropdown.value = 0; // Set default to "+"
        }

        // Make result input read-only
        if (resultInput != null)
        {
            resultInput.text = "";
        }

        // Add listeners for input changes
        if (number1Input != null)
        {
            number1Input.onValueChanged.AddListener(delegate { CalculateResult(); });
        }

        if (number2Input != null)
        {
            number2Input.onValueChanged.AddListener(delegate { CalculateResult(); });
        }

        if (operatorDropdown != null)
        {
            operatorDropdown.onValueChanged.AddListener(delegate { CalculateResult(); });
        }
    }

    public void CalculateResult()
    {
        // Get values from input fields
        float num1 = 0;
        float num2 = 0;

        if (number1Input != null && !string.IsNullOrEmpty(number1Input.text))
        {
            if (!float.TryParse(number1Input.text, out num1))
            {
                // Invalid input, clear result
                if (resultInput != null)
                {
                    resultInput.text = "";
                }
                return;
            }
        }
        else
        {
            // Empty input, clear result
            if (resultInput != null)
            {
                resultInput.text = "";
            }
            return;
        }

        if (number2Input != null && !string.IsNullOrEmpty(number2Input.text))
        {
            if (!float.TryParse(number2Input.text, out num2))
            {
                // Invalid input, clear result
                if (resultInput != null)
                {
                    resultInput.text = "";
                }
                return;
            }
        }
        else
        {
            // Empty input, clear result
            if (resultInput != null)
            {
                resultInput.text = "";
            }
            return;
        }

        // Get selected operator
        string selectedOperator = "+";
        if (operatorDropdown != null && operatorDropdown.options.Count > 0)
        {
            selectedOperator = operatorDropdown.options[operatorDropdown.value].text;
        }

        // Perform calculation
        float result = 0;
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

        // Display equation with result
        if (resultInput != null)
        {
            resultInput.text = "Equation : " + equation;
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (number1Input != null)
        {
            number1Input.onValueChanged.RemoveAllListeners();
        }

        if (number2Input != null)
        {
            number2Input.onValueChanged.RemoveAllListeners();
        }

        if (operatorDropdown != null)
        {
            operatorDropdown.onValueChanged.RemoveAllListeners();
        }
    }
}

