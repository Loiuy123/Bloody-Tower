﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Should be used for the targeting system in order to allow it to revert to older targeting
/// </summary>
public interface IRestrictTargeting
{
    /// <summary>
    /// If the current Target is valid
    /// </summary>
    /// <returns></returns>
    bool IsValidTarget();
    /// <summary>
    /// if targeting system should revert the last move
    /// </summary>
    /// <returns></returns>
    bool AllowTargetingMove();
}
