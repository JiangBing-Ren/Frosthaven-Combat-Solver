using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.X509.Store;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Cms;

public class RecipientID : X509CertStoreSelector
{
	private byte[] keyIdentifier;

	public byte[] KeyIdentifier
	{
		get
		{
			return Arrays.Clone(keyIdentifier);
		}
		set
		{
			keyIdentifier = Arrays.Clone(value);
		}
	}

	public override int GetHashCode()
	{
		int num = Arrays.GetHashCode(keyIdentifier) ^ Arrays.GetHashCode(SubjectKeyIdentifier);
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
			return true;
		}
		if (!(obj is RecipientID recipientID))
		{
			return false;
		}
		if (Arrays.AreEqual(keyIdentifier, recipientID.keyIdentifier) && Arrays.AreEqual(SubjectKeyIdentifier, recipientID.SubjectKeyIdentifier) && object.Equals(SerialNumber, recipientID.SerialNumber))
		{
			return X509CertStoreSelector.IssuersMatch(Issuer, recipientID.Issuer);
		}
		return false;
	}
}
