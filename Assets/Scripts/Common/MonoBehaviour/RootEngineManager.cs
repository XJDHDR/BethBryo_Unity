// The license for this source code may be found here:
// https://github.com/XJDHDR/BethBryo_for_Unity/blob/main/LICENSE
//

using UnityEngine;

namespace BethBryo_for_Unity_Common
{
	public class RootEngineManager : MonoBehaviour
	{
		/// <summary> True after the _startupCode() method finishes running. </summary>
		private static bool _startupIsDone;

		/// <summary> Null if the _postStartupCode() hasn't run yet, false if still running and true after it is finished. </summary>
		private static bool? _postStartupIsDone;


#pragma warning disable IDE0051 // Remove unused private members
		// Domain reloading is disabled: https://docs.unity3d.com/Manual/DomainReloading.html
		// Thus, we need this to reset the above static variables in editor mode.
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void _resetStaticVars()
		{
			_startupIsDone = false;
			_postStartupIsDone = null;
		}


		// Sneaky Unity engine hack. From what I've read, Unity doesn't run any of it's startup or update methods while the "Powered by Unity" splash screen is running.
		// However, it will run the constructor for any objects initially attached to the starting scene: https://gamedev.stackexchange.com/a/141402
		// 
		// My own testing seems to confirm this. In a test build version, this object's constructor ran 2.6 seconds before the Awake() or OnEnable() methods were called
		// and 3.2 seconds before the Start() and first Update() method calls.
#pragma warning disable IDE1006 // Naming Styles
#if !UNITY_EDITOR
		RootEngineManager()
		{
			// This code runs when the constructor is executed. This is only enabled for built versions because the editor doesn't run the constructor when expected. 
			// Instead, Unity says that it runs every time the script is recompiled. I've seen it run on exiting play mode. Some have also seen it run on entering play mode: 
			// https://ilkinulas.github.io/development/unity/2016/05/30/monobehaviour-constructor.html
			_startupCode();
		}
#endif

		/// <summary>
		/// Unity calls the Awake method right after constructing the GameObject this script is attached to.
		/// In this case, it will be used to construct the environment that the game will run in.
		/// </summary>
		void Awake()
		{
#if UNITY_EDITOR
			// This code runs when the GameObject awakes. This part is only enabled for editor versions because in addition to the reasons above, 
			// there is no splash screen in this case and hence, no delay in running it.
			_startupCode();
#endif

			if (_startupIsDone)
			{
				_postStartupCode();
			}
		}

		void Update()
		{
			if (_postStartupIsDone == null && _startupIsDone)
			{
				_postStartupCode();
			}



			// Finally, check if any new log messages need to be printed.
			LoggingHelper.PrintAllCurrentLogs();
		}
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0051 // Remove unused private members


		/// <summary>
		/// Used to execute any startup code that can be run during the Unity splash screen if a built version of the game is running. 
		/// This likely means only code which is not dependant on Unity. This is because Unity's classes might not be constructed yet 
		/// and the constructor is run in the loading thread, whereas Unity only allows calling it's methods in the main thread.
		/// </summary>
		private static void _startupCode()
		{
			// Do stuff here

			_startupIsDone = true;
		}

		/// <summary>
		/// Used to execute any startup code that will run after the Unity splash screen. 
		/// That means all construction code which requires Unity can go in here. However, this will run up to around 2.6 seconds after the above for builds.
		/// </summary>
		private static void _postStartupCode()
		{
			// Startup monitoring variables are arranged this way just in case the Awake() method runs before the constructor is finished.
			// In that case, Awake() is skipped and Update() runs this code after the constructor is done.
			_postStartupIsDone = false;

			// Do stuff here.

			_postStartupIsDone = true;
		}
	}
}
