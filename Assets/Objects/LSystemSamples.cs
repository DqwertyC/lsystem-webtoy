using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LSystemSamples
{
    public static LSystem.Definition KochSnowflake = new LSystem.Definition()
    {
        name = "Koch Snowflake",
        description = "Simple self-similar fractal.",
        axiom = "F--F--F",
        rules = new List<LSystem.Rule>()
        {
            new LSystem.Rule('F', "F+F--F+F", LSystem.DrawInstruction.DRAW),
            new LSystem.Rule('+', "+", LSystem.DrawInstruction.LEFT),
            new LSystem.Rule('-', "-", LSystem.DrawInstruction.RIGHT)
        },
        turnAngle = 60.0f,
        maxIteration = 6,
        startAngle = 0,
        renderCenter = new Vector2(0.5f,-0.2887f),
        renderRadius = 0.577f
    };

    public static LSystem.Definition DragonCurve = new LSystem.Definition()
    {
        name = "Dragon Curve",
        description = "Classic example of replacement systems.",
        axiom = "F",
        rules = new List<LSystem.Rule>()
        {
            new LSystem.Rule('F', "F+G", LSystem.DrawInstruction.DRAW),
            new LSystem.Rule('G', "F-G", LSystem.DrawInstruction.DRAW),
            new LSystem.Rule('+', "+", LSystem.DrawInstruction.LEFT),
            new LSystem.Rule('-', "-", LSystem.DrawInstruction.RIGHT)
        },
        turnAngle = 90.0f,
        maxIteration = 16,
        startAngle = 0,
        renderCenter = Vector2.zero,
        renderRadius = 0.0f
    };

    public static LSystem.Definition HilbertCurve = new LSystem.Definition()
    {
        name = "Hilbert Curve",
        description = "Classic space-filling curve.",
        axiom = "AG",
        rules = new List<LSystem.Rule>()
        {
            new LSystem.Rule('A', "+BF-AFA-FB+", LSystem.DrawInstruction.NONE),
            new LSystem.Rule('B', "-AF+BFB+FA-", LSystem.DrawInstruction.NONE),
            new LSystem.Rule('F', "F", LSystem.DrawInstruction.DRAW),
            new LSystem.Rule('G', "", LSystem.DrawInstruction.DRAW),
            new LSystem.Rule('+', "+", LSystem.DrawInstruction.LEFT),
            new LSystem.Rule('-', "-", LSystem.DrawInstruction.RIGHT)
        },
        turnAngle = 90.0f,
        maxIteration = 8,
        startAngle = 0,
        renderCenter = new Vector2(0.5f, 0.5f),
        renderRadius = 0.5f
    };

    public static LSystem.Definition BinaryTree = new LSystem.Definition()
    {
        name = "Binary Tree",
        description = "Simple example demonstrating push/pop.",
        axiom = "A",
        rules = new List<LSystem.Rule>()
        {
            new LSystem.Rule('A', "B[+A]-A", LSystem.DrawInstruction.DRAW),
            new LSystem.Rule('B', "BB", LSystem.DrawInstruction.DRAW),
            new LSystem.Rule('+', "+", LSystem.DrawInstruction.LEFT),
            new LSystem.Rule('-', "-", LSystem.DrawInstruction.RIGHT),
            new LSystem.Rule('[',"[",LSystem.DrawInstruction.PUSH),
            new LSystem.Rule(']',"]",LSystem.DrawInstruction.POP)
        },
        turnAngle = 45.0f,
        maxIteration = 8,
        startAngle = 90,
        renderCenter = new Vector2(0,0.5f),
        renderRadius = 0.5f
    };

    public static LSystem.Definition SampleFern = new LSystem.Definition()
    {
        name = "Fractal Fern",
        description = "Simple example showing emergence of organic structures.",
        axiom = "X",
        rules = new List<LSystem.Rule>()
        {
            new LSystem.Rule('X', "F+[[X]-X]-F[-FX]+X", LSystem.DrawInstruction.NONE),
            new LSystem.Rule('F', "FF", LSystem.DrawInstruction.DRAW),
            new LSystem.Rule('+', "+", LSystem.DrawInstruction.LEFT),
            new LSystem.Rule('-', "-", LSystem.DrawInstruction.RIGHT),
            new LSystem.Rule('[',"[",LSystem.DrawInstruction.PUSH),
            new LSystem.Rule(']',"]",LSystem.DrawInstruction.POP)
        },
        turnAngle = 25.0f,
        maxIteration = 8,
        startAngle = 60,
        renderCenter = Vector2.zero,
        renderRadius = 0.0f
    };

    public static LSystem.Definition Arrowhead = new LSystem.Definition()
    {
        name = "Sierpinski Arrowhead",
        description = "Curve that traces the Sierpinski Triangle.",
        axiom = "AF",
        rules = new List<LSystem.Rule>()
        {
            new LSystem.Rule('A', "B", LSystem.DrawInstruction.NONE),
            new LSystem.Rule('B', "A", LSystem.DrawInstruction.LEFT),
            new LSystem.Rule('F', "G-F-G", LSystem.DrawInstruction.DRAW),
            new LSystem.Rule('G', "F+G+F", LSystem.DrawInstruction.DRAW),
            new LSystem.Rule('+', "+", LSystem.DrawInstruction.LEFT),
            new LSystem.Rule('-', "-", LSystem.DrawInstruction.RIGHT),
            new LSystem.Rule('[',"[",LSystem.DrawInstruction.PUSH),
            new LSystem.Rule(']',"]",LSystem.DrawInstruction.POP)
        },
        turnAngle = 60.0f,
        maxIteration = 8,
        startAngle = 0,
        renderCenter = new Vector2(0.5f, 0.2887f),
        renderRadius = 0.2887f
    };
}
