using System;
using System.IO;

namespace BethBryo_for_Unity
{
	/// <summary>
	/// Defines the input variables required for all the FileStream caching methods to work.
	/// </summary>
	/// <param name="_bytesParams._fileStream">The FileStream you created for the file you want to read.</param>
	/// <param name="_bytesParams._streamCache">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
	/// <param name="_bytesParams._cacheCurrentPos">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
	/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
	internal struct BytesParams
	{
		/// <summary>The FileStream you created for the file you want to read.</summary>
		public FileStream FileStream;
		/// <summary>An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</summary>
		public byte[] StreamCache;
		/// <summary>A signed integer that will persist for the life of the FileStream.</param>
		public int CacheCurrentPos;
	}

	/// <summary>
	/// Provides a number of methods that cache a FileStream in 4KB chunks and then converts that cached data into different variable types.
	/// </summary>
	internal static class BytesToTypes
	{
		/// <summary>
		/// Used to initialise the FileStream cache and re-fill it periodically.
		/// You should call this function once to initialise the class before calling any of the other functions for the first time.
		/// Initialisation is done by calling this function like so: RefillBytesArray({FileStream}, ref {byte[]}, ref {int} = 4096)
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <returns>Returns a signed integer with the number of bytes that were read from the FileStream.</returns>
		internal static int RefillBytesArray(ref BytesParams BytesParams)
		{
			BytesParams.FileStream.Position -= 4096 - BytesParams.CacheCurrentPos;
			BytesParams.CacheCurrentPos = 0;
			return BytesParams.FileStream.Read(BytesParams.StreamCache, 0, 4096);
		}

		/// <summary>
		/// Reads a single byte from the byte array cache and converts it into an unsigned byte/8-bit integer.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <returns>Returns an unsigned 8-bit integer read from the byte array.</returns>
		internal static byte BytesToSingleByte8(ref BytesParams BytesParams, out int NumBytesRead)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4095) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			byte _returnVal = BytesParams.StreamCache[BytesParams.CacheCurrentPos];
			BytesParams.CacheCurrentPos += 1;
			return _returnVal;
		}

		/// <summary>
		/// Reads a single byte from the byte array cache and converts it into a signed byte/8-bit integer.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <returns>Returns a signed 8-bit integer read from the byte array.</returns>
		internal static sbyte BytesToSingleSByte8(ref BytesParams BytesParams, out int NumBytesRead)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4095) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			sbyte _returnVal = unchecked((sbyte)BytesParams.StreamCache[BytesParams.CacheCurrentPos]);
			BytesParams.CacheCurrentPos += 1;
			return _returnVal;
		}

		/// <summary>
		/// Reads two bytes from the byte array cache and converts it into a signed short/16-bit integer.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="ReverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a signed 16-bit integer read from the byte array.</returns>
		internal static short BytesToShort16(ref BytesParams BytesParams, out int NumBytesRead, bool ReverseEndianess = false)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4094) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			short _returnVal;
			switch (ReverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToInt16(new byte[2] { BytesParams.StreamCache[BytesParams.CacheCurrentPos + 1], 
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToInt16(BytesParams.StreamCache, BytesParams.CacheCurrentPos);
					break;
			}
			BytesParams.CacheCurrentPos += 2;
			return _returnVal;
		}

		/// <summary>
		/// Reads two bytes from the byte array cache and converts it into an unsigned short/16-bit integer.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="ReverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns an unsigned 16-bit integer read from the byte array.</returns>
		internal static ushort BytesToUShort16(ref BytesParams BytesParams, out int NumBytesRead, bool ReverseEndianess = false)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4094) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			ushort _returnVal;
			switch (ReverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToUInt16(new byte[2] { BytesParams.StreamCache[BytesParams.CacheCurrentPos + 1], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToUInt16(BytesParams.StreamCache, BytesParams.CacheCurrentPos);
					break;
			}
			BytesParams.CacheCurrentPos += 2;
			return _returnVal;
		}

		/// <summary>
		/// Reads two bytes from the byte array cache and converts it into a Unicode char.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="ReverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a Unicode char read from the byte array.</returns>
		internal static char BytesToChar(ref BytesParams BytesParams, out int NumBytesRead, bool ReverseEndianess = false)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4094) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			char _returnVal;
			switch (ReverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToChar(new byte[2] { BytesParams.StreamCache[BytesParams.CacheCurrentPos + 1], 
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos] }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToChar(BytesParams.StreamCache, BytesParams.CacheCurrentPos);
					break;
			}
			BytesParams.CacheCurrentPos += 2;
			return _returnVal;
		}

		/// <summary>
		/// Reads four bytes from the byte array cache and converts it into a signed int/32-bit integer.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="ReverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a signed 32-bit integer read from the byte array.</returns>
		internal static int BytesToInt32(ref BytesParams BytesParams, out int NumBytesRead, bool ReverseEndianess = false)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4092) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			int _returnVal;
			switch (ReverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToInt32(new byte[4] { BytesParams.StreamCache[BytesParams.CacheCurrentPos + 3], 
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos + 2],
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos + 1], 
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToInt32(BytesParams.StreamCache, BytesParams.CacheCurrentPos);
					break;
			}
			BytesParams.CacheCurrentPos += 4;
			return _returnVal;
		}

		/// <summary>
		/// Reads four bytes from the byte array cache and converts it into an unsigned int/32-bit integer.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="ReverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns an unsigned 32-bit integer read from the byte array.</returns>
		internal static uint BytesToUInt32(ref BytesParams BytesParams, out int NumBytesRead, bool ReverseEndianess = false)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4088) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			uint _returnVal;
			switch (ReverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToUInt32(new byte[4] { BytesParams.StreamCache[BytesParams.CacheCurrentPos + 3], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 2],
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 1], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToUInt32(BytesParams.StreamCache, BytesParams.CacheCurrentPos);
					break;
			}
			BytesParams.CacheCurrentPos += 4;
			return _returnVal;
		}

		/// <summary>
		/// Reads four bytes from the byte array cache and converts it into a single precision float.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="ReverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a single precision float read from the byte array.</returns>
		internal static float BytesToSingleFloat(ref BytesParams BytesParams, out int NumBytesRead, bool ReverseEndianess = false)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4088) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			float _returnVal;
			switch (ReverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToSingle(new byte[4] { BytesParams.StreamCache[BytesParams.CacheCurrentPos + 3], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 2],
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 1], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToSingle(BytesParams.StreamCache, BytesParams.CacheCurrentPos);
					break;
			}
			BytesParams.CacheCurrentPos += 4;
			return _returnVal;
		}

		/// <summary>
		/// Reads eight bytes from the byte array cache and converts it into a signed long/64-bit integer.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="ReverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a signed 64-bit integer read from the byte array.</returns>
		internal static long BytesToLong64(ref BytesParams BytesParams, out int NumBytesRead, bool ReverseEndianess = false)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4088) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			long _returnVal;
			switch (ReverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToInt64(new byte[8] { BytesParams.StreamCache[BytesParams.CacheCurrentPos + 7], 
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos + 6],
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos + 5], 
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos + 4],
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos + 3], 
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos + 2],
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos + 1], 
																	BytesParams.StreamCache[BytesParams.CacheCurrentPos ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToInt64(BytesParams.StreamCache, BytesParams.CacheCurrentPos);
					break;
			}
			BytesParams.CacheCurrentPos += 8;
			return _returnVal;
		}

		/// <summary>
		/// Reads eight bytes from the byte array cache and converts it into an unsigned long/64-bit integer.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="ReverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns an unsigned 64-bit integer read from the byte array.</returns>
		internal static ulong BytesToULong64(ref BytesParams BytesParams, out int NumBytesRead, bool ReverseEndianess = false)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4088) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			ulong _returnVal;
			switch (ReverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToUInt64(new byte[8] { BytesParams.StreamCache[BytesParams.CacheCurrentPos + 7], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 6],
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 5], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 4],
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 3], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 2],
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 1], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToUInt64(BytesParams.StreamCache, BytesParams.CacheCurrentPos);
					break;
			}
			BytesParams.CacheCurrentPos += 8;
			return _returnVal;
		}

		/// <summary>
		/// Reads eight bytes from the byte array cache and converts it into a double precision float.
		/// </summary>
		/// <param name="BytesParams">A BytesParams Struct filled with the data required by all the FileStream caching methods.</param>
		/// <param name="NumBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="ReverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a double precision float read from the byte array.</returns>
		internal static double BytesToDoubleFloat(ref BytesParams BytesParams, out int NumBytesRead, bool ReverseEndianess = false)
		{
			NumBytesRead = -1;
			if ((BytesParams.CacheCurrentPos >= 4088) || (BytesParams.CacheCurrentPos <= -1))
				NumBytesRead = RefillBytesArray(ref BytesParams);

			double _returnVal;
			switch (ReverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToDouble(new byte[8] { BytesParams.StreamCache[BytesParams.CacheCurrentPos + 7], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 6],
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 5], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 4],
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 3], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 2],
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos + 1], 
																	 BytesParams.StreamCache[BytesParams.CacheCurrentPos ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToDouble(BytesParams.StreamCache, BytesParams.CacheCurrentPos);
					break;
			}
			BytesParams.CacheCurrentPos += 8;
			return _returnVal;
		}
	}
}
