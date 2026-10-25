using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class Character3DDisplayManager : MonoBehaviour
{
	private class Character3D
	{
		private List<CharacterManager> models;

		public IEnumerable<CharacterManager> Models => models;

		public Character3D(List<CharacterManager> models)
		{
			this.models = models;
		}

		public void Show(string animation, bool freezeAnimation)
		{
			for (int i = 0; i < models.Count; i++)
			{
				models[i].gameObject.SetActive(value: true);
				if (animation != null)
				{
					CoroutineHelper.RunCoroutine(PlayAnimWhenAnimatorReadyCrt(models[i].gameObject, animation, freezeAnimation));
				}
			}
		}

		private IEnumerator PlayAnimWhenAnimatorReadyCrt(GameObject targetGO, string animation, bool freezeAnimation = false)
		{
			Animator animator = MF.GetGameObjectAnimator(targetGO);
			while (animator != null && !animator.isInitialized)
			{
				if (Singleton<FHMapChoreographer>.InstanceFast == null)
				{
					yield break;
				}
				yield return 0;
			}
			MF.AnimatorPlay(animator, animation);
			yield return 0;
			animator.enabled = !freezeAnimation;
		}

		public void FreezeAnimator(bool freezeAnimator)
		{
			for (int i = 0; i < models.Count; i++)
			{
				models[i].gameObject.SetActive(value: true);
				CoroutineHelper.RunCoroutine(FreezeAnimatorWhenReadyCrt(models[i].gameObject, freezeAnimator));
			}
		}

		private IEnumerator FreezeAnimatorWhenReadyCrt(GameObject targetGO, bool freezeAnimator = false)
		{
			Animator animator = MF.GetGameObjectAnimator(targetGO);
			while (animator != null && !animator.isInitialized)
			{
				if (Singleton<FHMapChoreographer>.InstanceFast == null)
				{
					yield break;
				}
				yield return 0;
			}
			animator.enabled = !freezeAnimator;
		}

		public void PlayVFX3D(string vfxName)
		{
			for (int i = 0; i < models.Count; i++)
			{
				models[i].gameObject.SetActive(value: true);
				CoroutineHelper.RunCoroutine(PlayVFX3DWhenReadyCrt(models[i].gameObject, vfxName));
			}
		}

		private IEnumerator PlayVFX3DWhenReadyCrt(GameObject targetGO, string vfxName)
		{
			Animator animator = MF.GetGameObjectAnimator(targetGO);
			while (animator != null && !animator.isInitialized)
			{
				if (Singleton<FHMapChoreographer>.InstanceFast == null)
				{
					yield break;
				}
				yield return 0;
			}
			GameObject propPrefab = GlobalSettings.GetPropPrefab(vfxName);
			if (!(propPrefab == null))
			{
				VFXShared.PlayEffect(propPrefab, targetGO.transform, default, default);
			}
		}

		public void Hide()
		{
			for (int i = 0; i < models.Count; i++)
			{
				models[i].gameObject.SetActive(value: false);
			}
		}

		public void Destroy()
		{
			if (!GloomUtility.IsAppQuitting)
			{
				for (int i = 0; i < models.Count; i++)
				{
					models[i].gameObject.SetActive(value: false);
					models[i].DeinitializeCharacter();
					ObjectPool.Recycle(models[i].gameObject);
				}
				models.Clear();
			}
		}
	}

	[SerializeField]
	private bool _isPreview;

	[SerializeField]
	private bool _useHolderPosition;

	[ConditionalField("_useHolderPosition", "true", true)]
	[SerializeField]
	private Vector3 _holderPositionBasedOnHeight = new Vector3(0f, -0.5f, 3f);

	[ConditionalField("_useHolderPosition", "true", true)]
	[SerializeField]
	private Vector3 _holderOffset = Vector3.zero;

	[SerializeField]
	private Transform character3DHolder;

	[SerializeField]
	private UI3DCharacterDisplayEventsChannel _eventsChannel;

	private Character3D character3D;

	private Dictionary<string, Character3D> characterModels = new Dictionary<string, Character3D>();

	private HashSet<Component> showRequests = new HashSet<Component>();

	private UnityAction m_CurrentCharacterBundleDisplayAction;

	private string _currentSkin;

	public ECharacter CurrentCharacterToDisplay { get; private set; }

	public IEnumerable<CharacterManager> CurrentModels => character3D?.Models ?? Enumerable.Empty<CharacterManager>();

	protected void Awake()
	{
		if (_eventsChannel != null)
		{
			_eventsChannel.SetReference(this);
		}
		gameObject.SetActive(value: false);
		SetupLighting();
	}

	private void SetupLighting()
	{
		Character3DDisplayUtils.SetLightingLayer(GetComponentsInChildren<Light>(includeInactive: true));
		Character3DDisplayUtils.SetRendererLayer(GetComponentsInChildren<Renderer>(includeInactive: true));
	}

	private Character3D CreateCharacter(ECharacter character, string skin = null)
	{
		string key = character.ToString() + (skin ?? string.Empty);
		if (characterModels.ContainsKey(key))
		{
			return characterModels[key];
		}
		CharacterConfigUI characterConfigUI = UIInfoTools.Instance.GetCharacterConfigUI(character, useDefault: false);
		List<CharacterManager> list = new List<CharacterManager> { CreateModel(CActor.EType.Player, character.ToString(), skin, characterConfigUI?.GetAssemblyCharacter3DPlacement(AdventureState.MapState == null || !AdventureState.MapState.IsCampaign)?.position, characterConfigUI?.GetAssemblyCharacter3DPlacement(AdventureState.MapState == null || !AdventureState.MapState.IsCampaign)?.rotation) };
		if (characterConfigUI != null)
		{
			for (int i = 0; i < characterConfigUI.assemblyCharacter3DCompanions.Count; i++)
			{
				CharacterConfigUI.Object3DPlacement object3DPlacement = characterConfigUI.assemblyCharacter3DCompanions[i].GetObject3DPlacement(AdventureState.MapState == null || !AdventureState.MapState.IsCampaign);
				CharacterManager characterManager = CreateModel(CActor.EType.HeroSummon, characterConfigUI.assemblyCharacter3DCompanions[i].summon.ToString(), skin, object3DPlacement.position, object3DPlacement.rotation);
				if (characterManager != null)
				{
					list.Add(characterManager);
				}
			}
		}
		characterModels[key] = new Character3D(list);
		return characterModels[key];
	}

	private CharacterManager CreateModel(CActor.EType type, string character, string skin = null, Vector3? position = null, Vector3? rotation = null)
	{
		GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(type, character, skin);
		if (characterPrefabFromBundle == null)
		{
			return null;
		}
		CharacterManager characterManager = CharacterManager.GetCharacterManager(ObjectPool.Spawn(characterPrefabFromBundle, character3DHolder));
		characterManager.CharacterPrefab = characterPrefabFromBundle;
		characterManager.InitialiseCharacter(forMenu: true, _isPreview);
		characterManager.FadeInVisibility(0f);
		characterManager.transform.localEulerAngles = rotation ?? Vector3.zero;
		if (_useHolderPosition)
		{
			float height = characterManager.Height;
			characterManager.transform.localPosition = height * _holderPositionBasedOnHeight + _holderOffset;
		}
		else if (!position.HasValue)
		{
			characterManager.transform.localPosition = new Vector3(0f, (0f - characterManager.Height) / 2f, 0f);
		}
		else
		{
			characterManager.transform.position = position.Value;
		}
		Character3DDisplayUtils.SetRendererLayer(characterManager.GetComponentsInChildren<Renderer>(includeInactive: true));
		Cloth[] componentsInChildren = characterManager.transform.GetComponentsInChildren<Cloth>();
		foreach (Cloth obj in componentsInChildren)
		{
			obj.coefficients = obj.coefficients;
		}
		return characterManager;
	}

	protected void OnDestroy()
	{
		if (GloomUtility.IsAppQuitting)
		{
			return;
		}
		foreach (Character3D value in characterModels.Values)
		{
			value.Destroy();
		}
	}

	public void Display(Component request, ECharacter character, string skin = null, string playAnimation = null)
	{
		if (character != ECharacter.None && showRequests.Contains(request) && CurrentCharacterToDisplay == character && _currentSkin != skin)
		{
			return;
		}
		SetupLighting();
		showRequests.Add(request);
		if (CurrentCharacterToDisplay != character && character3D != null)
		{
			character3D.Hide();
		}
		CurrentCharacterToDisplay = character;
		_currentSkin = skin;
		if (character == ECharacter.None)
		{
			Hide(request);
			return;
		}
		m_CurrentCharacterBundleDisplayAction = () =>
		{
			if (CurrentCharacterToDisplay == character && showRequests.Count != 0)
			{
				Character3D character3D = CreateCharacter(character, skin);
				if (this.character3D != null && character3D != this.character3D)
				{
					this.character3D.Hide();
				}
				this.character3D = character3D;
				PlayAnimation(playAnimation);
				gameObject.SetActive(value: true);
			}
		};
		AssetBundleManager.Instance.EnsureCharacterLoadedBeforeAction(character, m_CurrentCharacterBundleDisplayAction);
	}

	public void PlayAnimation(string playAnimation, bool freezeAnimation = false)
	{
		character3D.Show(playAnimation, freezeAnimation);
	}

	public void FreezeAnimator(bool freezeAnimator)
	{
		character3D.FreezeAnimator(freezeAnimator);
	}

	public void PlayVFX3D(string vfxName)
	{
		character3D.PlayVFX3D(vfxName);
	}

	public void Hide(Component request)
	{
		showRequests.Remove(request);
		if (character3D != null)
		{
			character3D.Hide();
		}
		if (showRequests.Count == 0)
		{
			gameObject.SetActive(value: false);
		}
	}
}
