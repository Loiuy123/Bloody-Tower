﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RNG
{
    public static int Seed; //no value to make it random

    static System.Random random;
    static int instanceRandom;

    public static void Reset(int Seed = -1)
    {
        if (Seed == -1)
        {
            random = new System.Random();            
        }
        else
        {
            random = new System.Random(Seed);
        }
        instanceRandom = random.Next();
    }
    // [min,max[
    public static int Range(int min, int max)
    {
        return random.Next(min, max);
    }
    public static float Range(float min, float max)
    {
        return min + (Next() * (max - min));
    }
    public static float Next()
    {
        return (float)random.NextDouble();
    }
    public static int InstanceRandom =>instanceRandom;
    static RNG()
    {
        random = new System.Random(Seed);
    }
}
