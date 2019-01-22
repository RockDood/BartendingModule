using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;
using KModkit;

public class Maker : MonoBehaviour
{
    //Bomb Sound and Info
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;

    //Buttons
    public KMSelectable serve;
    public KMSelectable slot1;
    public KMSelectable slot2;
    public KMSelectable bottled;
    public KMSelectable mix;
    public KMSelectable trash;
    public KMSelectable aged;
    public KMSelectable iced;
    public KMSelectable close;
    public KMSelectable[] BottledOptions;
    public KMSelectable[] ingredients;
    public GameObject[] backingButtons;
    public GameObject[] menuButtons;
    public Transform agedTransform;
    public Transform icedTransform;
    public Transform slot1Transform;
    public Transform slot2Transform;

    //Ingredient Stuff
    public Texture2D[] ingredientcolor;
    public Sprite[] DrinkSprites;
    public SpriteRenderer MixerScreen1;
    public SpriteRenderer MixerScreen2;
    public Renderer[] screens;
    public GameObject BottledScreen;
    public Texture2D GrayScreen;
    private static readonly string[] ingNames = { "Powdered Delta", "Flanergide", "Adelhyde", "Bronson Extract", "Karmotrine" };
    private static readonly string[] bottledOptions = { "Absinthe", "Rum", "Mulan Tea", "A Fedora" };
    private bool mixing = false;
    const float MaxTilt = 45f;
    const float SlowRotationPeriod = 1f;
    const float FastRotationPeriod = .3f;
    private float InteractionPunchIntensityModifier = .5f;

    private static readonly Recipe[] _Drinktionary = new Recipe[]
    {
        new Recipe { Name = "Beer", Ingredients = new[] { 1, 2, 1, 2, 4 }, State = DrinkState.Mixed, SpriteIndex = 6 },
        new Recipe { Name = "Fluffy Dream", Ingredients = new[] { 3, 0, 3, 0, 2 }, Aged = true, State = DrinkState.Mixed, SpriteIndex = 9 },
        new Recipe { Name = "Bleeding Jane", Ingredients = new[] { 3, 3, 0, 1, 4 }, State = DrinkState.Blended, SpriteIndex = 8 },
        new Recipe { Name = "Sugar Rush", Ingredients = new[] { 1, 0, 2, 0, 4 }, State = DrinkState.Mixed, SpriteIndex = 14 },
        new Recipe { Name = "Piano Man", Ingredients = new[] { 1, 1, 6, 3, 2 }, Iced = true, State = DrinkState.Blended, SpriteIndex = 12 },
        new Recipe { Name = "Moonblast", Ingredients = new[] { 1, 1, 6, 0, 2 }, Iced = true, State = DrinkState.Blended, SpriteIndex = 11 },
        new Recipe { Name = "Fringe Weaver", Ingredients = new[] { 0, 0, 1, 0, 9 }, Aged = true, State = DrinkState.Mixed, SpriteIndex = 15 },
        new Recipe { Name = "Blue Fairy", Ingredients = new[] { 0, 1, 4, 0, 5 }, Aged = true, State = DrinkState.Mixed, SpriteIndex = 16 },
        new Recipe { Name = "Grizzly Temple", Ingredients = new[] { 3, 0, 3, 3, 1 }, State = DrinkState.Blended, SpriteIndex = 10 },
        new Recipe { Name = "Bloom Light", Ingredients = new[] { 1, 2, 4, 0, 3 }, Aged = true, State = DrinkState.Mixed, SpriteIndex = 7 },
        new Recipe { Name = "Frothy Water", Ingredients = new[] { 1, 1, 1, 1, 0 }, Aged = true, State = DrinkState.Mixed, SpriteIndex = 6 },
        new Recipe { Name = "Piano Woman", Ingredients = new[] { 2, 3, 5, 5, 3 }, Aged = true, State = DrinkState.Mixed, SpriteIndex = 13 },
    };

    private static readonly Patron[] _customerPreferences = new Patron[]
    {
        new Patron { Customer = "Barbara", DrinkPreference = new[] {"Blue Fairy", "Piano Man", "Frothy Water", "Piano Woman", "Fringe Weaver", "Absinthe"}, },
        new Patron { Customer = "Patricia", DrinkPreference = new[] {"Frothy Water", "Piano Woman", "Fringe Weaver", "Grizzly Temple", "A Fedora", "Rum"}, },
        new Patron { Customer = "Karl", DrinkPreference = new[] {"Piano Man", "Rum", "Grizzly Temple", "Mulan Tea", "Absinthe", "Frothy Water"}, },
        new Patron { Customer = "Konrad", DrinkPreference = new[] {"Mulan Tea", "Frothy Water", "Fluffy Dream", "Bleeding Jane", "Rum", "A Fedora"}, },
        new Patron { Customer = "Vivi", DrinkPreference = new[] {"Fluffy Dream", "Moonblast", "Absinthe", "Piano Man", "Beer", "Fringe Weaver"}, },
        new Patron { Customer = "Engilika", DrinkPreference = new[] {"Piano Woman", "Blue Fairy", "A Fedora", "Absinthe", "Bleeding Jane", "Grizzly Temple"}, },
        new Patron { Customer = "Donna", DrinkPreference = new[] {"Beer", "Sugar Rush", "Piano Woman", "Bloom Light", "Moonblast", "Mulan Tea"}, },
        new Patron { Customer = "Gabe", DrinkPreference = new[] {"Moonblast", "Mulan Tea", "Sugar Rush", "A Fedora", "Frothy Water", "Bloom Light"}, },
        new Patron { Customer = "Clayton", DrinkPreference = new[] {"Grizzly Temple", "Bloom Light", "Bleeding Jane", "Fringe Weaver", "Piano Man", "Beer"}, },
        new Patron { Customer = "Chip", DrinkPreference = new[] {"Bleeding Jane", "Grizzly Temple", "Moonblast", "Sugar Rush", "Blue Fairy", "Fluffy Dream"}, },
    };

    Recipe slot1input = new Recipe { Ingredients = new[] { 0, 0, 0, 0, 0 } };
    Recipe slot2input = new Recipe { Name = null, Ingredients = new[] { 0, 0, 0, 0, 0 } };

    Recipe expectedDrink1;
    Recipe expectedDrink2;
    private string currentPatron;
    private bool bigDrinkExpected;
    private bool _IsSolved = false;

    public TextMesh[] ingValuesText;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool slot2active = false;
    private bool bottleDrinkMenuVisible = false;
    private int[] ingIndices = new int[5];
    private Coroutine mixingActive;

    void Awake()
    {
        moduleId = moduleIdCounter++;

        for (var i = 0; i < ingredients.Length; i++)
        {
            int j = i;
            ingredients[i].OnInteract += delegate { ingredientPress(j); return false; };
        }
        for (int i = 0; i < BottledOptions.Length; i++)
        {
            var j = i;
            BottledOptions[i].OnInteract += delegate { BottledDrinkSelection(j); return false; };
        }
        serve.OnInteract += delegate { PressServe(); return false; };
        trash.OnInteract += delegate { TrashPress(); return false; };
        iced.OnInteract += delegate { ice(); return false; };
        aged.OnInteract += delegate { agedDrink(); return false; };
        slot1.OnInteract += delegate { SlotPressed(slot2: false); return false; };
        slot2.OnInteract += delegate { SlotPressed(slot2: true); return false; };
        bottled.OnInteract += delegate { bottledDrinks(); return false; };
        close.OnInteract += delegate { closeMenu(); return false; };
        mix.OnInteract += delegate { Startmixing(); return false; };
    }

    void Start()
    {
        StartCoroutine(animateButton(slot1Transform, true));
        GenerateModule();
        closeMenu();
    }

    void ingredientPress(int screenIndex)
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ingredients[screenIndex].transform);
        ingredients[screenIndex].AddInteractionPunch(InteractionPunchIntensityModifier);
        var ingIndex = ingIndices[screenIndex];
        var slot = slot2active ? slot2input : slot1input;
        slot.Ingredients[ingIndex]++;
        ingValuesText[screenIndex].text = slot.Ingredients[ingIndex].ToString();
    }

    void GenerateModule()
    {
        // Shuffle up the ingredients
        ingIndices = Shuffle(Enumerable.Range(0, 5).ToArray());
        Debug.LogFormat("[Bartending #{0}] Ingredients are: {1}", moduleId, string.Join(", ", ingIndices.Select(ix => ingNames[ix]).ToArray()));

        // Assign the screen-frame textures for the ingredients
        foreach (int i in ingIndices)
            screens[i].material.mainTexture = ingredientcolor[ingIndices[i]];

        var patron = _customerPreferences[((ingIndices[0] + 1) * 2 + (ingIndices[1] + 1)) % 10];
        currentPatron = patron.Customer;
        var drink1Index = ((ingIndices[3] + 1) * 2 + (ingIndices[4] + 1)) % 6;
        if (drink1Index > 0)
            drink1Index--;

        expectedDrink1 = FindRecipeOrBottled(patron.DrinkPreference[drink1Index]);

        if (Bomb.GetSerialNumber().Intersect("CH4S3R").Count() >= 3)
        {
            var drink2Index = (drink1Index + ingIndices[2] + 1) % 6;
            if (drink2Index < drink1Index)
            {
                expectedDrink2 = expectedDrink1;
                expectedDrink1 = FindRecipeOrBottled(patron.DrinkPreference[drink2Index]);
            }
            else
            {
                expectedDrink2 = FindRecipeOrBottled(patron.DrinkPreference[drink2Index]);
            }
        }
        else
        {
            expectedDrink1 = FindRecipeOrBottled(patron.DrinkPreference[drink1Index]);
            expectedDrink2 = new Recipe { Name = null, State = DrinkState.Unprepared };
        }

        bigDrinkExpected = Bomb.GetSerialNumber().Intersect("B1G").Count() > 0;

        Debug.LogFormat("[Bartending #{0}] The first drink is a {1}{2}.{3}", moduleId, expectedDrink1.Name,
            expectedDrink2.Name != null ? " and the second drink is a " + expectedDrink2.Name : "",
            bigDrinkExpected ? " Ingredients are expected to be doubled." : "");
    }

    private Recipe FindRecipeOrBottled(string name)
    {
        var recipe = _Drinktionary.FirstOrDefault(drink => drink.Name == name);
        if (recipe != null)
            return recipe;
        return new Recipe { Name = name, State = DrinkState.Bottled };
    }

    void Startmixing()
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, mix.transform);
        mix.AddInteractionPunch(InteractionPunchIntensityModifier);
        if (mixing)
        {
            // This will cause the coroutine to end
            mixing = false;
            return;
        }

        if ((slot2active ? slot2input : slot1input).State == DrinkState.Unprepared)
        {
            mixing = true;
            mixingActive = StartCoroutine(mixCoroutine());
        }
        else
        {
            Debug.LogFormat("[Bartending #{0}] You cannot mix a drink twice in the same slot. Please trash the drink you are trying to mix and try again.", moduleId);
        }
    }

    private IEnumerator mixCoroutine()
    {
        float elapsedTime = 0;
        var mixerScreen = slot2active ? MixerScreen2 : MixerScreen1;
        var input = slot2active ? slot2input : slot1input;
        int slot = slot2active ? 2 : 1;

        while (mixing)
        {
            elapsedTime += Time.deltaTime;
            mixerScreen.transform.localEulerAngles = new Vector3(90, 0, Mathf.Sin(elapsedTime / (elapsedTime < 7 ? SlowRotationPeriod : FastRotationPeriod) * Mathf.PI * 2) * MaxTilt);
            yield return null;
        }
        mixerScreen.transform.localEulerAngles = new Vector3(90, 0, 0);
        var elapsedSeconds = Mathf.FloorToInt(elapsedTime);
        if (elapsedSeconds <= 3)
        {
            input.State = DrinkState.Failed;
            mixerScreen.sprite = DrinkSprites[5];
            Debug.LogFormat("[Bartending #{0}] You did not mix the drink for slot {1} long enough!", moduleId, slot);
        }
        else if (elapsedSeconds <= 10)
        {
            input.State = elapsedSeconds < 7 ? DrinkState.Mixed : DrinkState.Blended;
            var matchingRecipe = _Drinktionary.FirstOrDefault(rec => input.IsSameRecipeAs(rec, false) || input.IsSameRecipeAs(rec, true));
            if (matchingRecipe != null)
            {
                mixerScreen.sprite = DrinkSprites[matchingRecipe.SpriteIndex];
                input.Name = matchingRecipe.Name;
                Debug.LogFormat("[Bartending #{0}] You {1} the slot {2} drink!", moduleId, input.State == DrinkState.Blended ? "blended" : "mixed", slot);
                Debug.LogFormat("[Bartending #{0}] You created a {1}{2}!", moduleId, input.IsSameRecipeAs(matchingRecipe, true) ? "big " : "", input.Name);
            }
            else
            {
                input.State = DrinkState.Failed;
                mixerScreen.sprite = DrinkSprites[5];
                Debug.LogFormat("[Bartending #{0}] You {1} the slot {2} drink but it didn't match a known recipe! Your attempted drink was: {3}", moduleId, input.State == DrinkState.Blended ? "blended" : "mixed", slot, input.LoggingString(ingNames));
            }
        }
        else
        {
            input.State = DrinkState.Failed;
            mixerScreen.sprite = DrinkSprites[5];
            Debug.LogFormat("[Bartending #{0}] You mixed the drink for slot {1} too long!", moduleId, slot);
        }
    }

    void TrashPress()
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, trash.transform);
        trash.AddInteractionPunch(InteractionPunchIntensityModifier);
        StopAllCoroutines();
        mixing = false;
        var input = slot2active ? slot2input : slot1input;
        for (int i = 0; i < ingredients.Length; i++)
        {
            input.Ingredients[i] = 0;
            ingValuesText[i].text = "0";
        }
        if (input.Iced)
            StartCoroutine(animateButton(icedTransform, false));
        if (input.Aged)
            StartCoroutine(animateButton(agedTransform, false));
        input.Name = null;
        input.Iced = false;
        input.Aged = false;
        input.State = DrinkState.Unprepared;


        var mixerScreen = slot2active ? MixerScreen2 : MixerScreen1;
        mixerScreen.sprite = DrinkSprites[4];
        mixerScreen.transform.localEulerAngles = new Vector3(90, 0, 0);
    }

    void ice()
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, iced.transform);
        iced.AddInteractionPunch(InteractionPunchIntensityModifier);
        var input = slot2active ? slot2input : slot1input;
        input.Iced = !input.Iced;
        StartCoroutine(animateButton(icedTransform, input.Iced));
    }

    void agedDrink()
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, aged.transform);
        aged.AddInteractionPunch(InteractionPunchIntensityModifier);
        var input = slot2active ? slot2input : slot1input;
        input.Aged = !input.Aged;
        StartCoroutine(animateButton(agedTransform, input.Aged));
    }

    private IEnumerator animateButton(Transform btn, bool setting)
    {
        const float duration = .2f;
        var elapsed = 0f;
        const float depressed = -0.013f;
        const float undepressed = 0;
        var originalPosition = btn.localPosition;
        var startValue = setting ? undepressed : depressed;
        var endValue = setting ? depressed : undepressed;
        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            btn.localPosition = new Vector3(originalPosition.x, (endValue - startValue) * elapsed / duration + startValue, originalPosition.z);
        }
        btn.localPosition = new Vector3(originalPosition.x, endValue, originalPosition.z);
    }

    void bottledDrinks()
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, bottled.transform);
        bottled.AddInteractionPunch(InteractionPunchIntensityModifier);
        bottleDrinkMenuVisible = true;
        BottledScreen.SetActive(true);
        mainButtonToggle();
        menuButtonToggle();
    }

    void closeMenu()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, close.transform);
        close.AddInteractionPunch(InteractionPunchIntensityModifier);
        bottleDrinkMenuVisible = false;
        BottledScreen.SetActive(false);
        mainButtonToggle();
        menuButtonToggle();
    }

    void mainButtonToggle()
    {
        if (bottleDrinkMenuVisible)
        {
            for (int i = 0; i < backingButtons.Length; i++)
            {
                backingButtons[i].SetActive(false);
            }
        }
        else if (!bottleDrinkMenuVisible)
        {
            for (int i = 0; i < backingButtons.Length; i++)
            {
                backingButtons[i].SetActive(true);
            }
        }
    }

    void menuButtonToggle()
    {
        if (!bottleDrinkMenuVisible)
        {
            for (int i = 0; i < menuButtons.Length; i++)
            {
                menuButtons[i].SetActive(false);
            }
        }
        else if (bottleDrinkMenuVisible)
        {
            for (int i = 0; i < menuButtons.Length; i++)
                menuButtons[i].SetActive(true);
        }
    }

    void SlotPressed(bool slot2)
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, slot1.transform);
        slot1.AddInteractionPunch(InteractionPunchIntensityModifier);
        Debug.LogFormat("[Bartending #{0}] You switched to slot {1}.", moduleId, slot2 ? 2 : 1);
        slot2active = slot2;
        for (int i = 0; i < ingNames.Length; i++)
            ingValuesText[i].text = (slot2 ? slot2input : slot1input).Ingredients[ingIndices[i]].ToString();
        MixerScreen1.gameObject.SetActive(!slot2);
        MixerScreen2.gameObject.SetActive(slot2);
        var input = slot2 ? slot2input : slot1input;

        StartCoroutine(animateButton(slot1Transform, !slot2));
        StartCoroutine(animateButton(slot2Transform, slot2));

        if (slot1input.Aged != slot2input.Aged)
            StartCoroutine(animateButton(agedTransform, input.Aged));

        if (slot1input.Iced != slot2input.Iced)
            StartCoroutine(animateButton(icedTransform, input.Iced));

    }

    void BottledDrinkSelection(int i)
    {
        BottledOptions[i].AddInteractionPunch(InteractionPunchIntensityModifier);
        var mixerScreen = slot2active ? MixerScreen2 : MixerScreen1;
        var input = slot2active ? slot2input : slot1input;
        Debug.LogFormat("[Bartending #{0}] You pressed {1} which is now in slot {2}!", moduleId, bottledOptions[i], slot2active ? 2 : 1);
        input.Name = bottledOptions[i];
        input.State = DrinkState.Bottled;
        mixerScreen.sprite = DrinkSprites[i];
        closeMenu();
    }

    void PressServe()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, serve.transform);
        serve.AddInteractionPunch();
        if (_IsSolved)
            return;

        for (int slot = 0; slot < 2; slot++)
        {
            var preparedDrink = slot == 1 ? slot2input : slot1input;
            var expectedDrink = slot == 1 ? expectedDrink2 : expectedDrink1;

            if (preparedDrink.State == DrinkState.Bottled)
            {
                Debug.LogFormat("[Bartending #{0}] You submitted {1}. {2} wanted {3}.", moduleId, preparedDrink.Name, currentPatron, expectedDrink.Name);
                if (preparedDrink.Name == expectedDrink.Name)
                {
                    Debug.LogFormat("[Bartending #{0}] The {1} drink is correct!", moduleId, slot == 0 ? "first" : "second");
                }
                else
                {
                    StrikeAndRegenerate();
                    return;
                }
            }
            else if (preparedDrink.State == DrinkState.Mixed || preparedDrink.State == DrinkState.Blended)
            {
                var isBigDrink = preparedDrink.IsSameRecipeAs(expectedDrink, true);
                Debug.LogFormat("[Bartending #{0}] You submitted a {1}{2} ({3}). {4} wanted a {5}{6}.", moduleId,
                    isBigDrink ? "big " : "", preparedDrink.Name, preparedDrink.LoggingString(ingNames), currentPatron, bigDrinkExpected ? "big " : "", expectedDrink.Name);
                if (preparedDrink.Name == expectedDrink.Name && isBigDrink == bigDrinkExpected)
                {
                    Debug.LogFormat("[Bartending #{0}] The {1} drink is correct!", moduleId, slot == 0 ? "first" : "second");
                }
                else
                {
                    StrikeAndRegenerate();
                    return;
                }
            }
            else if (preparedDrink.State == DrinkState.Unprepared)
            {
                Debug.LogFormat("[Bartending #{0}] You submitted no drink in slot {1}. {2} wanted {3}.", moduleId, slot == 0 ? 1 : 2, currentPatron, expectedDrink2.State == DrinkState.Unprepared ? "1 drink" : "2 drinks");
                if (expectedDrink.State == DrinkState.Unprepared)
                {
                    Debug.LogFormat("[Bartending #{0}] The {1} drink is correct!", moduleId, slot == 0 ? "first" : "second");
                }
                else
                {
                    Debug.LogFormat("[Bartending #{0}] You submitted an unprepared drink when {1} expected a drink! Strike!", moduleId, currentPatron);
                    StrikeAndRegenerate();
                    return;
                }
            }
            else
            {
                Debug.LogFormat("[Bartending #{0}] You tried to submit a failed drink!", moduleId);
                StrikeAndRegenerate();
                return;
            }
        }

        // Both drinks were correct!
        Debug.LogFormat("[Bartending #{0}] Drinks correctly served! Module solved!", moduleId);
        Module.HandlePass();
        if (Bomb.GetSolvedModuleNames().Count < Bomb.GetSolvableModuleNames().Count)
            Audio.PlaySoundAtTransform("solve", transform);
        for (int i = 0; i < screens.Length; i++)
            screens[i].material.mainTexture = GrayScreen;
        _IsSolved = true;
    }

    private void StrikeAndRegenerate()
    {
        Module.HandleStrike();
        GenerateModule();
    }

    private static T Shuffle<T>(T list) where T : IList
    {
        if (list == null)
            throw new ArgumentNullException("list");
        for (int j = list.Count; j >= 1; j--)
        {
            int item = Rnd.Range(0, j);
            if (item < j - 1)
            {
                var t = list[item];
                list[item] = list[j - 1];
                list[j - 1] = t;
            }
        }
        return list;
    }
}


