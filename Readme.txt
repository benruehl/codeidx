CONTENTS OF THIS FILE
---------------------
  
1. Introduction
2. Features
	2.1 Create an Index
	2.2 Search in results
	2.3 Auto Update
	2.4 Editing
	2.5 Wildcard search
	2.6 Export Results
	2.7 Paging
3. Shortcuts
4. Acknowledgements
5. Author


1. INTRODUCTION
---------------
CodeIDX is an application designed to index the source code of large projects and make it possible to search them in a matter of seconds.
The results are presented in a clear fashion and the matches are highlighted accordingly.

2. FEATURES
-----------

2.1 CREATE AN INDEX
-------------------
To create a new index to search click on File.New and the index dialog will show.
Enter a name for the index. The name will be displayed in the title bar when the index is loaded.
Select a location for the index to be created in. The location should be a common location you want to put all your indices in.
Select or enter one or more folders to be indexed. All selected index sources will be merged into a single index.
In the filetypes section input the filetypes you want to index.

2.2 SEARCH IN RESULTS
---------------------
To enable search in results, open the options dialog and go to the search menu.
Click "Enable search in results".
A new icon will appear next to the search button when a search has been made.
Click the lock to lock the files currently displayed in the results.
You can remove files from the results first by right clicking them and selecting "Remove File" or pressing Delete.
When files are locked, the next search will only search the locked files.
To update the locked files, unlock and lock again.

2.3 AUTO UPDATE
---------------
To enable Auto Update, in the index menu, check "Auto Update".
When auto update is enabled, any changes made to an indexed folder while CodeIDX is running will automatically be updated in the index.
When a file is deleted, edited, renamed or a new file with an indexed file extension is inserted, the index will be updated accordingly.

WARNING: When a large number of files is updated at the same time, not all changes might be registered. In that case a refresh is recommended.

2.4 EDITING
-----------
The search results can be directly edited in CodeIDX.
Click Tools and select "Enable Editing" to enable editing.
Select a result and make changes to the file in the preview section.
After your changes press CTRL + S or click the save button to save the changes to the file.

2.5 Wildcard search
-------------------
To enable wildcard search, click "Use Wildcards" next to the search button.
When wildcard search is enabled, specific wildcards can be used to phrase a more advanced search.

Supported wildcards:

* - match zero or more characters

2.6 Export Results
-------------------
The search results of the active search can be exported to a text file.
To export the results select "Export Results" in the Tools menu.

2.7 Paging
-------------------
When executing a search, only a part of the results will be shown to increase the response time.
To load the next results, click the "load next" button in the bottom right corner or click "load all" to load the remaining results.
The next results will also be loaded when scrolling to the end of the displayed results.
While there are more results to be loaded, an asterisk will lead the header of the search tab.

3. SHORTCUTS
------------
CTRL + F - Jump to search box. When text is selected in the preview, the text is copied to the search box.
CTRL + SHIFT + F - Open new search and jump to search box. When text is selected in the preview, the text is copied to the search box.
CTRL + D - Show/hide preview
DELETE - When search result is selected, remove the selected file from the results.
CTRL + S - When the preview was edited, save the changes.
CTRL + CLICK ON FILETYPE - Select the clicked filetype and deselect all other filetypes
CTRL + SHIFT + CLICK ON FILETYPE - Deselect the clicked filetype and select all other filetypes
ENTER - If setting "Filter file on Enter" is set, the current file is filtered and other files are removed.
		Otherwise, the file is opened in the default editor.

4. ACKNOWLEDGEMENTS
-------------------
Thanks for testing and feedback goes to:
René Rönisch
Benjamin Klawitter
Lars Hildebrand
Kamil Pietraszko

5. AUTHOR
---------
Erik Kuhlig