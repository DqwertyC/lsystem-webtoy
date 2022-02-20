using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Text;
using System.Reflection;

public class LSystem
{
    public LSystem.Definition definition;
    public Bounds axiomBounds;

    private List<List<char>> calculatedChars;
    private Dictionary<(int, float), List<List<Vector3>>> calculatedPaths;
    private Dictionary<char, Rule> ruleDictionary;

    private int desiredIteration;
    private float desiredAngle;
    private Semaphore controlLock;

    ////////////////////////////////////////////////////////////////////////
    //********************************************************************//
    //********************************************************************//
    //**********************  System Instantiation  **********************//
    //********************************************************************//
    //********************************************************************//
    ////////////////////////////////////////////////////////////////////////

    public LSystem(LSystem.Definition systemDefinition)
    {
        definition = systemDefinition;
        ruleDictionary = new Dictionary<char, Rule>();
        calculatedChars = new List<List<char>>();
        calculatedPaths = new Dictionary<(int, float), List<List<Vector3>>>();
        calculatedChars.Add(new List<char>(definition.axiom.ToCharArray()));

        desiredIteration = 0;
        controlLock = new Semaphore(1, 1);

        UpdateDictionary();
        axiomBounds = new Bounds(definition.renderCenter, Vector2.zero);

        Vector2 startPoint = Vector2.up * definition.renderRadius;
        float angle = 0;

        while (angle <= 360)
        {
            axiomBounds.Encapsulate(((Vector3)definition.renderCenter) + Rotate(startPoint,angle));
            angle += definition.turnAngle;
        }
    }

    private void UpdateDictionary()
    {
        ruleDictionary = new Dictionary<char, Rule>();

        foreach (Rule entry in definition.rules)
        {
            ruleDictionary[entry.keyChar] = entry;
        }
    }

    public static bool TryAddToFile(Definition newDefinition, string resourceName, bool overwrite = false)
    {
        List<Definition> savedDefinitions = LoadFromFile(resourceName);
        List<Definition> collisions = new List<Definition>();

        foreach (Definition oldDefinition in savedDefinitions)
        {
            collisions.Add(oldDefinition);
        }

        if (collisions.Count > 0)
        {
            if (overwrite)
            {
                foreach (Definition collision in collisions)
                {
                    savedDefinitions.Remove(collision);
                }
            }
            else
            {
                return false;
            }
        }

        savedDefinitions.Add(newDefinition);
        SaveToFile(savedDefinitions, resourceName);
        return true;
    }

    public static List<Definition> LoadFromFile(string resourceName)
    {
        string path = Path.Combine(Application.persistentDataPath, resourceName + ".json");

        StreamReader file;
        if (File.Exists(path))
        {
            file = File.OpenText(path);
            string json = file.ReadToEnd();
            file.Close();

            return DefinitionList.CreateFromJSON(json).lsystems;
        }

        return new List<Definition>();
    }

    public static void SaveToFile(List<Definition> definitions, string resourceName)
    {
        string path = Path.Combine(Application.persistentDataPath, resourceName + ".json");

        FileStream file;
        if (File.Exists(path)) file = File.OpenWrite(path);
        else file = File.Create(path);

        file.Write(Encoding.ASCII.GetBytes(new DefinitionList() { lsystems = definitions }.SaveToString()));
        file.Close();
    }

    private static Dictionary<string, Definition> presets;
    public static Dictionary<string, Definition> GetPresets()
    {
        if (null == presets)
        {
            presets = new Dictionary<string, Definition>();

            foreach (FieldInfo publicField in typeof(LSystemSamples).GetFields())
            {
                if (typeof(LSystem.Definition) == publicField.FieldType)
                {
                    LSystem.Definition system = (LSystem.Definition)publicField.GetValue(typeof(LSystem.Definition));
                    presets[system.name] = system;
                }
            }
        }

        return presets;
    }

    ////////////////////////////////////////////////////////////////////////
    //********************************************************************//
    //********************************************************************//
    //***********************  System Calculation  ***********************//
    //********************************************************************//
    //********************************************************************//
    ////////////////////////////////////////////////////////////////////////

    public List<List<Vector3>> GetPaths(int desiredIteration, float desiredAngle = -1)
    {
        int iteration = Mathf.Clamp(desiredIteration, 0, definition.maxIteration);
        float angle = (desiredAngle == -1 ? definition.turnAngle : desiredAngle) % 360;

        if (!calculatedPaths.ContainsKey((iteration, angle)))
        {
            CalculatePaths(iteration, angle);
        }

        return calculatedPaths[(iteration, angle)];
    }

    private void CalculatePaths(int desiredIteration, float desiredAngle = -1)
    {
        int iteration = Mathf.Clamp(desiredIteration, 0, definition.maxIteration);
        float angle = (desiredAngle == -1 ? definition.turnAngle : desiredAngle) % 360;

        List<char> iterationInstructions = GetInstructionChars(iteration);
        List<List<Vector3>> paths = new List<List<Vector3>>();
        List<Vector3> currentPath = new List<Vector3>();
        currentPath.Add(Vector3.zero);

        Vector3 pos = Vector3.zero;
        Vector3 dir = Vector3.right;
        dir = Rotate(dir, definition.startAngle);

        Stack<(Vector3, Vector3)> stack = new Stack<(Vector3, Vector3)>();

        foreach (char c in iterationInstructions)
        {
            switch (ruleDictionary[c].instructionEnum)
            {
                case DrawInstruction.DRAW:
                    pos += dir;
                    currentPath.Add(pos);
                    break;
                case DrawInstruction.MOVE:
                    if (currentPath.Count >= 2)
                        paths.Add(currentPath);
                    pos += dir;
                    currentPath = new List<Vector3>();
                    currentPath.Add(pos);
                    break;
                case DrawInstruction.LEFT:
                    dir = Rotate(dir, angle);
                    break;
                case DrawInstruction.RIGHT:
                    dir = Rotate(dir, -angle);
                    break;
                case DrawInstruction.PUSH:
                    stack.Push((pos, dir));
                    break;
                case DrawInstruction.POP:
                    paths.Add(currentPath);
                    currentPath = new List<Vector3>();
                    (pos, dir) = stack.Pop();
                    currentPath.Add(pos);
                    break;
                default:
                    break;
            }

            // Keep each path to a maximum of 1000 points
            if (currentPath.Count >= 1000)
            {
                paths.Add(currentPath);
                currentPath = new List<Vector3>();
                currentPath.Add(pos);
            }
        }

        if (currentPath.Count >= 2)
            paths.Add(currentPath);

        calculatedPaths[(iteration, angle)] = paths;
    }

    private List<char> GetInstructionChars(int desiredIteration)
    {
        int iteration = Mathf.Clamp(desiredIteration, 0, definition.maxIteration);

        if (calculatedChars.Count <= iteration)
        {
            CalculateInstructionChars(iteration);
        }

        StringBuilder outbuilder = new StringBuilder();
        foreach (char c in calculatedChars[iteration]) outbuilder.Append(c);
        string outString = outbuilder.ToString();
        return calculatedChars[iteration];
    }

    private void CalculateInstructionChars(int desiredIteration)
    {
        int iteration = Mathf.Clamp(desiredIteration, 0, definition.maxIteration);
        List<char> previousIteration = GetInstructionChars(iteration - 1);
        List<char> currentIteration = new List<char>();

        foreach (char c in previousIteration)
        {
            currentIteration.AddRange(ruleDictionary[c].value);
        }

        calculatedChars.Add(currentIteration);
    }

    private static Vector3 Rotate(Vector3 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
        return new Vector3((cos * v.x) - (sin * v.y), (sin * v.x) + (cos * v.y));
    }

    ////////////////////////////////////////////////////////////////////////
    //********************************************************************//
    //********************************************************************//
    //**********************  Asynchronous Request  **********************//
    //********************************************************************//
    //********************************************************************//
    ////////////////////////////////////////////////////////////////////////

    public bool TrySetBackgroundIteration(int newIteration, float newAngle = -1)
    {
        if (controlLock.WaitOne(1))
        {
            desiredAngle = (newAngle == 0 ? definition.turnAngle : newAngle) % 360;
            desiredIteration = newIteration;
            controlLock.Release();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CalculateBackground()
    {
        controlLock.WaitOne();
        _ = GetPaths(desiredIteration, desiredAngle);
        controlLock.Release();
    }

    public bool IsCalculating()
    {
        if (controlLock.WaitOne(1))
        {
            controlLock.Release();
            return true;
        }

        return false;
    }

    ////////////////////////////////////////////////////////////////////////
    //********************************************************************//
    //********************************************************************//
    //*********************  SERIALIZABLE COMPONENT  *********************//
    //********************************************************************//
    //********************************************************************//
    ////////////////////////////////////////////////////////////////////////

    public enum DrawInstruction
    {
        NONE = 0,
        MOVE,
        DRAW,
        LEFT,
        RIGHT,
        PUSH,
        POP
    }

    [Serializable]
    private class DefinitionList
    {
        public List<Definition> lsystems;

        public static DefinitionList CreateFromJSON(string jsonString)
        {
            DefinitionList newDefinitions = JsonUtility.FromJson<DefinitionList>(jsonString);
            newDefinitions.Clean();
            return newDefinitions;
        }

        public string SaveToString()
        {
            return JsonUtility.ToJson(this,true);
        }

        public void Clean()
        {
            foreach (Definition definition in lsystems)
            {
                definition.Clean();
            }
        }
    }

    [Serializable]
    public class Definition
    {
        public string name;
        public string description;
        public string axiom;
        public List<Rule> rules;
        public float turnAngle;
        public int maxIteration;
        public float startAngle;
        public Vector2 renderCenter;
        public float renderRadius;

        public static Definition CreateFromJSON(string jsonString)
        {
            Definition newDefinition = JsonUtility.FromJson<Definition>(jsonString);
            newDefinition.Clean();
            return newDefinition;
        }

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }

        public void Clean()
        {
            foreach (Rule rule in rules)
            {
                rule.Clean();
            }
        }
    }

    [Serializable]
    public class Rule
    {
        [NonSerialized]
        public char keyChar;
        public string key;
        public string value;

        [NonSerialized]
        public DrawInstruction instructionEnum;
        public string instruction;

        public Rule(char key, string value, DrawInstruction instruction)
        {
            this.keyChar = key;
            this.key = key.ToString();
            this.value = value;

            this.instructionEnum = instruction;
            this.instruction = Enum.GetName(typeof(DrawInstruction), instruction);
        }

        public static Rule CreateFromJSON(string jsonString)
        {
            Rule newEntry = JsonUtility.FromJson<Rule>(jsonString);
            newEntry.Clean();
            return newEntry;
        }

        public string SaveToString()
        {
            this.key = this.keyChar.ToString();
            this.instruction = Enum.GetName(typeof(DrawInstruction), this.instructionEnum);
            return JsonUtility.ToJson(this);
        }

        public void Clean()
        {
            keyChar = key[0];
            instructionEnum = (DrawInstruction)Enum.Parse(typeof(DrawInstruction), instruction);
        }
    }
}
