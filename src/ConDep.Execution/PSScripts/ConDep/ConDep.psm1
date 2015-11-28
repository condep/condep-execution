Set-StrictMode -Version 3

function Assert-ConDepChocoExist() {
    if (Get-ConDepChocoPath) { return $true }
    return $false
}

function Get-ConDepChocoPath {
    if($env:ChocolateyInstall) {
        return $env:ChocolateyInstall
    }
    elseif (Test-Path $env:ProgramData\chocolatey) {
            return "$env:ProgramData\chocolatey"
    }
    elseif (Test-Path $env:HOMEDRIVE\chocolatey) {
        return "$env:HOMEDRIVE\chocolatey"
    }
    elseif (Test-Path $env:ALLUSERSPROFILE\chocolatey) {
        return "$env:ALLUSERSPROFILE\chocolatey"
    }
    return $null
}

function Get-ConDepChocoExe {
	if(Get-ConDepChocoPath) {
		return "$(Get-ConDepChocoPath)\bin\choco.exe"
	}
	return "choco.exe"
}

function Assert-ConDepChocoNew {
	$answer = &(Get-ConDepChocoExe) -v

	foreach($line in $answer -split "`n") {
		if(!$line) { next }
		if($line -match "Please run chocolatey.*") { return $false}
		else { return $true }
	}
}

function Invoke-ConDepChocoUpgrade {
	if(Assert-ConDepChocoNew) {
		write-Host "Upgrading Chocolatey..."
		&(Get-ConDepChocoExe) upgrade chocolatey -dvy
	}
	else {
		write-Host "No recent version of Chocolatey found. Installing now..."
		iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1')) | Out-Null 
	}
}

function Invoke-ConDepChocoInstall {
	iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))
}

Get-ChildItem -Path $PSScriptRoot\*.ps1 | Foreach-Object{ . $_.FullName }
Export-ModuleMember -Function *-*
