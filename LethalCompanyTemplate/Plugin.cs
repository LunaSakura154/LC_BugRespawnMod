using BepInEx;
using System.Collections.Generic;
using LC_API.GameInterfaceAPI;
using UnityEngine;
using GameNetcodeStuff;
using System.Threading.Tasks;
using System;

namespace HoarderMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        List<int> hoardID = new();
        HoarderBugAI[] bugs;
        RoundManager curRound;
        GameObject player;
        SelectableLevel curLevel;
        int hoardIndex = 2;
        PlayerControllerB[] players;

        bool roundActive;
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            GameState.LandOnMoon += RoundStart;
            GameState.ShipStartedLeaving += RoundEnd;
        }
        /// <summary>
        /// Function that is supposed to get called when a round has started (currently checked by moon landing)
        /// </summary>
        private void RoundStart()
        {
            roundActive = true;
            curRound = FindObjectOfType<RoundManager>();
            curLevel = curRound.currentLevel;
            player = GameObject.Find("Player");
            players = FindObjectsOfType<PlayerControllerB>();
#if DEBUG
            Logger.LogInfo($"{curLevel.PlanetName}");
            var pcb = FindObjectsOfType<PlayerControllerB>();
            Logger.LogInfo($" Current Player: {player.name} - {player.transform.position} - {player.layer} {player.tag} {ListAllComponents(player)}");
            for (int i = 0; i < pcb.Length; i++)
            {
                Logger.LogInfo($"{pcb[i].gameObject.name} - {pcb[i].transform.position}");
            }
#endif
            hoardIndex = FindEnemyID("Hoarding bug");
            Logger.LogInfo("Round has started, starting checks");
            try { BugCheck(); } catch (Exception e) { Logger.LogError(e); }
        }
        /// <summary>
        /// Function that is supposed to get called when a round has ended (currently checked by departing)
        /// </summary>
        private void RoundEnd()
        {
            roundActive = false;
            hoardID.Clear();
            Logger.LogInfo("Round has ended, stopping checks");
        }
        /// <summary>
        /// This function constantly checks what bugs exist (only when roundActive is true)
        /// </summary>
        /// <returns></returns>
        private async Task BugCheck()
        {
            while (roundActive == true)
            {
                bugs = FindObjectsOfType<HoarderBugAI>();
                foreach (HoarderBugAI b in bugs) 
                {
#if DEBUG
                    Logger.LogInfo($"{b.name} - {b.transform.position} - {b.nestPosition} - {b.isEnemyDead} - {b.GetInstanceID()}");
                    Logger.LogInfo(hoardID.Contains(b.GetInstanceID()));
#endif
                    if (b.isEnemyDead == true && hoardID.Contains(b.GetInstanceID()) == false)
                    {
                        for (int i = 0; i < 5; i++){ curRound.SpawnEnemyOnServer(b.transform.position, 0, hoardIndex); } // Spawns 5 Hoarding Bugs on a dead Hoarding Bug
                        hoardID.Add(b.GetInstanceID()); // Puts dead Hoarding Bug Instance ID in a list so that they dont spawn infinitely
                        try { AngerBugs(b); } catch (Exception e) { Logger.LogError(e); }
                    }
                }
                await Task.Delay(10); // for some reason it didn't like me using Update
            }
        }
        /// <summary>
        /// Finds the Enemy Index for a certain enemy since that is dependant on moon
        /// </summary>
        /// <param name="enemy">Enemy name (case sensitive I think)</param>
        /// <returns></returns>
        public int FindEnemyID(string enemy)
        {
            for (int i = 0; i < curLevel.Enemies.Count; i++)
            {
                SpawnableEnemyWithRarity e = curLevel.Enemies[i];
                if (e.enemyType.enemyName == enemy)
                {
                    Logger.LogInfo($"Found {enemy} at {i}");
                    return i;
                }
            }
            Logger.LogInfo($"{enemy} index not found, probably doesn't spawn in that case but I'm defaulting to 2 (Hoarding Bug Index on most moons)");
            return 2;
        }

        public void AngerBugs(HoarderBugAI b)
        {
            bugs = FindObjectsOfType<HoarderBugAI>();

            PlayerControllerB tMin = null;
            float minDist = Mathf.Infinity;
            Vector3 currentPos = b.transform.position;
            foreach (PlayerControllerB t in players)
            {
                float dist = Vector3.Distance(t.transform.position, currentPos);
                if (dist < minDist)
                {
                    tMin = t;
                    minDist = dist;
                }
            }
            foreach (var h in bugs)
            {
                h.angryAtPlayer = tMin;
                h.angryTimer += 200f;
            }
        }

#if DEBUG
        public string ListAllComponents(GameObject go)
        {
            List<string> t = new();
            Component[] comp = go.GetComponents<Component>();
            foreach (Component c in comp) { t.Add(c.GetType().ToString()); }
            return string.Join(',', t);
        }
#endif
    }
}