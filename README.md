# HashAxe
[![HashAxe](https://github.com/Wonik-Studios/HashAxe/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/Wonik-Studios/HashAxe/actions/workflows/dotnet-desktop.yml)


HashAxe is a CLI utility which can traverse large directories to detect blacklisted files specified in a hashlist in a very timely manner. It can be used to remove malicious files, check for the existance of files on folders/drives when the MD5 hash is known, and to recover lost files when they are moved to an unknown path. The search algorithm uses quadratic probing, which ensures speed & efficiency.

HashAxe is written entirely in C#.
## Usage
These are the list of the commands that are available on HashAxe. For clarification, the term "hashlist" will be referring to the collection of hashes provided by the user before compilation and "hashset" will be used to refer to these hashlists after the ```compile``` command has been run on it.
```
  i <path>                  Install a hashset.
  ls                        List hashsets.
  compile <input> <output>  Compile lists of hashes to hashset.
  rm <name>                 Uninstalls a hashset from the configuration.
  off <name>                Disables a hashset so it won't be included in the scan.
  on <name>                 Enables a previously disabled hashset.
  scan <target>             Scan a specified path.
```



# Installation

We provide x64 binaries of Hashaxe for all releases on both Windows and Linux. These binaries are self-contained, meaning that you do not need the .NET Framework installed on your computer to execute them. It is possible to to compile from source and install your own binaries instead if your platform isn't supported or you want to compile from source. For information about this, look under the installation instruction.

Start by downloading a binary arhcive for your platform at [releases](https://github.com/Wonik-Studios/HashAxe/releases) and extracting it.

An optional step is to check the integrity of the downloaded file to make sure it has not been corrupted or tampered with during download. We add the sha256 sums of all archives in the release notes. 

## Windows:

On Windows, it's prefered that you extract the archive into to a directory in C:/ to prevent it from being tampered with. To be able to use `hashaxe` as a command, you'll need to add the folder where the HashAxe executable is located to your Path enviornment variable. 

Search up "Enviornment variables" in Windows search and open "Edit enviornment variables for your account". Select "Path" in your list and click "Edit..". Press "New" to add a new entry. Insert in the path to the folder containing the HashAxe executable, and press OK. You can then close that app, HashAxe is now installed.

## Linux:

Open a terminal in the extracted directory. Give the binary executable access by running `sudo chmod +x HashAxe` in the current directory. To make the executable a commad, move the binary to `/usr/bin/` by running `mv HashAxe /usr/bin/hashaxe`. HashAxe is now installed.
 
## After installing:
 
 Verify that the HashAxe verison is installed correctly by running `hashaxe --version` in a terminal. To receive future updates, you will have to go through the same process.

# Compiling From Source

You will need the [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) to be able to compile the HashAxe source code to create an executable. Begin by downloading the source code from the release you want to compile, and extracting it.

Open a terminal in the source code folder, and run the follwing:
```sh
dotnet publish
```

If you are compiling for a different platform then the one you are on, you can use the `-r` option to specify the runtime identifier (here are a list of [rupported runtime identifiers](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)). For example, compiling for linux on x64:
```sh
dotnet publish -r linux-x64
```

After the compilation process is complete, you will find a folder named `publish` that contains the executable file. This folder will be located within another folder named after the Runtime Identifier (RID) you compiled for, inside the `bin` directory which is created after compilation. To illustrate, if you compiled for the `win-x64` RID, the executable file can be accessed at `bin/win-x64/publish/HashAxe`.

With this binary, you can then follow the normal installation procedures.
