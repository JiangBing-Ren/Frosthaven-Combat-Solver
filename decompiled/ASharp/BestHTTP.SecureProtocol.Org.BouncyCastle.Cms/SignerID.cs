using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.X509.Store;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Cms;

public class SignerID : X509CertStoreSelector
{
	public override int GetHashCode()
	{
		int num = Arrays.GetHashCode(SubjectKeyIdentifier);
		BigInteger bigInteger = SerialNumber;
		if (bigInteger != null)
		{
			num ^= bigInteger.GetHashCode();
		}
		X509Name x509Name = Issuer;
		if (x509Name != null)
		{
			num ^= x509Name.GetHashCode();
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return false;
		}
		if (!(obj is SignerID signerID))
		{
			return false;
		}
		if (Arrays.AreEqual(SubjectKeyIdentifier, signerID.SubjectKeyIdentifier) && object.Equals(SerialNumber, signerID.SerialNumber))
		{
			return X509CertStoreSelector.IssuersMatch(Issuer, signerID.Issuer);
		}
		return false;
	}
}
