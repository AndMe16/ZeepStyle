using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZeepkistClient;
using ZeepSDK.Storage;
using ZeepStyle.Patches;
using ZeepStyle.PointsUIManager;
using ZeepStyle.TrickManager;

namespace ZeepStyle.PointsManager;

public class StyleTrickPointsManager : MonoBehaviour
{
    public int totalRunPoints;
    public int bestPbAllTime;
    public int bestPbCurrentSession;

    public string currentHash = string.Empty;

    // Assign base points for each trick name
    private readonly Dictionary<string, int> basePointsByTrick = new()
    {
        { "Spin", 100 },
        { "Frontflip", 300 },
        { "Backflip", 400 },
        { "Roll", 200 },
        { "Sideflip", 300 }
    };

    private IModStorage pointsPBsStorage;

    private StylePointsUIManager pointsUIManager;

    private void Start()
    {
        pointsUIManager = FindObjectOfType<StylePointsUIManager>();
        pointsPBsStorage = StorageApi.CreateModStorage(Plugin.Instance);
    }

    // Method to calculate points for each trick
    public int CalculatePoints(Trick trick)
    {
        if (!basePointsByTrick.TryGetValue(trick.TrickName, out var basePoints)) return 0;
        // Base points based on trick name
        var points = basePoints;
        float rotationMulti;

        if (trick.TrickName is "Frontflip" or "Backflip" or "Sideflip")
            rotationMulti = float.Parse(trick.Rotation);
        else
            rotationMulti = float.Parse(trick.Rotation) / 360;

        points = (int)(points * rotationMulti); // Add rotation value as points


        // Apply multipliers or penalties for inverse tricks
        if (trick.IsInverse) points = (int)(points * 1.5); // 50% bonus for inverse tricks

        return points;

        // Default points if trick name is not found
    }

    public int CalculateTotalJumpPoints(List<Trick> tricksList)
    {
        return tricksList.Sum(CalculatePoints);
    }

    public int AddToTotalRunPoints(int extraPoints)
    {
        totalRunPoints += extraPoints;
        Plugin.logger.LogInfo($"Adding total run points: {totalRunPoints}  (+{extraPoints}) ");
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
        if (pointsUIManager.pointsInfoText) pointsUIManager.UpdatePointsInfoText();
    }


    public void SaveLevelPb(string levelHash)
    {
        if (levelHash == null)
        {
            Plugin.logger.LogError("SaveLevelPB: Current level hash is null");
            return;
        }

        if (!ZeepkistNetwork.IsConnected && PatchLoadOfflineLevel.isTestLevel)
        {
            Plugin.logger.LogInfo("SaveLevelPB: Current level is a test level, not saving PB");
            return;
        }

        pointsPBsStorage.SaveToJson($"{levelHash}_PB", bestPbAllTime);
    }

    public void LoadLevelPb(string levelHash)
    {
        if (levelHash == null)
        {
            Plugin.logger.LogError("LoadLevelPB: Current level hash is null");
            return;
        }

        if (pointsPBsStorage.JsonFileExists($"{levelHash}_PB"))
        {
            Plugin.logger.LogInfo($"Loading PB points from {levelHash}_PB");
            bestPbAllTime = pointsPBsStorage.LoadFromJson<int>($"{levelHash}_PB");
        }
        else
        {
            Plugin.logger.LogInfo($"{levelHash}_PB was not found, unable to load PB points");
            bestPbAllTime = 0;
        }
    }
}