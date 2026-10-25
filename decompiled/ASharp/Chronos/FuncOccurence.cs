namespace Chronos;

internal sealed class FuncOccurence<T> : Occurrence
{
	private ForwardFunc<T> forward { get; set; }

	private BackwardFunc<T> backward { get; set; }

	private T transfer { get; set; }

	public FuncOccurence(ForwardFunc<T> forward, BackwardFunc<T> backward)
	{
		this.forward = forward;
		this.backward = backward;
	}

	public override void Forward()
	{
		transfer = forward();
	}

	public override void Backward()
	{
		backward(transfer);
	}
}
