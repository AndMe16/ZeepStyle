﻿using System.Collections.Generic;
using UnityEngine;
using ZeepStyle;

public class Style_TrickPointsManager : MonoBehaviour
{
    // Assign base points for each trick name
    private Dictionary<string, int> basePointsByTrick = new Dictionary<string, int>()
    {
        { "Spin", 100 },
        { "Frontflip", 300 },
        { "Backflip", 400 },
        { "Roll", 200 }
    };

    // Method to calculate points for each trick
    public int CalculatePoints(Trick trick)
    {
        int basePoints = 0;
        if (basePointsByTrick.TryGetValue(trick.trickName, out basePoints))
        {
            // Base points based on trick name
            int points = basePoints;
            float rotationMulti;

            if ((trick.trickName == "Frontflip") || (trick.trickName == "Backflip"))
            {
                rotationMulti = float.Parse(trick.rotation);
            }
            else
            {
                rotationMulti = float.Parse(trick.rotation)/360;
            }

            points = (int)(points*rotationMulti); // Add rotation value as points
            

            // Apply multipliers or penalties for inverse tricks
            if (trick.isInverse)
            {
                points = (int)(points * 1.5); // 50% bonus for inverse tricks
            }

            return points;
        }

        // Default points if trick name is not found
        return 0;
    }

}

