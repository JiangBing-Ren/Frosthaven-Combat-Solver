using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SandboxUI : MonoBehaviour
{
	public GameObject ResumeButton;

	public GameObject ResumeButtonDummy;

	private void OnEnable()
	{
		ResumeButton.SetActive(value: false);
		ResumeButtonDummy.SetActive(value: true);
		StartCoroutine(LoadResumeSingleScenarioCrt());
	}

	public void ResumeSandbox()
	{
		SaveData.Instance.Global.SetResumeSingleScenario(SaveData.Instance.Global.AllSingleScenarios.FirstOrDefault((PartyAdventureData s) => s.PartyName == "SandboxScenario"));
		SaveData.Instance.Global.CurrentHostAccountID = PlatformLayer.UserData.PlatformAccountID;
		SceneController.Instance.ResumeSingleScenario();
	}

	public IEnumerator LoadResumeSingleScenarioCrt()
	{
		Task<List<PartyAdventureData>> task = SaveData.Instance.Global.LoadSavesFromDisk();
		yield return new WaitForTask(task);
		bool flag = SaveData.Instance.Global.AllSingleScenarios.Exists((PartyAdventureData s) => s.PartyName == "SandboxScenario" && s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID);
		ResumeButton.SetActive(flag);
		ResumeButtonDummy.SetActive(!flag);
	}
}
