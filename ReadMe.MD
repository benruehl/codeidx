# CodeIDX - A Source Code Indexer

This tool indexes source code using Lucene.

* It exposes a UI for searching the text of indexed files. 
It has a service subscribed to file system notification changes so that
new files are indexed, modified files are reindexed and deleted files are
removed from the index.

## Requirements

* Lucene.NET from http://lucenenet.apache.org/download.cgi
* Log4Net from 
* .NET Framework 4.5 or greater

## Setting Up to Build

1. Clone the repo from CodePlex (http://codeidx.codeplex.com/)
	a. If you want, you can create a Fork first
2. Install Visual Studio 2013 (or later)
3. Launch VS
4. Open CodeIdx.SLN
5. Tools / NuGet Package Manager / Manage NuGet packages for solution...
6. Click Restore to add all the missing packages (packages directory not committed to Git)
7. Change the certificate used to sign the EXE:
	a. Right click on CodeIDX in the solution, select Properties
	b. Go to Signing
	c. Click Create Test Certificate
	d. Create a password
	e. Save the project file.
8. Build and run the solution

## Testing
1. Select File / New to create a new index
2. Point at a directory containing sources.  
3. Click OK
4. Index will be built
5. Try searching using text box at top.

## Directories

- Helpers
- Properties
- Services - background tasks running in the EXE.
- Settings
- ViewModels
- Views

