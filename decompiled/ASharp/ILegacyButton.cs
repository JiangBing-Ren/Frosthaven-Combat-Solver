public interface ILegacyButton
{
	bool IsActiveAndEnabled { get; }

	bool IsInteractable { get; }

	string Text { get; set; }

	void OnClick();
}
