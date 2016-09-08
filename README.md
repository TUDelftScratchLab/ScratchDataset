This repository contains the dataset of 250K Scratch projects as described in paper "How Kids Code and How We Know: An Exploratory Study on the Scratch Repository", available here: http://dl.acm.org/citation.cfm?id=2960325

The dataset is in the form of CSV files in the **RawData** folder. To obtain them, clone or download this repository to a local folder and then use a file archiver (eg 7-Zip) to unzip RawData.7z.001.

The main file of the dataset is the allBlocks.csv. This contains, for each block used in the projects, the folowing columns:

1. project ID
2. script coordinates
3. script index
4. stage or sprite indication
5. sprite name
6. indentation
7. block index in script
8. block type
9. parameters

The source files of the scraping program that we used for obtaining this information from the Scratch website are in the **Kragle** folder.

The **PaperData** folder contains CSV files with information about extreme cases of Scratch projects.
