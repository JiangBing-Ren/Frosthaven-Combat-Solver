using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClockStone;
using FH.GraphicalUserInterface;
using FH.Misc;
using I2.Loc;
using Libs.Analysis;
using Libs.Utils;
using SETUtil.Extend;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersistentData : MonoBehaviour
{
	public static PersistentData s_Instance;

	public GameObject ObjectPoolPrefab;

	private const string MonsterCardPrefab = "MonsterCards/FH_UI_HUD_CARD_BASE_MonsterCardFull";

	private const string AbilityCardPrefab = "AbilityCard";

	private const string AbilityCardPrefabFH = "AbilityCard-FH";

	private const string LongRestPrefab = "LongRest card";

	private const string ItemCardPrefab = "ItemCard";

	private const string ALT_CARDS_SETTING = "UseAltCards";

	private Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

	public bool IsDataLoaded { get; private set; }

	public bool FailedLoading { get; set; }

	public static bool HasAlternativeCards
	{
		get
		{
			return PlayerPrefs.GetInt("UseAltCards") == 1;
		}
		set
		{
			PlayerPrefs.SetInt("UseAltCards", value ? 1 : 0);
		}
	}

	private void Awake()
	{
		s_Instance = this;
		IsDataLoaded = false;
		FailedLoading = false;
	}

	public IEnumerator InitGlobalData()
	{
		Log.Info("Waiting for PlatformLayer init and authorisation to be complete");
		while (!PlatformLayer.Instance.NewPlatformLayer.IsAvailableAndReady)
		{
			yield return null;
		}
		float initialAssetBundleLoadPortion = 15f;
		try
		{
			if (!AssetBundleManager.Instance.InitialLoadComplete)
			{
				AssetBundleManager.Instance.LoadAllInitiallyRequiredBundles();
			}
		}
		catch (Exception ex)
		{
			FailedLoading = true;
			Log.Exception(ex, "An exception occurred while loading Persistent Data");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
			yield break;
		}
		while (!AssetBundleManager.Instance.InitialLoadComplete)
		{
			if ((bool)SceneController.Instance.LoadingScreenInstance)
			{
				SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(initialAssetBundleLoadPortion * AssetBundleManager.Instance.InitialLoadProgress);
			}
			yield return null;
		}
		if ((bool)SceneController.Instance.LoadingScreenInstance)
		{
			SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(initialAssetBundleLoadPortion);
		}
		try
		{
			if (ObjectPool.instance == null)
			{
				UnityEngine.Object.Instantiate(ObjectPoolPrefab, transform);
			}
			else
			{
				ObjectPool.DestroyCardPools();
			}
		}
		catch (Exception ex2)
		{
			FailedLoading = true;
			Log.Exception(ex2, "An exception occurred while loading Persistent Data");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
			yield break;
		}
		if ((bool)SceneController.Instance.LoadingScreenInstance)
		{
			SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(initialAssetBundleLoadPortion + 15f);
			yield return null;
		}
		yield return GlobalSettings.InitInstanceCrt();
		yield return UIInfoTools.InitInstanceCrt();
		if (Application.isPlaying)
		{
			yield return CreateAudioMasterCrt();
		}
		BudgetCoroutineRunner budgetCoroutineRunner = new BudgetCoroutineRunner("LoadAllCards", LoadAllCardsCrt(initialAssetBundleLoadPortion));
		yield return budgetCoroutineRunner.RunCoroutine();
		IsDataLoaded = true;
	}

	private IEnumerator LoadAllCardsCrt(float initialAssetBundleLoadPortion)
	{
		if (!Application.isPlaying)
		{
			yield break;
		}
		GameObject newAbilityCard = null;
		AbilityCardUI abilityCardUI = null;
		GameObject newPetCard = null;
		ItemCardUI petCardUI = null;
		List<CAbilityCard> list;
		try
		{
			list = CharacterClassManager.AllAbilityCards.ToList();
		}
		catch (Exception ex)
		{
			FailedLoading = true;
			Log.Exception(ex, "An exception occurred while loading Persistent Data");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
			yield break;
		}
		bool enableCardCache = false;
		UnityTimingScope createAbilityCards = new UnityTimingScope("Create Ability Card");
		float abilityProgressIncrement = (60f - initialAssetBundleLoadPortion) / ((float)(list.Count + 1) * 2f);
		foreach (CAbilityCard item in list)
		{
			bool flag = false;
			if (enableCardCache)
			{
				GameObject gameObject = PrefabLoaderCache.MiscCards.Load($"AbilityCard_{item.ID}");
				if (gameObject != null)
				{
					abilityCardUI = UnityEngine.Object.Instantiate(gameObject.GetComponent<AbilityCardUI>(), ObjectPool.instance.transform);
					newAbilityCard = abilityCardUI.gameObject;
					flag = true;
					ObjectPool.CreateCardPool(item.ID, ObjectPool.ECardType.Ability, newAbilityCard, 1);
				}
				else
				{
					Log.Warning($"Ability card 'AbilityCard_{item.ID}' was not found in card cache, will be generated runtime.");
				}
			}
			if (!flag)
			{
				try
				{
					flag = CreateAbilityCard(item.GetAbilityCardYML, out newAbilityCard, out abilityCardUI);
				}
				catch (Exception ex2)
				{
					FailedLoading = true;
					Log.Exception(ex2, "An exception occurred while loading Persistent Data");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
					yield break;
				}
				if (!flag)
				{
					FailedLoading = true;
					Log.Error("Failed to create ability card ID: " + item.ID + " CardInstanceID: " + item.CardInstanceID);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
					yield break;
				}
				SceneController.Instance.LoadingScreenInstance.IncrementProgressBar(abilityProgressIncrement);
				if (enableCardCache)
				{
					PrefabLoaderCache.GenerateAbilityCardCache(item.GetAbilityCardYML);
				}
			}
			SceneController.Instance.LoadingScreenInstance.IncrementProgressBar(abilityProgressIncrement);
			yield return null;
		}
		bool flag2;
		try
		{
			flag2 = CreateAbilityCard(CharacterClassManager.LongRestLayout, out newAbilityCard, out abilityCardUI);
		}
		catch (Exception ex3)
		{
			FailedLoading = true;
			Log.Exception(ex3, "An exception occurred while loading Persistent Data");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex3.StackTrace, Application.Quit, ex3.Message);
			yield break;
		}
		if (flag2)
		{
			if ((bool)SceneController.Instance.LoadingScreenInstance)
			{
				SceneController.Instance.LoadingScreenInstance.IncrementProgressBar(abilityProgressIncrement);
			}
			if ((bool)SceneController.Instance.LoadingScreenInstance)
			{
				SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(75f);
			}
			createAbilityCards.Dispose();
			List<ItemCardYMLData> list2;
			try
			{
				list2 = ScenarioRuleClient.SRLYML.ItemCards.ToList();
			}
			catch (Exception ex4)
			{
				FailedLoading = true;
				Log.Exception(ex4, "An exception occurred while loading Persistent Data");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex4.StackTrace, Application.Quit, ex4.Message);
				yield break;
			}
			UnityTimingScope createItemCards = new UnityTimingScope("Create Item Cards");
			float itemProgressIncrement = 10f / ((float)list2.Count * 2f);
			foreach (ItemCardYMLData itemCardYML in list2)
			{
				bool flag3;
				try
				{
					flag3 = CreateItemCard1(itemCardYML, out newPetCard, out petCardUI);
				}
				catch (Exception ex5)
				{
					FailedLoading = true;
					Log.Exception(ex5, "An exception occurred while loading Persistent Data");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex5.StackTrace, Application.Quit, ex5.Message);
					yield break;
				}
				if (flag3)
				{
					if ((bool)SceneController.Instance.LoadingScreenInstance)
					{
						SceneController.Instance.LoadingScreenInstance.IncrementProgressBar(itemProgressIncrement);
					}
					yield return new WaitForEndOfFrame();
					try
					{
						CreateItemCard2(itemCardYML, newPetCard, petCardUI);
					}
					catch (Exception ex6)
					{
						FailedLoading = true;
						Log.Exception(ex6, "An exception occurred while loading Persistent Data");
						SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex6.StackTrace, Application.Quit, ex6.Message);
						yield break;
					}
					if ((bool)SceneController.Instance.LoadingScreenInstance)
					{
						SceneController.Instance.LoadingScreenInstance.IncrementProgressBar(itemProgressIncrement);
					}
					yield return null;
					continue;
				}
				FailedLoading = true;
				Log.Error("Failed to create item card " + itemCardYML.ID);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
				yield break;
			}
			createItemCards.Dispose();
			if (!SceneController.Instance.LoadingScreenInstance)
			{
				yield break;
			}
			SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(85f);
			List<ItemCardYMLData> list3;
			try
			{
				list3 = ScenarioRuleClient.SRLYML.PetCards.ToList();
			}
			catch (Exception ex7)
			{
				FailedLoading = true;
				Log.Exception(ex7, "An exception occurred while loading Persistent Data");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex7.StackTrace, Application.Quit, ex7.Message);
				yield break;
			}
			UnityTimingScope createPetCards = new UnityTimingScope("Create Item Cards");
			_ = 10f / ((float)list3.Count * 2f);
			foreach (ItemCardYMLData itemCardYML in list3)
			{
				bool flag4;
				try
				{
					flag4 = CreatePetCard1(itemCardYML, out newPetCard, out petCardUI);
				}
				catch (Exception ex8)
				{
					FailedLoading = true;
					Log.Exception(ex8, "An exception occurred while loading Persistent Data");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex8.StackTrace, Application.Quit, ex8.Message);
					yield break;
				}
				if (flag4)
				{
					SceneController.Instance.LoadingScreenInstance.IncrementProgressBar(itemProgressIncrement);
					yield return new WaitForEndOfFrame();
					try
					{
						CreateItemCard2(itemCardYML, newPetCard, petCardUI);
					}
					catch (Exception ex9)
					{
						FailedLoading = true;
						Log.Exception(ex9, "An exception occurred while loading Persistent Data");
						SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex9.StackTrace, Application.Quit, ex9.Message);
						yield break;
					}
					SceneController.Instance.LoadingScreenInstance.IncrementProgressBar(itemProgressIncrement);
					yield return null;
					continue;
				}
				FailedLoading = true;
				Log.Error("Failed to create item card " + itemCardYML.ID);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
				yield break;
			}
			createPetCards.Dispose();
			SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(95f);
		}
		else
		{
			FailedLoading = true;
			Log.Error("Failed to create Long Rest card");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
		}
	}

	private static IEnumerator CreateAudioMasterCrt()
	{
		if ((bool)SingletonMonoBehaviour<AudioController>.DoesInstanceExist())
		{
			yield break;
		}
		ByRef<GameObject> asset = new ByRef<GameObject>();
		yield return AssetBundleManager.Instance.LoadAssetFromBundleAsync("misc_globalsettings", "AudioMaster", "globalsettings", asset);
		if ((bool)asset.Value)
		{
			GameObject target = UnityEngine.Object.Instantiate(asset.Value);
			if (Application.isPlaying)
			{
				UnityEngine.Object.DontDestroyOnLoad(target);
			}
		}
		else
		{
			Log.Error("Failed to instantiate AudioMaster");
		}
	}

	private bool GenerateMonsterCard(int cardId, out GameObject monsterCardGO)
	{
		MonsterCardYMLData monsterCardYMLData = ScenarioRuleClient.SRLYML.MonsterCards.FirstOrDefault((MonsterCardYMLData mc) => mc.ID == cardId);
		if (monsterCardYMLData == null)
		{
			monsterCardGO = null;
			return false;
		}
		return CreateMonsterCard(monsterCardYMLData, out monsterCardGO, forceLayout: false, usePool: false);
	}

	public IEnumerator InitMonsterCards()
	{
		UnityTimingScope unityTimingScope = new UnityTimingScope("Create Monster Cards");
		try
		{
			FailedLoading = false;
			if (ObjectPool.instance != null && !ObjectPool.KeepMonsterCardsLoaded)
			{
				ObjectPool.ClearAllMonsterCards();
			}
			ObjectPool.SetCardGenerator(ObjectPool.ECardType.Monster, GenerateMonsterCard);
		}
		catch (Exception ex)
		{
			FailedLoading = true;
			Log.Exception(ex, "An exception occurred while loading Persistent Data");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
			FailedLoading = true;
			yield break;
		}
		unityTimingScope.Dispose();
	}

	public static bool CreateAbilityCardFromLayout(AbilityCardYMLData abilityCard, out GameObject newAbilityCard, out AbilityCardUI abilityCardUI, bool usePool = true)
	{
		return CreateAbilityCard(abilityCard, out newAbilityCard, out abilityCardUI, generateLayout: true, usePool);
	}

	public static bool CreateAbilityCard(AbilityCardYMLData abilityCard, out GameObject newAbilityCard, out AbilityCardUI abilityCardUI, bool generateLayout = false, bool usePool = true)
	{
		newAbilityCard = null;
		abilityCardUI = null;
		generateLayout = generateLayout || PlayerPrefs.GetInt("USE_LEGACY_ABILITY_CARDS") > 0;
		try
		{
			if (generateLayout)
			{
				GenerateAbilityCardLayout(abilityCard, out newAbilityCard, out abilityCardUI);
			}
			else if (abilityCard.ID == -1 || !InstantiateAbilityCardPrefab(abilityCard, out newAbilityCard, out abilityCardUI))
			{
				GenerateAbilityCardLayout(abilityCard, out newAbilityCard, out abilityCardUI);
			}
			if (usePool)
			{
				ObjectPool.CreateCardPool(abilityCard.ID, ObjectPool.ECardType.Ability, newAbilityCard, 1);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An error occurred creating the ability card " + abilityCard?.Name);
		}
		return true;
	}

	private static bool GenerateAbilityCardLayout(AbilityCardYMLData abilityCard, out GameObject newAbilityCard, out AbilityCardUI card)
	{
		newAbilityCard = null;
		card = null;
		try
		{
			if (abilityCard == null)
			{
				Log.Error("Ability card data is null!");
				return false;
			}
			string abilityCardPrefabInfo = GetAbilityCardPrefabInfo(abilityCard, out var playerClass, out var _);
			newAbilityCard = PrefabLoaderCache.MiscGui.Instantiate(abilityCardPrefabInfo);
			card = newAbilityCard.GetComponent<AbilityCardUI>();
			if (playerClass != null)
			{
				newAbilityCard.name = playerClass.AbilityCardArt + " " + ConvertCardIDToPaddedString(abilityCard.ID) + " - " + LocalizationManager.GetTranslation(abilityCard.Name) + "(Clone)";
			}
			if (abilityCard.ID == -1)
			{
				newAbilityCard.name = "All - Long Rest(Clone)";
			}
			card = newAbilityCard.GetComponent<AbilityCardUI>();
			card.transform.localScale = Vector3.one;
			card.MakeAbilityCard(abilityCard, alwaysShow: false, abilityCard.ID == -1);
			card.MakeAbilityCardContinued();
			if (playerClass != null)
			{
				card.EditorInit(playerClass);
			}
			return true;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An error occurred creating the ability card " + abilityCard?.Name);
			return false;
		}
	}

	public static string GetAbilityCardPrefabInfo(AbilityCardYMLData abilityCard, out CCharacterClass playerClass, out CharacterConfigUI config)
	{
		playerClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == abilityCard.CharacterID);
		config = null;
		string result;
		if (abilityCard.ID == -1)
		{
			result = "LongRest card";
		}
		else
		{
			result = ((abilityCard.ID >= 10000) ? "AbilityCard" : "AbilityCard-FH");
		}
		if (abilityCard.ID < 10000 && playerClass != null)
		{
			config = UIInfoTools.Instance.GetCharacterConfigUI(playerClass.AbilityCardArt, useDefault: true, playerClass.CharacterYML.CustomCharacterConfig);
			if ((bool)config)
			{
				result = config.customCardPrefab?.name ?? "AbilityCard-FH";
			}
		}
		return result;
	}

	public static bool InstantiateAbilityCardPrefab(AbilityCardYMLData abilityCard, out GameObject newAbilityCard, out AbilityCardUI card)
	{
		newAbilityCard = null;
		card = null;
		try
		{
			if (abilityCard == null)
			{
				Log.Error("Ability card data is null!");
				return false;
			}
			CCharacterClass playerClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == abilityCard.CharacterID);
			if (abilityCard.ID == -1)
			{
				newAbilityCard = PrefabLoaderCache.MiscGui.Instantiate("LongRest card");
				newAbilityCard.name = "All - Long Rest(Clone)";
			}
			else
			{
				GameObject abilityCardPrefab = GetAbilityCardPrefab(abilityCard, playerClass);
				if (abilityCardPrefab != null)
				{
					newAbilityCard = UnityEngine.Object.Instantiate(abilityCardPrefab);
				}
			}
			if (newAbilityCard != null)
			{
				card = newAbilityCard.GetComponent<AbilityCardUI>();
				card.transform.localScale = Vector3.one;
				card.Localize();
				card.MiniAbilityCard.MakeMiniCard(abilityCard.Name, abilityCard, abilityCard.ID == -1);
			}
			return card != null;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An error occurred creating the ability card " + abilityCard?.Name);
			return false;
		}
	}

	public static GameObject GetAbilityCardPrefab(AbilityCardYMLData abilityCard, CCharacterClass playerClass = null)
	{
		if (playerClass == null)
		{
			playerClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == abilityCard.CharacterID);
		}
		string text = "hero_" + playerClass.AbilityCardArt.ToLower(CultureInfo.InvariantCulture) + "cards";
		bool hasAlternativeCards = HasAlternativeCards;
		string fileName = abilityCard.Prefab ?? Path.GetFileNameWithoutExtension(abilityCard.FileName);
		GameObject gameObject = null;
		if (hasAlternativeCards)
		{
			string assetBundleName = text + "_alt";
			if (AssetBundleManager.Instance.IsAssetBundleLoaded(assetBundleName) || AssetBundleManager.Instance.LoadBundleManually(assetBundleName))
			{
				gameObject = AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>(assetBundleName, fileName, "cardPrefabs/" + playerClass.AbilityCardArt + "/AlternativeCards", "prefab", null, suppressError: true);
			}
		}
		if (gameObject == null && (AssetBundleManager.Instance.IsAssetBundleLoaded(text) || AssetBundleManager.Instance.LoadBundleManually(text)))
		{
			gameObject = AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>(text, fileName, "cardPrefabs/" + playerClass.AbilityCardArt, "prefab", null, suppressError: true);
		}
		return gameObject;
	}

	public static bool CreatePetCard1(ItemCardYMLData itemCard, out GameObject newItemCard, out ItemCardUI card)
	{
		newItemCard = null;
		card = null;
		try
		{
			if (itemCard == null)
			{
				Log.Error("Ability card data is null!");
				return false;
			}
			newItemCard = PrefabLoaderCache.MiscGui.Instantiate("ItemCard");
			card = newItemCard.GetComponent<ItemCardUI>();
			card.CardID = itemCard.ID;
			card.CardName = itemCard.Name;
			card.item = itemCard.GetPet;
			RectTransform rectTransform = card.layoutHolder.transform as RectTransform;
			card.layout = new CreateLayout(itemCard.CardLayout.ParentGroup, rectTransform.rect, itemCard.ID, isLongRest: false, inConsume: false, rectTransform, isItemCard: true);
			return true;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An error occurred creating the item card " + itemCard?.Name);
			return false;
		}
	}

	public static bool CreateItemCard1(ItemCardYMLData itemCard, out GameObject newItemCard, out ItemCardUI card)
	{
		newItemCard = null;
		card = null;
		try
		{
			if (itemCard == null)
			{
				Log.Error("Ability card data is null!");
				return false;
			}
			newItemCard = PrefabLoaderCache.MiscGui.Instantiate("ItemCard");
			card = newItemCard.GetComponent<ItemCardUI>();
			card.CardID = itemCard.ID;
			card.CardName = itemCard.Name;
			card.item = itemCard.GetItem;
			RectTransform rectTransform = card.layoutHolder.transform as RectTransform;
			card.layout = new CreateLayout(itemCard.CardLayout.ParentGroup, rectTransform.rect, itemCard.ID, isLongRest: false, inConsume: false, rectTransform, isItemCard: true);
			return true;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An error occurred creating the item card " + itemCard?.Name);
			return false;
		}
	}

	public static void CreateItemCard2(ItemCardYMLData itemCard, GameObject newItemCard, ItemCardUI card, bool usePool = true)
	{
		try
		{
			card.layout.GenerateFullLayout();
			card.CreateCard();
			if (usePool)
			{
				ObjectPool.CreateCardPool(card.item.ID, ObjectPool.ECardType.Item, newItemCard, 1);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An error occurred creating the item card " + itemCard?.Name);
		}
	}

	public static bool CreateItemCardLayout(int scenarioItemID, RectTransform container)
	{
		ItemCardYMLData itemCardYMLData = ScenarioRuleClient.SRLYML.ItemCards.FirstOrDefault((ItemCardYMLData item) => item.ID == scenarioItemID) ?? ScenarioRuleClient.SRLYML.PetCards.FirstOrDefault((ItemCardYMLData item) => item.ID == scenarioItemID);
		if (itemCardYMLData == null)
		{
			Log.GUIError($"Cannot build layout for item card {scenarioItemID}");
			return false;
		}
		if (itemCardYMLData.CardLayout?.ParentGroup == null)
		{
			return false;
		}
		Rect fullArea = new Rect(container.rect.position, new Vector2(container.rect.width, 400f));
		CreateLayout createLayout = new CreateLayout(new CardLayoutGroup(itemCardYMLData.CardLayout.ParentGroup, default), fullArea, scenarioItemID, isLongRest: false, inConsume: false, container, isItemCard: true);
		RectTransform rectTransform = createLayout.FullLayout.transform as RectTransform;
		if (rectTransform != null)
		{
			rectTransform.SetParent(container);
			rectTransform.localScale = Vector3.one;
			rectTransform.localPosition = Vector3.zero;
			TMP_Text[] componentsInChildren = rectTransform.GetComponentsInChildren<TMP_Text>();
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				componentsInChildren[num].fontSize = 20f;
			}
			float preferredHeight = createLayout.GenerateFullLayout();
			LayoutElement orAdd = rectTransform.GetOrAdd<LayoutElement>();
			orAdd.flexibleWidth = 1f;
			orAdd.preferredHeight = preferredHeight;
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			return true;
		}
		return false;
	}

	public static bool CreateMonsterCard(MonsterCardYMLData monsterCard, out GameObject monsterCardGO, bool forceLayout = false, bool usePool = true)
	{
		try
		{
			MonsterRoundCardUI monsterRoundCardUI = null;
			MonsterRoundCardUI monsterRoundCardUI2 = (forceLayout ? null : UIInfoTools.Instance.Monsters.GetMonsterCardPrefab(monsterCard.ID));
			if (monsterRoundCardUI2 == null)
			{
				Log.Error($"Monster card {monsterCard.ID} is not baked! Recreating from scratch, this may cause hiccups.");
				monsterRoundCardUI = PrefabLoaderCache.MiscGui.Instantiate("MonsterCards/FH_UI_HUD_CARD_BASE_MonsterCardFull").GetComponent<MonsterRoundCardUI>();
				monsterRoundCardUI.MakeCard(monsterCard);
			}
			else
			{
				monsterRoundCardUI = UnityEngine.Object.Instantiate(monsterRoundCardUI2);
				monsterRoundCardUI.Init(monsterCard);
			}
			monsterRoundCardUI.name = $"MonsterCard_{monsterCard.ID}";
			monsterRoundCardUI.transform.localScale = Vector3.one;
			if (usePool)
			{
				ObjectPool.CreateCardPool(monsterRoundCardUI.CardID, ObjectPool.ECardType.Monster, monsterRoundCardUI.gameObject, 2);
			}
			monsterCardGO = monsterRoundCardUI.gameObject;
			return true;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An error occurred creating the monster card " + monsterCard?.FileName);
		}
		monsterCardGO = null;
		return false;
	}

	private static string ConvertCardIDToPaddedString(int cardID)
	{
		string text = cardID.ToString();
		while (text.Length < 3)
		{
			text = "0" + text;
		}
		return text;
	}
}
