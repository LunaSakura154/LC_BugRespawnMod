using BepInEx;
using LC_API.GameInterfaceAPI;
using UnityEngine;
using Unity.Netcode;
using HarmonyLib;
using GameNetcodeStuff;
using System.Collections.Generic;

namespace HoarderMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        HoarderBugAI[] bugs;
        RoundManager curRound;
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            GameState.LandOnMoon += SpawnBug;
        }

        private void SpawnBug()
        {
            curRound = FindObjectOfType<RoundManager>();
            Logger.LogInfo($"{curRound.gameObject.name}");
            var pcb = FindObjectsOfType<PlayerControllerB>();
            GameObject player = GameObject.Find("Player");
            Logger.LogInfo($" Current Player: {player.name} - {player.transform.position} - {player.layer} {player.tag} {ListAllComponents(player)}");
            for (int i = 0; i < pcb.Length; i++)
            {
                Logger.LogInfo($"{pcb[i].gameObject.name} - {pcb[i].transform.position}");
            }
        }

        private void FixedUpdate()
        {
            bugs = FindObjectsOfType<HoarderBugAI>();
            foreach (var b in bugs) { Logger.LogInfo($"{b.name} - {b.transform.position} - {b.nestPosition} - {b.isEnemyDead}"); }
        }

        public string ListAllComponents(GameObject go)
        {
            List<string> t = new();
            Component[] comp = go.GetComponents<Component>();
            foreach (Component c in comp) { t.Add(c.GetType().ToString()); }
            return string.Join(',', t);
        }
    }
}