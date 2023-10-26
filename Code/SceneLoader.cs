using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using System.Collections;

namespace ShadeLord
{
    internal class SceneLoader : MonoBehaviour
    {
        internal static BossSceneController SceneController;
		float x = 100;

        private void Awake()
        {
            On.GameManager.EnterHero += OnEnterHero;
            USceneManager.activeSceneChanged += OnSceneChange;
        }

		private void OnEnterHero(On.GameManager.orig_EnterHero orig, GameManager gm, bool additiveGateSearch)
		{
			orig(gm, additiveGateSearch);
			if (gm.sceneName == "GG_Shade_Lord")
			{
				HeroController.instance.transform.SetPosition2D
				(HeroController.instance.gameObject.GetComponent<HeroController>().FindGroundPoint
				(PlayerData.instance.GetVector3("hazardRespawnLocation")));
			}
		}
		private void OnSceneChange(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "GG_Shade_Lord")
            {
				GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
				foreach (GameObject obj in allObjects)
				{
					if (obj.name.Contains("Respawn"))
						MakeRespawn(obj);
					else if (obj.name.Equals("CameraLock"))
						SetCameraLock(obj);
					else if (obj.name.Contains("VoidHazard"))
						obj.AddComponent<DamageHero>().hazardType = 5;//*/
					if (obj.GetComponent<CameraLockArea>() != null)
					{
						Modding.Logger.Log(obj.name+": " +obj.GetComponent<CameraLockArea>().cameraXMax + "," + obj.GetComponent<CameraLockArea>().cameraYMax);
					}
					if (obj.GetComponent<SpriteRenderer>() != null)
					{
						obj.GetComponent<SpriteRenderer>().material.shader = Shader.Find("Sprites/Default");
					}
				}

				PlayerData.instance.SetVector3("hazardRespawnLocation", new Vector3(x, 69f));

				var bsc = Instantiate(ShadeLord.GameObjects["Boss Scene Controller"]);
                bsc.SetActive(true);
                SceneController = bsc.GetComponent<BossSceneController>();
                StatueCreator.BossLevel = SceneController.BossLevel;

                var godseeker = Instantiate(ShadeLord.GameObjects["Godseeker"], new Vector3(x, 70f, 28.39f), Quaternion.identity);
                godseeker.SetActive(true);
                godseeker.transform.localScale = Vector3.one * 1.5f;
				
				// boss stuff
				GameObject.Find("ShadeLord").AddComponent<ShadeCtrl>();

				//SetCameraLock("Terrain/Area1");
				//SetCameraLock("Terrain/Area3");
				//GameObject.Find("Terrain/Area1/Floor").SetActive(true);

				// hazard respawns
				/*
				CreateSpawn("a1",		x, 68f, 53f, 1f, true);
				/*CreateSpawn("a2middle",	x, 55.6f, 5f, 1f, true);
				CreateSpawn("a2left",	x-12, 55.6f, 5f, 1f, true);
				CreateSpawn("a2right",	x+12, 55.6f, 5f, 1f, false);*/
				/*
				CreateSpawn("dcheck1",	x-24.5f, 37f, 3f, 3f, true);
				CreateSpawn("dplat1",	x-15.76f, 34f, 4f, 1f, true);
				CreateSpawn("dplat2",	x-9f, 30f, 3f, 1f, true);
				CreateSpawn("dplat3",	x+5f, 39f, 3f, 1f, true);
				CreateSpawn("dplat4",	x+13f, 32f, 3f, 1f, true);
				CreateSpawn("dcheck2",	x+14.5f, 37f, 3f, 3f, true);

				CreateSpawn("dcheck3",	x+40.5f, 24f, 3f, 3f, true);
				CreateSpawn("dplat5",	x+45.1f, 9f, 5f, 1f, true);
				CreateSpawn("dcheck4",	x+57.5f, 13.5f, 3f, 3f, true);

				CreateSpawn("dcheck5",	x+82.5f, 24.5f, 3f, 25f, true);
				CreateSpawn("dplat6",	x+90.1f, 11f, 5f, 1f, true);
				CreateSpawn("dplat7",	x+100.8f, 7.5f, 5f, 1f, true);

				CreateSpawn("a3",	x+110.11f, 4f, 5f, 1f, true);//*/


				//GameObject.Find("Void").AddComponent<DamageHero>().hazardType = 5;
				//GameObject.Find("PermaVoid").AddComponent<DamageHero>().hazardType = 5;
				//end boss stuff

				var rootGOs = nextScene.GetRootGameObjects();
                foreach (var go in rootGOs)
                {
                    foreach (var sprRend in go.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        sprRend.material.shader = Shader.Find("Sprites/Default");
                    }

                    foreach (var meshRend in go.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        meshRend.material.shader = Shader.Find(meshRend.GetComponent<BlurPlane>() ? "UI/Blur/UIBlur" : "Sprites/Default-ColorFlash");
                    }

                    foreach (var tileRend in UObject.FindObjectsOfType<TilemapRenderer>(true))
                    {
                        tileRend.material.shader = Shader.Find("Sprites/Default");
                    }
                }
				//StartCoroutine(position());
			}
        }
		private void SceneManagerOnStart(On.SceneManager.orig_Start orig, SceneManager self)
		{
			self.mapZone = GlobalEnums.MapZone.ABYSS_DEEP;
			self.isWindy = false;
			self.noParticles = false;
			self.environmentType = 5;
			self.overrideParticlesWith = GlobalEnums.MapZone.ABYSS_DEEP;
		}

        private void OnDestroy()
        {
            On.GameManager.EnterHero -= OnEnterHero;
        }

		private IEnumerator position()
		{
			Modding.Logger.Log("Player:" + HeroController.instance.gameObject.transform.position);
			Modding.Logger.Log("  Hazard Respawn Marker:" + PlayerData.instance.GetVector3("hazardRespawnLocation"));
			Modding.Logger.Log("  Find Ground: " + HeroController.instance.gameObject.GetComponent<HeroController>().FindGroundPoint(PlayerData.instance.GetVector3("hazardRespawnLocation"), true));
			Modding.Logger.Log("  Hero Spawn: " + SceneController.heroSpawn.position);
			yield return new WaitForSeconds(3f);
			StartCoroutine(position());
		}

		private void MakeRespawn(GameObject obj)
		{
			BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
			if (col == null)
				return;
			obj.GetComponent<BoxCollider2D>().isTrigger = true;

			HazardRespawnMarker marker = obj.AddComponent<HazardRespawnMarker>();
			HazardRespawnTrigger trigger = obj.AddComponent<HazardRespawnTrigger>();
			trigger.respawnMarker = marker;
			trigger.fireOnce = false;
			marker.respawnFacingRight = !obj.name.EndsWith("L");	// last letter is R

			marker.transform.SetPositionZ(0);
			marker.respawnFacingRight = true;

			obj.SetActive(true);//*/
		}
		private static void CreateSpawn(string name, float x, float y, float xSize, float ySize, bool respawnFacingRight = true)
		{
			GameObject go = new GameObject(name);
			go.transform.SetPosition2D(new Vector2(x, y));

			BoxCollider2D box = go.AddComponent<BoxCollider2D>();
			box.isTrigger = true;
			box.size = new Vector2(xSize, ySize);

			HazardRespawnMarker hrm = go.AddComponent<HazardRespawnMarker>();
			HazardRespawnTrigger hrt = go.AddComponent<HazardRespawnTrigger>();
			hrt.respawnMarker = hrm;
			hrt.fireOnce = false;
			hrm.respawnFacingRight = respawnFacingRight;

			hrm.transform.SetPositionZ(0);
			hrm.respawnFacingRight = true;

			go.SetActive(true);
		}
		private void SetCameraLock(GameObject obj)
		{
			Modding.Logger.Log("set camera lock: "+ obj.GetComponentsInChildren<BoxCollider2D>()[1].gameObject.name);
			CameraLockArea area = obj.AddComponent<CameraLockArea>();
			Bounds bounds = obj.GetComponentsInChildren<BoxCollider2D>()[1].bounds;

			area.cameraXMax = bounds.max.x;
			area.cameraXMin = bounds.min.x;
			area.cameraYMax = bounds.max.y;
			area.cameraYMin = bounds.min.y;//*/
		}
    }
}