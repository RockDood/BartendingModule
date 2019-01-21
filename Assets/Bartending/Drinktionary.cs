using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

enum DrinkState
{
    Unprepared,
    Bottled,
    Mixed,
    Blended,
    Failed
}

sealed class Recipe
{
    public string Name;
    public int[] Ingredients;
    public bool Iced;
    public bool Aged;
    public DrinkState State;
    public int SpriteIndex;

    public string LoggingString(string[] ingNames)
    {
        if (State == DrinkState.Bottled)
            return string.Format("Bottled drink: {0}", Name);

        return string.Format("Ingredients: {0}; Iced: {1}; Aged: {2}; {3}",
            string.Join(", ", Ingredients.Select((amount, index) => string.Format("{0} = {1}", ingNames[index], amount)).ToArray()),
            Iced ? "Yes" : "No",
            Aged ? "Yes" : "No",
            State);
    }

    public bool IsSameRecipeAs(Recipe original, bool expectDouble)
    {
        return (expectDouble ? original.Ingredients.Select(ing => 2 * ing) : original.Ingredients).SequenceEqual(Ingredients) && original.Iced == Iced && original.Aged == Aged && original.State == State;
    }
}

sealed class Patron
{
    public string Customer;
    public string[] DrinkPreference;
}
