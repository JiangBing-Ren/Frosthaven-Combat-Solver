using System;
using System.IO;
using System.Text;

public static class UnitySystemConsoleRedirector
{
	private class UnityTextWriter : TextWriter
	{
		private StringBuilder buffer = new StringBuilder();

		public override Encoding Encoding => Encoding.Default;

		public override void Flush()
		{
			Log.Info(buffer.ToString());
			buffer.Length = 0;
		}

		public override void Write(string value)
		{
			buffer.Append(value);
			if (value != null)
			{
				int length = value.Length;
				if (length > 0 && value[length - 1] == '\n')
				{
					Flush();
				}
			}
		}

		public override void Write(char value)
		{
			buffer.Append(value);
			if (value == '\n')
			{
				Flush();
			}
		}

		public override void Write(char[] value, int index, int count)
		{
			Write(new string(value, index, count));
		}
	}

	private static TextWriter s_OriginalWriter;

	public static void Redirect()
	{
		s_OriginalWriter = Console.Out;
		Console.SetOut(new UnityTextWriter());
	}

	public static void ClearRedirect()
	{
		if (s_OriginalWriter != null)
		{
			Console.SetOut(s_OriginalWriter);
		}
	}
}
