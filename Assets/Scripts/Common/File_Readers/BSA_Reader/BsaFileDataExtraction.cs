// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE

using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BethBryo_for_Unity_Common
{
	internal struct BsaFileExtractParams
	{
		internal bool ExtractToFile;
		internal uint FileDataOffset;
		internal bool IsDataCompressed;
		internal int FileDataSize;
		internal string FileFullPath;
	}

	internal static class BsaFileDataExtraction
	{
		public static int NumThreadsDone;

		private static bool _errorsOccurred;
		private static string _pathToBsa;
		private static BsaFileExtractParams[] _bsaFileExtractParams;
		private static MemoryStream[] _extractedFileData;

		internal static void ExtractBsaFileDataToFiles(string PathToBsa, BsaFileExtractParams[] PassedBsaFileExtractParams, out bool ErrorsOccurred)
		{
			_errorsOccurred = false;
			_pathToBsa = PathToBsa;
			_bsaFileExtractParams = PassedBsaFileExtractParams;
			
			Parallel.For(0, _bsaFileExtractParams.Length, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualByteSequenceInThread);
			ErrorsOccurred = _errorsOccurred;

			_errorsOccurred = false;
			_pathToBsa = null;
			_bsaFileExtractParams = null;
		}

		internal static void ExtractBsaFileDataToMemory(string PathToBsa, BsaFileExtractParams[] PassedBsaFileExtractParams, out MemoryStream[] ExtractedFileData, out bool ErrorsOccurred)
		{
			_errorsOccurred = false;
			_pathToBsa = PathToBsa;
			_bsaFileExtractParams = PassedBsaFileExtractParams;

			Parallel.For(0, _bsaFileExtractParams.Length, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualByteSequenceInThread);
			ExtractedFileData = _extractedFileData;
			ErrorsOccurred = _errorsOccurred;

			_errorsOccurred = false;
			_pathToBsa = null;
			_bsaFileExtractParams = null;
			for (uint _i = 0; _i < _extractedFileData.Length; ++_i)
			{
				_extractedFileData[_i].Dispose();
			}
			_extractedFileData = null;
		}

		private static void _extractIndividualByteSequenceInThread(int _iterNumber)
		{
			using (BinaryReader _bsaFileStream = new BinaryReader(new FileStream(_pathToBsa, FileMode.Open, FileAccess.Read)))  // might need FileShare.ReadWrite if access violation appears.
			{
				_bsaFileStream.BaseStream.Position = _bsaFileExtractParams[_iterNumber].FileDataOffset;
				uint _decompressedDataLength = 0;

				if (_bsaFileExtractParams[_iterNumber].IsDataCompressed == true)
				{
					_decompressedDataLength = _bsaFileStream.ReadUInt32();
					byte[] _dataExtractedFromBsa = _bsaFileStream.ReadBytes(_bsaFileExtractParams[_iterNumber].FileDataSize - 4);
					using (MemoryStream _extractedCompressedData = new MemoryStream(_dataExtractedFromBsa))
					{
						//MemoryStream _extractedDecompressedData = new MemoryStream(null);
						_decompressZlibData(_bsaFileExtractParams[_iterNumber].FileFullPath, _extractedCompressedData, out MemoryStream _extractedDecompressedData);
						if (_extractedDecompressedData.Length != _bsaFileExtractParams[_iterNumber].FileDataSize)
						{
							Debug.LogErrorFormat("Extracted file {0} has a decompressed length of {1} when it is supposed to be {2}. This could indicate a corrupted BSA.", 
								_bsaFileExtractParams[_iterNumber].FileFullPath, _extractedDecompressedData.Length, _bsaFileExtractParams[_iterNumber].FileDataSize);
							_extractedFileData[_iterNumber] = null;
							_errorsOccurred = true;
						}
						else
						{
							if (_bsaFileExtractParams[_iterNumber].ExtractToFile == true)
							{
								_extractedDecompressedData.Seek(0, SeekOrigin.Begin);
								using (FileStream _fileForData = new FileStream(_bsaFileExtractParams[_iterNumber].FileFullPath, FileMode.OpenOrCreate))
								{
									_extractedDecompressedData.CopyTo(_fileForData);
									_fileForData.Flush();
								}
							}
							else
							{
								_extractedFileData[_iterNumber] = _extractedDecompressedData;
							}
						}
						_extractedDecompressedData.Dispose();
					}
				}
				else
				{
					if (_bsaFileExtractParams[_iterNumber].ExtractToFile == true)
					{
						File.WriteAllBytes(_bsaFileExtractParams[_iterNumber].FileFullPath, _bsaFileStream.ReadBytes(_bsaFileExtractParams[_iterNumber].FileDataSize));
					}
					else
					{
						_extractedFileData[_iterNumber] = new MemoryStream(_bsaFileStream.ReadBytes(_bsaFileExtractParams[_iterNumber].FileDataSize));
					}
				}
			}
			Interlocked.Increment(ref NumThreadsDone);
		}

		private static void _decompressZlibData(string _fileName, MemoryStream _extractedFileBytes, out MemoryStream _decompressedData)
		{
			_decompressedData = new MemoryStream();
			using (DeflateStream _decompressionStream = new DeflateStream(_extractedFileBytes, CompressionMode.Decompress))
			{
				_decompressionStream.CopyTo(_decompressedData);
				Debug.Log("Decompressed: " + _fileName);
			}
		}
	}
}
