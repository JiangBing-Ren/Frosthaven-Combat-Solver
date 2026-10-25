using System.Collections.Generic;

namespace FH.YML;

public interface IGenericYMLRepo
{
	IEnumerable<object> GetAllItems();
}
