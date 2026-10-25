using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Base.Utils.Web;

public class TrustyWebClientWithTimeout : WebClient
{
	public int TimeoutMilliseconds = 10000;

	protected override WebRequest GetWebRequest(Uri address)
	{
		if (!(base.GetWebRequest(address) is HttpWebRequest httpWebRequest))
		{
			return null;
		}
		httpWebRequest.Timeout = TimeoutMilliseconds;
		httpWebRequest.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(httpWebRequest.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) => true));
		return httpWebRequest;
	}
}
