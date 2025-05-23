﻿using System.Collections.Generic;
using UnityEngine;
using ZeepkistClient;
using ZeepSDK.Storage;
using ZeepStyle.src.Patches;
using ZeepStyle.src.PointsUIManager;
using ZeepStyle.src.TrickManager;

namespace ZeepStyle.src.PointsManager
{
    public class Style_TrickPointsManager : MonoBehaviour
    {
        // Assign base points for each trick name
        private readonly Dictionary<string, int> basePointsByTrick = new()
    {
        { "Spin", 100 },
        { "Frontflip", 300 },
        { "Backflip", 400 },
        { "Roll", 200 },
        { "Sideflip", 300}
    };

        public int totalRunPoints = 0;
        public int bestPbAllTime = 0;
        public int bestPbCurrentSession = 0;

        public string currentHash = string.Empty;

        Style_PointsUIManager pointsUIManager;

        IModStorage pointsPBsStorage;

        void Start()
        {
            pointsUIManager = FindObjectOfType<Style_PointsUIManager>();
            pointsPBsStorage = StorageApi.CreateModStorage(Plugin.Instance);
        }

        // Method to calculate points for each trick
        public int CalculatePoints(Trick trick)
        {
            if (basePointsByTrick.TryGetValue(trick.trickName, out int basePoints))
            {
                // Base points based on trick name
                int points = basePoints;
                float rotationMulti;

                if (trick.trickName == "Frontflip" || trick.trickName == "Backflip" || trick.trickName == "Sideflip")
                {
                    rotationMulti = float.Parse(trick.rotation);
                }
                else
                {
                    rotationMulti = float.Parse(trick.rotation) / 360;
                }

                points = (int)(points * rotationMulti); // Add rotation value as points


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

        public int CalculateTotalJumpPoints(List<Trick> tricksList)
        {
            int totalPoints = 0;
            foreach (Trick trick in tricksList)
            {
                totalPoints += CalculatePoints(trick);
            }
            return totalPoints;
        }

        public int AddToTotalRunPoints(int extraPoints)
        {
            totalRunPoints += extraPoints;
            Plugin.Logger.LogInfo($"Adding total run points: {totalRunPoints}  (+{extraPoints}) ");
            return totalRunPoints;
        }

        public void ResetTotalRunPoints()
        {
            totalRunPoints = 0;
        }

        public void ResetCurrentSessionPoints()
        {
            bestPbCurrentSession = 0;
        }

        // Call this method to update the current run points
        public void UpdateCurrentRunPoints(int points)
        {
            totalRunPoints = points;
            if (pointsUIManager.pointsInfoText != null)
            {
                pointsUIManager.UpdatePointsInfoText();
            }
        }


        public void SaveLevelPB(string levelHash)
        {
            if (levelHash == null)
            {
                Plugin.Logger.LogError("SaveLevelPB: Current level hash is null");
                return;
            }

            if (!ZeepkistNetwork.IsConnected && PatchLoadOfflineLevel.isTestLevel)
            {
                Plugin.Logger.LogInfo("SaveLevelPB: Current level is a test level, not saving PB");
                return;
            }

            pointsPBsStorage.SaveToJson($"{levelHash}_PB", bestPbAllTime);
        }

        public void LoadLevelPB(string levelHash)
        {
            if (levelHash == null)
            {
                Plugin.Logger.LogError("LoadLevelPB: Current level hash is null");
                return;
            }
            if (pointsPBsStorage.JsonFileExists($"{levelHash}_PB"))
            {
                Plugin.Logger.LogInfo($"Loading PB points from {levelHash}_PB");
                bestPbAllTime = pointsPBsStorage.LoadFromJson<int>($"{levelHash}_PB");
            }
            else
            {
                Plugin.Logger.LogInfo($"{levelHash}_PB was not found, unable to load PB points");
                bestPbAllTime = 0;
            }
        }

    }
}


