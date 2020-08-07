using System;

namespace Lib.Frida
{
	internal class NoServerException : Exception
	{
		public NoServerException(string message) : base(message)
		{
		}
	}
}