﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FixedLevel")]
public class FixedLevels : ScriptableObject
{
   // [TextArea]
    public string fixedLevel;
    [Header("Light factor")]
    public int ligtFactor = 0; 


}
