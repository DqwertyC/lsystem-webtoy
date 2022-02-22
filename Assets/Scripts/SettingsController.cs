using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
  public RulePanelEntry ruleTemplate;
  public RectTransform rulesPanel;
  public TMP_InputField systemName;
  public TMP_InputField description;
  public TMP_InputField turnAngle;
  public TMP_InputField startAngle;
  public TMP_InputField maxIteration;
  public TMP_InputField axiom;
  public TMP_InputField centerX;
  public TMP_InputField centerY;
  public TMP_InputField radius;
  public TMP_InputField jsonInput;

  public GameObject editorView;
  public GameObject jsonView;

  public DropdownController systemChooser;
  public LSystemDrawer systemRenderer;
  public IterationController iterController;
  public Slider angleSlider;
  List<RulePanelEntry> rulesInView;
  Dictionary<string, LSystem.Definition> savedSystems;

  public bool trigger;
  private bool viewIsJson;

  // Start is called before the first frame update
  void Start()
  {
    rulesInView = new List<RulePanelEntry>();
    viewIsJson = false;
    trigger = false;

    ApplySystem(LSystemSamples.KochSnowflake);
    ResetAngle();

    savedSystems = new Dictionary<string, LSystem.Definition>();

    foreach (string key in LSystem.GetPresets().Keys)
    {
      savedSystems[key] = LSystem.GetPresets()[key];
    }

    LSystem.SaveToFile(new List<LSystem.Definition>(LSystem.GetPresets().Values), "preset_systems");

    foreach (LSystem.Definition userSystem in LSystem.LoadFromFile("user_systems"))
    {
      savedSystems[userSystem.name] = userSystem;
    }

    systemChooser.RefreshDropdown(new List<LSystem.Definition>(savedSystems.Values));

    LoadFromPreset();
  }

  // Update is called once per frame
  void Update()
  {
    if (trigger)
    {
      trigger = false;
      UpdateSystem(LSystemSamples.KochSnowflake);
    }
  }

  void UpdateSystem(LSystem.Definition newDefinition)
  {
    while (rulesInView.Count > 0)
    {
      RemoveRule();
    }

    systemName.text = newDefinition.name;
    description.text = newDefinition.description;
    turnAngle.text = newDefinition.turnAngle.ToString();
    startAngle.text = newDefinition.startAngle.ToString();
    maxIteration.text = newDefinition.maxIteration.ToString();
    axiom.text = newDefinition.axiom;

    centerX.text = newDefinition.renderCenter.x.ToString();
    centerY.text = newDefinition.renderCenter.y.ToString();
    radius.text = newDefinition.renderRadius.ToString();

    foreach (LSystem.Rule rule in newDefinition.rules)
    {
      AddRule(rule);
    }
  }

  public void AddRule()
  {
    AddRule(new LSystem.Rule('.', "", LSystem.DrawInstruction.NONE));
  }

  void AddRule(LSystem.Rule rule)
  {
    float height = rulesInView.Count * 32;
    rulesPanel.sizeDelta = new Vector2(rulesPanel.sizeDelta.x, height);
    RulePanelEntry newRule = Instantiate(ruleTemplate, rulesPanel);
    newRule.rectTransform = newRule.GetComponent<RectTransform>();
    newRule.rectTransform.anchoredPosition = new Vector2(0, -height);

    newRule.key.text = rule.keyChar.ToString();
    newRule.drawInstruction.value = (int)rule.instructionEnum;
    newRule.value.text = rule.value;

    rulesInView.Add(newRule);
  }

  public void RemoveRule()
  {
    RulePanelEntry toRemove = rulesInView[rulesInView.Count - 1];
    rulesInView.RemoveAt(rulesInView.Count - 1);
    toRemove.gameObject.SetActive(false);
    Destroy(toRemove);

    float height = (rulesInView.Count - 1) * 32;
    rulesPanel.sizeDelta = new Vector2(rulesPanel.sizeDelta.x, height);
  }

  public string ExportToJson()
  {
    if (viewIsJson)
    {
      return jsonInput.text;
    }
    else
    {
      LSystem.Definition definition = GetDefinition();
      return definition.SaveToString(true);
    }
  }

  public void ToggleView()
  {
    if (viewIsJson)
    {
      string temp = jsonInput.text;
      jsonView.SetActive(false);
      editorView.SetActive(true);
      UpdateSystem(LSystem.Definition.CreateFromJSON(temp));
    }
    else
    {
      string temp = GetDefinition().SaveToString(true);
      editorView.SetActive(false);
      jsonView.SetActive(true);
      jsonInput.text = temp;
    }

    viewIsJson = !viewIsJson;
  }

  public void ImportFromJson()
  {
    UpdateSystem(LSystem.Definition.CreateFromJSON(jsonInput.text));
  }

  public void LoadFromPreset()
  {
    if (viewIsJson)
    {
      jsonInput.text = (systemChooser.GetSelectedSystem().SaveToString(true));
    }
    else
    {
      UpdateSystem(systemChooser.GetSelectedSystem());
    }
  }

  public void ApplySystem()
  {
    ApplySystem(GetDefinition());
    Close();
  }

  public void SaveSystem()
  {
    LSystem.Definition newDefinition = GetDefinition();

    if (LSystem.GetPresets().ContainsKey(newDefinition.name))
    {
      newDefinition.name = "Custom " + newDefinition.name;
    }

    LSystem.TryAddToFile(newDefinition, "user_systems", true);
    savedSystems[newDefinition.name] = newDefinition;

    systemChooser.RefreshDropdown(new List<LSystem.Definition>(savedSystems.Values));
  }

  public void ApplySystem(LSystem.Definition newDefinition)
  {
    systemRenderer.currentSystem = new LSystem(newDefinition);
    systemRenderer.turnAngle = newDefinition.turnAngle;
    angleSlider.value = newDefinition.turnAngle;
    iterController.maxValue = newDefinition.maxIteration;
    iterController.value = Math.Min(iterController.value, iterController.maxValue);
    systemRenderer.redraw = true;
  }

  public void ResetAngle()
  {
    angleSlider.value = systemRenderer.currentSystem.definition.turnAngle;
  }

  public void Close()
  {
    transform.parent.gameObject.SetActive(false);
  }

  public LSystem.Definition GetDefinition()
  {
    if (viewIsJson)
    {
      return LSystem.Definition.CreateFromJSON(jsonInput.text);
    }
    else
    {
      return new LSystem.Definition()
      {
        name = systemName.text,
        description = description.text,
        axiom = axiom.text,
        rules = GetRules(),
        turnAngle = float.Parse(turnAngle.text),
        maxIteration = int.Parse(maxIteration.text),
        startAngle = float.Parse(startAngle.text),
        renderCenter = new Vector2(float.Parse(centerX.text), float.Parse(centerY.text)),
        renderRadius = float.Parse(radius.text)
      };
    }
  }

  public List<LSystem.Rule> GetRules()
  {
    List<LSystem.Rule> rules = new List<LSystem.Rule>();

    foreach (RulePanelEntry entry in rulesInView)
    {
      rules.Add(new LSystem.Rule(entry.key.text[0], entry.value.text, (LSystem.DrawInstruction)entry.drawInstruction.value));
    }

    return rules;
  }
}
