# HashAxe
[![HashAxe](https://github.com/Wonik-Studios/HashAxe/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/Wonik-Studios/HashAxe/actions/workflows/dotnet-desktop.yml)

[HashAxe Example Usage](https://wonik.info/projects/hashaxe)

HashAxe is a CLI utility which can traverse large directories to detect blacklisted files in a very timely manner. It can be used to remove malicious files, check for the existance of files on folders/drives when the MD5 hash is known, and to recover lost files when they are moved to an unknown path. 

HashAxe is written entirely in C#.

This README consists of only installation instructions. For a more in-depth overview of HashAxe, visit https://wonik.info/projects/hashaxe.

## Usage
These are the list of the commands that are available on HashAxe. For clarification, the term "hashlist" will be referring to the collection of hashes provided by the user before compilation and "hashset" will be used to refer to these hashlists after the ```compile``` command has been run on it.
```
hashaxe compile <Hashlist Input> <Hashset Output>
hashaxe install <HashSet Name> <DAT Binary Path>
hashaxe hashsets
hashaxe remove <hashset-name>
hashaxe enable <hashset-name>
hashaxe disable <hashset-name>
hashaxe rename <old-name> <new-name>
hashaxe traverse <search-path> -V true|false
```

##### The Compile Command
```hashaxe compile <Hashlist Input> <Hashset Output>``` is the install command and is used for formatting the file containing all of the md5 hashes so that HashAxe can efficiently check if a hash is contained in the file. <Hashlist Input> will specify the path of the file with the hashes that you wish to check your files against and <Hashset Output> will be the path that will store the resulting file. The method that's used for optimizing searches for the hashes will be explained under _Tech_.

##### The Install Command
After you've compiled your hashes and you've obtained your <Hashset Output> file, you can now add it into the HashAxe program by running ```hashaxe install <HashSet Name> <DAT Binary Path>```. The path that you've entered into <Hashset Output> or the file that was produced from the compile program will be your input for <DAT Binary Path>. It must be noted that **the <DAT Binary Path> argument must be of a file with a .dat extension**. The <HashSet Name> will be the name that will be given to the hashset or your collection of hashes. This name property will be used when enabling, disabling, removing or renaming the hashset.

##### The Hashsets Command
The Hashsets Command or ```hashaxe hashsets``` is a command that will log out all of the hashsets and information relating to it such as:

- **NAME**
    > The name of the hashset. This will be the unique identifier for it.
- **TIME OF CREATION**
    > The time of creation is the time at which the ```install``` command finishes the it's installation.
- **# OF HASHES**
    > This is the total number of hashes that is contained in the hashset
- **ENABLED**
    > This denotes the status of the hashset or whether it is enabled or disabled. The significance of this will be explained under the ```traverse``` command.

##### The Remove Command
The ```hashaxe remove <hashset-name>``` command removes the hashset under the name  of <hashset-name>.

##### The Enable Command
The ```hashaxe enable <hashset-name>``` command enables the hashset under the name <hashset-name>.

##### The Disable Command
The ```hashaxe disable <hashset-name>``` command disables the hashset under the name <hashset-name>.

##### The Rename Command
The ```hashaxe rename <old-name> <new-name>``` command renames the hashset <old-name> into <new-name> provided that hashset <old-name> exists and that there are no hashsets named <new-name>.

##### The Traverse Command
The ```hashaxe traverse <search-path>``` command recursively iterates through all the files and directories under <search-path>. For every file that it encounters, it will check to see if it's hash is contained in any of the _enabled_ hashsets. All _disabled_ hashsets will be ignored. After it has iterated through all of the subdirectories and files, it will print a list of files that have been flagged by one of the enabled hashsets. There will be a prompt asking the user whether they would like to delete all of these files or not. In-depth verbose logging can be enabled using the `-V` option on the command. Verbose logging will output the file paths and their hashes as they are completed. This could potentially slow down the process.

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
