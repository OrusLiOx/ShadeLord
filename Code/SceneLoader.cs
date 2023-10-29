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
				// add properties to certain elements
				// respawns, camera locks, hazards, etc
				GameObject[] allObjects = FindObjectsOfType<GameObject>();
				foreach (GameObject obj in allObjects)
				{
					if (obj.name.Contains("Respawn"))
						MakeRespawn(obj);
					else if (obj.name.Equals("CameraLock"))
						SetCameraLock(obj);
					else if (obj.name.Contains("VoidHazard"))
						obj.AddComponent<DamageHero>().hazardType = 5;//*/
					else if (obj.name.Contains("ShortTendril"))
						obj.AddComponent<RandomizeAnimationStart>();

					if (obj.GetComponent<SpriteRenderer>() != null)
					{
						obj.GetComponent<SpriteRenderer>().material.shader = Shader.Find("Sprites/Default");
					}
				}

				PlayerData.instance.SetVector3("hazardRespawnLocation", new Vector3(x, 69f));

				// scene stuf
				var bsc = Instantiate(ShadeLord.GameObjects["Boss Scene Controller"]);
                bsc.SetActive(true);
                SceneController = bsc.GetComponent<BossSceneController>();
                StatueCreator.BossLevel = SceneController.BossLevel;

                var godseeker = Instantiate(ShadeLord.GameObjects["Godseeker"], new Vector3(x, 75f, 18.39f), Quaternion.identity);
                godseeker.SetActive(true);
                godseeker.transform.localScale = Vector3.one * 1f;
				
				// boss stuff
				GameObject.Find("ShadeLord").AddComponent<ShadeCtrl>();
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
			CameraLockArea area = obj.AddComponent<CameraLockArea>();
			Bounds bounds = obj.GetComponentsInChildren<BoxCollider2D>()[1].bounds;

			area.cameraXMax = bounds.max.x;
			area.cameraXMin = bounds.min.x;
			area.cameraYMax = bounds.max.y;
			area.cameraYMin = bounds.min.y;//*/
		}
    }
}