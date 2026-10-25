namespace ScenarioRuleLibrary;

public class CTriggerCameraShake_MessageData(CActor actorSpawningMessage) : CMessageData(MessageType.CameraShake, actorSpawningMessage, string.Empty)
{
	public float duration = 2f;

	public float magnitude = 0.5f;

	public float speed = 100f;

	public float distanceForce = 100f;

	public float rotationDamper = 2f;
}
