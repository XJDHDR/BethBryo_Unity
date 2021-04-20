using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

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
		internal int FileDataSize;
		internal string FileFullPath;
		internal uint FileDataOffset;
	}

	internal static class BsaFileDataExtraction
	{
		public static long NumThreadsDone;

		private static string _pathToBsa;
		private static BsaFileExtractParams[] _bsaFileExtractParams;
		private static MemoryStream[] _extractedFiles;

		internal static void ExtractBsaFileDataToFiles(FileExtractionArgs FileExtractionArgs, BsaFileExtractParams[] PassedBsaFileExtractParams)
		{
			_pathToBsa = FileExtractionArgs.PassedPathToBsa;
			_bsaFileExtractParams = PassedBsaFileExtractParams;
			Parallel.For(0, FileExtractionArgs.TotalStoredFiles, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualByteSequenceToFileInThread);
		}
		
		internal static void ExtractBsaFileDataToMemory(FileExtractionArgs FileExtractionArgs, BsaFileExtractParams[] PassedBsaFileExtractParams, out MemoryStream[] ExtractedFiles)
		{
			new ExtractIndividualByteSequenceToMemory
			{
				PathToBSA = FileExtractionArgs.PassedPathToBsa,
				BsaFileExtractParams = PassedBsaFileExtractParams
				//ExtractedFiles = 
			};


			//_pathToBsa = FileExtractionArgs.PassedPathToBsa;
			//_bsaFileExtractParams = PassedBsaFileExtractParams;
			//Parallel.For(0, FileExtractionArgs.TotalStoredFiles, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualByteSequenceToMemoryInThread);

			ExtractedFiles = _extractedFiles;
			//Parallel.ForEach(_bsaFileExtractParams, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualFileInThread);
			//_extractIndividualFileInThread(_bsaFileExtractParams _BsaFileExtractParams)

			//_extractedFiles.Dispose(true);
		}

		private static void _extractIndividualByteSequenceToFileInThread(long _iterNumber)
		{
		}

		private static void _extractIndividualByteSequenceToMemoryInThread(long _iterNumber)
		{
			using (BinaryReader _bsaFileStream = new BinaryReader(new FileStream(_pathToBsa, FileMode.Open, FileAccess.Read)))  // might need FileShare.ReadWrite if access violation appears.
			{
				_bsaFileStream.BaseStream.Position = _bsaFileExtractParams[_iterNumber].FileDataOffset;

				if (_bsaFileExtractParams[_iterNumber].IsDataCompressed == true)
				{
					uint _decompressedDataLength = _bsaFileStream.ReadUInt32();
					using (MemoryStream _extractedCompressedData = new MemoryStream(_bsaFileStream.ReadBytes(_bsaFileExtractParams[_iterNumber].FileDataSize)))
					{
						_decompressZlibData(_extractedCompressedData);
					}
				}
				else
				{
					_extractedFiles[_iterNumber] = new MemoryStream(_bsaFileStream.ReadBytes(_bsaFileExtractParams[_iterNumber].FileDataSize));
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

	[BurstCompile(CompileSynchronously = true)]
	internal struct ExtractIndividualByteSequenceToMemory : IJobParallelFor
	{
		[ReadOnly] public string PathToBSA; // Managed type
		[ReadOnly] public BsaFileExtractParams[] BsaFileExtractParams;
		public MemoryStream[] ExtractedFiles;

		public void Execute(int Index)
		{
			using (BinaryReader _bsaFileStream = new BinaryReader(new FileStream(PathToBSA, FileMode.Open, FileAccess.Read)))  // might need FileShare.ReadWrite if access violation appears.
			{
				_bsaFileStream.BaseStream.Position = BsaFileExtractParams[Index].FileDataOffset;

				if (BsaFileExtractParams[Index].IsDataCompressed == true)
				{
					uint _decompressedDataLength = _bsaFileStream.ReadUInt32();
					using (MemoryStream _extractedCompressedData = new MemoryStream(_bsaFileStream.ReadBytes(BsaFileExtractParams[Index].FileDataSize)))
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
				else
				{
					ExtractedFiles[Index] = new MemoryStream(_bsaFileStream.ReadBytes(BsaFileExtractParams[Index].FileDataSize));
				}

				//_extractedFiles[_iterNumber] = _bsaFileStream;	// Edit
			}
			Interlocked.Increment(ref BsaFileDataExtraction.NumThreadsDone);
		}
	}
}
