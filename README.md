# HashAxe
HashAxe is a CLI utility which can detect blacklisted files by their MD5 hash in a very timely manner. It can be used to remove malicious files, check for the existance of a file when the MD5 hash is known, and to recover lost files when they are moved to an unknown path.

## These are the installation instructions for Windows
Extract zip into a path of your choice, preferably C:\Program Files(x86)\
2. Add newly extracted directory named HashAxe to Enviornment path variable
3. Test if hashaxe can be used in windows terminal
## These are the installation instructions for Mac

## These are the installation instructions for Linux
1. Extract linux binary into /etc/ so all the HashAxe files are located inside /etc/HashAxe
2. Claim ownership of the HashAxe binary by running "sudo chmod +x /etc/HashAxe/HashAxe"
3. Create a new file in /usr/bin/ called "hashaxe" and add the following content:
```
#! /bin/sh
exec /etc/HashAxe/HashAxe "$@"
```
4. Claim ownership of the new file you created by running ```sudo chmod +x /usr/bin/hashaxe```
