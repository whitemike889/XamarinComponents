/*
#########################################################################################
Installing

	Windows - powershell
		
        Invoke-WebRequest http://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
        .\build.ps1

	Windows - cmd.exe prompt	
	
        powershell ^
			Invoke-WebRequest http://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
        powershell ^
			.\build.ps1
	
	Mac OSX 

        rm -fr tools/; mkdir ./tools/ ; \
        cp cake.packages.config ./tools/packages.config ; \
        curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/osx ; \
        chmod +x ./build.sh ;
        ./build.sh

	Linux

        curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/linux
        chmod +x ./build.sh && ./build.sh

Running Cake to Build targets

	Windows

		tools\Cake\Cake.exe --verbosity=diagnostic --target=libs
		tools\Cake\Cake.exe --verbosity=diagnostic --target=nuget
		tools\Cake\Cake.exe --verbosity=diagnostic --target=samples

		tools\Cake\Cake.exe -experimental --verbosity=diagnostic --target=libs
		tools\Cake\Cake.exe -experimental --verbosity=diagnostic --target=nuget
		tools\Cake\Cake.exe -experimental --verbosity=diagnostic --target=samples
		
	Mac OSX 
	
		mono tools/Cake/Cake.exe --verbosity=diagnostic --target=libs
		mono tools/Cake/Cake.exe --verbosity=diagnostic --target=nuget
#########################################################################################
*/
#tool nuget:?package=XamarinComponent

#addin nuget:?package=Cake.XCode
#addin nuget:?package=Cake.Xamarin.Build
#addin nuget:?package=Cake.Xamarin
#addin nuget:?package=Cake.FileHelpers

var TARGET = Argument ("t", Argument ("target", "Default"));

string ARTIFACT_VERSION="1.9.0";
string AAR_URL="https://bintray.com/google/tensorflow/download_file?file_path=org%2Ftensorflow%2Ftensorflow-lite%2F1.9.0%2Ftensorflow-lite-1.9.0.aar";
string ARTIFACT_FILE = $"./externals/android/tensorflow-lite-{ARTIFACT_VERSION}.aar";
string NUGET_VERSION=$"{ARTIFACT_VERSION}-alpha01";


BuildSpec buildSpec = new BuildSpec () 
{
	Libs = new ISolutionBuilder [] 
	{
		new DefaultSolutionBuilder 
		{
			SolutionPath = "./source/Xamarin.TensorFlow.Lite.sln",
			OutputFiles = new [] 
			{ 
				new OutputFileCopy 
				{ 
					FromFile = "./source/Xamarin.TensorFlow.Lite.Bindings.XamarinAndroid/bin/Release/monoandroid81/Xamarin.TensorFlow.Lite.dll" 
				},
				new OutputFileCopy 
				{ 
					FromFile = "source/Xamarin.TensorFlow.Lite.Bindings.XamarinAndroid/bin/Release/Xamarin.TensorFlow.Lite.1.9.0-alpha01.nupkg" 
				},
			}
		}
	},

	Samples = new ISolutionBuilder [] 
	{
		new DefaultSolutionBuilder 
		{ 
			SolutionPath = "./samples/Xamarin.TensorFlow.Lite.Samples.sln" 
		},	
	},
};

Task ("externals")
	.IsDependentOn ("externals-base")
	.WithCriteria (!FileExists (ARTIFACT_FILE))
	.Does 
	(
		() => 
		{
			Information("externals ...");
			EnsureDirectoryExists("./externals/android");
			Information("    downloading ...");

			if ( ! string.IsNullOrEmpty(AAR_URL) && ! FileExists(ARTIFACT_FILE))
			{
				DownloadFile (AAR_URL, ARTIFACT_FILE);
			}
		}
	);


Task ("clean")
	.IsDependentOn ("clean-base")
	.Does
	(
		() => 
		{
			if (DirectoryExists ("./externals/"))
			{
				DeleteDirectory ("./externals", true);
			}
		}
	);

Task("nuget")
	.IsDependentOn("libs")
	.Does
	(
		() =>
		{
			EnsureDirectoryExists("./output");

			MSBuild
			(
				"./source/Xamarin.TensorFlow.Lite.Bindings.XamarinAndroid/Xamarin.TensorFlow.Lite.Bindings.XamarinAndroid.csproj", 
				configuration => 
					configuration
						.SetConfiguration("Release")
						.WithTarget("Pack")
						.WithProperty("PackageVersion", NUGET_VERSION)
						.WithProperty("PackageOutputPath", "../../output")
			);

		}
);

SetupXamarinBuildTasks (buildSpec, Tasks, Task);

RunTarget (TARGET);
