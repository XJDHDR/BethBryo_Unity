using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class OcclussionCullingLosDetection : SystemBase
{
	protected override void OnUpdate()
	{
	}

	private void _doRayCasts()
	{
		int _screenHeight = Screen.currentResolution.height;
		int _screenWidth = Screen.currentResolution.width;
		byte _resolutionFraction = 1;

		List<Vector3> _raycastDirections = new List<Vector3>();

		Camera _playerCamera = null;
		Vector3 _playerCameraPos = _playerCamera.transform.position;

		int _heightSteps = Screen.currentResolution.height / _resolutionFraction;
		int _widthSteps = Screen.currentResolution.width / _resolutionFraction;
		NativeArray<RaycastCommand> _raycastCommmands = new NativeArray<RaycastCommand>(_heightSteps * _widthSteps, Allocator.TempJob);

		Parallel.For(0, _heightSteps, (_x, _loopState) =>
		//for (int _x = 0; _x < _heightSteps; ++_x)
		{
			int _currentXOffset = _x * _heightSteps;
			int _currentXPixel = _x * _resolutionFraction;
			for (int _y = 0; _y < _widthSteps; ++_y)
			{
				_raycastCommmands[_currentXOffset + _y] = new RaycastCommand(_playerCameraPos, _playerCameraPos - 
					_playerCamera.ScreenToWorldPoint(new Vector3(_currentXPixel, _y * _resolutionFraction, _playerCamera.nearClipPlane)), float.MaxValue, -5, 2);
			}
		});

		NativeArray<RaycastHit> _raycastCommmandsResults = new NativeArray<RaycastHit>(_heightSteps * _widthSteps * 2, Allocator.Temp);
		JobHandle _raycastCommmandHandle = RaycastCommand.ScheduleBatch(_raycastCommmands, _raycastCommmandsResults, 1, default);
		_raycastCommmandHandle.Complete();

		_raycastCommmands.Dispose();
		RaycastHit[] _raycastCommmandsFilteredResults = _raycastCommmandsResults.ToArray().Where(_a => !_a.Equals(null)).Distinct().ToArray();
		_raycastCommmandsResults.Dispose();

		Parallel.For(0, _raycastCommmandsFilteredResults.Length, (_index, _loopState) =>
		{
			if (_raycastCommmandsFilteredResults[_index].transform.name == "Collision")
			{
				//var _occlussionCullingHiderComponent = _raycastCommmandsFilteredResults[_index].transform.parent.GetComponent(AudioSource);
				//cclussionCullingHiderComponent.ResetCounter();
			}
		});
	}
}
