using System;
using System.Runtime.Serialization;

namespace Lib.Frida
{
	[Serializable]
	internal class NoProcessException : Exception
	{
		public NoProcessException()
		{
		}

		public NoProcessException(string message) : base(message)
		{
		}

		public NoProcessException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected NoProcessException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}