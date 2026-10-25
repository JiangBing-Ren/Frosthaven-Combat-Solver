namespace FHGUI.Elements;

public interface IUIElementStateListener
{
	void StateChanged(string state);

	void ResetState();
}
