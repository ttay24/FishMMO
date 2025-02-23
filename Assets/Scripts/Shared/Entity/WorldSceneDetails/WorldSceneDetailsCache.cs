using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "WorldSceneDetails", menuName = "World Scene Details")]
public class WorldSceneDetailsCache : ScriptableObject
{
	[Tooltip("Apply this tag to any object in your starting scenes to turn them into initial spawn locations.")]
	public string initialSpawnTag = "InitialSpawnPosition";
	[Tooltip("Apply this tag to any object in your scene you would like to behave as a respawn location.")]
	public string respawnTag = "RespawnPosition";
	[Tooltip("Apply this tag to any object in your scene that you would like to act as a teleporter.")]
	public string teleporterTag = "Teleporter";
	[Tooltip("Apply this tag to any object in your scene that you would like to act as a teleporter destination.")]
	public string teleporterDestinationTag = "TeleporterDestination";

	public WorldSceneDetailsDictionary scenes = new WorldSceneDetailsDictionary();

	public void Search()
	{
#if UNITY_EDITOR
		Debug.Log("[" + DateTime.UtcNow + "] WorldSceneDetails: Searching");

		scenes.Clear();
		scenes = new WorldSceneDetailsDictionary();

		Dictionary<string, Dictionary<string, SceneTeleporterDetails>> teleporterCache = new Dictionary<string, Dictionary<string, SceneTeleporterDetails>>();
		Dictionary<string, TeleporterDestinationDetails> teleporterDestinationCache = new Dictionary<string, TeleporterDestinationDetails>();

		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (!scene.enabled)
				continue;

			// ensure the scene is a world scene
			if (!scene.path.Contains("/WorldScene/"))
				continue;

			// load the scene
			Scene s = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
			if (s.IsValid())
			{
				if (!scenes.ContainsKey(s.name))
				{
					Debug.Log("[" + DateTime.UtcNow + "] WorldSceneDetails: Scene: [" + s.name + "] - Loaded");

					// add the scene to our world scenes list
					WorldSceneDetails sceneDetails = new WorldSceneDetails();
					scenes.Add(s.name, sceneDetails);

					// search for initialSpawnPositions
					GameObject[] initialSpawns = GameObject.FindGameObjectsWithTag(initialSpawnTag);
					foreach (GameObject obj in initialSpawns)
					{
						Debug.Log("[" + DateTime.UtcNow + "] WorldSceneDetails: Found new InitialSpawnPosition: [" + obj.name + " Pos:" + obj.transform.position + " Rot:" + obj.transform.rotation + "]");

						sceneDetails.initialSpawnPositions.Add(obj.name, new CharacterInitialSpawnPosition()
						{
							spawnerName = obj.name,
							sceneName = s.name,
							position = obj.transform.position,
							rotation = obj.transform.rotation,
						});
					}

					// search for respawnPositions
					GameObject[] respawnPositions = GameObject.FindGameObjectsWithTag(respawnTag);
					foreach (GameObject obj in respawnPositions)
					{
						Debug.Log("[" + DateTime.UtcNow + "] WorldSceneDetails: Found new RespawnPosition: [" + obj.name + " " + obj.transform.position + "]");

						sceneDetails.respawnPositions.Add(obj.name, obj.transform.position);
					}

					// search for teleporters
					SceneTeleporter[] teleports = GameObject.FindObjectsOfType<SceneTeleporter>();
					foreach (SceneTeleporter obj in teleports)
					{
						obj.name = obj.name.Trim();

						SceneTeleporterDetails newDetails = new SceneTeleporterDetails()
						{
							from = obj.name, // used for validation
							// we still need to set toScene and toPosition later
						};

						if (!teleporterCache.TryGetValue(s.name, out Dictionary<string, SceneTeleporterDetails> teleporters))
						{
							teleporterCache.Add(s.name, teleporters = new Dictionary<string, SceneTeleporterDetails>());
						}
						teleporters.Add(obj.name, newDetails);
					}

					// search for teleporter destinations
					GameObject[] teleportDestinations = GameObject.FindGameObjectsWithTag(teleporterDestinationTag);
					foreach (GameObject obj in teleportDestinations)
					{
						string teleporterDestinationName = obj.name.Trim();

						Debug.Log("[" + DateTime.UtcNow + "] WorldSceneDetails: Found new TeleporterDestination: [Destination:" + teleporterDestinationName + " " + obj.transform.localPosition + "]");

						teleporterDestinationCache.Add(obj.name.Trim(), new TeleporterDestinationDetails()
						{
							scene = s.name,
							position = obj.transform.localPosition,
						});
					}

					Debug.Log("[" + DateTime.UtcNow + "] WorldSceneDetails: Scene: [" + s.name + "] - Unloaded");
				}
			}
			// unload the scene
			EditorSceneManager.CloseScene(s, true);
		}

		Debug.Log("[" + DateTime.UtcNow + "] WorldSceneDetails: Connecting teleporters...");

		// assign teleporter destination positions
		foreach (KeyValuePair<string, Dictionary<string, SceneTeleporterDetails>> teleporterDetailsPair in teleporterCache)
		{
			foreach (KeyValuePair<string, SceneTeleporterDetails> pair in teleporterDetailsPair.Value)
			{
				if (teleporterDestinationCache.TryGetValue("From" + pair.Value.from, out TeleporterDestinationDetails destination))
				{
					if (scenes.TryGetValue(teleporterDetailsPair.Key, out WorldSceneDetails sceneDetails))
					{
						pair.Value.toScene = destination.scene;
						pair.Value.toPosition = destination.position;

						Debug.Log("[" + DateTime.UtcNow + "] WorldSceneDetails: Teleporter [" + pair.Key + "] linked to: [Scene:" + destination.scene + " " + pair.Value.toPosition + "]");

						sceneDetails.teleporters.Add(pair.Key, pair.Value);
					}
				}
			}
		}

		Debug.Log("[" + DateTime.UtcNow + "] WorldSceneDetails: Search Complete");
#endif
	}
}