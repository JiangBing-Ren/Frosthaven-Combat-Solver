using System.Collections.Generic;

public interface IOptionHolder
{
	List<IOption> SelectedOptions { get; set; }

	void ClearSelection();

	void OnHoveredOption(IOption option, bool hovered);
}
