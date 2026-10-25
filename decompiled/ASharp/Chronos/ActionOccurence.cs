namespace Chronos;

internal sealed class ActionOccurence : Occurrence
{
	private ForwardAction forward { get; set; }

	private BackwardAction backward { get; set; }

	public ActionOccurence(ForwardAction forward, BackwardAction backward)
	{
		this.forward = forward;
		this.backward = backward;
	}

	public override void Forward()
	{
		forward();
	}

	public override void Backward()
	{
		backward();
	}
}
