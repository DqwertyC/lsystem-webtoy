# L-System Webtoy
Simple [Lindenmayer System](https://en.wikipedia.org/wiki/L-system) exploration webtoy built in Unity. You can try it out online [here](https://dqwertyc.github.io/lsystem-webtoy/)!

## Navigation
Use the scroll wheel to zoom in and out. Center click the scroll wheel to pan the image.

Arrows on the top, left, and bottom of the screen reveal controls.
The top selector controls the current iteration being rendered. If any controls start lagging too much, try decreasing this!
The left slider controls the current turn angle used for drawing. Each system has a default turn angle, but modifying this can lead to some interesting results. Press the button at the bottom of the slider to reset the angle to the system's default.
The bottom slider controls current draw percentage. Using the media controls, you can also animate the system being drawn over time.

The eye button in the upper right toggles UI elements.
The gear icon in the upper right opens the L-System rules panel.

## Changing L-System Rules
In the system chooser, you can either load a preset system from the dropdown menu, or a system stored as a JSON string. Click the appropriate "Load" button to populate fields below.

Once a system is loaded, you can review the system rules and settings in the bottom half of the panel:
* Name: The name of this system.
* Axiom: The instructions for the starting iteration of this system.
* Start Angle: The direction the system starts drawing in before any turns.
* Turn Angle: The default turn angle used in drawing the system.
* Center X,Y: The center of the draw area for the starting iteration.
* Radius: The radius of the draw area for the starting iteration.
* Limiter: The max iteration allowed for this system. Increase at risk of crashing!
* Rules: A list of rules for the system. Each rule has the following elements:
  * Rule Key: The character that this rule replaces each iteration.
  * Rule Instruction: The draw instruction that this character represents.
  * Rule Value: What the key is replaced with each iteration.
* Notes: Human readable notes on this system.

The "+" button will add a new, blank rule, and the "-" button will remove the bottom rule from the list.  
The Copy button will copy the current system as JSON to the users clipboard (Unimplemented in WebGL player).


Once you're satisfied with the system rules, click "Apply" to update the drawn curve and "Close" to exit the menu.

## JSON Format for LSystems
Here's a sample of how to format LSystem into JSON to be readable by the application:
```json
{
  "name": "Dragon Curve",
  "description": "Classic example of replacement systems.",
  "axiom": "F",
  "rules": [
    {
      "key": "F",
      "value": "F+G",
      "instruction": "DRAW"
    },
    {
      "key": "G",
      "value": "F-G",
      "instruction": "DRAW"
    },
    {
      "key": "+",
      "value": "+",
      "instruction": "LEFT"
    },
    {
      "key": "-",
      "value": "-",
      "instruction": "RIGHT"
    }
  ],
  "turnAngle": 90.0,
  "maxIteration": 16,
  "startAngle": 0.0,
  "renderCenter": {
    "x": 0.0,
    "y": 0.0
  },
  "renderRadius": 0.0
}
```
