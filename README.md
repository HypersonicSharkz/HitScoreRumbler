# Hit Score Rumbler
A Beat Saber Mod that changes the cut haptics based on the cut distance to the note center

Made this mod originally for making poodles easier to acc.  
The idea is that rumble will let you know in real time if you are close to the optimal swing path of the poodle, and can help guide you back to the center.

## Settings

![image](https://github.com/user-attachments/assets/33553cc9-4875-4173-9c4e-3e1afbbb8c2b)

| | |
| -- | -- |
| Strength | Base strenght of the cut haptics, will be multiplied by the curve |
| Duration | Duration of the cut haptics |

## Curve
Haptics are based on the actual cut distance and not the acc points themself

- **X-Axis**: Cut Accuracy
- **Y-Axis**: Strength Multiplier

![image](https://github.com/user-attachments/assets/dad97bbd-00e8-4a99-87c0-069c98f99ca0)

Each column corresponds to the range of an acc value.  
This means that the second column would be 0 rumble for 15, and 0.1 rumble 13.  
Hitting a 14 would therefore lineary interpret between 0.1 and 0 based on the actual cut distance
