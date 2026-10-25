namespace FFSNet;

public class CreatingCharacter
{
	public int PlayerID { get; private set; }

	public string CharacterName { get; private set; }

	public CreatingCharacter(int playerID, string characterName)
	{
		PlayerID = playerID;
		CharacterName = characterName;
	}
}
