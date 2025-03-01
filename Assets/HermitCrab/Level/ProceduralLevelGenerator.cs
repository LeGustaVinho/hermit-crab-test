using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HermitCrab.Level
{
    public class ProceduralLevelGeneratorRuntime : MonoBehaviour
    {
        // Prefab references
        public GameObject floorPrefab;
        public GameObject floatingPlatformPrefab;
        public GameObject spikePrefab;
        public GameObject explosiveBarrelPrefab;
        public GameObject poisonBarrelPrefab;
        public GameObject acidPrefab;
        public GameObject acidFullPrefab; // Full acid column prefab
        public GameObject sawHorizontalPrefab;
        public GameObject sawVerticalPrefab;
        public GameObject zombie1Prefab;
        public GameObject zombie2Prefab;
        public GameObject robotPrefab;

        // Player-related elements
        public GameObject playerSpawnPrefab;    // Spawn point
        public GameObject playerVictoryPrefab;  // Victory point

        // Background element (instantiated along the level)
        public GameObject levelBackgroundPrefab;
        // Y position at which to place the background elements
        public float levelBackgroundY = 5f;

        // Collectible Items
        [Header("Collectible Items")]
        public GameObject batteryPrefab;
        public GameObject repairKitPrefab;
        [Range(0f, 1f)]
        public float collectibleChance = 0.1f;

        // Grid and level parameters
        public float gridSize = 2.5f;
        public Vector2 startPosition = new Vector2(-2.5f, -7.5f); // Exposed start position
        public float minY = -10f;
        public float maxY = 20f;
        public float maxJumpX = 7.5f;
        public float maxJumpY = 5f;

        // Level width (in world units)
        public float levelWidthMin = 500f;

        // Gap settings
        public int maxGapTiles = 2;

        // Seed settings
        public int seed = 0;
        public bool useRandomSeed = true;

        // Base probabilities (scale with progress)
        [Range(0f, 1f)]
        public float gapInsertionChance = 0.15f;
        [Range(0f, 1f)]
        public float baseObstacleChance = 0.2f;
        [Range(0f, 1f)]
        public float baseEnemyChance = 0.1f;
        [Range(0f, 1f)]
        public float floatingPlatformChance = 0.15f;

        // Obstacle type distribution (cumulative)
        [Range(0f, 1f)]
        public float spikeChance = 0.4f;
        [Range(0f, 1f)]
        public float explosiveBarrelChance = 0.6f;
        [Range(0f, 1f)]
        public float poisonBarrelChance = 0.8f;
        [Range(0f, 1f)]
        public float movingSawChance = 1.0f;

        // Minimum contiguous floor length (in world units) for enemy placement
        public float minEnemyFloorLength = 8f;

        // Safe zone: first safeZoneBlocks have no enemies/obstacles/gaps/acid/floating platforms
        public int safeZoneBlocks = 12;

        // End clear zone: last endClearZoneBlocks will be forced continuous (clear of obstacles, gaps, acid, or platforms)
        public int endClearZoneBlocks = 5;

        // Consecutive floor parameters (force floors at the same Y)
        [Range(0f, 1f)]
        public float consecutiveFloorProbability = 0.3f;
        public int consecutiveFloorCount = 3;

        // Yield threshold: number of instantiations before yielding to next frame
        public int yieldThreshold = 10;
        private int instantiationCount = 0;

        // Parent container for level elements
        private GameObject levelParent;

        // Public properties to expose generated objects
        public GameObject ProceduralLevelInstance { get { return levelParent; } }
        public GameObject DoorLockedInstance { get; private set; }
        public GameObject DoorOpenInstance { get; private set; }

        // Call this method (e.g., from Start or via another script) to generate the level at runtime.
        [Button]
        public void GenerateLevel()
        {
            StartCoroutine(GenerateLevelCoroutine());
        }

        IEnumerator GenerateLevelCoroutine()
        {
            // Initialize random seed
            if (useRandomSeed)
                seed = System.Environment.TickCount;
            Random.InitState(seed);

            // Create parent GameObject for the level
            levelParent = new GameObject("ProceduralLevel");

            // Generate main path (list of floor positions)
            List<Vector3> mainPath = GenerateMainPath();

            // Loop through each floor position in the main path
            for (int i = 0; i < mainPath.Count; i++)
            {
                Vector3 floorPos = SnapToGrid(mainPath[i]);
                // Instantiate floor block and fill vertically below it to Y = -15
                InstantiateAndYield(floorPrefab, floorPos);
                FillBelow(floorPos, floorPrefab, -15f);

                // Process gaps, obstacles, and enemies only outside the safe zone and end clear zone
                if (i >= safeZoneBlocks && i < mainPath.Count - endClearZoneBlocks)
                {
                    // Check for gap between previous and current floor blocks
                    float gapDistance = mainPath[i].x - mainPath[i - 1].x;
                    int gapCount = Mathf.RoundToInt(gapDistance / gridSize) - 1;
                    if (gapCount > 0)
                    {
                        float acidY = SnapToGrid(mainPath[i - 1]).y;
                        for (int j = 1; j <= gapCount; j++)
                        {
                            float acidX = mainPath[i - 1].x + j * gridSize;
                            Vector3 acidPos = SnapToGrid(new Vector3(acidX, acidY, 0));
                            InstantiateAndYield(acidPrefab, acidPos);
                            FillBelow(acidPos, acidFullPrefab, -15f);
                        }
                    }

                    // Calculate progress factor and scale obstacle/enemy probabilities
                    float progress = Mathf.Clamp01(floorPos.x / levelWidthMin);
                    float effectiveObstacleChance = baseObstacleChance * (1f + progress);
                    float effectiveEnemyChance = baseEnemyChance * (1f + progress);

                    // Place obstacles on current floor block
                    if (Random.value < effectiveObstacleChance)
                    {
                        float rnd = Random.value;
                        Vector3 obstaclePos = SnapToGrid(floorPos + new Vector3(0, gridSize, 0));
                        if (rnd < spikeChance)
                            InstantiateAndYield(spikePrefab, obstaclePos);
                        else if (rnd < explosiveBarrelChance)
                            InstantiateAndYield(explosiveBarrelPrefab, obstaclePos);
                        else if (rnd < poisonBarrelChance)
                            InstantiateAndYield(poisonBarrelPrefab, obstaclePos);
                        else if (rnd < movingSawChance)
                        {
                            // Floating saw placement (independent of floor)
                            float sawOffsetY = Random.Range(gridSize, 4 * gridSize);
                            Vector3 sawPos = SnapToGrid(new Vector3(floorPos.x, floorPos.y + sawOffsetY, 0));
                            if (Random.value < 0.5f)
                                InstantiateAndYield(sawHorizontalPrefab, sawPos);
                            else
                                InstantiateAndYield(sawVerticalPrefab, sawPos);
                        }
                    }

                    // Procedural generation of collectible items (RepairKit and Battery)
                    if (Random.value < collectibleChance)
                    {
                        // Choose randomly between Battery and RepairKit (50% chance each)
                        GameObject collectiblePrefab = (Random.value < 0.5f) ? batteryPrefab : repairKitPrefab;
                        // Position the collectible slightly above the floor block
                        Vector3 collectiblePos = SnapToGrid(floorPos + new Vector3(0, gridSize, 0));
                        InstantiateAndYield(collectiblePrefab, collectiblePos);
                    }

                    // Check for contiguous floor segment (same Y) to place enemies
                    int contiguousCount = 1;
                    int jIndex = i;
                    Vector3 currentSnapped = SnapToGrid(mainPath[i]);
                    while (jIndex < mainPath.Count - 1)
                    {
                        Vector3 nextSnapped = SnapToGrid(mainPath[jIndex + 1]);
                        if (Mathf.Approximately(currentSnapped.y, nextSnapped.y) &&
                            Mathf.Approximately(nextSnapped.x - currentSnapped.x, gridSize))
                        {
                            contiguousCount++;
                            currentSnapped = nextSnapped;
                            jIndex++;
                        }
                        else break;
                    }
                    if (contiguousCount * gridSize >= minEnemyFloorLength && Random.value < effectiveEnemyChance)
                    {
                        Vector3 enemyPos = SnapToGrid(floorPos + new Vector3(0, 5f, 0));
                        float enemyRand = Random.value;
                        if (enemyRand < 0.33f)
                            InstantiateAndYield(zombie1Prefab, enemyPos);
                        else if (enemyRand < 0.66f)
                            InstantiateAndYield(zombie2Prefab, enemyPos);
                        else
                            InstantiateAndYield(robotPrefab, enemyPos);
                    }

                    // Optionally, add floating platforms above the floor block
                    if (Random.value < floatingPlatformChance)
                    {
                        float platformY = floorPos.y + Random.Range(2 * gridSize, 4 * gridSize);
                        Vector3 platformPos = SnapToGrid(new Vector3(floorPos.x, platformY, 0));
                        InstantiateAndYield(floatingPlatformPrefab, platformPos);
                    }
                }
            }

            // Place Player Spawn 5 blocks after the start (index 5)
            if (mainPath.Count > 5 && playerSpawnPrefab != null)
            {
                Vector3 spawnBlock = SnapToGrid(mainPath[5]);
                Vector3 spawnPos = spawnBlock + new Vector3(0, gridSize + 1.15f, 0);
                // Instantiate DoorLocked object (used as spawn point)
                DoorLockedInstance = Instantiate(playerSpawnPrefab, spawnPos, Quaternion.identity, levelParent.transform);
                DoorLockedInstance.name = "DoorLocked";
            }

            // Place Player Victory 5 blocks before the end (index = count - 6)
            if (mainPath.Count > 5 && playerVictoryPrefab != null)
            {
                int victoryIndex = mainPath.Count - 6;
                if (victoryIndex < 0) victoryIndex = 0;
                Vector3 victoryBlock = SnapToGrid(mainPath[victoryIndex]);
                Vector3 victoryPos = victoryBlock + new Vector3(0, gridSize + 1.15f, 0);
                // Instantiate DoorOpen object (to be used as door trigger)
                DoorOpenInstance = Instantiate(playerVictoryPrefab, victoryPos, Quaternion.identity, levelParent.transform);
                DoorOpenInstance.name = "DoorOpen";
            }

            // Create vertical walls at the first and last blocks from Y=20 down to Y=-15
            if (mainPath.Count > 0)
            {
                Vector3 firstBlock = SnapToGrid(mainPath[0]);
                FillVertical(firstBlock, floorPrefab, 20f, -15f);
                Vector3 lastBlock = SnapToGrid(mainPath[mainPath.Count - 1]);
                FillVertical(lastBlock, floorPrefab, 20f, -15f);
            }

            // Instantiate level background elements along the X axis
            for (float x = startPosition.x; x < levelWidthMin; x += 5f)
            {
                Vector3 bgPos = new Vector3(x, levelBackgroundY, 0);
                InstantiateAndYield(levelBackgroundPrefab, bgPos);
            }

            yield break;
        }

        // Helper method to instantiate a prefab and yield control after a threshold
        void InstantiateAndYield(GameObject prefab, Vector3 position)
        {
            if (prefab != null)
            {
                Instantiate(prefab, position, Quaternion.identity, levelParent.transform);
                instantiationCount++;
                if (instantiationCount >= yieldThreshold)
                {
                    instantiationCount = 0;
                    StartCoroutine(YieldFrame());
                }
            }
        }

        IEnumerator YieldFrame()
        {
            yield return null;
        }

        // Fills below the given top position with the prefab down to targetY
        void FillBelow(Vector3 topPos, GameObject prefab, float targetY)
        {
            for (float y = topPos.y - gridSize; y >= targetY; y -= gridSize)
            {
                Vector3 fillPos = new Vector3(topPos.x, y, 0);
                InstantiateAndYield(prefab, fillPos);
            }
        }

        // Fills a vertical column at pos with the prefab from topY down to bottomY
        void FillVertical(Vector3 pos, GameObject prefab, float topY, float bottomY)
        {
            for (float y = topY; y >= bottomY; y -= gridSize)
            {
                Vector3 wallPos = new Vector3(pos.x, y, 0);
                InstantiateAndYield(prefab, wallPos);
            }
        }

        // Generates the main path based on current parameters
        List<Vector3> GenerateMainPath()
        {
            List<Vector3> path = new List<Vector3>();
            float currentX = startPosition.x;
            float currentY = startPosition.y;
        
            // Generate safe zone: first safeZoneBlocks with no gaps or vertical changes
            for (int i = 0; i < safeZoneBlocks; i++)
            {
                path.Add(new Vector3(currentX, currentY, 0));
                currentX += gridSize;
            }
        
            // Generate path until nearing end clear zone
            while (currentX < levelWidthMin - (endClearZoneBlocks * gridSize))
            {
                // With a chance, force consecutive floors at the same Y
                if (Random.value < consecutiveFloorProbability)
                {
                    int count = consecutiveFloorCount;
                    for (int k = 0; k < count; k++)
                    {
                        path.Add(new Vector3(currentX, currentY, 0));
                        currentX += gridSize;
                        if (currentX >= levelWidthMin - (endClearZoneBlocks * gridSize))
                            break;
                    }
                    continue;
                }
            
                bool createGap = (Random.value < gapInsertionChance);
                int gapTiles = createGap ? Random.Range(1, maxGapTiles + 1) : 0;
                int stepTiles = (gapTiles > 0) ? (gapTiles + 1) : 1;
                currentX += stepTiles * gridSize;
            
                // If not a gap, apply vertical variation; if gap, maintain currentY
                if (!createGap)
                {
                    float verticalChange = Random.Range(-gridSize, gridSize);
                    if (Random.value < 0.2f)
                        verticalChange *= 2;
                    float newY = Mathf.Clamp(currentY + verticalChange, minY, maxY);
                    if (Mathf.Abs(newY - currentY) > maxJumpY)
                        newY = currentY + Mathf.Sign(newY - currentY) * maxJumpY;
                    currentY = newY;
                }
                path.Add(new Vector3(currentX, currentY, 0));
            }
        
            // Final region: force continuous floor for end clear zone
            while (currentX < levelWidthMin)
            {
                path.Add(new Vector3(currentX, currentY, 0));
                currentX += gridSize;
            }
            return path;
        }

        // Snaps a position to the defined grid
        Vector3 SnapToGrid(Vector3 position)
        {
            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.y = Mathf.Round(position.y / gridSize) * gridSize;
            return position;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
