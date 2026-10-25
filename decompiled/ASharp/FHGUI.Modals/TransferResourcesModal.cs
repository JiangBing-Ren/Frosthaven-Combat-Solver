using System;
using System.Linq;
using FHGUI.Elements;
using FHRL;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modals;

public class TransferResourcesModal : MonoBehaviour
{
	public delegate void TransferResourcesModalSetup(out CharacterClassDef character, out int characterGold, out OutpostResources characterResources, out int outpostGold, out OutpostResources outpostResources);

	public delegate void TransferResourcesCallback(int gold, OutpostResources resources);

	[SerializeField]
	private UIResourceSlider _gold;

	[Space]
	[SerializeField]
	private UIResourceSlider _lumber;

	[SerializeField]
	private UIResourceSlider _metal;

	[SerializeField]
	private UIResourceSlider _hide;

	[Space]
	[SerializeField]
	private UIResourceSlider _arrowvine;

	[SerializeField]
	private UIResourceSlider _axenut;

	[SerializeField]
	private UIResourceSlider _corpsecap;

	[SerializeField]
	private UIResourceSlider _flamefruit;

	[SerializeField]
	private UIResourceSlider _rockroot;

	[SerializeField]
	private UIResourceSlider _snowthistle;

	[Space]
	[SerializeField]
	private Image _characterPortrait;

	[Space]
	[SerializeField]
	private FHButton _dismiss;

	[SerializeField]
	private FHButton _confirm;

	private TransferResourcesCallback _onConfirm;

	private Action _onClose;

	private UIResourceSlider[] _resourceSliders;

	private int _playerGold;

	private int _outpostGold;

	private OutpostResources _playerResources;

	private OutpostResources _outpostResources;

	private void Awake()
	{
		_dismiss.onClick.AddListener(OnDismiss);
		_confirm.onClick.AddListener(OnConfirm);
		_resourceSliders = GetComponentsInChildren<UIResourceSlider>();
	}

	private void OnDisable()
	{
		_onConfirm = null;
		_onClose = null;
	}

	public void Show(TransferResourcesModalSetup setup, bool forceRefresh = true, TransferResourcesCallback onConfirm = null, Action onClose = null)
	{
		setup(out var character, out var characterGold, out var characterResources, out var outpostGold, out var outpostResources);
		Show(character, characterGold, characterResources, outpostGold, outpostResources, forceRefresh, onConfirm, onClose);
	}

	public void Show(CharacterClassDef character, int gold, OutpostResources characterResources, int outpostGold, OutpostResources outpostResources, bool forceRefresh = true, TransferResourcesCallback onConfirm = null, Action onClose = null)
	{
		gameObject.SetActive(value: true);
		_onConfirm = onConfirm;
		_onClose = null;
		_confirm.interactable = false;
		_gold.Init(gold, outpostGold, null, forceRefresh);
		_lumber.Init(characterResources.Lumber, outpostResources.Lumber, null, forceRefresh);
		_metal.Init(characterResources.Metal, outpostResources.Metal, null, forceRefresh);
		_hide.Init(characterResources.Hide, outpostResources.Hide, null, forceRefresh);
		_arrowvine.Init(characterResources.Arrowvine, outpostResources.Arrowvine, null, forceRefresh);
		_axenut.Init(characterResources.Axenut, outpostResources.Axenut, null, forceRefresh);
		_corpsecap.Init(characterResources.Corpsecap, outpostResources.Corpsecap, null, forceRefresh);
		_flamefruit.Init(characterResources.Flamefruit, outpostResources.Flamefruit, null, forceRefresh);
		_rockroot.Init(characterResources.Rockroot, outpostResources.Rockroot, null, forceRefresh);
		_snowthistle.Init(characterResources.Snowthistle, outpostResources.Snowthistle, null, forceRefresh);
		ECharacter eCharacter = CrossReference.ClassIdToECharacterModel(character.ID.ID);
		Sprite characterHeroPortrait = UIInfoTools.Instance.GetCharacterHeroPortrait(eCharacter.ToString());
		_characterPortrait.sprite = characterHeroPortrait;
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	private void OnDismiss()
	{
		Hide();
	}

	private void Update()
	{
		if (_resourceSliders.Any((UIResourceSlider s) => s.CurrentVal != 0))
		{
			_confirm.interactable = true;
		}
	}

	private void OnConfirm()
	{
		if (_onConfirm != null)
		{
			_onConfirm(_gold.CurrentVal, new OutpostResources
			{
				Lumber = _lumber.CurrentVal,
				Metal = _metal.CurrentVal,
				Hide = _hide.CurrentVal,
				Arrowvine = _arrowvine.CurrentVal,
				Axenut = _axenut.CurrentVal,
				Corpsecap = _corpsecap.CurrentVal,
				Flamefruit = _flamefruit.CurrentVal,
				Rockroot = _rockroot.CurrentVal,
				Snowthistle = _snowthistle.CurrentVal
			});
		}
	}
}
