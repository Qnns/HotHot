using System.Text;

public static class ByteHelper
{
	public static string ToHex(this byte b)
	{
		return b.ToString("X2");
	}

	public static string ToHex(this byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in bytes)
		{
			stringBuilder.Append(b.ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	public static string ToHex(this byte[] bytes, string format)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in bytes)
		{
			stringBuilder.Append(b.ToString(format));
		}
		return stringBuilder.ToString();
	}

	public static string Utf8ToStr(this byte[] bytes)
	{
		return Encoding.UTF8.GetString(bytes);
	}

	public static byte[] StrToUtf8(this string str)
	{
		return Encoding.UTF8.GetBytes(str);
	}
}