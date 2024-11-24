# Hit Score Rumbler
A Beat Saber Mod that changes the cut haptics based on the cut distance to the note center

This mod was originally created for playing poodles.  
The isea is that the rumble feedback will let you know how close your cut was to the note's center, and can help guide you back to the center of the notes.  

It can also simply be used as a way to indicate how precise your swings are on top of mods like HitScoreVisualizer.

> [!WARNING]
> This mod will not work with any other rumble override mods enabled! If you are using Tweaks55 you can keep the arc and chain rumble settings, however you must set the "Normal Cut Rumble" to 0.  
> Hit Score Rumbler will not change haptics for arcs or chains!

## Settings

![image](https://github.com/user-attachments/assets/33553cc9-4875-4173-9c4e-3e1afbbb8c2b)

| | |
| -- | -- |
| Strength | Base strength of the cut haptics, will be multiplied by the curve |
| Duration | Duration of the cut haptics |

## Curve
Haptics are based on the actual cut distance and not the acc points themself

- **X-Axis**: Cut Accuracy
- **Y-Axis**: Strength Multiplier

![image](https://github.com/user-attachments/assets/00091ab1-5b9d-4b31-b928-e76dc4d75b6f)


Each column corresponds to the range of an acc value.  
Column 1 corresponds to a multiplier of 0 for every cut of 15

Column 2 corresponds to a multiplier between 0 and 0.1, the closer to a 13 the higher the rumble
