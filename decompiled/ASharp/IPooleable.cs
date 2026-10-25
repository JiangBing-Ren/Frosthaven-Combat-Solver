public interface IPooleable
{
	void OnReturnedToPool();

	void OnRemovedFromPool();
}
