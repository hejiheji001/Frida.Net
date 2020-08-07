using System;
using System.Runtime.Serialization;

namespace Lib.Frida
{
	[Serializable]
	internal class ServerFailException : Exception
	{
		public ServerFailException()
		{
		}

		public ServerFailException(string message) : base(message)
		{
		}

		public ServerFailException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ServerFailException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}