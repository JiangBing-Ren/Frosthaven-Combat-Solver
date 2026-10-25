public interface IRequirementCheckResult
{
	bool IsUnlocked();

	bool IsOnlyMissingCharacters();

	string ToString(string format);
}
