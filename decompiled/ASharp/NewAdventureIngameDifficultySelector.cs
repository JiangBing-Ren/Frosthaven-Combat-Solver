using UnityEngine;

public class NewAdventureIngameDifficultySelector : MonoBehaviour
{
	[SerializeField]
	private UIDifficultySelector m_DifficultySelector;

	private void Awake()
	{
		m_DifficultySelector.DifficultySelected += OnDifficultySelected;
	}

	private void OnDifficultySelected(object sender, UIDifficultySelector.DifficultySelectedEventArgs eventArgs)
	{
		SaveData.Instance.Global.CurrentAdventureData.ChangeDifficulty(eventArgs.SelectedDifficulty);
	}

	private void OnDisable()
	{
		m_DifficultySelector.ClearSelection();
	}

	private void OnDestroy()
	{
		m_DifficultySelector.DifficultySelected -= OnDifficultySelected;
	}
}
