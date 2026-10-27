// -------------------------------------------------------------------------------------------------------------------------------------------------------------
// Author: 3dapi (https://github.com/3dapi)
// -------------------------------------------------------------------------------------------------------------------------------------------------------------

#define ACTIVE_GLC2DLIB

internal static class AppMain
{
#if ACTIVE_GLC2DLIB

	[STAThread]
	private static void Main(string[] args)
	{
		ApplicationConfiguration.Initialize();

		using G2AppBase app = args.Contains("--original")
			? new GameMain()
			: new ResourcePreview(args.Contains("--gallery"));
		app.Run();
	}
#endif
}
