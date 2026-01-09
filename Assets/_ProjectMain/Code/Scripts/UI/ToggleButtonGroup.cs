using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonGroup : MonoBehaviour
{
    [Header("Buttons (Drag manually)")]
    [SerializeField] private List<Button> buttons = new List<Button>(); //Button list

    [Header("Simple color button")]
    [SerializeField] private Color normalColor = Color.white; //Default color, usually white
    [SerializeField] private Color defaultSelectedColor = Color.gray; //Select color when press

    [Header("Per-Button Selected Colors")]
    [SerializeField] private List<Color> selectedColorsPerButton = new List<Color>(); //Separate selected color for each button.

    [Header("Default select index")]
    [SerializeField] private int defaultSelectedIndex = 0; //Start selected index (-1 = none)

    // Event báo ra button nào đang được chọn (có thể dùng index hoặc Button)
            // index trong list

    private Button currentSelectedButton;
    private int currentIndex = -1;

    private void Awake()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        // Step 1: Loop through buttons and add listener.
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == null) continue;

            int index = i; // capture
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => SelectButton(index));
        }
        // Step 2: Auto resize selected color list if missing.
        while (selectedColorsPerButton.Count < buttons.Count)
        {
            selectedColorsPerButton.Add(Color.white); // default = simple color button
        }

        // Step 3: Select default if available.
        if (defaultSelectedIndex >= 0 && defaultSelectedIndex < buttons.Count && buttons[defaultSelectedIndex] != null)
        {
            SelectButton(defaultSelectedIndex);
        }
        else if (buttons.Count > 0 && buttons[0] != null)
        {
            SelectButton(0);
        }
        else
        {
            UpdateButtonColors();
        }
    }

    public void SelectButton(int index)
    {
        if (index < 0 || index >= buttons.Count || buttons[index] == null) return;

        if (index == currentIndex) return;

        // Step 1: Update current.
        currentIndex = index;
        currentSelectedButton = buttons[index];

        // Step 2: Update the color of all buttons.
        UpdateButtonColors();
        // Step 3: Broadcast event.
        GameEvent.FilterButtonSelected(index);
    }

    private void UpdateButtonColors()
    {
        // Step 1: Loop through buttons.
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == null) continue;

            Image img = buttons[i].GetComponent<Image>();
            if (img == null) continue;

            // Step 2: If selected, use your own color or default.
            if (i == currentIndex)
            {
                //Prioritize using your own color if available, otherwise use defaultSelectedColor
                Color selectedCol = selectedColorsPerButton[i];
                if (selectedCol.a == 0 || selectedCol == Color.white) //if same as default color
                { 
                    selectedCol = defaultSelectedColor;   
                } 

                img.color = selectedCol;
            }
            else
            {
                img.color = normalColor;
            }
        }
    }

    // Used when you want to select from code without emitting an event
    public void SelectSilent(int index)
    {
        if (index >= 0 && index < buttons.Count && buttons[index] != null)
        {
            currentIndex = index;
            currentSelectedButton = buttons[index];
            UpdateButtonColors();
        }
    }

    // Preview from Editor
    private void OnValidate()
    {
        UpdateButtonColors();
    }
}
