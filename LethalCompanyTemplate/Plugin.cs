using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using LC_API.GameInterfaceAPI;
using UnityEngine;
using Unity.Netcode;
using HarmonyLib;
using GameNetcodeStuff;
using System.Threading.Tasks;

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
        int hoardIndex;
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            GameState.LandOnMoon += RoundStart;
        }

        private void RoundStart()
        {
            curRound = FindObjectOfType<RoundManager>();
            curLevel = curRound.currentLevel;
            Logger.LogInfo($"{curLevel.PlanetName}");
            var pcb = FindObjectsOfType<PlayerControllerB>();
            player = GameObject.Find("Player");
            Logger.LogInfo($" Current Player: {player.name} - {player.transform.position} - {player.layer} {player.tag} {ListAllComponents(player)}");
            for (int i = 0; i < pcb.Length; i++)
            {
                Logger.LogInfo($"{pcb[i].gameObject.name} - {pcb[i].transform.position}");
            }
            hoardIndex = FindBugID("Hoarding bug");
            BugCheck();
        }

        private async Task BugCheck()
        {
            while (true)
            {
                bugs = FindObjectsOfType<HoarderBugAI>();
                foreach (var b in bugs) 
                { 
                    Logger.LogInfo($"{b.name} - {b.transform.position} - {b.nestPosition} - {b.isEnemyDead} - {b.GetInstanceID()}");
                    Logger.LogInfo(hoardID.Contains(b.GetInstanceID()));
                    if (b.isEnemyDead == true && hoardID.Contains(b.GetInstanceID()) == false)
                    {
                        for (int i = 0; i < 5; i++){ curRound.SpawnEnemyOnServer(b.transform.position, 0, 2); }
                        hoardID.Add(b.GetInstanceID());
                    }
                }
                
                await Task.Delay(10);
            }
        }

        public string ListAllComponents(GameObject go)
        {
            List<string> t = new();
            Component[] comp = go.GetComponents<Component>();
            foreach (Component c in comp) { t.Add(c.GetType().ToString()); }
            return string.Join(',', t);
        }

        public int FindBugID(string enemy)
        {
            for (int i = 0; i < curLevel.Enemies.Count; i++)
            {
                SpawnableEnemyWithRarity e = curLevel.Enemies[i];
                if (e.enemyType.enemyName == enemy)
                {
                    Logger.LogInfo(e.enemyType.enemyName);
                    Logger.LogInfo($"Found {enemy} at {i}");
                    return i;
                }
            }
            Logger.LogInfo($"{enemy} index not found, probably doesn't spawn in that case but I'm defaulting to 2");
            return 2;
        }
    }
}