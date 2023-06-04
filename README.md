# HashAxe
HashAxe is a CLI utility which can detect blacklisted files by their MD5 hash in a very timely manner. It can be used to remove malicious files, check for the existance of a file when the MD5 hash is known, and to recover lost files when they are moved to an unknown path. HashAxe is written entirely in C#.

This README consists of only installation instructions and documentation of the CLI. For a more in-depth overview of HashAxe, visit https://wonik.info/projects/hashaxe.

# Installation

We provide x64 binaries of Hashaxe for all releases on both Windows and Linux. These binaries are self-contained, meaning that you do not need the .NET Framework installed on your computer to execute them. It is possible to to compile from source and install your own binaries instead if your platform isn't supported or you want to compile from source. For information about this, look under the installation instruction.

Start by downloading a binary for your platform from [releases](https://github.com/Wonik-Studios/HashAxe/releases) and extracting it.

## Windows:

On Windows, it's preffered that you extract it to a directory in C:/ to prevent it from being tampered with. To be able to use `hashaxe` as a command, you'll need to add the folder where the HashAxe executable is located to your Path enviornment variable. 

Search up "Enviornment variables" in Windows search and open "Edit enviornment variables for your account". Select "Path" in your list and click "Edit..". Press "New" to add a new entry. Insert in the path to the folder containing the HashAxe executable, and press OK. You can then close that app, HashAxe is now installed.

## Linux:

Open a terminal in the extracted directory. Give the binary executable access by running `sudo chmod +x HashAxe` in the current directory. To make the executable a commad, move the binary to `/usr/bin/` by running `mv HashAxe /usr/bin/hashaxe`. HashAxe is now installed.
 
## After installing:
 
 Verify that the HashAxe verison is installed correctly by running `hashaxe --version` in a terminal. To

## Compiling From Source
