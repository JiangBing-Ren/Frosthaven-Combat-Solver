namespace ScenarioRuleLibrary;

public class CDifficultTerrainRemoved_MessageData : CMessageData
{
	public CObjectDifficultTerrain m_DifficultTerrainRemoved;

	public CDifficultTerrainRemoved_MessageData()
		: base(MessageType.RemoveDifficultTerrain, null)
	{
	}
}
