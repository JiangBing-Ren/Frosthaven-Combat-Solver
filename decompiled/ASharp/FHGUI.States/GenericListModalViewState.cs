using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Modules;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class GenericListModalViewState : FHMapViewState
{
	public class ListItem
	{
		public object Item;

		public string Text;

		public bool Enabled = true;

		public List<UIMultiplayerUser.Data> SuggestingUsers;
	}

	private string Title;

	private string Text;

	private Action<ListItem> OnResult;

	private bool BackgroundEnabled = true;

	private List<ListItem> Items;

	private bool IsSyncPoint;

	private string ConfirmButton;

	private string CancelButton;

	private ListItem SelectedItem;

	private int Columns;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	[Preserve]
	private GenericListModalModule ListModule => View.Modules.GenericModals.ListModal;

	public GenericListModalViewState(string text, IEnumerable<ListItem> items, Action<ListItem> onResult)
		: this(null, text, items, -1, null, null, onResult)
	{
	}

	public GenericListModalViewState(string title, string text, IEnumerable<ListItem> items, int selectedIndex, string confirmButton, string cancelButton, Action<ListItem> onResult, bool backgroundEnabled = false, int columns = 1)
	{
		Title = title;
		Text = text;
		ConfirmButton = confirmButton;
		CancelButton = cancelButton;
		OnResult = onResult;
		Items = items.ToList();
		SelectedItem = ((selectedIndex != -1 && Items.Count > selectedIndex) ? Items[selectedIndex] : null);
		BackgroundEnabled = backgroundEnabled;
		Columns = columns;
	}

	public override void OnShow()
	{
		Background.SetEnabled(BackgroundEnabled);
		ListModule.Init(new GenericListModalModule.Params
		{
			TitleLocalised = Title,
			DescriptionLocalised = Text,
			Items = Items,
			SelectedItem = SelectedItem,
			ConfirmButton = ConfirmButton,
			CancelButton = CancelButton,
			SuggestingUsers = (IsSyncPoint ? View.TryGetReadyUsersList() : null),
			OnItemSelected = OnItemsSelected,
			OnResult = HandleConfirmAction,
			Columns = Columns
		});
	}

	private void HandleConfirmAction(bool confirm)
	{
		PopThisState();
		OnResult?.Invoke(confirm ? SelectedItem : null);
	}

	private void OnItemsSelected(ListItem item)
	{
		if (ConfirmButton == null)
		{
			PopThisState();
			OnResult?.Invoke(item);
		}
		else
		{
			SelectedItem = item;
			View.RefreshView();
		}
	}
}
