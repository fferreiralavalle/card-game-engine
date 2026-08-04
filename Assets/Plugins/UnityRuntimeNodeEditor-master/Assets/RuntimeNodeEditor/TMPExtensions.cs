using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class TMP_DropdownExtensions
{
    /// <summary>
    /// Returns a list of indices for all selected options in a MultiSelect TMP_Dropdown.
    /// </summary>
    public static List<int> GetSelectedIndexes(this TMP_Dropdown dropdown)
    {
        List<int> selectedIndices = new List<int>();
        int value = dropdown.value;
        int optionCount = dropdown.options.Count;

        // Iterate through all available options
        for (int i = 0; i < optionCount; i++)
        {
            // Check if the bit at position 'i' is set
            if ((value & (1 << i)) != 0)
            {
                selectedIndices.Add(i);
            }
        }

        return selectedIndices;
    }

    /// <summary>
    /// Sets the selected options in a MultiSelect TMP_Dropdown based on a list of indices.
    /// </summary>
    public static void SetSelectedIndexes(this TMP_Dropdown dropdown, List<int> indices)
    {
        if (dropdown == null || indices == null) return;

        int value = 0;
        int optionCount = dropdown.options.Count;

        foreach (int index in indices)
        {
            // Ensure the index is valid before setting the bit
            if (index >= 0 && index < optionCount)
            {
                value |= (1 << index);
            }
        }

        // Assign the calculated bitfield to the dropdown
        dropdown.value = value;

        // Force refresh if the event didn't trigger automatically
        dropdown.RefreshShownValue();
    }

    /// <summary>
    /// Sets the selected options in a MultiSelect TMP_Dropdown based on a list of option texts.
    /// </summary>
    public static void SetSelectedOptionsByText(this TMP_Dropdown dropdown, List<string> optionTexts)
    {
        if (dropdown == null || optionTexts == null) return;

        int value = 0;
        int optionCount = dropdown.options.Count;

        foreach (string text in optionTexts)
        {
            // Find the index of the option with the matching text
            int index = dropdown.options.FindIndex(opt => opt.text == text);

            // If found, set the corresponding bit
            if (index >= 0 && index < optionCount)
            {
                value |= (1 << index);
            }
            else
            {
                MonoBehaviour.print($"Option with text '{text}' not found in dropdown.");
            }
        }

        dropdown.value = value;
        dropdown.RefreshShownValue();
    }
}