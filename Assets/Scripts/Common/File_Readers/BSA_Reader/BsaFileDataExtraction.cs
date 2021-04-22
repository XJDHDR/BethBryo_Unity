using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace BethBryo_for_Unity_Common
{
	internal struct BsaFileExtractToFileParams
	{
		internal uint FileDataOffset;
		internal bool IsDataCompressed;
		internal int FileDataSize;
		internal string FileFullPath;
	}

	internal struct BsaFileExtractToMemParams
	{
		internal uint FileDataOffset;
		internal bool IsDataCompressed;
		internal int FileDataSize;
	}

	internal static class BsaFileDataExtraction
	{
		public static int NumThreadsDone;

		private static string _pathToBsa;
		private static BsaFileExtractToMemParams[] _bsaFileExtractToMemParams;
		private static MemoryStream[] _extractedFileData;

		internal static void ExtractBsaFileDataToFiles(string PathToBsa, BsaFileExtractToMemParams[] PassedBsaFileExtractParams)
		{
			_pathToBsa = PathToBsa;
			_bsaFileExtractToMemParams = PassedBsaFileExtractParams;
			Parallel.For(0, _bsaFileExtractToMemParams.Length, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualByteSequenceToFileInThread);
		}
		
		internal static void ExtractBsaFileDataToMemory(string PathToBsa, BsaFileExtractToMemParams[] PassedBsaFileExtractParams, out MemoryStream[] ExtractedFileData)
		{
			_pathToBsa = PathToBsa;
			_bsaFileExtractToMemParams = PassedBsaFileExtractParams;
			Parallel.For(0, _bsaFileExtractToMemParams.Length, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualByteSequenceToMemoryInThread);
			ExtractedFileData = _extractedFileData;
		}

		private static void _extractIndividualByteSequenceToFileInThread(int _iterNumber)
		{
		}

		private static void _extractIndividualByteSequenceToMemoryInThread(int _iterNumber)
		{
			using (BinaryReader _bsaFileStream = new BinaryReader(new FileStream(_pathToBsa, FileMode.Open, FileAccess.Read)))  // might need FileShare.ReadWrite if access violation appears.
			{
				_bsaFileStream.BaseStream.Position = _bsaFileExtractToMemParams[_iterNumber].FileDataOffset;

				if (_bsaFileExtractToMemParams[_iterNumber].IsDataCompressed == true)
				{
					uint _decompressedDataLength = _bsaFileStream.ReadUInt32();
					using (MemoryStream _extractedCompressedData = new MemoryStream(_bsaFileStream.ReadBytes(_bsaFileExtractToMemParams[_iterNumber].FileDataSize)))
					{
						_decompressZlibData(_extractedCompressedData);
					}
				}
				else
				{
					//_extractedFiles[_iterNumber] = new MemoryStream(_bsaFileStream.ReadBytes(_bsaFileExtractParams[_iterNumber].FileDataSize));
				}

				//_extractedFiles[_iterNumber] = _bsaFileStream;	// Edit
			}
			Interlocked.Increment(ref NumThreadsDone);
		}
		private static void _decompressZlibData(MemoryStream _extractedFileBytes)
		{
			//		using (FileStream originalFileStream = fileToDecompress.OpenRead())	// Might not need this
			{
				//			string currentFileName = fileToDecompress.FullName;
				//			string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

				//			using (FileStream decompressedFileStream = File.Create(newFileName))
				{
					//				using (DeflateStream decompressionStream = new DeflateStream(originalFileStream, CompressionMode.Decompress))
					{
						//					decompressionStream.CopyTo(decompressedFileStream);
						//					Debug.Log("Decompressed: {0}", fileToDecompress.Name);
					}
				}
			}
		}
	}
}
