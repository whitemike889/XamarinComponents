#addin nuget:?package=Xamarin.Nuget.Validator&version=1.1.1


using Xamarin.Nuget.Validator;


// SECTION: Arguments and Settings

var ROOT_DIR = (DirectoryPath)Argument ("root", ".");
var ROOT_OUTPUT_DIR = ROOT_DIR.Combine ("output");

var PACKAGE_NAMESPACES = Argument ("n", Argument ("namespaces", ""))
	.Split (new [] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
	.ToList ();
PACKAGE_NAMESPACES.AddRange (new [] {
	"Xamarin",
});


// SECTION: Main Script

Information ("");
Information ("Script Arguments:");
Information ("  Root directory: {0}", MakeAbsolute (ROOT_DIR));
Information ("  Root output directory: {0}", ROOT_OUTPUT_DIR);
Information ("  Valid package namespaces: {0}", string.Join (", ", PACKAGE_NAMESPACES));
Information ("");


// SECTION: Validate Output

var options = new NugetValidatorOptions {
	Copyright = "© Microsoft Corporation. All rights reserved.",
	Author = "Microsoft",
	Owner = "Microsoft",
	NeedsProjectUrl = true,
	NeedsLicenseUrl = true,
	ValidateRequireLicenseAcceptance = true,
	ValidPackageNamespace = PACKAGE_NAMESPACES.ToArray (),
};

var nupkgFiles = GetFiles (ROOT_OUTPUT_DIR + "/**/*.nupkg");

Information ("Found {0} NuGet packages to validate.", nupkgFiles.Count);

foreach (var nupkgFile in nupkgFiles) {
	Information ("Verifying NuGet metadata of {0}...", nupkgFile);
	var result = NugetValidator.Validate (nupkgFile.FullPath, options);
	if (result.Success) {
		Information ("NuGet metadata validation passed.");
	} else {
		Error ("NuGet metadata validation failed:");
		Error (string.Join (Environment.NewLine + "    ", result.ErrorMessages));

		throw new Exception ($"Invalid NuGet metadata for: {nupkgFile}");
	}
}
