using System;
using System.IO;

namespace BethBryo_for_Unity
{
	/// <summary>
	/// Provides a number of methods that cache a FileStream in 4KB chunks and then converts that cached data into different variable types.
	/// </summary>
	public static class BytesToTypes
	{
		/// <summary>
		/// Used to initialise the FileStream cache and re-fill it periodically.
		/// You should call this function once to initialise the class before calling any of the other functions for the first time.
		/// Initialisation is done by calling this function like so: RefillBytesArray({FileStream}, ref {byte[]}, ref {int} = 4096)
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <returns>Returns a signed integer with the number of bytes that were read from the FileStream.</returns>
		public static int RefillBytesArray(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex)
		{
			_fileStream.Position -= 4096 - _currentArrayIndex;
			_currentArrayIndex = 0;
			return _fileStream.Read(_byteArray, 0, 4096);
		}

		/// <summary>
		/// Reads a single byte from the byte array cache and converts it into an unsigned byte/8-bit integer.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <returns>Returns an unsigned 8-bit integer read from the byte array.</returns>
		public static byte BytesToSingleByte8(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4095) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			byte _returnVal = _byteArray[_currentArrayIndex];
			_currentArrayIndex += 1;
			return _returnVal;
		}

		/// <summary>
		/// Reads a single byte from the byte array cache and converts it into a signed byte/8-bit integer.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <returns>Returns a signed 8-bit integer read from the byte array.</returns>
		public static sbyte BytesToSingleSByte8(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4095) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			sbyte _returnVal = unchecked((sbyte)_byteArray[_currentArrayIndex]);
			_currentArrayIndex += 1;
			return _returnVal;
		}

		/// <summary>
		/// Reads two bytes from the byte array cache and converts it into a signed short/16-bit integer.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="_reverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a signed 16-bit integer read from the byte array.</returns>
		public static short BytesToShort16(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead, bool _reverseEndianess = false)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4094) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			short _returnVal;
			switch (_reverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToInt16(new byte[2] { _byteArray[_currentArrayIndex + 1], _byteArray[_currentArrayIndex ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToInt16(_byteArray, _currentArrayIndex);
					break;
			}
			_currentArrayIndex += 2;
			return _returnVal;
		}

		/// <summary>
		/// Reads two bytes from the byte array cache and converts it into an unsigned short/16-bit integer.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="_reverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns an unsigned 16-bit integer read from the byte array.</returns>
		public static ushort BytesToUShort16(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead, bool _reverseEndianess = false)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4094) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			ushort _returnVal;
			switch (_reverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToUInt16(new byte[2] { _byteArray[_currentArrayIndex + 1], _byteArray[_currentArrayIndex ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToUInt16(_byteArray, _currentArrayIndex);
					break;
			}
			_currentArrayIndex += 2;
			return _returnVal;
		}

		/// <summary>
		/// Reads two bytes from the byte array cache and converts it into a Unicode char.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="_reverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a Unicode char read from the byte array.</returns>
		public static char BytesToChar(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead, bool _reverseEndianess = false)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4094) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			char _returnVal;
			switch (_reverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToChar(new byte[2] { _byteArray[_currentArrayIndex + 1], _byteArray[_currentArrayIndex] }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToChar(_byteArray, _currentArrayIndex);
					break;
			}
			_currentArrayIndex += 2;
			return _returnVal;
		}

		/// <summary>
		/// Reads four bytes from the byte array cache and converts it into a signed int/32-bit integer.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="_reverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a signed 32-bit integer read from the byte array.</returns>
		public static int BytesToInt32(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead, bool _reverseEndianess = false)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4092) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			int _returnVal;
			switch (_reverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToInt32(new byte[4] { _byteArray[_currentArrayIndex + 3], _byteArray[_currentArrayIndex + 2],
																	_byteArray[_currentArrayIndex + 1], _byteArray[_currentArrayIndex ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToInt32(_byteArray, _currentArrayIndex);
					break;
			}
			_currentArrayIndex += 4;
			return _returnVal;
		}

		/// <summary>
		/// Reads four bytes from the byte array cache and converts it into an unsigned int/32-bit integer.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="_reverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns an unsigned 32-bit integer read from the byte array.</returns>
		public static uint BytesToUInt32(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead, bool _reverseEndianess = false)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4088) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			uint _returnVal;
			switch (_reverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToUInt32(new byte[4] { _byteArray[_currentArrayIndex + 3], _byteArray[_currentArrayIndex + 2],
																	 _byteArray[_currentArrayIndex + 1], _byteArray[_currentArrayIndex ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToUInt32(_byteArray, _currentArrayIndex);
					break;
			}
			_currentArrayIndex += 4;
			return _returnVal;
		}

		/// <summary>
		/// Reads four bytes from the byte array cache and converts it into a single precision float.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="_reverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a single precision float read from the byte array.</returns>
		public static float BytesToSingleFloat(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead, bool _reverseEndianess = false)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4088) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			float _returnVal;
			switch (_reverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToSingle(new byte[4] { _byteArray[_currentArrayIndex + 3], _byteArray[_currentArrayIndex + 2],
																	 _byteArray[_currentArrayIndex + 1], _byteArray[_currentArrayIndex ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToSingle(_byteArray, _currentArrayIndex);
					break;
			}
			_currentArrayIndex += 4;
			return _returnVal;
		}

		/// <summary>
		/// Reads eight bytes from the byte array cache and converts it into a signed long/64-bit integer.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="_reverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a signed 64-bit integer read from the byte array.</returns>
		public static long BytesToLong64(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead, bool _reverseEndianess = false)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4088) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			long _returnVal;
			switch (_reverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToInt64(new byte[8] { _byteArray[_currentArrayIndex + 7], _byteArray[_currentArrayIndex + 6],
																	_byteArray[_currentArrayIndex + 5], _byteArray[_currentArrayIndex + 4],
																	_byteArray[_currentArrayIndex + 3], _byteArray[_currentArrayIndex + 2],
																	_byteArray[_currentArrayIndex + 1], _byteArray[_currentArrayIndex ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToInt64(_byteArray, _currentArrayIndex);
					break;
			}
			_currentArrayIndex += 8;
			return _returnVal;
		}

		/// <summary>
		/// Reads eight bytes from the byte array cache and converts it into an unsigned long/64-bit integer.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="_reverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns an unsigned 64-bit integer read from the byte array.</returns>
		public static ulong BytesToULong64(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead, bool _reverseEndianess = false)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4088) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			ulong _returnVal;
			switch (_reverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToUInt64(new byte[8] { _byteArray[_currentArrayIndex + 7], _byteArray[_currentArrayIndex + 6],
																	 _byteArray[_currentArrayIndex + 5], _byteArray[_currentArrayIndex + 4],
																	 _byteArray[_currentArrayIndex + 3], _byteArray[_currentArrayIndex + 2],
																	 _byteArray[_currentArrayIndex + 1], _byteArray[_currentArrayIndex ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToUInt64(_byteArray, _currentArrayIndex);
					break;
			}
			_currentArrayIndex += 8;
			return _returnVal;
		}

		/// <summary>
		/// Reads eight bytes from the byte array cache and converts it into a double precision float.
		/// </summary>
		/// <param name="_fileStream">The FileStream you created for the file you want to read.</param>
		/// <param name="_byteArray">An empty byte array which has been initialised with an index size of exactly 4096 that will persist for the life of the FileStream.</param>
		/// <param name="_currentArrayIndex">A signed integer that will persist for the life of the FileStream. Indicates the position of the BytesToTypes' pointer.
		/// If you move the FileStream's pointer manually, you should move this pointer by the same amount.</param>
		/// <param name="_numBytesRead">Returns the number of bytes read from the FileStream while re-filling the byte array cache. Or -1 if the cache didn't need refilling.</param>
		/// <param name="_reverseEndianess">Optional boolean that you can set to True to return the bytes read from the array with Endianess reversed. 
		/// i.e. convert a Big-Endian FileStream into a Little-Endian integer or vice versa.</param>
		/// <returns>Returns a double precision float read from the byte array.</returns>
		public static double BytesToDoubleFloat(FileStream _fileStream, ref byte[] _byteArray, ref int _currentArrayIndex, out int _numBytesRead, bool _reverseEndianess = false)
		{
			_numBytesRead = -1;
			if ((_currentArrayIndex >= 4088) || (_currentArrayIndex <= -1))
				_numBytesRead = RefillBytesArray(_fileStream, ref _byteArray, ref _currentArrayIndex);

			double _returnVal;
			switch (_reverseEndianess)
			{
				case true:
					_returnVal = BitConverter.ToDouble(new byte[8] { _byteArray[_currentArrayIndex + 7], _byteArray[_currentArrayIndex + 6],
																	 _byteArray[_currentArrayIndex + 5], _byteArray[_currentArrayIndex + 4],
																	 _byteArray[_currentArrayIndex + 3], _byteArray[_currentArrayIndex + 2],
																	 _byteArray[_currentArrayIndex + 1], _byteArray[_currentArrayIndex ]  }, 0);
					break;

				case false:
				default:
					_returnVal = BitConverter.ToDouble(_byteArray, _currentArrayIndex);
					break;
			}
			_currentArrayIndex += 8;
			return _returnVal;
		}
	}
}
