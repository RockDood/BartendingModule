using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using KModkit;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Maker : MonoBehaviour
{
    //Bomb Sound and Info
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMRuleSeedable RuleSeedable;

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
    private bool mixing = false;

    private const int _failedDrinkSpriteIndex = 5;
    private const float _maxTilt = 45f;
    private const float _slowRotationPeriod = 1f;
    private const float _fastRotationPeriod = .3f;
    private const float _interactionPunchIntensity = .5f;

    // The patron names don’t have any impact on the rules, so they stay the same in all rule seeds
    private static readonly string[] _patronNames = { "Barbara", "Patricia", "Karl", "Konrad", "Vivi", "Angelika", "Donna", "Gabe", "Clayton", "Chip" };
    private static readonly string[] _ingNames = { "Powdered Delta", "Flanergide", "Adelhyde", "Bronson Extract", "Karmotrine" };
    private static readonly Drink[] _bottledOptions = { Drink.Absinthe, Drink.Rum, Drink.MulanTea, Drink.AFedora };

    private static readonly Recipe[] _seed1_drinktionary = new Recipe[]
    {
        new Recipe { Drink = Drink.Beer, Ingredients = new[] { 1, 2, 1, 2, 4 }, State = DrinkState.Mixed },
        new Recipe { Drink = Drink.FluffyDream, Ingredients = new[] { 3, 0, 3, 0, 2 }, Aged = true, State = DrinkState.Mixed },
        new Recipe { Drink = Drink.BleedingJane, Ingredients = new[] { 3, 3, 0, 1, 4 }, State = DrinkState.Blended },
        new Recipe { Drink = Drink.SugarRush, Ingredients = new[] { 1, 0, 2, 0, 4 }, State = DrinkState.Mixed },
        new Recipe { Drink = Drink.PianoMan, Ingredients = new[] { 1, 1, 6, 3, 2 }, Iced = true, State = DrinkState.Blended },
        new Recipe { Drink = Drink.Moonblast, Ingredients = new[] { 1, 1, 6, 0, 2 }, Iced = true, State = DrinkState.Blended },
        new Recipe { Drink = Drink.FringeWeaver, Ingredients = new[] { 0, 0, 1, 0, 9 }, Aged = true, State = DrinkState.Mixed },
        new Recipe { Drink = Drink.BlueFairy, Ingredients = new[] { 0, 1, 4, 0, 5 }, Aged = true, State = DrinkState.Mixed },
        new Recipe { Drink = Drink.GrizzlyTemple, Ingredients = new[] { 3, 0, 3, 3, 1 }, State = DrinkState.Blended },
        new Recipe { Drink = Drink.BloomLight, Ingredients = new[] { 1, 2, 4, 0, 3 }, Aged = true, Iced = true, State = DrinkState.Mixed },
        new Recipe { Drink = Drink.FrothyWater, Ingredients = new[] { 1, 1, 1, 1, 0 }, Aged = true, State = DrinkState.Mixed },
        new Recipe { Drink = Drink.PianoWoman, Ingredients = new[] { 2, 3, 5, 5, 3 }, Aged = true, State = DrinkState.Mixed },
    };

    private static readonly Drink[][] _seed1_customerPreferences = new Drink[][]
    {
        new[] { Drink.BlueFairy, Drink.PianoMan, Drink.FrothyWater, Drink.PianoWoman, Drink.FringeWeaver, Drink.Absinthe },
        new[] { Drink.FrothyWater, Drink.PianoWoman, Drink.FringeWeaver, Drink.GrizzlyTemple, Drink.AFedora, Drink.Rum },
        new[] { Drink.PianoMan, Drink.Rum, Drink.GrizzlyTemple, Drink.MulanTea, Drink.Absinthe, Drink.FrothyWater },
        new[] { Drink.MulanTea, Drink.FrothyWater, Drink.FluffyDream, Drink.BleedingJane, Drink.Rum, Drink.AFedora },
        new[] { Drink.FluffyDream, Drink.Moonblast, Drink.Absinthe, Drink.PianoMan, Drink.Beer, Drink.FringeWeaver },
        new[] { Drink.PianoWoman, Drink.BlueFairy, Drink.AFedora, Drink.Absinthe, Drink.BleedingJane, Drink.GrizzlyTemple },
        new[] { Drink.Beer, Drink.SugarRush, Drink.PianoWoman, Drink.BloomLight, Drink.Moonblast, Drink.MulanTea },
        new[] { Drink.Moonblast, Drink.MulanTea, Drink.SugarRush, Drink.AFedora, Drink.FrothyWater, Drink.BloomLight },
        new[] { Drink.GrizzlyTemple, Drink.BloomLight, Drink.BleedingJane, Drink.FringeWeaver, Drink.PianoMan, Drink.Beer },
        new[] { Drink.BleedingJane, Drink.GrizzlyTemple, Drink.Moonblast, Drink.SugarRush, Drink.BlueFairy, Drink.FluffyDream },
    };

    private static readonly int[] _seed1_ingredientValues = { 1, 2, 3, 4, 5 };
    private static readonly string _seed1_chaser = "CH4S3R";
    private static readonly string _seed1_big = "B1G";

    private Recipe[] _drinktionary;
    private Drink[][] _customerPreferences;
    private int[] _ingredientValues;
    private string _chaser;
    private string _big;

    readonly Recipe slot1input = new Recipe { Drink = null, Ingredients = new[] { 0, 0, 0, 0, 0 } };
    readonly Recipe slot2input = new Recipe { Drink = null, Ingredients = new[] { 0, 0, 0, 0, 0 } };

    Drink expectedDrink1;
    Drink expectedDrink2;
    private int currentPatron;
    private bool bigDrinkExpected;
    private bool _IsSolved = false;

    public TextMesh[] ingValuesText;

    static int _moduleIdCounter = 1;
    int _moduleId;
    private bool slot2active = false;
    private bool bottleDrinkMenuVisible = false;
    private int[] ingIndices = new int[5];
    //private int[] ingTest = new[] { 1, 4, 3, 2, 0 };
    private int _Blank = 0;

    void Awake()
    {
        _moduleId = _moduleIdCounter++;

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

        // Rule seed!
        var rnd = RuleSeedable.GetRNG();
        if (rnd.Seed == 1)
        {
            _drinktionary = _seed1_drinktionary;
            _customerPreferences = _seed1_customerPreferences;
            _ingredientValues = _seed1_ingredientValues;
            _chaser = _seed1_chaser;
            _big = _seed1_big;
        }
        else
        {
            _drinktionary = new Recipe[12];
            var nonBottledDrinks = rnd.ShuffleFisherYates(Drink.AllDrinks.Where(d => !d.Bottled).ToList());
            for (var dr = 0; dr < _drinktionary.Length; dr++)
            {
                var ingredients = new int[5];
                var iter = 0;
                do
                {
                    iter++;
                    for (var ing = 0; ing < 5; ing++)
                        // Cube the random number to allow lower numbers to occur more frequently and high numbers less frequently
                        ingredients[ing] = (int) (10 * Math.Pow(rnd.NextDouble(), 3));
                    if (iter > 1000)
                    {
                        Debug.LogFormat(@"<Bartending #{0}> iteration fail", _moduleId);
                        return;
                    }
                }
                while (_drinktionary.Take(dr).Any(d => d.Ingredients.SequenceEqual(ingredients)));

                _drinktionary[dr] = new Recipe { Drink = nonBottledDrinks[dr], Ingredients = ingredients, Aged = rnd.Next(0, 2) != 0, Iced = rnd.Next(0, 2) != 0, State = rnd.Next(0, 2) != 0 ? DrinkState.Blended : DrinkState.Mixed };
                Debug.LogFormat(@"<Bartending #{0}> Drinktionary entry {1}: {2} [{3}] Aged={4}, Iced={5}, {6}", _moduleId, dr + 1, _drinktionary[dr].Drink.Name,
                    _drinktionary[dr].Ingredients.Join(", "), _drinktionary[dr].Aged, _drinktionary[dr].Iced, _drinktionary[dr].State);
            }

            var allDrinks = nonBottledDrinks.Take(_drinktionary.Length).Concat(Drink.AllDrinks.Where(d => d.Bottled)).ToList();
            _customerPreferences = new Drink[_patronNames.Length][];
            for (var c = 0; c < _patronNames.Length; c++)
            {
                rnd.ShuffleFisherYates(allDrinks);
                _customerPreferences[c] = allDrinks.Take(6).ToArray();
                Debug.LogFormat(@"<Bartending #{0}> Customer {1} preferences: {2}", _moduleId, _patronNames[c], _customerPreferences[c].Select(d => d.Name).Join(", "));
            }
            _ingredientValues = rnd.ShuffleFisherYates(new[] { 1, 2, 3, 4, 5 });
            Debug.LogFormat(@"<Bartending #{0}> Ingredient values = [{1}]", _moduleId, _ingredientValues.Join(", "));
            var chars = rnd.ShuffleFisherYates("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()).Join("");
            _chaser = chars.Substring(0, 6);
            _big = chars.Substring(6, 3);
            Debug.LogFormat(@"<Bartending #{0}> Chaser = {1}, Big = {2}", _moduleId, _chaser, _big);
        }

        GenerateModule();
        closeMenu();
    }

    void ingredientPress(int screenIndex)
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ingredients[screenIndex].transform);
        ingredients[screenIndex].AddInteractionPunch(_interactionPunchIntensity);
        var ingIndex = ingIndices[screenIndex];
        var slot = slot2active ? slot2input : slot1input;
        slot.Ingredients[ingIndex]++;
        ingValuesText[screenIndex].text = slot.Ingredients[ingIndex].ToString();
    }

    void GenerateModule()
    {
        // Shuffle up the ingredients
        ingIndices = Shuffle(Enumerable.Range(0, 5).ToArray());
        //ingIndices = ingTest;
        Debug.LogFormat("[Bartending #{0}] Ingredients are: {1}", _moduleId, string.Join(", ", ingIndices.Select(ix => _ingNames[ix]).ToArray()));

        // Assign the screen-frame textures for the ingredients
        foreach (int i in ingIndices)
            screens[i].material.mainTexture = ingredientcolor[ingIndices[i]];

        currentPatron = (_ingredientValues[ingIndices[0]] * 2 + _ingredientValues[ingIndices[1]]) % 10;
        var drink1Index = (_ingredientValues[ingIndices[3]] * 2 + _ingredientValues[ingIndices[4]]) % 7;
        if (drink1Index > 0)
            drink1Index--;
        Debug.LogFormat("[Bartending #{0}] You are now serving {1}.", _moduleId, _patronNames[currentPatron]);

        expectedDrink1 = _customerPreferences[currentPatron][drink1Index];

        if (Bomb.GetSerialNumber().Intersect(_chaser).Count() >= 3)
        {
            var drink2Index = (drink1Index + _ingredientValues[ingIndices[2]]) % 6;
            if (drink2Index < drink1Index)
            {
                expectedDrink2 = expectedDrink1;
                expectedDrink1 = _customerPreferences[currentPatron][drink2Index];
            }
            else
            {
                expectedDrink2 = _customerPreferences[currentPatron][drink2Index];
            }
        }
        else
        {
            expectedDrink1 = _customerPreferences[currentPatron][drink1Index];
            expectedDrink2 = null;
        }

        bigDrinkExpected = Bomb.GetSerialNumber().Intersect(_big).Count() > 0;

        if (expectedDrink2 == null)
            Debug.LogFormat(@"[Bartending #{0}] The expected drink is a {1}.", _moduleId, expectedDrink1.Name);
        else
            Debug.LogFormat(@"[Bartending #{0}] The expected drinks are {1} in slot 1 and {2} in slot 2.", _moduleId, expectedDrink1.Name, expectedDrink2.Name);
        if (bigDrinkExpected)
            Debug.LogFormat(@"[Bartending #{0}] Ingredients are expected to be doubled.", _moduleId);
    }

    void Startmixing()
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, mix.transform);
        mix.AddInteractionPunch(_interactionPunchIntensity);
        if (mixing)
        {
            // This will cause the coroutine to end
            mixing = false;
            return;
        }

        if ((slot2active ? slot2input : slot1input).State == DrinkState.Unprepared)
        {
            mixing = true;
            StartCoroutine(mixCoroutine());
        }
        else
        {
            Debug.LogFormat("[Bartending #{0}] You cannot mix a drink twice in the same slot. Please trash the drink you are trying to mix and try again.", _moduleId);
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
            mixerScreen.transform.localEulerAngles = new Vector3(90, 0, Mathf.Sin(elapsedTime / (elapsedTime < 7 ? _slowRotationPeriod : _fastRotationPeriod) * Mathf.PI * 2) * _maxTilt);
            yield return null;
        }
        mixerScreen.transform.localEulerAngles = new Vector3(90, 0, 0);
        var elapsedSeconds = Mathf.FloorToInt(elapsedTime);
        if (elapsedSeconds <= 3)
        {
            input.State = DrinkState.Failed;
            mixerScreen.sprite = DrinkSprites[_failedDrinkSpriteIndex];
            Debug.LogFormat("[Bartending #{0}] You did not mix the drink for slot {1} long enough!", _moduleId, slot);
        }
        else if (elapsedSeconds <= 10)
        {
            input.State = elapsedSeconds < 7 ? DrinkState.Mixed : DrinkState.Blended;
            var matchingRecipe = _drinktionary.FirstOrDefault(rec => input.IsSameRecipeAs(rec, false) || input.IsSameRecipeAs(rec, true));
            if (matchingRecipe != null)
            {
                mixerScreen.sprite = DrinkSprites[matchingRecipe.Drink.SpriteIndex];
                input.Drink = matchingRecipe.Drink;
                Debug.LogFormat("[Bartending #{0}] You {1} the slot {2} drink!", _moduleId, input.State == DrinkState.Blended ? "blended" : "mixed", slot);
                Debug.LogFormat("[Bartending #{0}] You created a {1}{2}!", _moduleId, input.IsSameRecipeAs(matchingRecipe, true) ? "big " : "", input.Drink.Name);
            }
            else
            {
                input.State = DrinkState.Failed;
                mixerScreen.sprite = DrinkSprites[_failedDrinkSpriteIndex];
                Debug.LogFormat("[Bartending #{0}] You {1} the slot {2} drink but it didn't match a known recipe! Your attempted drink was: {3}", _moduleId, input.State == DrinkState.Blended ? "blended" : "mixed", slot, input.LoggingString(_ingNames));
            }
        }
        else
        {
            input.State = DrinkState.Failed;
            mixerScreen.sprite = DrinkSprites[_failedDrinkSpriteIndex];
            Debug.LogFormat("[Bartending #{0}] You mixed the drink for slot {1} too long!", _moduleId, slot);
        }
    }

    void TrashPress()
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, trash.transform);
        trash.AddInteractionPunch(_interactionPunchIntensity);
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
        input.Drink = null;
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
        iced.AddInteractionPunch(_interactionPunchIntensity);
        var input = slot2active ? slot2input : slot1input;
        input.Iced = !input.Iced;
        StartCoroutine(animateButton(icedTransform, input.Iced));
    }

    void agedDrink()
    {
        if (_IsSolved)
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, aged.transform);
        aged.AddInteractionPunch(_interactionPunchIntensity);
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
        bottled.AddInteractionPunch(_interactionPunchIntensity);
        bottleDrinkMenuVisible = true;
        BottledScreen.SetActive(true);
        mainButtonToggle();
        menuButtonToggle();
    }

    void closeMenu()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, close.transform);
        close.AddInteractionPunch(_interactionPunchIntensity);
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
        slot1.AddInteractionPunch(_interactionPunchIntensity);
        Debug.LogFormat("[Bartending #{0}] You switched to slot {1}.", _moduleId, slot2 ? 2 : 1);
        slot2active = slot2;
        for (int i = 0; i < _ingNames.Length; i++)
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
        BottledOptions[i].AddInteractionPunch(_interactionPunchIntensity);
        var mixerScreen = slot2active ? MixerScreen2 : MixerScreen1;
        var input = slot2active ? slot2input : slot1input;
        Debug.LogFormat("[Bartending #{0}] You pressed {1} which is now in slot {2}!", _moduleId, _bottledOptions[i].Name, slot2active ? 2 : 1);
        input.Drink = _bottledOptions[i];
        input.State = DrinkState.Bottled;
        mixerScreen.sprite = DrinkSprites[input.Drink.SpriteIndex];
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
            var prepared = slot == 1 ? slot2input : slot1input;
            var expectedDrink = slot == 1 ? expectedDrink2 : expectedDrink1;

            if (prepared.State == DrinkState.Bottled)
            {
                Debug.LogFormat("[Bartending #{0}] You served a {1}. {2} wanted {3}.", _moduleId, prepared.Drink.Name, _patronNames[currentPatron], expectedDrink == null ? "nothing" : expectedDrink.Name);
                if (prepared.Drink == expectedDrink)
                {
                    Debug.LogFormat("[Bartending #{0}] The {1} drink is correct!", _moduleId, slot == 0 ? "first" : "second");
                }
                else
                {
                    StrikeAndRegenerate();
                    return;
                }
            }
            else if (prepared.State == DrinkState.Mixed || prepared.State == DrinkState.Blended)
            {
                if (expectedDrink == null)
                {
                    Debug.LogFormat("[Bartending #{0}] You served a {2}, but {1} didn’t want a second drink.", _moduleId, _patronNames[currentPatron], prepared.Drink.Name);
                    StrikeAndRegenerate();
                    return;
                }
                if (expectedDrink.Bottled)
                {
                    Debug.LogFormat("[Bartending #{0}] You served a {3}, but {1} wanted a {2}.", _moduleId, _patronNames[currentPatron], expectedDrink.Name, prepared.Drink.Name);
                    StrikeAndRegenerate();
                    return;
                }
                var expectedRecipe = _drinktionary.First(rec => rec.Drink == expectedDrink);
                var isBigDrink = prepared.IsSameRecipeAs(expectedRecipe, true);
                Debug.LogFormat("[Bartending #{0}] You served a {1}{2} ({3}). {4} wanted a {5}{6}.", _moduleId,
                    isBigDrink ? "big " : "", prepared.Drink.Name, prepared.LoggingString(_ingNames), _patronNames[currentPatron], bigDrinkExpected ? "big " : "", expectedDrink.Name);
                if (prepared.Drink == expectedDrink && isBigDrink == bigDrinkExpected)
                {
                    Debug.LogFormat("[Bartending #{0}] The {1} drink is correct!", _moduleId, slot == 0 ? "first" : "second");
                }
                else
                {
                    StrikeAndRegenerate();
                    return;
                }
            }
            else if (prepared.State == DrinkState.Unprepared)
            {
                Debug.LogFormat("[Bartending #{0}] You served no drink in slot {1}. {2} wanted {3}.", _moduleId, slot == 0 ? 1 : 2, _patronNames[currentPatron], expectedDrink2 == null ? "1 drink" : "2 drinks");
                if (expectedDrink == null)
                {
                    Debug.LogFormat("[Bartending #{0}] The {1} drink is correct!", _moduleId, slot == 0 ? "first" : "second");
                }
                else
                {
                    Debug.LogFormat("[Bartending #{0}] You served no drink when {1} expected a drink! Strike!", _moduleId, _patronNames[currentPatron]);
                    StrikeAndRegenerate();
                    return;
                }
            }
            else
            {
                Debug.LogFormat("[Bartending #{0}] You tried to serve a failed drink!", _moduleId);
                StrikeAndRegenerate();
                return;
            }
        }

        // Both drinks were correct!
        Debug.LogFormat("[Bartending #{0}] Drinks correctly served! Module solved!", _moduleId);
        Module.HandlePass();
        if (Bomb.GetSolvedModuleNames().Count < Bomb.GetSolvableModuleNames().Count)
            Audio.PlaySoundAtTransform("solve", transform);
        for (int i = 0; i < screens.Length; i++)
            screens[i].material.mainTexture = GrayScreen;
        for (int i = 0; i < _ingNames.Length; i++)
            ingValuesText[i].text = _Blank.ToString();
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

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} add 1 9, 2 2 [add 9 times ingredient 1, 2 times ingredient 2] | !{0} slot 1 | !{0} slot 2 | !{0} trash | !{0} iced | !{0} aged | !{0} mix 4 [mix for that many seconds] | !{0} bottled A Fedora | !{0} serve";
    private bool TwitchShouldCancelCommand;
#pragma warning restore 414

    public IEnumerator ProcessTwitchCommand(string command)
    {
        Match match;

        // !{0} add 1 9, 2 2 [add 9 times ingredient 1, 2 times ingredient 2]
        if ((match = Regex.Match(command, @"^\s*add\s+(\d\s+\d+(,\s*\d\s+\d+)*)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            var data = match.Groups[1].Value.Split(',')
                .Select(inf => Regex.Match(inf, @"^\s*(\d+)\s+(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                .Select(m => new { Ingredient = int.Parse(m.Groups[1].Value), Amount = int.Parse(m.Groups[2].Value) })
                .ToArray();
            if (data.Any(inf => inf.Ingredient < 1 || inf.Ingredient > 5 || inf.Amount < 1 || inf.Amount > 20))
            {
                yield return "sendtochaterror Ingredients must be 1–5 (reading order) and amount must be 1–20.";
                yield break;
            }
            yield return null;
            if (bottleDrinkMenuVisible)
                yield return new[] { close };
            if (mixing)
                yield return new[] { mix };
            yield return data.SelectMany(inf => Enumerable.Repeat(ingredients[inf.Ingredient - 1], inf.Amount)).ToArray();
            yield break;
        }

        // !{0} slot 1, slot 2
        else if ((match = Regex.Match(command, @"^\s*slot\s+([12])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            yield return null;
            if (bottleDrinkMenuVisible)
                yield return new[] { close };
            if (mixing)
                yield return new[] { mix };
            yield return new[] { match.Groups[1].Value == "1" ? slot1 : slot2 };
        }

        // !{0} trash
        else if (Regex.IsMatch(command, @"^\s*trash\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (bottleDrinkMenuVisible)
                yield return new[] { close };
            if (mixing)
                yield return new[] { mix };
            yield return new[] { trash };
        }

        else if (Regex.IsMatch(command, @"^\s*stopmixing\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (mixing)
            {
                yield return null;
                yield return new[] { mix };
            }
        }

        // !{0} mix 4 [mix for that many seconds]
        else if ((match = Regex.Match(command, @"^\s*mix\s+(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            if (mixing)
            {
                yield return "sendtochaterror The drink is already mixing. To cancel it, use: !{0} stopmixing";
                yield break;
            }

            yield return null;
            if (bottleDrinkMenuVisible)
                yield return new[] { close };

            mix.OnInteract();
            var remainingDuration = float.Parse(match.Groups[1].Value);
            TwitchShouldCancelCommand = false;

            while (remainingDuration > 0 && !TwitchShouldCancelCommand)
            {
                yield return null;
                remainingDuration -= Time.deltaTime;
            }
            yield return new[] { mix };

            if (TwitchShouldCancelCommand)
                yield return "cancelled";
        }

        // !{0} iced
        else if (Regex.IsMatch(command, @"^\s*iced\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (bottleDrinkMenuVisible)
                yield return new[] { close };
            if (mixing)
                yield return new[] { mix };
            yield return new[] { iced };
        }

        // !{0} aged
        else if (Regex.IsMatch(command, @"^\s*aged\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (bottleDrinkMenuVisible)
                yield return new[] { close };
            if (mixing)
                yield return new[] { mix };
            yield return new[] { aged };
        }

        // !{0} bottled <drink>
        else if ((match = Regex.Match(command, @"^\s*bottled\s+(.*?)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            var button =
                Regex.IsMatch(match.Groups[1].Value, @"^\s*absinthe\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ? BottledOptions[0] :
                Regex.IsMatch(match.Groups[1].Value, @"^\s*rum\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ? BottledOptions[1] :
                Regex.IsMatch(match.Groups[1].Value, @"^\s*mulan\s+tea\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ? BottledOptions[2] :
                Regex.IsMatch(match.Groups[1].Value, @"^\s*a\s+fedora\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ? BottledOptions[3] : null;
            if (button == null)
            {
                yield return @"sendtochaterror That’s not a bottled drink I recognize!";
                yield break;
            }

            yield return null;
            if (mixing)
                yield return new[] { mix };
            if (!bottleDrinkMenuVisible)
                yield return new[] { bottled };
            yield return new[] { button };
        }

        // !{0} serve
        else if (Regex.IsMatch(command, @"^\s*serve\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (bottleDrinkMenuVisible)
                yield return new[] { close };
            if (mixing)
                yield return new[] { mix };
            yield return new[] { serve };
        }
    }
}
