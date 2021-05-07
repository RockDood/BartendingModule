using System.Linq;

enum DrinkState
{
    Unprepared,
    Bottled,
    Mixed,
    Blended,
    Failed
}

sealed class Drink
{
    public string Name { get; private set; }
    public int SpriteIndex { get; private set; }
    public bool Bottled { get; private set; }

    // Bottled Drinks
    public static Drink Absinthe = new Drink { Name = "Absinthe", SpriteIndex = 0, Bottled = true };
    public static Drink Rum = new Drink { Name = "Rum", SpriteIndex = 1, Bottled = true };
    public static Drink MulanTea = new Drink { Name = "Mulan Tea", SpriteIndex = 2, Bottled = true };
    public static Drink AFedora = new Drink { Name = "A Fedora", SpriteIndex = 3, Bottled = true };

    // Non-bottled Drinks
    public static Drink BadTouch = new Drink { Name = "Bad Touch", SpriteIndex = 17 };
    public static Drink Beer = new Drink { Name = "Beer", SpriteIndex = 6 };
    public static Drink BleedingJane = new Drink { Name = "Bleeding Jane", SpriteIndex = 8 };
    public static Drink BloomLight = new Drink { Name = "Bloom Light", SpriteIndex = 7 };
    public static Drink BlueFairy = new Drink { Name = "Blue Fairy", SpriteIndex = 16 };
    public static Drink Brandtini = new Drink { Name = "Brandtini", SpriteIndex = 7 };
    public static Drink CobaltVelvet = new Drink { Name = "Cobalt Velvet", SpriteIndex = 18 };
    public static Drink CreviceSpike = new Drink { Name = "Crevice Spike", SpriteIndex = 19 };
    public static Drink FlamingMoai = new Drink { Name = "Flaming Moai", SpriteIndex = 20 };
    public static Drink FluffyDream = new Drink { Name = "Fluffy Dream", SpriteIndex = 9 };
    public static Drink FringeWeaver = new Drink { Name = "Fringe Weaver", SpriteIndex = 15 };
    public static Drink FrothyWater = new Drink { Name = "Frothy Water", SpriteIndex = 6 };
    public static Drink GrizzlyTemple = new Drink { Name = "Grizzly Temple", SpriteIndex = 10 };
    public static Drink GutPunch = new Drink { Name = "Gut Punch", SpriteIndex = 21 };
    public static Drink Marsblast = new Drink { Name = "Marsblast", SpriteIndex = 22 };
    public static Drink Mercuryblast = new Drink { Name = "Mercuryblast", SpriteIndex = 23 };
    public static Drink Moonblast = new Drink { Name = "Moonblast", SpriteIndex = 11 };
    public static Drink PianoMan = new Drink { Name = "Piano Man", SpriteIndex = 12 };
    public static Drink PianoWoman = new Drink { Name = "Piano Woman", SpriteIndex = 13 };
    public static Drink Piledriver = new Drink { Name = "Pile driver", SpriteIndex = 24 };
    public static Drink SparkleStar = new Drink { Name = "Sparkle Star", SpriteIndex = 25 };
    public static Drink SugarRush = new Drink { Name = "Sugar Rush", SpriteIndex = 14 };
    public static Drink SunshineCloud = new Drink { Name = "Sunshine Cloud", SpriteIndex = 26 };
    public static Drink Suplex = new Drink { Name = "Suplex", SpriteIndex = 27 };
    public static Drink ZenStar = new Drink { Name = "Zen Star", SpriteIndex = 28 };

    public static Drink[] AllDrinks = { AFedora, Absinthe, BadTouch, Beer, BleedingJane, BloomLight, BlueFairy, Brandtini, CobaltVelvet, CreviceSpike, FlamingMoai, FluffyDream, FringeWeaver, FrothyWater, GrizzlyTemple, GutPunch, Marsblast, Mercuryblast, Moonblast, MulanTea, PianoMan, PianoWoman, Piledriver, Rum, SparkleStar, SugarRush, SunshineCloud, Suplex, ZenStar };
}

sealed class Recipe
{
    public Drink Drink;
    public int[] Ingredients;
    public bool Iced;
    public bool Aged;
    public DrinkState State;

    public string LoggingString(string[] ingNames)
    {
        if (State == DrinkState.Bottled)
            return string.Format("Bottled drink: {0}", Drink.Name);

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
