using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace BethBryo_for_Unity
{
	internal struct FileExtractionArgs
	{
		internal string PassedPathToBsa;
		internal long TotalStoredFiles;
	}

	internal struct BsaFileExtractParams
	{
		internal bool IsDataCompressed;
		internal bool SaveDataToPath;
		internal string FileFullPath;
		internal uint FileDataSize;
		internal uint FileDataOffset;
	}

	internal static class BsaFileDataExtraction
	{
		internal static long NumThreadsDone;

		private static string _pathToBsa;
		private static BsaFileExtractParams[] _bsaFileExtractParams;
		private static MemoryStream[] _extractedFiles;

		internal static void ExtractBsaFileDataToFiles(FileExtractionArgs FileExtractionArgs, BsaFileExtractParams[] PassedBsaFileExtractParams)
		{
			_pathToBsa = FileExtractionArgs.PassedPathToBsa;
			_bsaFileExtractParams = PassedBsaFileExtractParams;
			_extractedFiles = new MemoryStream[FileExtractionArgs.TotalStoredFiles];
			Parallel.For(0, FileExtractionArgs.TotalStoredFiles, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualByteSequenceToMemoryStreamInThread);
		}
		
		internal static void ExtractBsaFileDataToMemoryStreams(FileExtractionArgs FileExtractionArgs, BsaFileExtractParams[] PassedBsaFileExtractParams)
		{
			_pathToBsa = FileExtractionArgs.PassedPathToBsa;
			_bsaFileExtractParams = PassedBsaFileExtractParams;
			_extractedFiles = new MemoryStream[FileExtractionArgs.TotalStoredFiles];
			Parallel.For(0, FileExtractionArgs.TotalStoredFiles, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualByteSequenceToMemoryStreamInThread);

			//Parallel.ForEach(_bsaFileExtractParams, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualFileInThread);
			//_extractIndividualFileInThread(_bsaFileExtractParams _BsaFileExtractParams)
		}

		private static void _extractIndividualByteSequenceToFileInThread(long _iterNumber)
		{

		}

		private static void _extractIndividualByteSequenceToMemoryStreamInThread(long _iterNumber)
		{
			using (BinaryReader _bsaFileStream = new BinaryReader(new FileStream(_pathToBsa, FileMode.Open, FileAccess.Read)))  // might need FileShare.ReadWrite if access violation appears.
			{
				_bsaFileStream.BaseStream.Position = _bsaFileExtractParams[_iterNumber].FileDataOffset;
				MemoryStream _extractedFileBytes = new MemoryStream();
				_extractedFileBytes.SetLength(_bsaFileExtractParams[_iterNumber].FileDataSize);

				if (_bsaFileExtractParams[_iterNumber].IsDataCompressed == true)
				{
					uint _decompressedDataLength = _bsaFileStream.ReadUInt32();
					if (_bsaFileExtractParams[_iterNumber].FileDataSize > 2147483647)
					{
						_extractedFileBytes.Write(_bsaFileStream.ReadBytes(2147483647), 0, 2147483647);
						_extractedFileBytes.WriteByte(_bsaFileStream.ReadByte());
						_extractedFileBytes.Write(_bsaFileStream.ReadBytes((int)(_bsaFileExtractParams[_iterNumber].FileDataSize - 2147483648)), 0,
							(int)(_bsaFileExtractParams[_iterNumber].FileDataSize - 2147483648));
					}
					else
						_extractedFileBytes = new MemoryStream(_bsaFileStream.ReadBytes((int)_bsaFileExtractParams[_iterNumber].FileDataSize));
					_decompressZlibData(_extractedFileBytes);
				}
				else
				{
					//_extractedFileBytes = new MemoryStream(_bsaFileStream.ReadBytes(_bsaFileExtractParams[_iterNumber]._fileDataSize));
				}

				//_extractedFiles[_iterNumber] = _bsaFileStream;	// Edit

				Interlocked.Increment(ref NumThreadsDone);
			}
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
