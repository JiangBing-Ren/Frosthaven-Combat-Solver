using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Base.Utils.Web;
using Ionic.Zip;
using Libs.Utils;
using SETUtil;

namespace ReportIssueGui;

public class IssueUploader
{
	public struct UploadFile
	{
		public string Name;

		public string GroupName;

		public string ContentType;

		public byte[] Data;

		public string Directory;

		public bool SuffixIssueId;

		public int UploadPriority;

		public bool DeleteWhenDone;
	}

	public delegate void UploadStatusChangedCallback(string message, Exception exception, bool finished, object context);

	private const long AttachmentSizeToZip = 20971520L;

	private Thread _uploadThread;

	public event UploadStatusChangedCallback OnUploadStatusChanged;

	public void CreateIssueAsync(string jiraUser, string jiraKey, string project, string issueType, IDictionary<string, object> fields, string tempFolder, IList<UploadFile> filesToUpload)
	{
		if (_uploadThread != null)
		{
			throw new InvalidOperationException("Upload already started");
		}
		_uploadThread = new Thread(() =>
		{
			CreateIssueThread(jiraUser, jiraKey, project, issueType, fields, tempFolder, filesToUpload);
		});
		_uploadThread.Start();
	}

	private void CreateIssueThread(string jiraUser, string jiraKey, string project, string issueType, IDictionary<string, object> fields, string tempFolder, IList<UploadFile> filesToUpload)
	{
		try
		{
			string text = JiraConstants.JiraAuthOverride;
			if (string.IsNullOrEmpty(text))
			{
				if (jiraKey == null)
				{
					using TrustyWebClientWithTimeout trustyWebClientWithTimeout = new TrustyWebClientWithTimeout
					{
						TimeoutMilliseconds = 10000
					};
					jiraKey = AesEncryptor.DecryptString(trustyWebClientWithTimeout.DownloadString(JiraConstants.JiraKeyCustomerUrl));
				}
				text = JiraConstants.CreateJiraAuth(jiraUser, jiraKey);
			}
			JiraWebApi jiraWebApi = new JiraWebApi(text, tempFolder);
			OnUploadStatusChanged?.Invoke("Creating ticket...", null, finished: false, null);
			string text2 = jiraWebApi.CreateIssue(project, issueType, fields);
			if (filesToUpload != null)
			{
				IEnumerable<IGrouping<string, UploadFile>> enumerable = from f in filesToUpload
					group f by f.GroupName;
				List<UploadFile> list = new List<UploadFile>();
				foreach (IGrouping<string, UploadFile> item2 in enumerable)
				{
					if (item2.Key == null)
					{
						foreach (UploadFile item3 in item2)
						{
							UploadFile uf = TagFileNameWithIssueIdIfNeeded(item3, text2);
							uf = ZipDataIfNeeded(uf);
							list.Add(uf);
						}
					}
					else
					{
						UploadFile item = CreateZipFileForGroup(item2.Key, item2.ToList(), text2);
						list.Add(item);
					}
				}
				foreach (UploadFile item4 in list.OrderByDescending((UploadFile uploadFile) => uploadFile.UploadPriority))
				{
					try
					{
						OnUploadStatusChanged?.Invoke("Uploading file: " + item4.Name + "...", null, finished: false, null);
						jiraWebApi.AttachFileToIssue(text2, item4.Name, item4.ContentType, item4.Data);
					}
					catch (Exception ex)
					{
						IList<string> errorsFromException = JiraWebApi.GetErrorsFromException(ex);
						string text3 = (errorsFromException.Any() ? string.Join(" ", errorsFromException) : ex.Message);
						OnUploadStatusChanged?.Invoke("Upload error: " + text3 + "...", ex, finished: false, null);
						Thread.Sleep(1500);
					}
				}
				foreach (UploadFile item5 in filesToUpload)
				{
					if (item5.DeleteWhenDone)
					{
						FileUtil.TryDeleteAllContents(item5.Directory);
					}
				}
			}
			OnUploadStatusChanged?.Invoke("Upload complete", null, finished: true, text2);
		}
		catch (Exception ex2)
		{
			IList<string> errorsFromException2 = JiraWebApi.GetErrorsFromException(ex2);
			string text4 = (errorsFromException2.Any() ? string.Join(" ", errorsFromException2) : ex2.Message);
			OnUploadStatusChanged?.Invoke("Upload error: " + text4, ex2, finished: true, null);
		}
	}

	private UploadFile CreateZipFileForGroup(string groupName, List<UploadFile> files, string issueId)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (ZipFile zipFile = new ZipFile(Encoding.UTF8))
		{
			foreach (UploadFile file in files)
			{
				zipFile.AddEntry(file.Name, file.Data);
			}
			zipFile.Save(memoryStream);
		}
		UploadFile uf = new UploadFile
		{
			Data = memoryStream.ToArray(),
			Name = groupName + ".zip",
			ContentType = "application/zip",
			SuffixIssueId = true,
			UploadPriority = (int)files.Average((UploadFile f) => f.UploadPriority)
		};
		return TagFileNameWithIssueIdIfNeeded(uf, issueId);
	}

	private UploadFile TagFileNameWithIssueIdIfNeeded(UploadFile uf, string issueId)
	{
		if (uf.SuffixIssueId)
		{
			uf.Name = Path.GetFileNameWithoutExtension(uf.Name) + "_" + issueId + Path.GetExtension(uf.Name);
		}
		return uf;
	}

	private UploadFile ZipDataIfNeeded(UploadFile uf)
	{
		if (uf.Directory != null || (long)uf.Data.Length > 20971520L)
		{
			MemoryStream memoryStream = new MemoryStream();
			using (ZipFile zipFile = new ZipFile(Encoding.UTF8))
			{
				if (uf.Directory != null)
				{
					zipFile.AddDirectory(uf.Directory);
				}
				else
				{
					zipFile.AddEntry(uf.Name, uf.Data);
				}
				zipFile.Save(memoryStream);
			}
			uf.Data = memoryStream.ToArray();
			uf.Name += ".zip";
			uf.ContentType = "application/zip";
		}
		return uf;
	}
}
