Configuration VisualStudio2
{
    Node localhost {
	
		Import-DscResource -Module xPSDesiredStateConfiguration

		xPackage VS
        {
            Ensure="Present"
            Name = "Microsoft Visual Studio Ultimate 2013"
            Path = "\\products\public\PRODUCTS\Developers\Visual Studio 2013\ultimate\vs_ultimate.exe"
            ProductId = ""
        }
    }
}

. VisualStudio2