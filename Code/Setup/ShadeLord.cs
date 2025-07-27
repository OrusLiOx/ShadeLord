using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ShadeLord.Setup
{
	internal class ShadeLord : Mod, ILocalSettings<LocalSettings>
	{
		internal static ShadeLord Instance { get; private set; }

		public static Dictionary<string, AssetBundle> Bundles = new Dictionary<string, AssetBundle>();
		public static Dictionary<string, GameObject> GameObjects = new Dictionary<string, GameObject>();
		public static Texture2D statueTex;

		private LocalSettings _localSettings = new LocalSettings();
		public LocalSettings LocalSettings => _localSettings;

		private Dictionary<string, (string, string)> preload = new Dictionary<string, (string, string)>()
		{
			["Boss Scene Controller"] = ("GG_Radiance", "Boss Scene Controller"),
			["Godseeker"] = ("GG_Collector", "GG_Arena_Prefab/Godseeker Crowd"),
			["Godseeker Throne"] = ("GG_Collector", "GG_Arena_Prefab/BG/throne"),
			["Collector"] = ("GG_Collector", "Battle Scene/Jar Collector"),
            ["Abyss Mist"] = ("GG_Radiance", "Boss Control/Abyss Pit/Pt Mist"),
            ["Abyss Particles"] = ("GG_Radiance", "Boss Control/Abyss Pit/Pt Surface"),
            ["Abyss Msk"] = ("GG_Radiance", "Boss Control/Abyss Pit/msk_generic_soft"),
            ["Abyss Scenery"] = ("Abyss_06_Core", "_Scenery")
        };

        private Material _blurMat;

		// info
		public ShadeLord() : base("ShadeLord") { }
		public override string GetVersion()
		{
			return Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		public override List<(string, string)> GetPreloadNames()
		{
            return preload.Values.ToList();
		}

		// statue information
		private string LangGet(string key, string sheettitle, string orig)
		{
			switch (key)
			{
				case "LORD_MAIN": return "Lord of Shades";
				case "LORD_SUB": return "";
				case "LORD_SUPER": return "";
				case "LORD_NAME": return "Shade Lord";
				case "LORD_DESC": return "God of unified void";
				default: return orig;
			}
		}

		public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
            Instance = this;

			Unload();
            _blurMat = Resources.FindObjectsOfTypeAll<Material>().First(mat => mat.shader.name.Contains("UI/Blur/UIBlur"));
			foreach (var (name, (scene, path)) in preload)
			{
				GameObjects.Add(name, preloadedObjects[scene][path]);
			}

            LoadAssets();
			ModHooks.AfterSavegameLoadHook += AfterSaveGameLoad;
			ModHooks.GetPlayerVariableHook += GetVariableHook;
			ModHooks.LanguageGetHook += LangGet;
			ModHooks.NewGameHook += AddComponent;
			On.GameManager.StartNewGame += StartNewGame;
			On.HealthManager.TakeDamage += OnTakeDamage;
			ModHooks.SetPlayerVariableHook += SetVariableHook;

			On.BlurPlane.Awake += OnBlurPlaneAwake;
			On.SceneManager.Start += OnSceneManagerStart;
			On.tk2dTileMap.Awake += OnTileMapAwake;
		}

		// settings
		public void OnLoadLocal(LocalSettings localSettings) => _localSettings = localSettings;
		public LocalSettings OnSaveLocal() => _localSettings;

		private object GetVariableHook(Type t, string key, object orig)
		{
			if (key == "statueStateLord")
			{
				return _localSettings.Completion;
			}

			return orig;
		}
		private object SetVariableHook(Type t, string key, object obj)
		{
			if (key == "statueStateLord")
			{
				_localSettings.Completion = (BossStatue.Completion)obj;
			}

			return obj;
		}

		// set stuff up
		private void AfterSaveGameLoad(SaveGameData data) => AddComponent();
		private void StartNewGame(On.GameManager.orig_StartNewGame orig, GameManager self, bool permaDeath, bool bossRush)
		{
			orig(self, permaDeath, bossRush);
			AddComponent();
		}

		private void AddComponent()
		{
			GameManager.instance.gameObject.AddComponent<StatueCreator>();
			GameManager.instance.gameObject.AddComponent<SceneLoader>();

		}

		private void LoadAssets()
		{
			var assembly = Assembly.GetExecutingAssembly();
			foreach (string resourceName in assembly.GetManifestResourceNames())
			{

				using (Stream stream = assembly.GetManifestResourceStream(resourceName))
				{
					if (stream == null) continue;
					if (resourceName.Contains("gg_shade_lord"))
					{
						var bundle = AssetBundle.LoadFromStream(stream);
						Bundles.Add(bundle.name, bundle);
					}
					else if (resourceName.Contains("GG_Statue_ShadeLord"))
					{
						var buffer = new byte[stream.Length];
						stream.Read(buffer, 0, buffer.Length);
						statueTex = new Texture2D(2, 2);
						statueTex.LoadImage(buffer);
					}

					stream.Dispose();
				}
			}
		}
		private void Unload()
		{
			ModHooks.AfterSavegameLoadHook -= AfterSaveGameLoad;
			ModHooks.GetPlayerVariableHook -= GetVariableHook;
			ModHooks.LanguageGetHook -= LangGet;
			ModHooks.SetPlayerVariableHook -= SetVariableHook;
			ModHooks.NewGameHook -= AddComponent;

			On.BlurPlane.Awake -= OnBlurPlaneAwake;
			On.SceneManager.Start -= OnSceneManagerStart;
			On.tk2dTileMap.Awake -= OnTileMapAwake;

			var finder = GameManager.instance?.gameObject.GetComponent<StatueCreator>();
			if (finder == null)
			{
				return;
			}

			UObject.Destroy(finder);
		}

		public void OnTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitinstance)
		{
			ShadeLordCtrl ctrl;
			Modding.Logger.Log(self.gameObject.name);
			if (!self.gameObject.TryGetComponent<ShadeLordCtrl>(out ctrl))
			{
				Modding.Logger.Log("normal hit: " + self.hp + " " + hitinstance.DamageDealt + " " + hitinstance.Direction);
				orig(self, hitinstance);
				return;
			}
			Modding.Logger.Log("ShadeLord hit: " +self.hp + " " + hitinstance.DamageDealt + " " + hitinstance.Direction);
			// deal hit then check phase
			ctrl.particleEm.rotation = new Vector3(0, -hitinstance.Direction + 90, 0);
			ctrl.particles.transform.position = ctrl.transform.position;
			ctrl.particles.transform.SetPositionY(ctrl.particles.transform.position.y + -2.5f);
			ctrl.particles.Play();
			ctrl.StartCoroutine(flicker());

			orig(self, hitinstance);
			//SpawnHitEffect(hitinstance.Direction);
			if (ctrl.health.hp < ctrl.hpMarkers[ctrl.phase])
			{
				ctrl.nextPhase();
			}//*/

			IEnumerator flicker()
			{
				ctrl.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
				for (int k = 0; k < 10; k++)
				{
					float c = .1f * k;
					ctrl.gameObject.GetComponent<SpriteRenderer>().color = new Color(c, c, c);
					yield return new WaitForSeconds(1 / 60f);
				}
				ctrl.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
			}
		}


		// scene stuff
		private void OnBlurPlaneAwake(On.BlurPlane.orig_Awake orig, BlurPlane self)
		{
			orig(self);

			if (self.OriginalMaterial.shader.name == "UI/Default")
			{
				self.SetPlaneMaterial(_blurMat);
			}
		}

		private void OnSceneManagerStart(On.SceneManager.orig_Start orig, SceneManager self)
		{
			orig(self);

			self.tag = "SceneManager";
		}

		private void OnTileMapAwake(On.tk2dTileMap.orig_Awake orig, tk2dTileMap self)
		{
			orig(self);

			self.tag = "TileMap";
		}

		// Change music script from Pale Court
		public static void PlayMusic(AudioClip clip)
		{
			MusicCue musicCue = ScriptableObject.CreateInstance<MusicCue>();
			MusicCue.MusicChannelInfo channelInfo = new MusicCue.MusicChannelInfo();
			ReflectionHelper.SetField(channelInfo, "clip", clip);
			MusicCue.MusicChannelInfo[] channelInfos = new MusicCue.MusicChannelInfo[]
			{
				channelInfo, null, null, null, null, null
			};
			ReflectionHelper.SetField(musicCue, "channelInfos", channelInfos);
			GameManager.instance.AudioManager.ApplyMusicCue(musicCue, 0, 0, false);
		}

		public static void DreamDelayed()
		{
			StatueCreator.WonFight = true;

			var bsc = SceneLoader.SceneController.GetComponent<BossSceneController>();
			GameObject transition = UObject.Instantiate(bsc.transitionPrefab);
			PlayMakerFSM transitionsFSM = transition.LocateMyFSM("Transitions");
			transitionsFSM.SetState("Out Statue");
			bsc.DoDreamReturn();
		}
	}
}