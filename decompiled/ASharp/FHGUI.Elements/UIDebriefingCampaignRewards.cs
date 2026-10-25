using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHRL;
using GLOOM;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIDebriefingCampaignRewards : MonoBehaviour
{
	public struct Params
	{
		public BattleResultDTO BattleResults;
	}

	[Header("Components")]
	[SerializeField]
	public RectTransform CampaignRewardsContainer;

	[Header("Inspiration")]
	[SerializeField]
	public RectTransform InspirationContainer;

	[SerializeField]
	public FHSimpleIcon InspirationIcon;

	[Header("Herbs")]
	[SerializeField]
	public GameObject Herbs;

	[SerializeField]
	public RectTransform HerbIconsContainer;

	[SerializeField]
	public FHSimpleIcon HerbIconPrototype;

	[Header("Pets")]
	[SerializeField]
	public GameObject CapturedPets;

	[SerializeField]
	public RectTransform CapturedPetIconsContainer;

	[SerializeField]
	public FHSimpleIcon PetIconPrototype;

	[Header("Blueprints")]
	[SerializeField]
	public GameObject Blueprints;

	[SerializeField]
	public RectTransform BlueprintIconsContainer;

	[SerializeField]
	public FHSimpleIcon BlueprintIconPrototype;

	[Header("Other Effects")]
	[SerializeField]
	private GameObject m_EffectPrototype;

	[SerializeField]
	private RectTransform m_EffectsContainer;

	private List<GameObject> m_Effects = new List<GameObject>(5);

	[Header("Challenges")]
	[SerializeField]
	public GameObject Challenges;

	[SerializeField]
	public RectTransform ChallengesContainer;

	[SerializeField]
	public UIScenarioBriefingSuggestibleSlot ChallengesPrototype;

	private List<FHSimpleIcon> m_HerbsIcons = new List<FHSimpleIcon>();

	private List<FHSimpleIcon> m_PetsIcons = new List<FHSimpleIcon>();

	private List<FHSimpleIcon> m_BlueprintsIcons = new List<FHSimpleIcon>();

	private List<UIScenarioBriefingSuggestibleSlot> m_ChallengesIcons = new List<UIScenarioBriefingSuggestibleSlot>();

	private bool m_Initialzied;

	public Params Data { get; private set; }

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			HerbIconsContainer.GetComponentsInChildren(includeInactive: true, m_HerbsIcons);
			CapturedPetIconsContainer.GetComponentsInChildren(includeInactive: true, m_PetsIcons);
			BlueprintIconsContainer.GetComponentsInChildren(includeInactive: true, m_BlueprintsIcons);
			ChallengesContainer.GetComponentsInChildren(includeInactive: true, m_ChallengesIcons);
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		Data = @params;
		bool flag = false;
		flag |= PopulateInspiration();
		flag |= PopulateHerbs();
		flag |= PopulatePets();
		flag |= PopulateBlueprints();
		CampaignRewardsContainer.gameObject.SetActive(flag);
		PopulateChallenges();
		PopulateEffects();
	}

	private void OnEnable()
	{
		if (m_Initialzied)
		{
			Init(Data);
		}
	}

	private bool PopulateInspiration()
	{
		if (Data.BattleResults == null)
		{
			return false;
		}
		int num = Data.BattleResults.InspirationFromScenario + Data.BattleResults.InspirationFromLoot + Data.BattleResults.InspirationFromFromMissingCharacters;
		if (num == 0)
		{
			InspirationContainer.gameObject.SetActive(value: false);
			return false;
		}
		InspirationContainer.gameObject.SetActive(value: true);
		InspirationIcon.Init(new SpriteWithTooltipDTO
		{
			Sprite = UIInfoTools.Instance.GetResourceConfig("Inspiration").Icon,
			TextValue = num.ToString()
		});
		return true;
	}

	private bool PopulateHerbs()
	{
		if (Data.BattleResults == null || Data.BattleResults.Herbs.TotalHerbs == 0)
		{
			Herbs.gameObject.SetActive(value: false);
			return false;
		}
		List<SpriteWithTooltipDTO> list = new List<SpriteWithTooltipDTO>();
		foreach (OutpostHerbType item in OutpostResources.AllHerbTypes())
		{
			int herbAmount = Data.BattleResults.Herbs.GetHerbAmount(item);
			if (herbAmount > 0)
			{
				list.Add(new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetHerbConfig(item).Icon,
					TextValue = herbAmount.ToString(),
					Description = LocalizationManager.GetTranslation(FHUILocKeys.GetHerbLocalizationKey(item))
				});
			}
		}
		HelperTools.NormalizePool(ref m_HerbsIcons, HerbIconPrototype.gameObject, HerbIconsContainer.transform, list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			m_HerbsIcons[i].Init(list[i]);
		}
		Herbs.gameObject.SetActive(list.Count > 0);
		return true;
	}

	private bool PopulatePets()
	{
		if (Data.BattleResults?.CapturedPets == null || !Data.BattleResults.CapturedPets.Any())
		{
			CapturedPets.gameObject.SetActive(value: false);
			return false;
		}
		List<SpriteWithTooltipDTO> list = new List<SpriteWithTooltipDTO>();
		foreach (ItemDataDTO capturedPet in Data.BattleResults.CapturedPets)
		{
			list.Add(new SpriteWithTooltipDTO
			{
				Sprite = capturedPet.Icon,
				Description = capturedPet.NameLocalized
			});
		}
		HelperTools.NormalizePool(ref m_PetsIcons, PetIconPrototype.gameObject, CapturedPetIconsContainer.transform, list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			m_PetsIcons[i].Init(list[i]);
		}
		CapturedPets.gameObject.SetActive(list.Count > 0);
		return true;
	}

	private bool PopulateBlueprints()
	{
		if (Data.BattleResults?.UnlockedBlueprints == null || Data.BattleResults.UnlockedBlueprints.Count == 0)
		{
			Blueprints.gameObject.SetActive(value: false);
			return false;
		}
		List<SpriteWithTooltipDTO> list = new List<SpriteWithTooltipDTO>();
		foreach (ItemDataDTO unlockedBlueprint in Data.BattleResults.UnlockedBlueprints)
		{
			list.Add(new SpriteWithTooltipDTO
			{
				Sprite = unlockedBlueprint.Icon,
				Description = unlockedBlueprint.NameLocalized
			});
		}
		HelperTools.NormalizePool(ref m_BlueprintsIcons, BlueprintIconPrototype.gameObject, BlueprintIconsContainer.transform, list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			m_BlueprintsIcons[i].Init(list[i]);
		}
		Blueprints.SetActive(list.Count > 0);
		return true;
	}

	private void PopulateChallenges()
	{
		if (Data.BattleResults?.Challenges == null || Data.BattleResults.Challenges.Count == 0)
		{
			Challenges.SetActive(value: false);
			return;
		}
		int count = Data.BattleResults.Challenges.Count;
		HelperTools.NormalizePool(ref m_ChallengesIcons, ChallengesPrototype.gameObject, ChallengesContainer.transform, count);
		for (int i = 0; i < count; i++)
		{
			m_ChallengesIcons[i].Init(Data.BattleResults.Challenges[i], null, Data.BattleResults.ChallengesTooltipParams[i]);
		}
		Challenges.SetActive(count > 0);
	}

	private void PopulateEffects()
	{
		List<OutpostEventDTO.Effect> list = Data.BattleResults.Effects.Where((OutpostEventDTO.Effect effect2) => !effect2.Hidden).ToList();
		int count = list.Count;
		m_EffectsContainer.gameObject.SetActive(count > 0);
		UIHelperUtil.UpdateUIList(count, m_Effects, m_EffectPrototype, m_EffectsContainer);
		for (int num = 0; num < m_Effects.Count; num++)
		{
			OutpostEventDTO.Effect effect = ((num < list.Count) ? list[num] : null);
			GameObject gameObject = m_Effects[num];
			gameObject.gameObject.SetActive(effect != null);
			if (effect != null)
			{
				TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
				if ((object)componentInChildren != null)
				{
					componentInChildren.text = effect.Text;
				}
			}
		}
	}
}
