using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using static Extensions_Scr;

public class UI_DiceColoring_Scr : MonoBehaviour
{
    private UIDocument doc;

    //private Button backBtn;
    private List<Button> setButtons = new List<Button>();

    [SerializeField] private UI_MainMenu_Scr mainUI;

    [SerializeField] private VisualTreeAsset coloringOptionBtn;

    [SerializeField] private GameObject[] dices;
    private int[] dicesColoringShemeIds = new int[6] { 0, 0, 0, 0, 0, 0 };

    [SerializeField] private List<DiceMaterialSetSO_Scr> diceMaterialSets;

    private int selectedMaterialId = -1;
    //TODO: прибраться в переменных
    //TODO: вырубать/врубать дайсы при включении/выключении меню
    //TODO: сохранение


    private void Awake()
    {
        doc = GetComponent<UIDocument>();

        Button backBtn = doc.rootVisualElement.Query<Button>("Back");
        backBtn.RegisterCallback<ClickEvent>(BackClick);
        Button changeAllBtn = doc.rootVisualElement.Query<Button>("ChangeAll");
        changeAllBtn.RegisterCallback<ClickEvent>(ChangeAllClick);
        Button saveBtn = doc.rootVisualElement.Query<Button>("Save");
        saveBtn.RegisterCallback<ClickEvent>(SaveClick);

        InstantiateAllColoringOptions();
        LoadDiceMaterials();
    }

    private void InstantiateAllColoringOptions()
    {
        VisualElement setsContainer = doc.rootVisualElement.Q("OptionsContainer");
        foreach (DiceMaterialSetSO_Scr set in diceMaterialSets)
        {
            TemplateContainer templateContainer = coloringOptionBtn.Instantiate();
            Button button = templateContainer.Q<Button>();
            button.RegisterCallback<ClickEvent>(MaterialSetClick);
            setButtons.Add(button);
            templateContainer.Q("Image").style.backgroundImage = new StyleBackground(set.image);
            setsContainer.Add(templateContainer);
        }
    }
    public void ChangeDiceColor(int diceId)
    {
        if (selectedMaterialId == -1)
            return;
        //Debug.Log("tryin to change color of dice " + diceId + " to material set " + selectedMaterialId);

        Renderer diceRenderer = dices[diceId].GetComponent<Renderer>();
        List<Material> materials = diceMaterialSets[selectedMaterialId].materials;
        diceRenderer.SetMaterials(materials);
        dicesColoringShemeIds[diceId] = selectedMaterialId;
    }
    public void SetDicesActive(bool isActive)
    {
        foreach (GameObject dice in dices)
            dice.SetActive(isActive);
    }
    private void LoadDiceMaterials()
    {
        string saveFilePath = Application.persistentDataPath + "/diceColoring.json";

        if (File.Exists(saveFilePath))
        {
            string jsonString = File.ReadAllText(saveFilePath);
            dicesColoringShemeIds = JsonUtility.FromJson<IntArrayWrapper>(jsonString).intArray;
        }

        for (int i = 0; i < 6; i++)
        {
            Renderer renderer = dices[i].GetComponent<Renderer>();
            List<Material> materials = diceMaterialSets[dicesColoringShemeIds[i]].materials;
            renderer.SetMaterials(materials);
        }

        //TODO: 
    }
    private void SaveDiceMaterials()
    {
        //TODO: защиту какую-то нада

        string saveFilePath = Application.persistentDataPath + "/diceColoring.json";
        IntArrayWrapper intArrayWrapper = new IntArrayWrapper(dicesColoringShemeIds);
        string jsonString = JsonUtility.ToJson(intArrayWrapper);
        File.WriteAllText(saveFilePath, jsonString);
    }


    private void ChangeAllClick(ClickEvent click)
    {
        if (selectedMaterialId == -1)
            return;

        for (int i = 0; i < 6; i++)
        {
            Renderer diceRenderer = dices[i].GetComponent<Renderer>();
            List<Material> materials = diceMaterialSets[selectedMaterialId].materials;
            diceRenderer.SetMaterials(materials);
            dicesColoringShemeIds[i] = selectedMaterialId;
        }
    }
    private void SaveClick(ClickEvent click)
    {
        SaveDiceMaterials();
    }
    private void BackClick(ClickEvent click)
    {
        mainUI.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        doc.rootVisualElement.style.display = DisplayStyle.None;

        SetDicesActive(false);
    }
    private void MaterialSetClick(ClickEvent click)
    {
        Debug.Log("clicked");
        foreach (Button btn in setButtons)
        {
            btn.style.backgroundColor = new Color(0, 0, 0, 0);
        }
        VisualElement target = click.target as VisualElement;
        target.style.backgroundColor = new Color(1, 0.796f, 0, 1);

        //Debug.Log(setButtons.IndexOf(target as Button));
        selectedMaterialId = setButtons.IndexOf(target as Button);
    }
}
