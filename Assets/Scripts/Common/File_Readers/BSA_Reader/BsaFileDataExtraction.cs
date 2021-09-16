// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//
// The code in this file was mainly written thanks to the BSA file format specification found on the UESP:
// https://en.uesp.net/wiki/Oblivion_Mod:BSA_File_Format

using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BethBryo_for_Unity_Common
{
	internal struct BsaFileExtractParams
	{
		internal bool ExtractToFile;
		internal uint FileDataOffset;
		internal bool IsDataCompressed;
		internal int FileDataSize;
		internal string FileDirectoryPath;
		internal string FileNameAndExtension;
	}

	internal static class BsaFileDataExtraction
	{
		public static int NumThreadsDone;

		private static bool _errorsOccurred;
		private static string _pathToBsa;
		private static BsaFileExtractParams[] _bsaFileExtractParams;
		private static byte[][] _allExtractedFileData;

		private static BsaFileFilterAndPatching _bsaFileFilterAndPatching;
		private static Dictionary<string, byte> _preExtractNames;
		private static Dictionary<string, ulong> _postExtractPatches;

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
		internal static void ExtractBsaFileDataToFiles(string PathToBsa, BsaFileExtractParams[] BsaFileExtractParams, out bool ErrorsOccurred)
		{
			_errorsOccurred = false;
			_pathToBsa = PathToBsa;
			_bsaFileExtractParams = BsaFileExtractParams;

			_bsaFileFilterAndPatching = new BsaFileFilterAndPatching(SupportedGames.Oblivion, ref _preExtractNames, ref _postExtractPatches);

			Parallel.For(0, BsaFileExtractParams.Length, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualByteSequenceInThread);
			ErrorsOccurred = _errorsOccurred;

			_errorsOccurred = false;
			_pathToBsa = null;
			_bsaFileExtractParams = null;
			_preExtractNames = null;
			_postExtractPatches = null;
		}

		internal static void ExtractBsaFileDataToArray(string PathToBsa, BsaFileExtractParams[] BsaFileExtractParams, out byte[][] AllExtractedFileData, out bool ErrorsOccurred)
		{
			_errorsOccurred = false;
			_pathToBsa = PathToBsa;
			_bsaFileExtractParams = BsaFileExtractParams;
			_allExtractedFileData = new byte[BsaFileExtractParams.Length][];

			Parallel.For(0, BsaFileExtractParams.Length, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, _extractIndividualByteSequenceInThread);
			AllExtractedFileData = _allExtractedFileData;
			ErrorsOccurred = _errorsOccurred;

			_errorsOccurred = false;
			_pathToBsa = null;
			_bsaFileExtractParams = null;
			_allExtractedFileData = null;
		}

		private static void _extractIndividualByteSequenceInThread(int _iterNumber)
		{
			if (_bsaFileFilterAndPatching.PreExtractNameFilter(_preExtractNames, _bsaFileExtractParams[_iterNumber].FileNameAndExtension, _pathToBsa))
			{
				Interlocked.Increment(ref NumThreadsDone);
				return;
			}

			using (BinaryReader _bsaFileStream = new BinaryReader(new FileStream(_pathToBsa, FileMode.Open, FileAccess.Read)))
			{
				_bsaFileStream.BaseStream.Position = _bsaFileExtractParams[_iterNumber].FileDataOffset;
				string _fileFullPath = _bsaFileExtractParams[_iterNumber].FileDirectoryPath + _bsaFileExtractParams[_iterNumber].FileNameAndExtension;
				uint _decompressedDataLength = 0;

				byte[] _extractedFileData;
				if (_bsaFileExtractParams[_iterNumber].IsDataCompressed == true)
				{
					_decompressedDataLength = _bsaFileStream.ReadUInt32();
					byte[] _dataExtractedFromBsa = _bsaFileStream.ReadBytes(_bsaFileExtractParams[_iterNumber].FileDataSize - 4);
					_decompressZlibData(_dataExtractedFromBsa, _decompressedDataLength, out _extractedFileData);
				}
				else
				{
					_extractedFileData = _bsaFileStream.ReadBytes(_bsaFileExtractParams[_iterNumber].FileDataSize);
				}

				_bsaFileFilterAndPatching.PostExtractFilePatching(_postExtractPatches, _pathToBsa, _bsaFileExtractParams[_iterNumber].FileNameAndExtension, ref _extractedFileData);

				if (_bsaFileExtractParams[_iterNumber].ExtractToFile == true)
				{
					Directory.CreateDirectory(_bsaFileExtractParams[_iterNumber].FileDirectoryPath);
					File.WriteAllBytes(_fileFullPath, _extractedFileData);
				}
				else
				{
					_allExtractedFileData[_iterNumber] = _extractedFileData;
				}
			}
			Interlocked.Increment(ref NumThreadsDone);
		}

		private static void _decompressZlibData(byte[] _extractedFileBytes, uint _decompressedDataLength, out byte[] _decompressedData)
		{
			Inflater _dataDecompressor = new Inflater();
			_dataDecompressor.SetInput(_extractedFileBytes, 0, _extractedFileBytes.Length);

			_decompressedData = new byte[_decompressedDataLength];
			_dataDecompressor.Inflate(_decompressedData);
		}
	}
}
