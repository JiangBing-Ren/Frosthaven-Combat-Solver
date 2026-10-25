using FHRL;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class ResourceUIController : MonoBehaviour
{
	public TMP_Text TxtLumber;

	public TMP_Text TxtMetal;

	public TMP_Text TxtHide;

	public TMP_Text TxtArrowvine;

	public TMP_Text TxtAxenut;

	public TMP_Text TxtCorpsecap;

	public TMP_Text TxtFlamefruit;

	public TMP_Text TxtRockroot;

	public TMP_Text TxtSnowthistle;

	public void Init(OutpostResources r)
	{
		TxtLumber.text = r.Lumber.ToString();
		TxtMetal.text = r.Metal.ToString();
		TxtHide.text = r.Hide.ToString();
		TxtArrowvine.text = r.Arrowvine.ToString();
		TxtAxenut.text = r.Axenut.ToString();
		TxtCorpsecap.text = r.Corpsecap.ToString();
		TxtFlamefruit.text = r.Flamefruit.ToString();
		TxtRockroot.text = r.Rockroot.ToString();
		TxtSnowthistle.text = r.Snowthistle.ToString();
	}
}
